using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using lok_wss.database.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Websocket.Client;
using Object = lok_wss.Models.Object;

namespace lok_wss
{
    internal class Program
    {
        static int iterations = 0;
        private static lokContext _context;
        private static IServiceProvider _services;
        private const double D33Px = 941;
        private const double D33Py = 1289;
        private static void Main()
        {
            _services = ConfigureServices();
            _context = _services.GetRequiredService<lokContext>();

            for (int i = 0; i < 1; i++)
            {
                Thread c14Thread = new Thread(new ThreadStart(C14));
                // Start secondary thread  
                c14Thread.Start();

                Thread c24Thread = new Thread(new ThreadStart(c24));
                // Start secondary thread  
                c24Thread.Start();

            }



            var thread = new Thread(
                        () => { while (true) Thread.Sleep(50000); }
                    );
            thread.Start();
        }

        private static void C14()
        {
            var thisContinent = 14;
            try
            {
                var exitEvent = new ManualResetEvent(false);
                var url = new Uri("wss://socf-lok-live.leagueofkingdoms.com:443/socket.io/?EIO=4&transport=websocket");
                using (var client = new WebsocketClient(url))
                {
                    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ReconnectionHappened.Subscribe(info =>
                    {
                        //Console.WriteLine("Reconnection happened, type: " + info.Type);
                    });
                    _ = client.MessageReceived.Subscribe(async msg =>
                    {
                        string message = msg.Text;
                        string json = "";
                        JObject parse = new();

                        if (message.Contains("{"))
                        {
                            json = ExtractJson(message[message.IndexOf("{", StringComparison.Ordinal)..]);
                            parse = JObject.Parse(json);
                        }
                        if (!string.IsNullOrEmpty(parse["sid"]?.ToString()))
                        {
                            Console.WriteLine("Message received: " + msg);
                        }
                        if (msg.Text == "40") { }
                        else
                        {
                            var mapObjects = JsonConvert.DeserializeObject<Models.Root>(json);

                            if (mapObjects != null && mapObjects.objects != null && mapObjects.objects.Count != 0)
                            {
                                Console.WriteLine($"c{thisContinent}: " + mapObjects?.objects?.Count + " Objects received");
                                List<Object> crystalMines = mapObjects.objects.Where(x => x.code.ToString() == "20100105").ToList();
                                if (crystalMines.Count >= 1)
                                    ParseCmines(crystalMines, thisContinent);
                            }
                        }

                    });
                    client.Start();

                    _ = new Timer(
                        e => MyMethod(client, thisContinent),
                        null,
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(5));

                    exitEvent.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }

        }

        private static void c24()
        {
            var thisContinent = 24;
            try
            {
                var exitEvent = new ManualResetEvent(false);
                var url = new Uri("wss://socf-lok-live.leagueofkingdoms.com:443/socket.io/?EIO=4&transport=websocket");
                using (var client = new WebsocketClient(url))
                {
                    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ReconnectionHappened.Subscribe(info =>
                    {
                        //Console.WriteLine("Reconnection happened, type: " + info.Type);
                    });
                    _ = client.MessageReceived.Subscribe(async msg =>
                    {
                        string message = msg.Text;
                        string json = "";
                        JObject parse = new();

                        if (message.Contains("{"))
                        {
                            if (message.Contains("42[\"/field/objects\","))
                            {
                                message = message.Replace("42[\"/field/objects\",", "");
                                json = message.Remove(message.Length - 1, 1);
                            }
                            else
                            {
                                json = ExtractJson(message[message.IndexOf("{", StringComparison.Ordinal)..]);
                            }

                            parse = JObject.Parse(json);
                        }
                        if (!string.IsNullOrEmpty(parse["sid"]?.ToString()))
                        {
                            Console.WriteLine("Message received: " + msg);
                        }
                        if (msg.Text == "40") { }
                        else
                        {
                            var mapObjects = JsonConvert.DeserializeObject<Models.Root>(json);

                            if (mapObjects != null && mapObjects.objects != null && mapObjects.objects.Count != 0 && mapObjects.objects.First().code != 0)
                            {
                                Console.WriteLine($"c{thisContinent}: " + mapObjects?.objects?.Count + " Objects received");
                                List<Object> crystalMines = mapObjects.objects.Where(x => x.code.ToString() == "20100105").ToList();
                                if (crystalMines.Count >= 1)
                                    ParseCmines(crystalMines, thisContinent);
                            }
                        }

                    });
                    client.Start();

                    _ = new Timer(
                        e => MyMethod(client, thisContinent),
                        null,
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(1));

                    exitEvent.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }

        private static async void ParseCmines(IEnumerable<Object> mines, int thisContinent)
        {

            foreach (var mine in mines)
            {
                if (mine.occupied == null)
                {
                    try
                    {
                        if (!_context.crystalMine.Any(x => x.id == mine._id))
                        {
                            string location = mine.loc[1] + ":" + mine.loc[2];
                            _context.crystalMine.Add(new crystalMine()
                            {
                                id = mine._id,
                                continent = $"{thisContinent}",
                                found = DateTime.UtcNow,
                                location = location
                            });
                            await _context.SaveChangesAsync();
                            postToDiscordCmine(thisContinent, mine.code.ToString(), location,
                                mine.level.ToString(), mine.param.value.ToString());
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static void MyMethod(WebsocketClient client, int continent)
        {

            int count = 40;
            string zones = "";
            Random rand = new Random();


            for (int i = 0; i < count; i++)
            {
                int number = rand.Next(2048, 4090);
                zones += $"{number},";
            }

            zones = zones.Substring(0, zones.Length - 1);


            Task.Run(() =>
                client.Send("42[\"/zone/enter/list\", {\"world\":" + continent + ", \"zones\":\"[" + zones +
                            "]\"}]"));


        }


        public static async void postToDiscordCmine(int continent, string Name, string coords, string level, string value)
        {
            var continentUrl = "";
            switch (continent)
            {
                case 24:
                    continentUrl = "https://discord.com/api/webhooks/948937096461156432/-EPALnQkk0Ow9KKA7pcqgh0yZOYvDP249ePeOBiNV4ixG6ks_SV65uK-QIHsu8Vgd5Lw";
                    break;
                case 14:
                    continentUrl = "https://discord.com/api/webhooks/948951511591960657/YsaTGH9ffYRMHgut6QX1tfjJJjB2Z9d9M_ACVXDiLaouF47yHMsvXt9jWT89Eg4VshmA";
                    break;
            }
            try
            {
                using (DiscordWebhookClient client = new(
                continentUrl)) //holdnut uoa
                {

                    var distance = string.Format("{0:0}", GetDistance(D33Px, D33Py, double.Parse(coords.Split(":")[0]),
                        double.Parse(coords.Split(":")[1])));

                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = $"L{level} CMine Found",
                        Description = coords,
                        ThumbnailUrl = "https://i.imgur.com/d9ICitd.png"
                    };


                    EmbedFieldBuilder detailFieldLevel = new() { IsInline = true, Name = "Level", Value = level };
                    EmbedFieldBuilder detailFieldCoords = new() { IsInline = true, Name = "Value", Value = value };
                    if (continent == 14)
                    {
                        EmbedFieldBuilder detailFieldDistance = new() { IsInline = true, Name = "Distance from AC", Value = distance + "km" };
                        embed.AddField(detailFieldLevel).AddField(detailFieldCoords).AddField(detailFieldDistance);
                    }
                    else
                    {
                        embed.AddField(detailFieldLevel).AddField(detailFieldCoords);
                    }
                    await client.SendMessageAsync("", embeds: new[] { embed.Build() });

                    // Webhooks are able to send multiple embeds per message
                    // As such, your embeds must be passed as a collection.



                }
            }
            catch (Exception)
            {

            }
        }

        public static async void postToDiscordGoblins(int continent, string Name, string coords, string level, string value)
        {
            try
            {
                var continentUrl = "";
                switch (continent)
                {
                    case 24:
                        continentUrl = "https://discord.com/api/webhooks/948940283305918544/yiKPAUA1DH_2miukIJ77Nt5866aALYt2RbOv68WFOvY_Zh3AJDNf5tHr-4EWSwPAX3Sf";
                        break;
                    case 14:
                        continentUrl = "https://discord.com/api/webhooks/948952111129980989/4YqpL_K8ItDURGkm0JVgfhoQPunLvhb6mOfUdZgB_PjWlZjlmdsDrgumLhtZNSYR4Sl9";
                        break;
                }

                using (DiscordWebhookClient client = new(
                    continentUrl)) //holdnut uoa
                {
                    EmbedFieldBuilder detailFieldLevel = new() { IsInline = true, Name = "Level", Value = level };
                    EmbedFieldBuilder detailFieldCoords = new() { IsInline = true, Name = "Health", Value = value };

                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = "Treasure Goblin detected",
                        Description = coords,
                        ThumbnailUrl = "https://i.imgur.com/d9ICitd.png"
                    }
                        .AddField(detailFieldLevel)
                        .AddField(detailFieldCoords);

                    // Webhooks are able to send multiple embeds per message
                    // As such, your embeds must be passed as a collection.
                    await client.SendMessageAsync("", embeds: new[] { embed.Build() });
                }
                //
            }
            catch (Exception)
            {

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

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<Program>()
                .AddDbContext<lokContext>(ServiceLifetime.Transient)
                .BuildServiceProvider();
        }

        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
        }

    }
}



//var deathkar = mapObjects?.objects.Where(x => x.code.ToString() == "20200201");
//var greenDrag = mapObjects?.objects.Where(x => x.code.ToString() == "20200202");
//var redDrag = mapObjects?.objects.Where(x => x.code.ToString() == "20200203");
//var goldDrag = mapObjects?.objects.Where(x => x.code.ToString() == "20200204");


//var player = mapObjects?.objects.Where(x => x.code.ToString() == "20300101");

//var allianceCenter = mapObjects?.objects.Where(x => x.code.ToString() == "20600101");
//var allianceTower = mapObjects?.objects.Where(x => x.code.ToString() == "20600102");
//var allianceOutpost = mapObjects?.objects.Where(x => x.code.ToString() == "20600103");

//var shrine1 = mapObjects?.objects.Where(x => x.code.ToString() == "20400101");
//var shrine2 = mapObjects?.objects.Where(x => x.code.ToString() == "20400201");
//var shrine3 = mapObjects?.objects.Where(x => x.code.ToString() == "20400301");
//var shrine4 = mapObjects?.objects.Where(x => x.code.ToString() == "20400401");


//var farm = mapObjects?.objects.Where(x => x.code.ToString() == "20100101");
//var lumber = mapObjects?.objects.Where(x => x.code.ToString() == "20100102");
//var quarry = mapObjects?.objects.Where(x => x.code.ToString() == "20100103");
//var goldMine = mapObjects?.objects.Where(x => x.code.ToString() == "20100104");

//var orc = mapObjects?.objects.Where(x => x.code.ToString() == "20200101");
//var skeleton = mapObjects?.objects.Where(x => x.code.ToString() == "20200102");
//var golem = mapObjects?.objects.Where(x => x.code.ToString() == "20200103");

//var charmStone = mapObjects?.objects.Where(x => x.code.ToString() == "20500101");
//var charmAttack = mapObjects?.objects.Where(x => x.code.ToString() == "20500102");
//var charmLumber = mapObjects?.objects.Where(x => x.code.ToString() == "20500103");
//var charmLoad = mapObjects?.objects.Where(x => x.code.ToString() == "20500104");


//var alianceBuilding =
//    mapObjects?.objects.Where(x => x.code.ToString() == "20600101");
//var whatsLeft = mapObjects?.objects
//    .Except(farm)
//    .Except(lumber)
//    .Except(quarry)
//    .Except(goldMine)
//    .Except(skeleton)
//    .Except(orc)
//    .Except(golem)
//    .Except(goblin)
//    .Except(player)
//    .Except(alianceBuilding)
//    .Except(deathkar)
//    .Except(greenDrag)
//    .Except(redDrag)
//    .Except(goldDrag)
//    .Except(allianceCenter)
//    .Except(allianceOutpost)
//    .Except(allianceTower)
//    .Except(shrine1)
//    .Except(shrine2)
//    .Except(shrine3)
//    .Except(shrine4)
//    .Except(charmStone)
//    .Except(charmAttack)
//    .Except(charmLumber)
//    .Except(charmLoad)
//    .Except(crystalMine)
//    .Where(x => x.code != 0).ToList();

//if (whatsLeft.Count > 0)
//{
//    Console.WriteLine(whatsLeft?.Count + " Objects unknown");
//    unknowns.AddRange(whatsLeft);
//}


//foreach (var unknownObject in whatsLeft)
//{
//    postToDiscord(24, unknownObject.code.ToString(), unknownObject.loc[1] + ":" + unknownObject.loc[2]);
//}



//foreach (var goblin in goblins)
//{
//    if (!_context.treasureGoblin.Any(x => x.id == goblin._id))
//    {
//        string location = goblin.loc[1] + ":" + goblin.loc[2];
//        _context.treasureGoblin.Add(new treasureGoblin()
//        {
//            id = goblin._id,
//            continent = $"{thisContinent}",
//            found = DateTime.UtcNow,
//            location = location
//        });

//        await _context.SaveChangesAsync();
//        postToDiscordGoblins(thisContinent, goblin.code.ToString(), location,
//            goblin.level.ToString(), goblin.param.value.ToString());
//    }
//}