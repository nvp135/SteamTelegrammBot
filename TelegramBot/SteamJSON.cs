using System.Collections.Generic;
using Newtonsoft.Json;

namespace TelegramBot
{
    public class InventoryResponse
    {
        public bool success;

        [JsonProperty("Error")]
        public string Error;

        [JsonProperty("rgInventory")]
        public object Inventory { get; set; }

        [JsonProperty("rgDescriptions")]
        public object Descriptions { get; set; }

        public Dictionary<string, rgInventory> dicInventory
        {
            get
            {
                var json = this.Inventory.ToString();
                if (json == "[]")
                {
                    return new Dictionary<string, rgInventory>();
                }
                else
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, rgInventory>>(json);
                }
            }
        }

        public Dictionary<string, rgDescriptions> dicDescriptions
        {
            get
            {
                var json = this.Descriptions.ToString();
                if (json == "[]")
                {
                    return new Dictionary<string, rgDescriptions>();
                }
                else
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, rgDescriptions>>(json);
                }
            }
        }
    }

    public class rgInventory
    {
        public string id { get; set; }
        public string classid { get; set; }
        public string instanceid { get; set; }
        public string amount { get; set; }
        public int pos { get; set; }
    }

    public class rgDescriptions
    {
        public string classid { get; set; }
        public string instanceid { get; set; }
        public string icon_url { get; set; }
        public string name { get; set; }
    }

    public class MarketResponse
    {
        public bool success { get; set; }

        public string buy_order_table { get; set; }
        public string buy_order_summary { get; set; }
        public string highest_buy_order { get; set; }
        public string lowest_sell_order { get; set; }

        [JsonProperty("buy_order_graph")]
        public IList<IList<string>> bog { get; set; }

        [JsonProperty("sell_order_graph")]
        public IList<IList<string>> sog { get; set; }

        public string price_prefix { get; set; }
        public string price_suffix { get; set; }
    }

}