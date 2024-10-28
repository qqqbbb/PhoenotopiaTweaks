using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoenotopiaTweaks
{
    internal class Util
    {
        public static void Message(string s)
        {
            PT2.display_messages.DisplayMessage(s, DisplayMessagesLogic.MSG_TYPE.SMALL_ITEM_GET);
        }
    }
}
