using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TShockAPI;
using Terraria;

namespace Schematics
{
    [Serializable()]
    public class Scheme
    {
        public bool useChests = false;
        public TileData2[][] tiles;
        public Dictionary<int, Chest2> chests = new Dictionary<int, Chest2>( 0 );
 
        public Scheme()
        {

        }

        public Scheme(bool c)
        {
            useChests = c;
        }

        public void Save(CommandArgs args, String path)
        {
            chests.Clear();

            var x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
            var y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
            var width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
            var height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);

            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs =
                new FileStream(path, FileMode.Create, FileAccess.Write);

            tiles = new TileData2[width + 1][];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new TileData2[height + 1];
                for (int j = 0; j < tiles[i].Length; j++)
                {
                    TileData2 data = new TileData2();
                    data.active = Main.tile[x + i, y + j].Data.active;
                    data.checkingLiquid = Main.tile[x + i, y + j].Data.checkingLiquid;
                    data.frameNumber = Main.tile[x + i, y + j].Data.frameNumber;
                    data.frameX = Main.tile[x + i, y + j].Data.frameX;
                    data.frameY = Main.tile[x + i, y + j].Data.frameY;
                    data.lava = Main.tile[x + i, y + j].Data.lava;
                    data.lighted = Main.tile[x + i, y + j].Data.lighted;
                    data.liquid = Main.tile[x + i, y + j].Data.liquid;
                    data.skipLiquid = Main.tile[x + i, y + j].Data.skipLiquid;
                    data.type = Main.tile[x + i, y + j].Data.type;
                    data.wall = Main.tile[x + i, y + j].Data.wall;
                    data.wallFrameNumber = Main.tile[x + i, y + j].Data.wallFrameNumber;
                    data.wallFrameX = Main.tile[x + i, y + j].Data.wallFrameX;
                    data.wallFrameY = Main.tile[x + i, y + j].Data.wallFrameY;
                    data.wire = Main.tile[x + i, y + j].Data.wire;

                    if (Main.tile[x + i, y + j].type == 21)
                    {
                        int id = Terraria.Chest.FindChest(x + i, y + j);
                        if (id != -1 && !chests.ContainsKey(id))
                        {
                            Chest c = Main.chest[id];
                            Chest2 temp = new Chest2( Chest.maxItems, (c.x - x), (c.y - y));

                            Item[] items = c.item;
                            for( int k = 0; k < items.Length; k++ )
                            {

                                Item2 temp2;

                                if( items[k] == null)
                                {
                                    temp2 = new Item2(-100, 0);
                                }
                                else
                                {
                                    temp2 = new Item2(items[k].netID, items[k].stack);
                                }

                                temp.item[k] = temp2;
                            }
                            chests.Add(id, temp);
                        }
                    }
                    tiles[i][j] = data;
                }
            }

            bf.Serialize(fs, this);
            fs.Close();
        }

        public void Load(CommandArgs args, int x, int y)
        {
            var width = tiles.Length;
            var height = tiles[0].Length;

            bool nagged = false;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    TileData t = Main.tile[x + i, y + j].Data;
                    TileData2 data = tiles[i][j];
                    t.active = data.active;
                    t.checkingLiquid = data.checkingLiquid;
                    t.frameNumber = data.frameNumber;
                    t.frameX = data.frameX;
                    t.frameY = data.frameY;
                    t.lava = data.lava;
                    t.lighted = data.lighted;
                    t.liquid = data.liquid;
                    t.skipLiquid = data.skipLiquid;
                    t.type = data.type;
                    t.wall = data.wall;
                    t.wallFrameNumber = data.wallFrameNumber;
                    t.wallFrameX = data.wallFrameX;
                    t.wallFrameY = data.wallFrameY;
                    t.wire = data.wire;
                    Tile tile = new Tile(Main.tile, x + i, y + j);

                    if( t.type == 21 && !useChests )
                    {
                        t.type = 0;
                        t.active = false;
                    }

                    tile.Data = t;
                }
            }

            foreach (var kvpair in chests)
            {
                Chest2 c = kvpair.Value;
                int success = Chest.CreateChest(c.x + x, c.y + y);
                if (success == -1 && !nagged)
                {
                    args.Player.SendMessage("You have reached the maximum of chests, skipping the rest.", Color.Red);
                    nagged = true;
                }
                else
                {
                    
                    Chest newChest = Main.chest[success];
                    for (int k = 0; k < c.item.Length; k++)
                    {
                        Item2 it2 = c.item[k];

                        if (it2.netId < -50)
                            continue;

                        Item newItem = new Item();
                        newItem.SetDefaults( it2.netId );
                        newItem.stack = it2.stack;
                        newChest.item[k] = newItem;
                    }
                }
            }
            args.Player.SendMessage("Updating map.", Color.Yellow);
            sendSquares(x, y, width, height);
            args.Player.SendMessage("Finished updating map.", Color.Green);
        }

        private void sendSquares(int x, int y, int w, int h)
        {

            if (w > h)
            {
                int squares = w / h;
                int rem = w % h;

                for (int i = 0; i < squares; i++)
                {
                    int newX = x + (h * i);
                    int midX = newX + (h / 2);
                    TSPlayer.All.SendTileSquare(midX, y + (h / 2), h + 1 );
                }

                if (rem != 0)
                {
                    int newX = x + w - h;
                    int midX = newX + (h / 2);
                    TSPlayer.All.SendTileSquare(midX, y + (h / 2), h);
                }
            }
            else
            {
                int squares = h / w;
                int rem = h % w;

                for (int i = 0; i < squares; i++)
                {
                    int newY = y + (w * i);
                    int midY = newY + (w / 2);
                    TSPlayer.All.SendTileSquare(x + w / 2, midY, w + 1);
                }

                if (rem != 0)
                {
                    int newY = y + h - w;
                    int midY = newY + (w / 2);
                    TSPlayer.All.SendTileSquare(x + (w / 2), midY, w);
                }
            }

        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Serializable()]
    public struct TileData2
    {
        public byte liquid;
        public byte type;
        public byte wall;
        public byte wallFrameNumber;
        public byte wallFrameX;
        public byte wallFrameY;
        public short frameX;
        public short frameY;

        public bool active
        {
            get { return (byte)(this.Flags & TileFlag.Active) != 0; }
            set { this.SetFlag(TileFlag.Active, value); }
        }

        public bool checkingLiquid
        {
            get { return (byte)(this.Flags & TileFlag.CheckingLiquid) != 0; }
            set { this.SetFlag(TileFlag.CheckingLiquid, value); }
        }

        public byte frameNumber
        {
            get { return (byte)(this.Flags & (TileFlag)3); }
            set { this.Flags = ((this.Flags & (TileFlag)252) | (TileFlag)value); }
        }

        //Perhaps this is causing statue/mannequin issues.
        /*
public short frameX
{
get
{
int num = this.frame >> 8;
return (short) ((num != 255) ? ((short) (num << 1)) : -1);
}
set { this.frame = (ushort) (value >> 1 << 8 | (int) (this.frame & 255)); }
}

public short frameY
{
get
{
int num = (int) (this.frame & 255);
return (short) ((num != 255) ? ((short) (num << 1)) : -1);
}
set { this.frame = (ushort) (value >> 1 | (int) (this.frame & 65280)); }
}*/

        public bool lava
        {
            get { return (byte)(this.Flags & TileFlag.Lava) != 0; }
            set { this.SetFlag(TileFlag.Lava, value); }
        }

        public bool lighted
        {
            get { return (byte)(this.Flags & TileFlag.Lighted) != 0; }
            set { this.SetFlag(TileFlag.Lighted, value); }
        }

        public bool skipLiquid
        {
            get { return (byte)(this.Flags & TileFlag.SkipLiquid) != 0; }
            set { this.SetFlag(TileFlag.SkipLiquid, value); }
        }

        public bool wire
        {
            get { return (byte)(this.Flags & TileFlag.Wire) != 0; }
            set { this.SetFlag(TileFlag.Wire, value); }
        }

        private void SetFlag(TileFlag flag, bool set)
        {
            if (set)
            {
                this.Flags |= flag;
                return;
            }
            this.Flags &= ~flag;
        }

        private TileFlag Flags;
    }

    public enum TileFlag : byte
    {
        Unknown,
        Reserved1,
        Wire = 4,
        Active = 8,
        SkipLiquid = 16,
        Lighted = 32,
        CheckingLiquid = 64,
        Lava = 128
    }
}
