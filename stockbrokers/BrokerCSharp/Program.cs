using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BrokerCSharp
{
    class Program
    {

        //decide profitmargin for broker
        static Random rng = new Random(); 
        static double quota1 = rng.Next(150,200)/10.0; // low volume purchase
        static double quota2 = rng.Next(100,150)/10.0; // high volume purchase

        static void Main(string[] args)
        {
            string HostName = "localrabbit";
            string BrokerId;
            if (args.Length <= 2){
                BrokerId = Guid.NewGuid().ToString();
            }else
            {
                BrokerId = args[2];
            }
            //decide what type of stocks the broker can handle
            var BrokerStockType = SetBrokerStockType(args);
            //decide what type of format the broker uses for communication
            var MessageTypeFormat = SetMessageTypeFormat(args);
            //set random response time, reasons (100-1000ms)
            var responseDelay = rng.Next(10,100)*10;
            var factory = new ConnectionFactory(){HostName = HostName};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    System.Console.WriteLine("*********************************************");
                    System.Console.WriteLine($"Starting Broker with ID : {BrokerId}");
                    System.Console.WriteLine("*********************************************");

                    //create queue to receive data
                    var RandomQueueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(RandomQueueName, "stock_type", BrokerStockType);

                    //create exchanger to send data
                    channel.ExchangeDeclare("normalizer", ExchangeType.Direct);
                    //create appropriate headers for normalizer
                    var dict = new Dictionary<string, object>();
                    dict.Add("resptype", MessageTypeFormat);
                    var props = channel.CreateBasicProperties();
                    props.Headers = dict;

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) => {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                    System.Console.WriteLine("*********************************************");
                        System.Console.WriteLine($"DATA RECEIVED {message} - {MessageTypeFormat} - {BrokerStockType}");
                    System.Console.WriteLine("*********************************************");

                        //working
                        Thread.Sleep(responseDelay);
                        var resp = CreateResp(message, MessageTypeFormat, BrokerId);


                        var respmsg = Encoding.UTF8.GetBytes(resp);
                        //channel.BasicPublish(exchange: "normalizer", routingKey: "", basicProperties: props, body: respmsg);
                        channel.BasicPublish("normalizer", routingKey: "", basicProperties: props, body: respmsg);
                    System.Console.WriteLine("*********************************************");
                        System.Console.WriteLine($"RESPONSE : {resp}");
                    System.Console.WriteLine("*********************************************");


                    };

                    channel.BasicConsume(RandomQueueName, true, consumer);

                    System.Console.WriteLine("Waiting ... enter to kill");
                    Console.ReadLine();
                }
                System.Console.WriteLine("Shutting down");
            }
        }

        static string CreateResp(string data, string format, string BankId){
            var datamodel = JsonConvert.DeserializeObject<StockreqModel>(data);
            double totalPrice;
            if(datamodel.amount<=100){
                var subtotal = datamodel.amount * 100.0;
                totalPrice = subtotal * ((quota1)/100.0)+1.0;                           
            }else
            {
                var subtotal = datamodel.amount * 100.0;
                totalPrice = subtotal * ((quota2)/100.0)+1.0;                           
            }
            //create response object
            var tmpobj = new ResponseModel(){
                BrokerId = BankId,
                TotalPrice = totalPrice,
                ClientRequestId = datamodel.id,
                OriginalMessage = data
            };

            // wrap response message as JSON
            if (format == "json"){
                return JsonConvert.SerializeObject(tmpobj);
            }
            // wrap response message as XML
            if (format == "xml"){
                var strWrtr = new StringWriter();
                var serializer = new XmlSerializer(tmpobj.GetType());
                serializer.Serialize(strWrtr, tmpobj);
                return strWrtr.ToString();
            }
            return String.Empty;

        }
        static string SetBrokerStockType(string[] args){
            if(args.Length <= 1){
                System.Console.WriteLine("[INFO] - no BrokerStockType set, using default(nasq)");
                return "ClassA";
            }else
            {
                if(args[1] != "nasq" && args[1] != "dow"){
                    System.Console.WriteLine("[INFO] - Unknow BrokerStockType specified, using default(nasq)");
                    return "ClassA";
                }else
                {
                    System.Console.WriteLine($"[INFO] - BrokerStockType set to {args[1]}");
                    return "ClassB";
                }
            }
        }
        static string SetMessageTypeFormat(string[] args){
            if(args.Length == 0){
                System.Console.WriteLine("[INFO] - no messagetype-format set, using default(json)");
                return "json";
            }else
            {
                if(args[0] != "json" && args[0] != "xml"){
                    System.Console.WriteLine("[INFO] - Unknow messagetype-format specified, using default(json)");
                    return "json";
                }else
                {
                    System.Console.WriteLine($"[INFO] - messagetype-format set to {args[0]}");
                    return args[0];
                }
            }
        }
    }
}
