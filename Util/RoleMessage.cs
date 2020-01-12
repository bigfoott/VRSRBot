using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRSRBot.Util
{
    [Serializable]
    class RoleMessage
    {
        public ulong MessageId;
        public ulong RoleId;

        public RoleMessage(ulong msg, ulong role)
        {
            MessageId = msg;
            RoleId = role;
        }
    }
}
