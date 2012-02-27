using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Schematics
{
    [Serializable()]
    public struct Item2
    {
        public int netId;
        public int stack;

        public Item2( int netId, int stack)
        {
            this.netId = netId;
            this.stack = stack;
        }
    }
}
