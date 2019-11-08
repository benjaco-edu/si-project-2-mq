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
            string CompName = "CONTENT-BASED-ROUTER COMPONENT";
            var factory = new ConnectionFactory(){HostName=HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //queue to receive data from the client - using default
                    channel.QueueBind("msg_expiration", "expiration_ex", "");
                    //exchange/queues to send received data further through the pipeline
                    channel.ExchangeDeclare("stock_type", type:ExchangeType.Direct);
                    
                    
                    channel.BasicPublish("logger_ex","", body: Encoding.UTF8.GetBytes($"Starting - {CompName}"));

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var RKey = GetRoutingKeyRuleEngine(message);
                        channel.BasicPublish(exchange: "stock_type", routingKey: RKey, body: body); 
                        channel.BasicPublish("logger_ex", "", body: Encoding.UTF8.GetBytes($"{CompName} - RECEIVED : {message} - Routing using key : {RKey}"));
                    };

                    channel.BasicConsume("msg_expiration", true, consumer);

                    System.Console.WriteLine("Press enter to kill");
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
