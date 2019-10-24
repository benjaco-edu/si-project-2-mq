## MQ channels

*sends* events to `stock_requests`

format: 

```
{
    id: <number>,
    amount: <string>,
    stock: <string>
}
```

*receives* event from `stock_offers`

format: json object with the id prop set to the same as `stock_requests`

# Run the server in demo mode

Run the server, and see some events come back for the bookings, give it a secound or 2 to start RabbitMQ

`sudo docker run -e demoserver=1 -p 3000:3000 --rm --link localrabbit:rabbitmq bslcphbussiness/si-mq-server`