using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;
using Object = lok_wss.Models.Object;

namespace lok_wss
{
    public static class C24
    {
        private static Timer _c24Timer;
        public static string LeaveZones = "";

        public static void C24Scan()
        {
            const int thisContinent = 24;
            try
            {
                var exitEvent = new ManualResetEvent(false);
                var url = new Uri("wss://socf-lok-live.leagueofkingdoms.com/socket.io/?EIO=4&transport=websocket");
                using var client = new WebsocketClient(url)
                {
                    ReconnectTimeout = TimeSpan.FromSeconds(30),
                    IsReconnectionEnabled = true,
                    ErrorReconnectTimeout = TimeSpan.FromSeconds(5)

                };
                
                client.ReconnectionHappened.Subscribe(_ =>
                {
                    CustomConsole.WriteLine($"[Connection] {_.Type}", ConsoleColor.Cyan);
                });


                _ = client.MessageReceived.Subscribe(msg =>
                {
                    string message = msg.Text;
                    string json = "";
                    JObject parse = new();

                    if (message.Contains("{"))
                    {
                        json = Helpers.ExtractJson(message[message.IndexOf("{", StringComparison.Ordinal)..]);
                        parse = JObject.Parse(json);
                    }
                    if (!string.IsNullOrEmpty(parse["sid"]?.ToString()))
                    {

                        CustomConsole.WriteLine("[Message] " + msg, ConsoleColor.DarkGray);
                        Task.Run(() => client.Send("40"));
                    }
                    else
                    {
                        var mapObjects = JsonConvert.DeserializeObject<Models.Root>(json);

                        if (mapObjects != null && mapObjects.objects != null && mapObjects.objects.Count != 0)
                        {
                            CustomConsole.WriteLine($"[Objects] c{thisContinent}: " + mapObjects.objects?.Count + " Objects received", ConsoleColor.Green);
                            List<Models.Object> crystalMines = mapObjects.objects.Where(x => x.code.ToString() == "20100105").ToList();
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
                    _ => SendRequest(client, thisContinent, _c24Timer, exitEvent),
                    null,
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(5));


                exitEvent.WaitOne();
            }
            catch (Exception ex)
            {

                Discord.logError("C24", ex);
            }
        }


        private static void SendRequest(WebsocketClient client, int continent, Timer timer, ManualResetEvent exitEvent)
        {

            int count = 9;
            string zones = "";
            Random rand = new();
            if (!string.IsNullOrEmpty(LeaveZones))
            {
                Task.Run(() =>
                    client.Send("42[\"/zone/leave/list/v2\", {\"world\":" + continent + ", \"zones\":\"[" + zones +
                                "]\"}]"));

            }
            
            for (int i = 0; i < count; i++)
            {
                int number = rand.Next(2000, 4090);
                zones += $"{number},";
            }

            zones = zones.Substring(0, zones.Length - 1);
            LeaveZones = zones;

            Task.Run(() =>
                client.Send("42[\"/zone/enter/list/v2\", {\"world\":" + continent + ", \"zones\":\"[" + zones +
                            "]\"}]"));
            CustomConsole.WriteLine($"[Requested] {continent}: {zones}", ConsoleColor.Yellow);

        }
    }
}
