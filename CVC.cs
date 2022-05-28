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
    public class CVC
    {
        private static Timer _cvcTimer;
        public static string leaveZones = "";
        public static void CVCScan()
        {
            const int thisContinent = 100002;
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

                            //Helpers.ParseObjects("allUnknown", mapObjects.objects.ToList(), thisContinent);

                            Console.WriteLine($"c{thisContinent}: " + mapObjects.objects?.Count + " Objects received");
                            List<Object> crystalMines = mapObjects.objects.Where(x => x.code.ToString() == "20100105").ToList();
                            if (crystalMines.Count >= 1)
                                Helpers.ParseObjects("cmines", crystalMines, thisContinent);          //20200204
                            List<Object> magdar = mapObjects.objects.Where(x => x.code.ToString() == "20700505").ToList();
                            if (magdar.Count >= 1)
                                Helpers.ParseObjects("magdar", magdar, thisContinent);
                        }
                    }

                });
                client.Start();

                _cvcTimer = new Timer(
                    _ => SendRequest(client, thisContinent, _cvcTimer, exitEvent),
                    null,
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(5));

                exitEvent.WaitOne();
            }
            catch (Exception ex)
            {
                DiscordWebhooks.logError("CVC", ex);
            }
        }

        private static void SendRequest(WebsocketClient client, int continent, Timer timer, ManualResetEvent exitEvent)
        {

            int count = 9;
            string zones = "";
            Random rand = new();
            if (!string.IsNullOrEmpty(leaveZones))
            {
                Task.Run(() =>
                    client.Send("42[\"/zone/leave/list/v2\", {\"world\":" + continent + ", \"zones\":\"[" + zones +
                                "]\"}]"));
            }

            for (int i = 0; i < count; i++)
            {
                int number = rand.Next(3200, 4090);
                zones += $"{number},";
            }

            zones = zones.Substring(0, zones.Length - 1);
            leaveZones = zones;

            Task.Run(() =>
                client.Send("42[\"/zone/enter/list/v2\", {\"world\":" + continent + ", \"zones\":\"[" + zones +
                            "]\"}]"));
            Console.WriteLine($"{continent}: Requested {zones}");

        }
    }
}
