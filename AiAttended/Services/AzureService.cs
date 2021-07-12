using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AiAttended.Data;
using AiAttended.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;

namespace AiAttended.Services
{
    public class AzureService : IAzureService
    {
        private FaceClient faceClient;
        private IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private AiAttendedContext _context;

        public AzureService(IConfiguration configuration, IWebHostEnvironment hostEnvironment, AiAttendedContext context)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            InitFaceClient();
            _context = context;
        }

        public async Task<ResponseManager> AddPersonAsync(AddPersonViewModel model)
        {
            var existingUser = _context.People.Where(x => x.Name == model.Name);
            if(existingUser != null)
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "User already exists",
                };
            }
            var personGroup = await GetPersonGroup();
            var personGroupId = _configuration["AzureDetails:PersonGroupID"];
            var recognitionModel = _configuration["AzureDetails:RecognitionModel"];
            if (personGroup.isSuccess == false)
            {
                await faceClient.LargePersonGroup.CreateAsync(personGroupId, recognitionModel: recognitionModel, name: "TestGroup");
            }
            //foreach (var groupedFace in personFaceList.Keys)
            //{
            //    // Limit TPS
            //    //await Task.Delay(250);
            //    Person person = await faceClient.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: name);

            //    // Add face to the person group person.
            //    foreach (var similarImage in personFaceList[groupedFace])
            //    {
            //        PersistedFace face = await faceClient.PersonGroupPerson.AddFaceFromUrlAsync(personGroupId, person.PersonId,
            //            $"{url}{similarImage}", similarImage);
            //    }
            //}

            Person person = await faceClient.LargePersonGroupPerson.CreateAsync(largePersonGroupId: personGroupId, name: model.Name);

            if(person == null)
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "Unable to create user profile. Try Again",
                };
            }
            // Add face to the person group person.
            foreach (var similarImage in model.Images)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(similarImage.FileName);
                string extension = Path.GetExtension(similarImage.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwRootPath + "/img/", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await similarImage.CopyToAsync(fileStream);
                    PersistedFace face = await faceClient.LargePersonGroupPerson.AddFaceFromStreamAsync(largePersonGroupId: personGroupId, personId: person.PersonId, image: fileStream);
                }
            }

            _context.People.Add(person);
            var result = await _context.SaveChangesAsync();

            if(result > 0)
            {
                return new ResponseManager
                {
                    isSuccess = true,
                    Message = "User Added Successfully",
                };
            }
            else
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "Unable to save user to Database. Try Again",
                };
            }
        }




        public async Task<ResponseManager> TrainGroupAsync()
        {
            var personGroupId = _configuration["AzureDetails:PersonGroupID"];
            var personGroupExists = await GetPersonGroup();
            if (!personGroupExists.isSuccess)
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = "Unable to find person group. First, create a group by adding a person",
                };
            }
            await faceClient.LargePersonGroup.TrainAsync(personGroupId);

            // Wait until the training is completed.
            while (true)
            {
                await Task.Delay(1000);
                var trainingStatus = await faceClient.LargePersonGroup.GetTrainingStatusAsync(personGroupId);
                if (trainingStatus.Status == TrainingStatusType.Succeeded)
                {
                    return new ResponseManager
                    {
                        isSuccess = true,
                        Message = "Training completed",
                    };
                }
                else if(trainingStatus.Status == TrainingStatusType.Failed)
                {
                    return new ResponseManager
                    {
                        isSuccess = false,
                        Message = "Training Failed. Try again",
                    };
                }
            }
        }





        public async Task<Tuple<ResponseManager, List<Tuple<Person, double>>>> IdentifyFacesAsync(AddPersonViewModel model)
        {
            var response = new ResponseManager();

            var personGroupId = _configuration["AzureDetails:PersonGroupID"];
            var recognitionModel = _configuration["AzureDetails:RecognitionModel"];
            var identifiedPersons = new List<Tuple<Person, double>>();

            List<Guid> sourceFaceIds = new List<Guid>();

            var personGroupExists = await GetPersonGroup();
            if (!personGroupExists.isSuccess)
            {
                response = new ResponseManager
                {
                    isSuccess = false,
                    Message = "Unable to find person group. First, create a group by adding a person",
                };
                return new Tuple<ResponseManager, List<Tuple<Person, double>>>(response, null);
            }

            List<DetectedFace> detectedFaces = await DetectFace(faceClient, model.Images, recognitionModel);
            if(detectedFaces == null)
            {
                response = new ResponseManager
                {
                    isSuccess = false,
                    Message = "No face detected in the image(s)",
                };
                return new Tuple<ResponseManager, List<Tuple<Person, double>>>(response, null);
            }

            // Add detected faceId to sourceFaceIds.
            foreach (var detectedFace in detectedFaces)
            {
                sourceFaceIds.Add(detectedFace.FaceId.Value);
            }

            var identifyResults = await faceClient.Face.IdentifyAsync(sourceFaceIds, largePersonGroupId: personGroupId);

            if(identifyResults == null)
            {
                response = new ResponseManager
                {
                    isSuccess = false,
                    Message = "Unable to identify persons. Try again",
                };
                return new Tuple<ResponseManager, List<Tuple<Person, double>>>(response, null);
            }

            foreach (var identifyResult in identifyResults)
            {
                Person person = await faceClient.LargePersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                var confidence = identifyResult.Candidates[0].Confidence;

                identifiedPersons.Add(new Tuple<Person, double>(person, confidence));
            }

            var attendance = new Attendance
            {
                DateTime = DateTime.Now,
                Attendees = identifiedPersons,
            };

            _context.Attendances.Add(attendance);
            var result = await _context.SaveChangesAsync();

            if(result > 0)
            {
                response = new ResponseManager
                {
                    isSuccess = true,
                    Message = "Attendance Generated Successfully",
                };
                return new Tuple<ResponseManager, List<Tuple<Person, double>>>(response, identifiedPersons);
            }
            else
            {
                response = new ResponseManager
                {
                    isSuccess = false,
                    Message = "Unable to generate attendance. Try again",
                };
                return new Tuple<ResponseManager, List<Tuple<Person, double>>>(response, null);
            }
        }





        public async Task<ResponseManager> GetPersonGroup()
        {
            var client = new RestClient($"https://{_configuration["AzureDetails:Location"]}.api.cognitive.microsoft.com/");
            var personGroupId = _configuration["AzureDetails:PersonGroupID"];
            var request = new RestRequest($"face/v1.0/largepersongroups/{personGroupId}[?returnRecognitionModel]", Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", _configuration["AzureDetails:Key"]);
            var response = await client.ExecuteAsync<PersonGroupResponseModel>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new ResponseManager
                {
                    isSuccess = true,
                    Message = response.Data.LargePersonGroupId.ToString(),
                };
            }
            else
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = response.StatusDescription,
                };
            }
        }


        public async Task<List<DetectedFace>> DetectFace(IFaceClient faceClient, IFormFileCollection image, string recognition_model)
        {
            IList<DetectedFace> detectedFaces = null;
            List<DetectedFace> allDetectedFaces = new List<DetectedFace>();
            // We use detection model 3 because we are not retrieving attributes.
            foreach (var similarImage in image)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(similarImage.FileName);
                string extension = Path.GetExtension(similarImage.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string path = Path.Combine(wwwRootPath + "/images/", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    await similarImage.CopyToAsync(fileStream);
                    using (Stream imageStream = File.OpenRead(path))
                    {
                        detectedFaces = await faceClient.Face.DetectWithStreamAsync(imageStream, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03);
                        if (detectedFaces.Count < 1)
                            return null;
                        foreach (var face in detectedFaces)
                        {
                            allDetectedFaces.Add(face);
                        }
                    }
                }
            }
            if (allDetectedFaces.Count < 1)
                return null;
            return allDetectedFaces;
        }


        void InitFaceClient()
        {
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(_configuration["AzureDetails:Key"]);
            faceClient = new FaceClient(credentials);
            faceClient.Endpoint = _configuration["AzureDetails:Endpoint"];
            FaceOperations faceOperations = new FaceOperations(faceClient);
        }
    }
}
