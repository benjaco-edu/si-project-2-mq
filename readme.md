
# Run Rabbit mq

`sudo docker run -d --rm -p 5672:5672 -p 15672:15672 --name localrabbit rabbitmq:3-management`

The RabbitMQ dashboard can now be viewed at `http://localhost:15672` user: guest, password: guest

# Run the server

Run the server, open `http://localhost:3000` to see the app

`sudo docker run -d -p 3000:3000 --rm --name frontend --link localrabbit:rabbitmq bslcphbussiness/si-mq-server`

# Integration components
The integration components are their own little app setup on to run inside a container

If you dont care about the debug/info/console dump you can simply run them all as detached
```
sudo docker run -d -it --name normalizer --link localrabbit cphjs284/si2normalizer
sudo docker run -d -it --name aggregator --link localrabbit cphjs284/si2aggregator
sudo docker run -d -it --name contentbasedrouter --link localrabbit cphjs284/si2contentbasedrouter
sudo docker run -d -it --name splitter --link localrabbit cphjs284/si2splitter
```
If you want to see the output, run of each the above commands without the -d flag and in separate terminals.

# Broker
The Broker app takes 3 parameters [MESSAGE-DATA-FORMAT][STOCK-TYPE][BROKER-NAME] , where valid type of message-data-format are either xml or json - this indicates the data format the broker use for its reply. Stock-type is either nasq or dow, indicating wether the specific broker trades in tech-stock (nasq) or regular (dow). Broker-name is used to differentiate between the brokers.
Start up as many brokers as you like, here we start up 3. 2 of them trading normal stock; 1 replying in in xml and 1 in json. The last 1 trades tech stock and replies in json format.

```
sudo docker run -d -it --name highbroker --link localrabbit cphjs284/si2broker json nasq highbroker
sudo docker run -d -it --name lowbroker --link localrabbit cphjs284/si2broker xml dow lowbroker
sudo docker run -d -it --name crapbroker --link localrabbit cphjs284/si2broker json dow crapbroker
```
Again - if you want to see the output produced by the brokerapps execute the above commands without the -d flag and in separate terminals.

### Note
The aggregator, whos job it is to accumulate the output from all the brokers allows for a 5 secs timeout (giving each broker time to reply). So there will be a delay between clicking the button and receiving the data.

