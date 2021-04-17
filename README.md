# CatanGameManager
A project meant to help track game changes when playing the board game Settlers of Catan with the Cities and Knights expansion pack

This project can be run the following ways:
1. Docker
2. local host
3. Partially with kubernetes

To run with docker
Run docker-compose from the root of the library

To run in localhost, you must first configure kafka on your OS or enable docker / kubernetes support for Kafka.


For kubernetes kafka run the following:
kubectl create namespace CatanGame
kubectl -n CatanGame apply -f kafka.yaml (kafka yaml is located in the root folder)

