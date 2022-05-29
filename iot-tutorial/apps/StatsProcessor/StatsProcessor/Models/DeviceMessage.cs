using System;
using Newtonsoft.Json;

namespace StatsProcessor.Models
{
    public class DeviceMessage
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }

        //[JsonProperty("timestamp")]
        //public long Timestamp { get; set; }

        [JsonProperty("deviceValue")]
        public double DeviceValue { get; set; }
    }
}
