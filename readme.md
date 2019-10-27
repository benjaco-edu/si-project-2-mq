
# Run Rabbit mq

`sudo docker run -d --rm -p 5672:5672 -p 15672:15672 --name localrabbit rabbitmq:3-management`

The RabbitMQ dashboard can now be viewed at `http://localhost:15672` user: guest, password: guest

# Run the server

Run the server, open `http://localhost:3000` to see the app

`sudo docker run -d -p 3000:3000 --rm --name frontend --link localrabbit:rabbitmq bslcphbussiness/si-mq-server`
