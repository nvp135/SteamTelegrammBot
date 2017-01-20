using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TelegramBot
{
    [JsonObject]
    public class Response<T>
    {
        [JsonProperty(PropertyName ="ok", Required = Required.Always)]
        public bool Ok { get; internal set; }

        [JsonProperty(PropertyName = "descripton", Required = Required.Default)]
        public string Description { get; internal set; }

        [JsonProperty(PropertyName = "error_code", Required = Required.Default)]
        public int ErrorCode { get; internal set; }

        [JsonProperty(PropertyName = "result", Required = Required.Default)]
        public T Result { get; internal set; }
    }
}
