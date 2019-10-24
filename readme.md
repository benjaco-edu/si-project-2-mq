
# Run Rabbit mq

`sudo docker run -d --rm -p 5672:5672 -p 15672:15672 --name localrabbit rabbitmq:3-management`

The RabbitMQ dashboard can now be viewed at `http://localhost:15672` user: guest, password: guest

# Run the server

Run the server, and see some events come back for the bookings, give it a secound or 2 to start RabbitMQ

`sudo docker run --rm --link localrabbit:rabbitmq bslcphbussiness/si-mq-server`
