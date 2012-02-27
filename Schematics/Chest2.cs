using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace Schematics
{
    [Serializable()]
    public struct Chest2
    {
        public Item2[] item;
        public int x;
        public int y;

        public Chest2( int mi, int x, int y )
        {
            item = new Item2[mi];
            this.x = x;
            this.y = y;
        }
    }
}
