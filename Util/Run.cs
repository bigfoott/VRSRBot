using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRSRBot.Core;

namespace VRSRBot.Util
{
    class Run
    {
        public string GameName;
        public string GameAbbr;
        public string GameLink;
        public string Category;
        public string CategoryLink;
        public string Link;
        public TimeSpan Time;
        public string Runner;
        public string RunnerLink;
        public string Comment;
        public string DeviceType;
        public string DeviceValue;
        public DateTime TimeStamp;

        public Run(string result)
        {
            dynamic json = JsonConvert.DeserializeObject(result);

            GameName = json.data.game.data.names.international;
            GameAbbr = json.data.game.data.abbreviation;
            GameLink = json.data.game.data.weblink;
            Category = json.data.category.data.name;
            CategoryLink = json.data.category.data.weblink;
            Link = json.data.weblink;
            TimeStamp = DateTime.Parse(json.data.date);

            Time = TimeSpan.FromSeconds((double)json.data.times.primary_t);

            if (json.data.players.data[0].rel == "user")
            {
                Runner = json.data.players.data[0].names.international;
                RunnerLink = json.data.players.data[0].weblink;
            }
            else
            {
                Runner = json.data.players.data[0].name;
                RunnerLink = "";
            }

            Comment = json.data.comment;

            Game gameObj = Prog.Games.FirstOrDefault(g => g.Name == GameAbbr);
            if (gameObj.HardwareVariable != "")
            {
                string hardwarevar = json.data.values[gameObj.HardwareVariable];
                string hardwareval = "";

                for (int i = 0; i < (json.data.category.data.variables.data).Count; i++)
                {
                    if (json.data.category.data.variables.data[i].id == gameObj.HardwareVariable)
                    {
                        hardwareval = json.data.category.data.variables.data[i].values.values[hardwarevar].label;
                    }
                }

                DeviceType = "Hardware";
                DeviceValue = hardwareval;
            }
            else
            {
                DeviceType = "Platform";
                DeviceValue = json.data.platform.data.name;
            }
        }
    }
}
