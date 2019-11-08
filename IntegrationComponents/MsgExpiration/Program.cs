using System;
using System.Collections.Generic;
using System.Text;
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
            string CompName = "MESSAGE EXPIRATION COMPONENT";
            var Qargs = new Dictionary<string, object>();
            if(args.Length==0){
                msgTTL = 5000;
            }else
            {
                msgTTL = int.Parse(args[0]);
            }

            Qargs.Add("x-message-ttl", msgTTL);

            var factory = new ConnectionFactory(){HostName=HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    //publish to this queue
                    //messages send via this queue get "time-stamped" defined in Qargs
                    channel.ExchangeDeclare("expiration_ex", ExchangeType.Direct);
                    channel.QueueDeclare("msg_expiration", false, false, false, Qargs);
                    channel.QueueBind("msg_expiration", "expiration_ex", "");
                    
                    //log msg
                    channel.BasicPublish("logger_ex", "",body: Encoding.UTF8.GetBytes($"Starting - {CompName} - Using MSGTTL : {msgTTL}"));

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        //no need to extract message from body
                        var body = ea.Body;
                        //send to log
                        channel.BasicPublish("logger_ex", "",body: Encoding.UTF8.GetBytes($" {CompName} - Message Timestamped - {Encoding.UTF8.GetString(body)}"));
                        //send downstream
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
