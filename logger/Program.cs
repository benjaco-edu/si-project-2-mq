using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace logger
{
    class Program
    {
        //very simple logger machine - prints everything to console
        static void Main(string[] args)
        {
            string HostName = "localrabbit";
            var factory = new ConnectionFactory(){HostName=HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("logger_queue", false, false, false, null);
                    channel.ExchangeDeclare("logger_ex", ExchangeType.Direct);
                    channel.QueueBind("logger_queue", "logger_ex", "");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        System.Console.WriteLine(message);
                        System.Console.WriteLine("-----------------------------------");
                    };

                    channel.BasicConsume("logger_queue", true, consumer:consumer);
                    System.Console.WriteLine("Logger running.");
                    System.Console.WriteLine("Press enter to kill.");
                    Console.ReadLine();
                }

                System.Console.WriteLine("Shutting down...");
            }
        }
    }
}
