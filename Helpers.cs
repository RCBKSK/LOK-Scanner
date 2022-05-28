﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using lok_wss.database.Models;
using Object = lok_wss.Models.Object;

namespace lok_wss
{
    internal class Helpers
    {
        public static async void ParseObjects(string type, IEnumerable<Object> objects, int thisContinent)
        {
            //checkObjects(objects.ToList(), thisContinent.ToString());

            foreach (Object gameObject in objects)
            {
                try
                {
                    using (lokContext context = new())
                    {
                        string location = gameObject.loc[1] + ":" + gameObject.loc[2];
                        switch (type)
                        {
                            case "cmines":
                                if (context.crystalMine.Any(x => x.id == gameObject._id))
                                {
                                    crystalMine storedCMine = context.crystalMine.AsQueryable()
                                        .Where(s => s.id == gameObject._id)
                                        .OrderByDescending(x => x.found).ToList()[0];
                                    double timeBetween = (DateTime.UtcNow - storedCMine.found).TotalHours;
                                    if (timeBetween <= 3) continue;
                                }

                                context.crystalMine.Add(new crystalMine
                                {
                                    id = gameObject._id,
                                    continent = $"{thisContinent}",
                                    found = DateTime.UtcNow,
                                    location = location,
                                    uguid = Guid.NewGuid()
                                });

                                if (gameObject.occupied == null)
                                {
                                    DiscordWebhooks.PostToDiscordCmine(thisContinent, gameObject.code.ToString(),
                                        location,
                                        gameObject.level.ToString(), gameObject.param.value.ToString(), underKingdom(gameObject));
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                }

                                await context.SaveChangesAsync();

                                break;
                            case "goblins":
                                if (context.treasureGoblin.Any(x => x.id == gameObject._id)) continue;
                                context.treasureGoblin.Add(new treasureGoblin
                                {
                                    id = gameObject._id,
                                    continent = $"{thisContinent}",
                                    found = DateTime.UtcNow,
                                    location = location,
                                    uguid = Guid.NewGuid()
                                });

                                if (gameObject.occupied == null)
                                {
                                    DiscordWebhooks.PostToDiscordGoblins(thisContinent, gameObject.code.ToString(),
                                        location,
                                        gameObject.level.ToString(), gameObject.param.value.ToString());
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                }


                                await context.SaveChangesAsync();
                                break;

                            case "deathkar":
                                if (context.deathkar.Any(x => x.id == gameObject._id)) continue;
                                context.treasureGoblin.Add(new treasureGoblin
                                {
                                    id = gameObject._id,
                                    continent = $"{thisContinent}",
                                    found = DateTime.UtcNow,
                                    location = location,
                                    uguid = Guid.NewGuid()
                                });

                                if (gameObject.occupied == null && gameObject.level >= 5)
                                {
                                    DiscordWebhooks.PostToDiscordDeathkar(thisContinent, gameObject.code.ToString(),
                                        location,
                                        gameObject.level.ToString(), gameObject.param.value.ToString(), underKingdom(gameObject));
                                    Thread.Sleep(TimeSpan.FromSeconds(1));
                                }


                                await context.SaveChangesAsync();
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    DiscordWebhooks.logError("parse Objects", e);
                }
            }
        }


        public static async void ParseKingdoms(List<Object> kingdoms)
        {
            foreach (Object kingdom in kingdoms)
            {
                try
                {
                    await using (lokContext context = new())
                    {
                        string location = kingdom.loc[1] + ":" + kingdom.loc[2];

                        var exists = context.kingdoms.FirstOrDefault(x => x.Id == kingdom.occupied.id);
                        if (exists != null)
                        {
                            exists.Location = location;
                            exists.LastSeen = DateTime.UtcNow;
                            exists.CastleLevel = kingdom.level;
                            exists.Alliance = kingdom.occupied.allianceTag;

                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            context.kingdoms.Add(new kingdomItem
                            {
                                Id = kingdom.occupied.id,
                                Name = kingdom.occupied.name,
                                CastleLevel = kingdom.level,
                                Alliance = kingdom.occupied.allianceTag,
                                Location = location,
                                DateStarted = kingdom.occupied.started,
                                LastSeen = DateTime.UtcNow
                            });
                            await context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    DiscordWebhooks.logError("parse Objects", e);
                }
            }
        }

        public static bool underKingdom(Models.Object gameObject)
        {
            using (lokContext context = new())
            {
                var objectLocation = gameObject.loc[1] + ":" + gameObject.loc[2];
                var exists = context.kingdoms.FirstOrDefault(x => x.Location == objectLocation);

                if(exists != null) return true;
            }
            return false;
        }


        public static string ExtractJson(string source)
        {
            StringBuilder buffer = new StringBuilder();
            int depth = 0;

            // We trust that the source contains valid json, we just need to extract it.
            // To do it, we will be matching curly braces until we even out.
            for (int i = 0; i < source.Length; i++)
            {
                char ch = source[i];
                char chPrv = i > 0 ? source[i - 1] : default;

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
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }


        public static void CheckObjects(List<Object> mapObjects, string thisContinent)
        {
            var objects = mapObjects;

            var deathkar = objects.Where(x => x.code.ToString() == "20200201");
            var greenDrag = objects.Where(x => x.code.ToString() == "20200202");
            var redDrag = objects.Where(x => x.code.ToString() == "20200203");
            var goldDrag = objects.Where(x => x.code.ToString() == "20200204");


            var player = objects.Where(x => x.code.ToString() == "20300101");

            var allianceCenter = objects.Where(x => x.code.ToString() == "20600101");
            var allianceTower = objects.Where(x => x.code.ToString() == "20600102");
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


            //var cvcMagdar = objects.Where(x => x.code.ToString() == "20700505");


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


            foreach (Object unknownObject in whatsLeft)
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