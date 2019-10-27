using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Normalizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var HostName = "localrabbit";
            var factory = new ConnectionFactory(){HostName=HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("normalizer_queue", false, false, false, null);
                    channel.QueueBind("normalizer_queue", "normalizer", "");
                    channel.ExchangeDeclare("aggregator", ExchangeType.Direct);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        var body = ea.Body;
                        ea.BasicProperties.Headers.TryGetValue("resptype", out object header);
                        var message = Encoding.UTF8.GetString(body);
                        var headerVal = Encoding.UTF8.GetString((byte[])header);

                        System.Console.WriteLine("NORMALIZER RECEIVED:");
                        System.Console.WriteLine("*********************************************");
                        System.Console.WriteLine("HEADER (messagetype-format) : " + headerVal);
                        System.Console.WriteLine("==============================================");
                        System.Console.WriteLine("MESSAGE : " + message);
                        System.Console.WriteLine("*********************************************");
                        System.Console.WriteLine("SENDING:");
                        System.Console.WriteLine("---------------------------------------------");
                        System.Console.WriteLine(NormalizeAllTheThings(message, headerVal));
                        var normalizedMessageBody =Encoding.UTF8.GetBytes(NormalizeAllTheThings(message, headerVal));

                        channel.BasicPublish("aggregator", "", body: normalizedMessageBody);
                    };

                    channel.BasicConsume("normalizer_queue", true, consumer);


                    System.Console.WriteLine("waiting ... press enter to kill");
                    Console.ReadLine();
                } 
                System.Console.WriteLine("Shutting down ...");               
            }
        }

        //Normalizer will convert all to json-format
        private static string NormalizeAllTheThings(string messageData, string ConvertFromType){

            //change it from XML into ResponseModel-POCO into JSON
            if(ConvertFromType == "xml"){
                using(var stringReader = new StringReader(messageData))
                {
                    var serializer = new XmlSerializer(typeof(ResponseModel));
                    var tmpconvert = serializer.Deserialize(stringReader) as ResponseModel;
                    return JsonConvert.SerializeObject(tmpconvert);
                }
            }

            if(ConvertFromType == "json"){
                //yeah...
                return messageData;
            }

            return String.Empty;
        }
    }
}
