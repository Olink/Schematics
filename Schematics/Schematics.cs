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
    [APIVersion(1, 11)]
    public class Schematics : TerrariaPlugin
    {
        public override string Author
        {
            get { return "Zack Piispanen"; }
        }

        public override string Description
        {
            get{return "Export creations and import them into a new map.";}
        }

        public override string Name
        {
            get { return "Schematics"; }
        }

        public override Version Version
        {
            get { return new Version("1.0.0.1"); }
        }

        public Schematics(Main game) : base(game)
        {
            Order = 3;
            if( !Directory.Exists( Path.Combine(TShock.SavePath, "schematics")))
            {
                Directory.CreateDirectory(Path.Combine(TShock.SavePath, "schematics"));
            }
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("scheme", HandleCommand, "scheme"));
        }

        private void HandleCommand( CommandArgs args )
        {
            if( args.Player == null )
            {
                return;
            }
            if( args.Parameters.Count < 2 )
            {
                args.Player.SendMessage("You must specifiy if you are exporting or importing and a name.", Color.Red);
                return;
            }

            string cmd = args.Parameters[0].ToLower();

            String path = Path.Combine(TShock.SavePath, "schematics", args.Parameters[1] + ".scheme");
            if( cmd == "export" || cmd == "e" || cmd=="save" || cmd=="s" )
            {
                if( File.Exists( path ) )
                {
                    args.Player.SendMessage("That file already exists, please pick a new name.", Color.Red);
                    return;
                }
                if (!args.Player.TempPoints.Any(p => p == Point.Zero))
                {
                    var x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                    var y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                    var width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                    var height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs =
                        new FileStream(path, FileMode.Create, FileAccess.Write);

                    TileData2[][] tiles = new TileData2[width+1][];
                    for( int i = 0; i < tiles.Length; i++)
                    {
                        tiles[i] = new TileData2[height+1];
                        for( int j = 0; j < tiles[i].Length;j++)
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
                            tiles[i][j] = data;
                        }
                    }
                    
                    bf.Serialize(fs, tiles);
                    args.Player.SendMessage("Saved schematic.", Color.Green);
                    args.Player.TempPoints[0] = Point.Zero;
                    args.Player.TempPoints[1] = Point.Zero;
                }
                else
                {
                    args.Player.SendMessage("Please define two points using /region set [1 or 2]", Color.Red);
                }
            }
            else if (cmd == "import" || cmd == "i" || cmd == "load" || cmd == "l")
            {
                if (!File.Exists(path))
                {
                    args.Player.SendMessage("That file doesn't exists, please pick a real schematic.", Color.Red);
                    return;
                }
                Point pt = args.Player.TempPoints.First(p => p != Point.Zero);
                if (pt != Point.Zero)
                {
                    var x = pt.X;
                    var y = pt.Y;

                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream fs =
                        new FileStream(path, FileMode.Open, FileAccess.Read);

                    TileData2[][] tiles = (TileData2[][])bf.Deserialize(fs);

                    var width = tiles.Length;
                    var height = tiles[0].Length;

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
                            Tile tile = new Tile( Main.tile, x+i, y+j);
                            tile.Data = t;

                            Console.WriteLine( "Placing {0} at {1}, {2}", t.type, x+i, y+j);
                        }    
                    }
                    args.Player.SendMessage("Updating map.", Color.Yellow);
                    sendSquares(x, y, width, height);
                    args.Player.SendMessage("Finished updating map.", Color.Green);
                    args.Player.TempPoints[0] = Point.Zero;
					args.Player.TempPoints[1] = Point.Zero;
                }
                else
                {
                    args.Player.SendMessage("Please define one point using /region set [1 or 2]", Color.Red);
                }
            }

        }

        private void sendSquares(int x, int y, int w, int h)
        {

            if( w > h )
            {
                int squares = w/h;
                int rem = w%h;

                for( int i = 0; i < squares; i++ )
                {
                    int newX = x + (h*i);
                    int midX = newX + (h / 2);
                    TSPlayer.All.SendTileSquare(midX, y + (h/2), h);
                }

                if( rem != 0 )
                {
                    int newX = x + w - h;
                    int midX = newX + (h/2);
                    TSPlayer.All.SendTileSquare(midX, y + (h / 2), h+1);
                }
            }
            else
            {
                int squares = h / w;
                int rem = h% w;

                for (int i = 0; i < squares; i++)
                {
                    int newY = y + (w * i);
                    int midY = newY + (w / 2);
                    TSPlayer.All.SendTileSquare(x + w/2, midY, w+1);
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
    internal struct TileData2
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
            get { return (byte) (this.Flags & TileFlag.Active) != 0; }
            set { this.SetFlag(TileFlag.Active, value); }
        }

        public bool checkingLiquid
        {
            get { return (byte) (this.Flags & TileFlag.CheckingLiquid) != 0; }
            set { this.SetFlag(TileFlag.CheckingLiquid, value); }
        }

        public byte frameNumber
        {
            get { return (byte) (this.Flags & (TileFlag) 3); }
            set { this.Flags = ((this.Flags & (TileFlag) 252) | (TileFlag) value); }
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
            get { return (byte) (this.Flags & TileFlag.Lava) != 0; }
            set { this.SetFlag(TileFlag.Lava, value); }
        }

        public bool lighted
        {
            get { return (byte) (this.Flags & TileFlag.Lighted) != 0; }
            set { this.SetFlag(TileFlag.Lighted, value); }
        }

        public bool skipLiquid
        {
            get { return (byte) (this.Flags & TileFlag.SkipLiquid) != 0; }
            set { this.SetFlag(TileFlag.SkipLiquid, value); }
        }

        public bool wire
        {
            get { return (byte) (this.Flags & TileFlag.Wire) != 0; }
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
