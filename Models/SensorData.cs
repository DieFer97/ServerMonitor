namespace ServerMonitor.Models
{
    public class SensorData
    {
        public DateTime TimeStamp { get; set; }
        public double Temperature { get; set; }
        public int Light { get; set; }
        public bool Alarm { get; set; }
    }
}