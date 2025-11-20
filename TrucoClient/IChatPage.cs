using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrucoClient
{
    public interface IChatPage
    {
        void AddChatMessage(string senderName, string message);
    }
}
