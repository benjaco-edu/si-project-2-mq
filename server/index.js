const amqp = require('amqplib');
const express = require('express');
const ws = require('ws');
const http = require("http");

const app = express();
const server = http.createServer(app);


app.get('/', function (req, res) {
    res.sendfile(__dirname + '/client.html');
});

let nextId = 1;

(async () => {
    const connection = await amqp.connect('amqp://rabbitmq');
    const channel = await connection.createChannel();
    channel.assertQueue('stock_requests', {durable: false});
    channel.assertQueue('stock_offers', {durable: false});


    const wss = new ws.Server({server: server, path: "/ws"});

    wss.on('connection', function (ws) {
        ws.id = nextId++;


        ws.on('message', function (message) {
            let msg = JSON.parse(message);
            channel.sendToQueue("stock_requests", Buffer.from(JSON.stringify({
                id: ws.id,
                ...msg
            })));
        });

    });


    channel.consume('stock_offers', function (msg) {
        let offer = JSON.parse(msg.content.toString());
        Array.from(wss.clients).filter(i => i.id == offer[0].id).forEach(ws => {
            ws.send(msg.content.toString());
        })
    }, {
        noAck: true
    });

    server.listen(3000, "0.0.0.0", function () {
        console.log('App listening on port 3000!')
    });

})();


// demo server
if (typeof process.env.demoserver !== "undefined") {
    (async () => {
        const connection = await amqp.connect('amqp://rabbitmq')
        const channel = await connection.createChannel();
        channel.assertQueue('stock_requests', {durable: false});
        channel.assertQueue('stock_offers', {durable: false});

        channel.consume('stock_requests', async function (msg) {
            let {id, amount, stock} = JSON.parse(msg.content.toString());

            await new Promise(resolve => setTimeout(resolve, 2500));

            channel.sendToQueue("stock_offers", Buffer.from(JSON.stringify([
                                    {
                                      "id": id,
                                      "amount": "666",
                                      "stock": "IBM",
                                      "broker": "yosuke",
                                      "totalPrice": 1.19
                                    },{
                                        "id": id,
                                        "amount": "666",
                                        "stock": "IBM",
                                        "broker": "Jacom",
                                        "totalPrice": 134
                                      }
                                    ]
            )));

        }, {
            noAck: true
        });

    })();
}
