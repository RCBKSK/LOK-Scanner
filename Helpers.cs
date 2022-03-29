using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lok_wss.database.Models;

namespace lok_wss
{
    class Helpers
    {
        public static async void ParseObjects(string type, IEnumerable<Models.Object> objects, int thisContinent)
        {
            checkObjects(objects.ToList(), thisContinent.ToString());

            foreach (var gameObject in objects)
            {

                try
                {
                    using (var _context = new lokContext())
                    {

                        string location = gameObject.loc[1] + ":" + gameObject.loc[2];
                        switch (type)
                        {
                            case "cmines":
                                if (_context.crystalMine.Any(x => x.id == gameObject._id))
                                {
                                    var storedCMine = _context.crystalMine.AsQueryable().Where(s => s.id == gameObject._id)
                                        .OrderByDescending(x => x.found).ToList()[0];
                                    var timeBetween = (DateTime.UtcNow - storedCMine.found).TotalHours;
                                    if (timeBetween <= 3)
                                    {
                                        continue;
                                    }
                                }
                                _context.crystalMine.Add(new crystalMine()
                                {
                                    id = gameObject._id,
                                    continent = $"{thisContinent}",
                                    found = DateTime.UtcNow,
                                    location = location,
                                    uguid = Guid.NewGuid()
                                });

                                if (gameObject.occupied == null)
                                    Discord.PostToDiscordCmine(thisContinent, gameObject.code.ToString(), location,
                                        gameObject.level.ToString(), gameObject.param.value.ToString());

                                await _context.SaveChangesAsync();

                                break;
                            case "goblins":
                                if (_context.treasureGoblin.Any(x => x.id == gameObject._id))
                                {
                                    continue;
                                }
                                _context.treasureGoblin.Add(new treasureGoblin()
                                {
                                    id = gameObject._id,
                                    continent = $"{thisContinent}",
                                    found = DateTime.UtcNow,
                                    location = location,
                                    uguid = Guid.NewGuid()
                                });

                                if (gameObject.occupied == null)
                                    Discord.PostToDiscordGoblins(thisContinent, gameObject.code.ToString(), location,
                                        gameObject.level.ToString(), gameObject.param.value.ToString());

                                await _context.SaveChangesAsync();
                                break;

                            case "magdar":
                                Discord.PostToDiscordMagdar(thisContinent, gameObject.code.ToString(), location,
                                        gameObject.level.ToString(), gameObject.param.value.ToString());
                                    break;

                        }
                    }
                }
                catch (Exception e)
                {
                    Discord.logError("parse Objects", e);
                }

            }

        }


        public static string ExtractJson(string source)
        {
            var buffer = new StringBuilder();
            var depth = 0;

            // We trust that the source contains valid json, we just need to extract it.
            // To do it, we will be matching curly braces until we even out.
            for (var i = 0; i < source.Length; i++)
            {
                var ch = source[i];
                var chPrv = i > 0 ? source[i - 1] : default;

                buffer.Append(ch);

                // Match braces
                if (ch == '{' && chPrv != '\\')
                    depth++;
                else if (ch == '}' && chPrv != '\\')
                    depth--;

                // Break when evened out
                if (depth == 0)
                    break;
            }

            return buffer.ToString();
        }

        public static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }


        public static void checkObjects(List<Models.Object> mapObjects, string thisContinent)
        {
            var objects = mapObjects;

            var deathkar = objects.Where(x => x.code.ToString() == "20200201");
            var greenDrag = objects.Where(x => x.code.ToString() == "20200202");
            var redDrag = objects.Where(x => x.code.ToString() == "20200203");
            var goldDrag = objects.Where(x => x.code.ToString() == "20200204");


            var player = objects.Where(x => x.code.ToString() == "20300101");

            var allianceCenter = objects.Where(x => x.code.ToString() == "20600101");
            var allianceTower =objects.Where(x => x.code.ToString() == "20600102");
            var allianceOutpost = objects.Where(x => x.code.ToString() == "20600103");

            var shrine1 = objects.Where(x => x.code.ToString() == "20400101");
            var shrine2 = objects.Where(x => x.code.ToString() == "20400201");
            var shrine3 = objects.Where(x => x.code.ToString() == "20400301");
            var shrine4 = objects.Where(x => x.code.ToString() == "20400401");


            var farm = objects.Where(x => x.code.ToString() == "20100101");
            var lumber = objects.Where(x => x.code.ToString() == "20100102");
            var quarry = objects.Where(x => x.code.ToString() == "20100103");
            var goldMine = objects.Where(x => x.code.ToString() == "20100104");
            var cMine = objects.Where(x => x.code.ToString() == "20100105");

            //var cvcFarm = objects.Where(x => x.code.ToString() == "20700405");
            //var cvcLumber = objects.Where(x => x.code.ToString() == "20700602");
            //var cvcQuarry = objects.Where(x => x.code.ToString() == "20700603");
            var cvcGoldMine = objects.Where(x => x.code.ToString() == "20700604");

            var orc = objects.Where(x => x.code.ToString() == "20200101");
            var skeleton = objects.Where(x => x.code.ToString() == "20200102");
            var golem = objects.Where(x => x.code.ToString() == "20200103");
            var goblin = objects.Where(x => x.code.ToString() == "20200104");

            var cvcCyclops = objects.Where(x => x.code.ToString() == "20700407");


            var cvcMagdar = objects.Where(x => x.code.ToString() == "20700505");


            var charmStone = objects.Where(x => x.code.ToString() == "20500101");
            var charmAttack = objects.Where(x => x.code.ToString() == "20500102");
            var charmLumber = objects.Where(x => x.code.ToString() == "20500103");
            var charmLoad = objects.Where(x => x.code.ToString() == "20500104");


            var alianceBuilding = objects.Where(x => x.code.ToString() == "20600101");
            var whatsLeft = objects
                .Except(farm)
                .Except(lumber)
                .Except(quarry)
                .Except(goldMine)
                .Except(skeleton)
                .Except(orc)
                .Except(golem)
                .Except(goblin)
                .Except(player)
                .Except(alianceBuilding)
                .Except(deathkar)
                .Except(greenDrag)
                .Except(redDrag)
                .Except(goldDrag)
                .Except(allianceCenter)
                .Except(allianceOutpost)
                .Except(allianceTower)
                .Except(shrine1)
                .Except(shrine2)
                .Except(shrine3)
                .Except(shrine4)
                .Except(charmStone)
                .Except(charmAttack)
                .Except(charmLumber)
                .Except(charmLoad)
                .Except(cMine)
                //.Except(cvcFarm)
                //.Except(cvcLumber)
                //.Except(cvcQuarry)
                .Except(cvcGoldMine)
                .Except(cvcCyclops)
                .Where(x => x.code != 0).ToList();

           


            foreach (var unknownObject in whatsLeft)
            {
                switch (unknownObject.level)
                {
                    case 1:
                    case 2:
                    case 3:
                        //Discord.PostToDiscordUnknown(thisContinent, unknownObject.code.ToString(), unknownObject.loc[1] + ":" + unknownObject.loc[2], unknownObject.level.ToString(), unknownObject.param.value.ToString());

                        break;
                }
            }


        }

    }
}
