using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Aggregator
{
    class Program
    {
        static TimerFactory TimeDat = new TimerFactory();
        static Dictionary<string, List<string>> messageStore = new Dictionary<string, List<string>>();
        static void Main(string[] args)
        {   // storage to aggregate all messages with same clientId
            string HostName = "localrabbit";
            string CompName = "AGGREGATOR COMPONENT";
            var factory = new ConnectionFactory(){HostName=HostName};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("aggregator", ExchangeType.Direct);
                    channel.QueueDeclare("aggregator_queue", false, false, false, null);
                    channel.QueueBind("aggregator_queue", "aggregator", "");

                    channel.BasicPublish("logger_ex", "", body: Encoding.UTF8.GetBytes($"starting {CompName}"));

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        channel.BasicPublish("logger_ex", "", body: Encoding.UTF8.GetBytes($"{CompName} - RECEIVED : {message} - timer started"));
                        StoreMessage(message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };

                    TimeDat.TimerExpiredEvent += (tea) => {
                        channel.BasicPublish("logger_ex", "", body: Encoding.UTF8.GetBytes($"{CompName} - timeout for ClientRequestId {tea.Name}"));
                        var Agrmessage = new AggregateRespModel();
                        Agrmessage.AggregateRespMsg = new List<string>();
                        foreach (var item in messageStore[tea.Name])
                        {Agrmessage.AggregateRespMsg.Add(item);}
                        var jsonstr = JsonConvert.SerializeObject(Agrmessage);
                        
                        var agrBody = Encoding.UTF8.GetBytes(jsonstr);
                        channel.BasicPublish("splitter", "", body:agrBody);
                    };

                    channel.BasicConsume("aggregator_queue", false, consumer);

                    System.Console.WriteLine("Press enter to kill");
                    Console.ReadLine();
                }
                System.Console.WriteLine("Shutting down ...");
            }
        }
        static void StoreMessage(string message){
            var tmp = JsonConvert.DeserializeObject<ResponseModel>(message);
            var key = tmp.ClientRequestId.ToString();
            if(!messageStore.ContainsKey(key)){
                messageStore.Add(key, new List<string>());
                messageStore[key].Add(message);
                Task.Run(()=>TimeDat.CreateTimer(key, 5000));                
            }else
            {
                messageStore[key].Add(message);
            }
        }
    }
}
