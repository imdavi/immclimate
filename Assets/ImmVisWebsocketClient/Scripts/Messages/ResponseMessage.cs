using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ImmVis.Messages
{
    [JsonObject]
    class ResponseMessage : Message
    {
        [JsonProperty("action")] private string action;

        [JsonProperty("data")] private Dictionary<string,object> data;

        [JsonIgnore]
        public string Action
        {
            get { return action; }
        }

        public ResponseMessage() : base(MessageType) { }

        public const string MessageType = "response";
        
        public static Message CreateInstance()
        {
            return new ResponseMessage();
        }
    }
}
