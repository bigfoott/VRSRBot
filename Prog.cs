using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRSRBot.Util;

namespace VRSRBot
{
    class Prog
    {
        public static Config Config;
        public static Bot Bot;
        public static Game[] Games;

        static void Main(string[] args)
        {
            Log("Starting...");

            if (!Directory.Exists("files"))
            {
                Directory.CreateDirectory("files");
                Log("'files' directory not found. Directory created.", "&e"); 
            }
            if (!File.Exists("files/config.json"))
            {
                File.WriteAllText("files/config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                Log("'files/config.json' file not found. File created.", "&e");
            }

            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("files/config.json"));
            if (Config.Token == "")
            {
                Log("Error: You need to fill out the 'token' field in 'files/config.json' before starting.\n\n&7Press any key to continue...", "&c");
                Console.Read();
                return;
            }

            if (!File.Exists("files/games.json"))
            {
                File.WriteAllText("files/games.json", JsonConvert.SerializeObject(new Game[0], Formatting.Indented));
                Log("'files/games.json' file not found. File created.", "&e");
            }
            Games = JsonConvert.DeserializeObject<Game[]>(File.ReadAllText("files/games.json"));

            Log("Loaded config files.");

            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult(); 
        }

        static async Task MainAsync(string[] args)
        {
            Log("Starting MainAsync...", "&2");

            Bot = new Bot(Config);
            
            await Task.Delay(-1);
        }

        public static void Log(string message, string color = "&7")
        {
            FConsole.WriteLine($"{color}[{DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")}]%0&f {message}");
        }
    }
}
