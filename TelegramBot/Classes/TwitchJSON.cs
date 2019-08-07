using System.Collections.Generic;

namespace TelegramBot
{

    public class TwitchJSON
    {
        public int chatter_count { get; set; }
        public Chatters chatters { get; set; }
    }

    public class Chatters
    {
        public List<string> viewers { get; set; }
    }

}