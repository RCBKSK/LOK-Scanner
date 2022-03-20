using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
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
        //private static lokContext _context;
        private static IServiceProvider _services;

        private static bool killThread = false;

        private static Timer _c24Timer;

        private static void Main()
        {
            _services = ConfigureServices();


            Thread c14Thread = new(C14.C14Scan);
            c14Thread.Start();

            //Thread c24Thread = new(C24); 

            Thread cvcThread = new(CVC.CVCScan);
            cvcThread.Start();
            // Start secondary thread  

            var thread = new Thread(
                // ReSharper disable once FunctionNeverReturns
                () =>
                {
                    while (true)
                    {
                        //if (!c14Thread.IsAlive)
                        //{
                        //    c14Thread = new Thread(C14.C14Scan);
                        //    c14Thread.Start();
                        //}

                        //if (!c24Thread.IsAlive)
                        //{
                        //    c24Thread = new Thread(C24);
                        //    c24Thread.Start();
                        //}

                        //if (!cvcThread.IsAlive)
                        //{
                        //    cvcThread = new Thread(CVC);
                        //    cvcThread.Start();
                        //}


                        Thread.Sleep(300000);
                        killThread = true;
                        Thread.Sleep(5000);
                        killThread = false;
                    }
                }
                    );
            thread.Start();
        }

       

        private static void C24()
        {
            const int thisContinent = 24;
            try
            {
                var exitEvent = new ManualResetEvent(false);
                var url = new Uri("wss://socf-lok-live.leagueofkingdoms.com:443/socket.io/?EIO=4&transport=websocket");
                using var client = new WebsocketClient(url) { ReconnectTimeout = TimeSpan.FromSeconds(30) };
                client.ReconnectionHappened.Subscribe(_ =>
                {
                    //Console.WriteLine("Reconnection happened, type: " + info.Type);
                });
                _ = client.MessageReceived.Subscribe(msg =>
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
                            json = Helpers.ExtractJson(message[message.IndexOf("{", StringComparison.Ordinal)..]);
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
                            Console.WriteLine($"c{thisContinent}: " + mapObjects.objects?.Count + " Objects received");
                            List<Object> crystalMines = mapObjects.objects.Where(x => x.code.ToString() == "20100105").ToList();
                            if (crystalMines.Count >= 1)
                                Helpers.ParseObjects("cmines", crystalMines, thisContinent);
                            List<Object> treasureGoblins = mapObjects.objects.Where(x => x.code.ToString() == "20200104").ToList();
                            if (treasureGoblins.Count >= 1)
                                Helpers.ParseObjects("goblins", treasureGoblins, thisContinent);
                        }
                    }

                });
                client.Start();

                _c24Timer = new Timer(
                    _ => MyMethod(client, thisContinent, _c24Timer, exitEvent),
                    null,
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(60));

                exitEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Discord.logError("c24", ex);
            }
        }

  
        private static void MyMethod(WebsocketClient client, int continent, Timer timer, ManualResetEvent exitEvent)
        {

            int count = 40;
            int startCount = 2000;
            int endCount = 2040;
            int iterations = 1;
            string zones = "";


            if (killThread)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                client.Dispose();
                exitEvent.Set();
                Console.WriteLine("closed socket");
            }
            if (client.IsRunning)
            {

                for (int i = 0; i < 50; i++)
                {
                    for (int y = startCount; y < endCount; y++)
                    {

                        zones += $"{y},";
                    }
                    zones = zones.Substring(0, zones.Length - 1);
                    Task.Run(() =>
                        client.Send("42[\"/zone/enter/list\", {\"world\":" + continent + ", \"zones\":\"[" + zones +
                                    "]\"}]"));
                    Console.WriteLine($"{continent}: Requested {startCount} to {endCount}");
                    startCount = endCount;
                    endCount += count;
                    Thread.Sleep(1000);
                }

            }


        }


      

     

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<Program>()
                .AddDbContext<lokContext>(ServiceLifetime.Scoped)
                .BuildServiceProvider();
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