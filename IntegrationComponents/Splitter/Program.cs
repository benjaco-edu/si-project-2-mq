using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Splitter
{
    class Program
    {
        static void Main(string[] args)
        {
            var HostName = "localhost";
            var factory = new ConnectionFactory(){HostName=HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("splitter_queue", false,false,false,null);
                    channel.QueueBind("splitter_queue", "splitter", "");
                    channel.ExchangeDeclare("server_response", ExchangeType.Direct);
                    channel.QueueBind("stock_offers", "server_response", "");
                    //tmp storage to to sort for the best offer
                    List<SimpleRespModel> sortedList;

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        sortedList = new List<SimpleRespModel>();
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var liste = JsonConvert.DeserializeObject<AggregateRespModel>(message).AggregateRespMsg;

                        foreach (var item in liste)
                        {
                            var tmpobj = JsonConvert.DeserializeObject<ResponseModel>(item);
                            System.Console.WriteLine("-------------------------------------");
                            System.Console.WriteLine($"[INFO] RECEIVING : {tmpobj.BrokerId} - {tmpobj.ClientRequestId} - {tmpobj.TotalPrice} - {tmpobj.OriginalMessage}");
                            System.Console.WriteLine("-------------------------------------");

                            var jsonOri = JsonConvert.DeserializeObject<OriginalMsg>(tmpobj.OriginalMessage);
                            var tmp = new SimpleRespModel(){id=tmpobj.ClientRequestId, amount=jsonOri.amount, stock=jsonOri.stock, totalPrice=tmpobj.TotalPrice, broker=tmpobj.BrokerId};
                            sortedList.Add(tmp);
                        }

                        sortedList.Sort();
                        var msgbody = JsonConvert.SerializeObject(sortedList[0]);
                        var Rbody = Encoding.UTF8.GetBytes(msgbody);
                        channel.BasicPublish("server_response","", body:Rbody);
                        System.Console.WriteLine("[INFO] - Sending : " + msgbody);

                    };

                    channel.BasicConsume("splitter_queue", true, consumer);

                    System.Console.WriteLine("Waiting for data ... press enter to kill");
                    Console.ReadLine();
                }
                System.Console.WriteLine("Shutting down ...");
            }
        }
    }
}
