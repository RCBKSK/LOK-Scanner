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

    }
}
