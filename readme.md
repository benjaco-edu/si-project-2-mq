# System-Integration project 2

# Business-case
A user wants to buy stocks, sending out requests to all potential brokers - the user wants to choose the cheapest option (the broker with the lowest fees)

# Diagrams
The folder diagram contains a workflow diagram using BMP notation (this can be viewed with the Camunda modeler). Also in this folder is a IE component diagram (the diagram used have been lifted from here [www.enterpriseintegrationpatterns.com](https://www.enterpriseintegrationpatterns.com/patterns/messaging/toc.html) )

# Setting it up

## Run Rabbit mq

`sudo docker run -d --rm -p 5672:5672 -p 15672:15672 --name localrabbit rabbitmq:3-management`

The RabbitMQ dashboard can now be viewed at `http://localhost:15672` user: guest, password: guest

## Run the server

Run the server, open `http://localhost:3000` to see the app

`sudo docker run -d -p 3000:3000 --rm --name frontend --link localrabbit:rabbitmq bslcphbussiness/si-mq-server`

# Components
The integration components are small apps setup on to run inside a container
Output from all containers are routed to a log container, via rabbitmq - to execute the logger
```
sudo docker run -it --name logger --link localrabbit cphjs284/si2logger
```

Once this container spins up all messages send to the logger will be printed to the console.
Setup the integration components (new terminal window)
(You will notice that as these components spin up the log will output appropriate messages)
```
sudo docker run -d -it --name msgexpiration --link localrabbit cphjs284/si2msgexpiration 5000
sudo docker run -d -it --name contentbasedrouter --link localrabbit cphjs284/si2contentbasedrouter
sudo docker run -d -it --name normalizer --link localrabbit cphjs284/si2normalizer
sudo docker run -d -it --name aggregator --link localrabbit cphjs284/si2aggregator
sudo docker run -d -it --name splitter --link localrabbit cphjs284/si2splitter
```

Next connect one or more brokers to the system, see below for broker argument explaination.
No matter how many brokers are connected to the system, the splitter will always only return the best 3 offers. Incase less than 3 brokers are connected to the system, the total amount of broker offers are returned.
```
sudo docker run -d -it --name highbroker --link localrabbit cphjs284/si2broker xml dow highbroker
sudo docker run -d -it --name lowbroker --link localrabbit cphjs284/si2broker xml dow lowbroker
sudo docker run -d -it --name crapbroker --link localrabbit cphjs284/si2broker xml dow crapbroker
sudo docker run -d -it --name sutterbroker --link localrabbit cphjs284/si2broker xml dow sutterbroker
sudo docker run -d -it --name pybrokera --link localrabbit youe73/si2broker elitebroker
sudo docker run -d -it --name pybrokerb --link localrabbit youe73/si2broker middlebroker
sudo docker run -d -it --name pybrokerc --link localrabbit youe73/si2broker uselessbroker
```

# Broker
The Broker app takes 3 parameters [MESSAGE-DATA-FORMAT][STOCK-TYPE][BROKER-NAME] , where valid type of message-data-format are either xml or json - this indicates the data format the broker use for its reply. Stock-type is either nasq or dow, indicating wether the specific broker trades in tech-stock (nasq) or regular (dow). Broker-name is used to differentiate between the brokers.
Start up as many brokers as you like, here we start up 3. 2 of them trading normal stock; 1 replying in in xml and 1 in json. The last 1 trades tech stock and replies in json format.

### Note
The aggregator, whos job it is to accumulate the output from all the brokers allows for a 5 secs timeout (giving each broker time to reply). So there will be a delay between clicking the button and receiving the data.


# Clean up
Remove all containers by executing
```
sudo docker rm -f logger
sudo docker rm -f localrabbit
sudo docker rm -f frontend
sodu docker rm -f msgexpiration
sudo docker rm -f aggregator
sudo docker rm -f normalizer
sudo docker rm -f contentbasedrouter
sudo docker rm -f splitter
sudo docker rm -f highbroker
sudo docker rm -f lowbroker
sudo docker rm -f crapbroker
sudo docker rm -f sutterbroker
sudo docker rm -f pybrokera
sudo docker rm -f pybrokerb
sudo docker rm -f pybrokerc
```

