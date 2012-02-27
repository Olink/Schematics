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
            get { return new Version("1.0.0.0"); }
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

            bool chest = false;
            String path = Path.Combine(TShock.SavePath, "schematics", args.Parameters[1] + ".scheme");
            if( args.Parameters[1] == "-c" && args.Parameters.Count > 2 )
            {
                path = Path.Combine(TShock.SavePath, "schematics", args.Parameters[2] + ".scheme");
                chest = true;
            }
            else if( args.Parameters[1] == "-c" )
            {
                args.Player.SendMessage("You need to specify a file name.", Color.Red);
                return;
            }
            
            if( cmd == "export" || cmd == "e" || cmd=="save" || cmd=="s" )
            {
                if( File.Exists( path ) )
                {
                    args.Player.SendMessage("That file already exists, please pick a new name.", Color.Red);
                    return;
                }
                if (!args.Player.TempPoints.Any(p => p == Point.Zero))
                {
                    Scheme s = new Scheme(chest);
                    s.Save( args, path);
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

                    Scheme s = (Scheme)bf.Deserialize(fs);

                    s.Load( args, x, y );
                    fs.Close();
                    args.Player.TempPoints[0] = Point.Zero;
					args.Player.TempPoints[1] = Point.Zero;
                }
                else
                {
                    args.Player.SendMessage("Please define one point using /region set [1 or 2]", Color.Red);
                }
            }

        }
    }
}
