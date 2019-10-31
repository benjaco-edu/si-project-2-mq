using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace MsgExpiration
{
    class Program
    {
        static void Main(string[] args)
        {
            var HostName = "localrabbit";
            int msgTTL;
            var Qargs = new Dictionary<string, object>();
            if(args.Length==0){
                System.Console.WriteLine("Using default MSGTTL : 5000ms");
                msgTTL = 5000;
            }else
            {
                System.Console.WriteLine($"Using MSGTTL : {args[0]}");
                msgTTL = int.Parse(args[0]);
            }

            Qargs.Add("x-message-ttl", msgTTL);

            var factory = new ConnectionFactory(){HostName=HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //consume from this queue
                    channel.QueueDeclare("stock_requests", false, false, false, null);
                    
                    //publish to this queue
                    //messages send via this queue get "time-stamped" defined in Qargs
                    channel.ExchangeDeclare("expiration_ex", ExchangeType.Direct);
                    channel.QueueDeclare("msg_expiration", false, false, false, Qargs);
                    channel.QueueBind("msg_expiration", "expiration_ex", "");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        //no need to extract message from body
                        var body = ea.Body;
                        System.Console.WriteLine("DATA RECEIVED AND TIME-STAMPED");
                        channel.BasicPublish("expiration_ex", "", body:body);
                    };
                    channel.BasicConsume("stock_requests", true, consumer);
                    System.Console.WriteLine("Waiting ... press to kill ");
                    Console.ReadLine();
                }   
                System.Console.WriteLine("Shutting down ...");            
            }

        }
    }
}
