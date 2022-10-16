using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AiAttended.Data;
using AiAttended.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

        //ADD PERSON TO LARGE PERSON GROUP
        public async Task<ResponseManager> AddPersonAsync(AddPersonViewModel model)
        {
            try
            {
                //Check if User Already exists
                var existingUser = _context.Users.FirstOrDefault(x => x.Name.ToLower() == model.Name.ToLower());
                if (existingUser != null)
                {
                    return new ResponseManager
                    {
                        isSuccess = false,
                        Message = "User already exists",
                    };
                }
                var personGroupId = _configuration["AzureDetails:PersonGroupID"];
                var personGroup = await faceClient.LargePersonGroup.GetAsync(personGroupId);
                var recognitionModel = _configuration["AzureDetails:RecognitionModel"];
                if (personGroup == null)
                {
                    //Create new Large person group if it doesn't exist 
                    await faceClient.LargePersonGroup.CreateAsync(personGroupId, recognitionModel: recognitionModel, name: "TestGroup");
                }

                //Create person
                Person person = await faceClient.LargePersonGroupPerson.CreateAsync(largePersonGroupId: personGroupId, name: model.Name, userData: model.Name);

                if (person == null)
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
                    string path = Path.Combine(wwwRootPath, $"img/{fileName}");
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await similarImage.CopyToAsync(fileStream);
                        using (Stream imageStream = File.OpenRead(path))
                        {
                            PersistedFace face = await faceClient.LargePersonGroupPerson.AddFaceFromStreamAsync(largePersonGroupId: personGroupId, personId: person.PersonId, image: imageStream);
                        }
                    }
                }

                var user = new User
                {
                    Id = person.PersonId,
                    Name = model.Name,
                    Created = DateTime.Now,
                    UserGroupId = personGroupId,
                    UserData = $"AiAttended: {model.Name}-{person.PersonId}",
                };

                //Save to DB
                _context.Users.Add(user);
                var result = await _context.SaveChangesAsync();

                return new ResponseManager
                {
                    isSuccess = result > 0,
                    Message = result > 0 ? "User Added Successfully" : "Unable to save user to Database. Try Again",
                };
            }
            catch (Exception ex)
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = ex.Message,
                };
            }
        }



        //TRAIN PERSON GROUP
        public async Task<ResponseManager> TrainGroupAsync()
        {
            try
            {
                var personGroupId = _configuration["AzureDetails:PersonGroupID"];
                var personGroup = await faceClient.LargePersonGroup.GetAsync(personGroupId);
                if (personGroup == null)
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
                    else if (trainingStatus.Status == TrainingStatusType.Failed)
                    {
                        return new ResponseManager
                        {
                            isSuccess = false,
                            Message = "Training Failed. Try again",
                        };
                    }
                }
            }
            catch(Exception ex)
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = ex.Message,
                };
            }
        }




        //IDENTIFY FACES
        public async Task<Tuple<ResponseManager, List<User>>> IdentifyFacesAsync(AddPersonViewModel model)
        {
            try
            {
                var response = new ResponseManager();

                var personGroupId = _configuration["AzureDetails:PersonGroupID"];
                var recognitionModel = _configuration["AzureDetails:RecognitionModel"];
                var identifiedPersons = new List<User>();
                var meetingId = Guid.NewGuid();

                List<Guid> sourceFaceIds = new List<Guid>();

                var personGroup = await faceClient.LargePersonGroup.GetAsync(personGroupId);
                if (personGroup == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = "Unable to find person group. First, create a group by adding a person",
                    };
                    return new Tuple<ResponseManager, List<User>>(response, null);
                }

                //Detect faces in image
                List<DetectedFace> detectedFaces = await DetectFace(faceClient, model.Images, recognitionModel);
                if (detectedFaces == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = "No face detected in the image(s)",
                    };
                    return new Tuple<ResponseManager, List<User>>(response, null);
                }

                // Add detected faceId to sourceFaceIds.
                foreach (var detectedFace in detectedFaces)
                {
                    sourceFaceIds.Add(detectedFace.FaceId.Value);
                }

                var identifyResults = await faceClient.Face.IdentifyAsync(sourceFaceIds, largePersonGroupId: personGroupId);

                if (identifyResults == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = "Unable to identify persons. Try again",
                    };
                    return new Tuple<ResponseManager, List<User>>(response, null);
                }

                var meeting = new Meeting
                {
                    Id = meetingId,
                    Name = model.Name,
                    DateTime = DateTime.Now,
                };

                var meetingDetails = new List<MeetingDetails>();

                foreach (var identifyResult in identifyResults)
                {
                    if (identifyResult.Candidates.Count > 0)
                    {
                        Person person = await faceClient.LargePersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                        var confidence = identifyResult.Candidates[0].Confidence;

                        if (confidence >= 0.5)
                        {
                            var user = _context.Users.FirstOrDefault(x => x.Name == person.Name);
                            identifiedPersons.Add(user);

                            var details = new MeetingDetails
                            {
                                Id = Guid.NewGuid(),
                                MeetingId = meetingId,
                                UserId = user.Id,
                            };
                            meetingDetails.Add(details);
                        }
                    }
                }

                _context.Meetings.Add(meeting);
                await _context.MeetingDetails.AddRangeAsync(meetingDetails);
                var result = await _context.SaveChangesAsync();

                response = new ResponseManager
                {
                    isSuccess = result > 0,
                    Message = result > 0 ? "Meeting saved Successfully" : "Unable to generate attendance. Try again",
                };
                return new Tuple<ResponseManager, List<User>>(response, identifiedPersons);
            }
            catch(Exception ex)
            {
                var response = new ResponseManager
                {
                    isSuccess = false,
                    Message = ex.Message,
                };
                return new Tuple<ResponseManager, List<User>>(response, null);
            }
        }




        //DETECT FACES IN IMAGES
        public async Task<List<DetectedFace>> DetectFace(IFaceClient faceClient, IFormFileCollection image, string recognition_model)
        {
            try
            {
                IList<DetectedFace> detectedFaces = null;
                List<DetectedFace> allDetectedFaces = new List<DetectedFace>();
                // We use detection model 3 because we are not retrieving attributes.
                foreach (var similarImage in image)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(similarImage.FileName);
                    string extension = Path.GetExtension(similarImage.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension; ;
                    string path = Path.Combine(wwwRootPath, $"img/{fileName}");
                    using (var fileStream = new FileStream(path, FileMode.Create))
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

            catch(Exception)
            {
                return null;
            }
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
