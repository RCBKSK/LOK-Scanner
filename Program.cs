using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace lok_wss
{
    internal class Program
    {
        //private static lokContext _context;
        private static IServiceProvider _services;


        private static void Main()
        {
            _services = ConfigureServices();


            Thread c14Thread = new(C14.C14Scan);
            c14Thread.Start();

            Thread c24Thread = new(C24.C24Scan);
            c24Thread.Start();


            //Thread cvcThread = new(CVC.CVCScan);
            //cvcThread.Start();
            // Start secondary thread  

            var thread = new Thread(
                // ReSharper disable once FunctionNeverReturns
                () =>
                {
                    while (true)
                    {

                        Thread.Sleep(300000);
                    }
                }
                    );
            thread.Start();
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
//}#





//private static void C24()
//{
//    const int thisContinent = 24;
//    try
//    {
//        var exitEvent = new ManualResetEvent(false);
//        var url = new Uri("wss://socf-lok-live.leagueofkingdoms.com:443/socket.io/?EIO=4&transport=websocket");
//        using var client = new WebsocketClient(url) { ReconnectTimeout = TimeSpan.FromSeconds(30) };
//        client.ReconnectionHappened.Subscribe(_ =>
//        {
//            //Console.WriteLine("Reconnection happened, type: " + info.Type);
//        });
//        _ = client.MessageReceived.Subscribe(msg =>
//        {
//            string message = msg.Text;
//            string json = "";
//            JObject parse = new();

//            if (message.Contains("{"))
//            {
//                if (message.Contains("42[\"/field/objects\","))
//                {
//                    message = message.Replace("42[\"/field/objects\",", "");
//                    json = message.Remove(message.Length - 1, 1);
//                }
//                else
//                {
//                    json = Helpers.ExtractJson(message[message.IndexOf("{", StringComparison.Ordinal)..]);
//                }

//                parse = JObject.Parse(json);
//            }
//            if (!string.IsNullOrEmpty(parse["sid"]?.ToString()))
//            {
//                Console.WriteLine("Message received: " + msg);
//            }
//            if (msg.Text == "40") { }
//            else
//            {
//                var mapObjects = JsonConvert.DeserializeObject<Models.Root>(json);

//                if (mapObjects != null && mapObjects.objects != null && mapObjects.objects.Count != 0 && mapObjects.objects.First().code != 0)
//                {
//                    Console.WriteLine($"c{thisContinent}: " + mapObjects.objects?.Count + " Objects received");
//                    List<Object> crystalMines = mapObjects.objects.Where(x => x.code.ToString() == "20100105").ToList();
//                    if (crystalMines.Count >= 1)
//                        Helpers.ParseObjects("cmines", crystalMines, thisContinent);
//                    List<Object> treasureGoblins = mapObjects.objects.Where(x => x.code.ToString() == "20200104").ToList();
//                    if (treasureGoblins.Count >= 1)
//                        Helpers.ParseObjects("goblins", treasureGoblins, thisContinent);
//                }
//            }

//        });
//        client.Start();

//        _c24Timer = new Timer(
//            _ => MyMethod(client, thisContinent, _c24Timer, exitEvent),
//            null,
//            TimeSpan.FromSeconds(20),
//            TimeSpan.FromSeconds(60));

//        exitEvent.WaitOne();
//    }
//    catch (Exception ex)
//    {
//        Discord.logError("c24", ex);
//    }
//}


//private static void MyMethod(WebsocketClient client, int continent, Timer timer, ManualResetEvent exitEvent)
//{

//    int count = 40;
//    int startCount = 2000;
//    int endCount = 2040;
//    int iterations = 1;
//    string zones = "";


//    if (killThread)
//    {
//        timer.Change(Timeout.Infinite, Timeout.Infinite);
//        client.Dispose();
//        exitEvent.Set();
//        Console.WriteLine("closed socket");
//    }
//    if (client.IsRunning)
//    {

//        for (int i = 0; i < 50; i++)
//        {
//            for (int y = startCount; y < endCount; y++)
//            {

//                zones += $"{y},";
//            }
//            zones = zones.Substring(0, zones.Length - 1);
//            Task.Run(() =>
//                client.Send("42[\"/zone/enter/list\", {\"world\":" + continent + ", \"zones\":\"[" + zones +
//                            "]\"}]"));
//            Console.WriteLine($"{continent}: Requested {startCount} to {endCount}");
//            startCount = endCount;
//            endCount += count;
//            Thread.Sleep(1000);
//        }

//    }


//}

