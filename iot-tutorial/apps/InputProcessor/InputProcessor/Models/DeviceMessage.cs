using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InputProcessor.Models
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

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; } 
    }
}
