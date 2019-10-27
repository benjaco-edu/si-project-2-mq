namespace BrokerCSharp
{
    public class ResponseModel
    {
        public string BrokerId { get; set; }
        public double TotalPrice { get; set; }
        public int ClientRequestId { get; set; }
        public string OriginalMessage { get; set; }
    }
}