# CatanGameManager
A project meant to help track game changes when playing the board game Settlers of Catan with the Cities and Knights expansion pack

This is a REST API only project. Later versions will include some GUI.

This project can be run using docker, which consitsts of 4 parts
 - User registarion microservice.
 - Game management microservice.
 - Mongo database.
 - Kafka server.

- Ensure you have enabled file sharing for the folder called 'https' under the root folder
- Run docker-compose up from the root of the library


Usage:

Under Tests folder, there is postman collection that was used to test the app.
First run 'AddPlayer', then 'AddPlayerToGame.'
It is incoporated with SwaggerUI to see the available endpoints for  each microservice.
After you have done 'docker-compose up'
 - For user registration type in the browser https://localhost:5001 to see available end points.
 - For game managmenr type in the browser https://localhost:5003 to see avaiable end points.

