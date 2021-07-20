# AiAttended

 AiAttended is an ASP.NET core web app that leverages Azure Vision to record attendees of a meeting. By creating Users and User groups, Training Models for Facial Detection and Recognition and Fetching Meeting history

#### Features

1. Face Detection 
2. Face Recognition
3. User and User Group Creation
4. Meeting History
5. Model Training
 
Check out the project [here](https://aiattended.herokuapp.com)

## How it works

#### 1. Add Person
By entering the name and uploading pictures of a person, firstly, it checks if the user already exists after which it checks if the PersonGroup exists on Azure. If it does not exists, it creats one.
After that, it creates a Person class on the PersonGroup, adds all the faces in the images uploaded as a PersistedFace object and also registers a new user entry in the database.

#### 2. Train Model
Trains/Retrains the model with new persons added.

#### 3. Identify Persons
This is used to identify members of the PersonGroup that were present in a meeting by uploading images from that meeting and giving the meeting a name.
Firstly, it scans the images for faces (Face Detection) then it extracts the unique face ids from the detected faces and compare them agains the face ids of members of the PersonGroup

#### 4. Meetings
The meeting tab is used to fetch details of meetings. By entering the name and date of a particular meeting, it fetches the records from the database and displays the date, User Id, Name and a binary 
value of 1 if the user attended the meeting and 0 if the user did not.
The Download CSV button downloads a .CSV version of the table.


## Tools, Frameworks & Languages

- IDE - Microsoft Visual Studio
- Version Control and CI/CD - Github
- Artificial Intelligence - Azure
- Database - PostgreSQL
- Languages - C#, Javascript
- ORM - Entity Framework
- Framework - Asp.NET Core
- Libraries - Bootstrap, Jquery
- Hosting - ElephantSQL, Heroku
