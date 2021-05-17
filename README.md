# About

A project meant to help track game changes when playing the board game Settlers of Catan with the Cities and Knights expansion pack.

This is a REST API only project. Later versions will include some GUI. 
This was developed using dot net 5.0 web api.

# Setup
## Locally:

### Requirements:
 - Dotnet 5.0 SDK.
 - MongoDb.
 - Kafka.
 - IIS / IIS Express

Publish and run on IIS Express / IIS.

## Using Docker.
The docker-compose file consitsts of 4 parts.
 - User registarion microservice.
 - Game management microservice.
 - MongoDb.
 - Kafka.

- Ensure you have enabled file sharing for the folder called 'https' under the root folder.
- Run docker-compose up from the root of the library


# Usage (docker only):

It is incoporated with SwaggerUI to see the available endpoints for each microservice.

 - For user registration type in the browser https://localhost:5001 to see available end points.
 - For game managmenr type in the browser https://localhost:5003 to see avaiable end points.
 
Altenernately, under 'Tests' folder, there is a postman collection that was used to test the app.
It is recommended to first run 'AddPlayer', then 'AddPlayerToGame.' Ater that it is possible to use the different end points.


