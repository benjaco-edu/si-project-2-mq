using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace ContentBasedRouter
{
    class Program
    {

        static void Main(string[] args)
        {
            string HostName = "localrabbit";
            var factory = new ConnectionFactory(){HostName=HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //queue to receive data from the client - using default
                    channel.QueueDeclare("requests", false, false, false, null);
                    channel.QueueBind("requests", "expiration_ex", "");
                    //exchange to send received data further through the pipeline
                    channel.ExchangeDeclare("stock_type", type:ExchangeType.Direct);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var RKey = GetRoutingKeyRuleEngine(message);
                    
                        channel.BasicPublish(exchange: "stock_type", routingKey: RKey, body: body); 
                        System.Console.WriteLine("********************************************");
                        System.Console.WriteLine($"[INFO] - RECEIVED : {message} - Routing using key : {RKey}");   
                        System.Console.WriteLine("********************************************");
                    };

                    channel.BasicConsume("requests", true, consumer);

                    System.Console.WriteLine("Waiting for data to route ... press enter to kill");
                    Console.ReadLine();
                }
                System.Console.WriteLine("Shutting down...");
            }

        }
        static string GetRoutingKeyRuleEngine(string message){
            string res="";

            var Stockreq = JsonConvert.DeserializeObject<StockreqModel>(message);
            var Company = Stockreq.stock;

            if(Company == "IBM" || Company == "Microsoft" || Company == "Intel"){
                res = "ClassA";
            }else
            {
                res = "ClassB";
            }
            return res;
        }

    }
}
