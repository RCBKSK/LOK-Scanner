using AutoMapper;
using lok_wss.database.Models;
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
    public class ContinentScanner
    {
        private static Timer _c15Timer;
        private static string _leaveZones = "";
        public static bool _runOnce = false;
        public static Mapper mapper;
        private int runningCount;

        public ContinentScanner(int continent)
        {
            runningCount = 1;
            var config = new MapperConfiguration(cfg =>
                cfg.CreateMap<Object, Kingdom>()
            );

            mapper = new Mapper(config);

            int thisContinent = continent;
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

                    //CustomConsole.WriteLine(
                    //    $"[Field Enter] c{thisContinent}: " + (message.Length >= 90 ? message.Substring(0, 90) : message),
                    //    ConsoleColor.Green);

                    if (message == "40")
                    {
                        var fieldEnter = "{\"token\":\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJfaWQiOiI2MjIxZTZjNTVjZjkwZjEwMGRhYTEyYzMiLCJraW5nZG9tSWQiOiI2MjIxZTZjNjVjZjkwZjEwMGRhYTEyYzQiLCJ2ZXJzaW9uIjoxNDQ3LCJ0aW1lIjoxNjUzNzc5ODAyNjAxLCJpYXQiOjE2NTM3Nzk4MDIsImV4cCI6MTY1NDM4NDYwMiwiaXNzIjoibm9kZ2FtZXMuY29tIiwic3ViIjoidXNlckluZm8ifQ.Bytc5pkx-KGDFrxK8Y7k__bPjdI9GxOjgQrbD90tsbc\"}";
                        var base64FieldEnter = "42[\"/field/enter/v3\",\"" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(fieldEnter)) + "\"]";
                        Task.Run(() =>
                            client.Send(base64FieldEnter));
                        //CustomConsole.WriteLine($"[Requested] c{continent}: Field Enter", ConsoleColor.Yellow);
                    }

                    if (message.Contains("/field/objects/v3"))
                    {
                        var enc = message.Split(",")[1];
                        var dec = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(enc.Substring(1, enc.Length - 3)));

                        var mapObjects = JsonConvert.DeserializeObject<Models.Root>(dec);

                        if (mapObjects != null && mapObjects.objects != null && mapObjects.objects.Count != 0)
                        {
                            CustomConsole.WriteLine(
                                $"[Objects] c{thisContinent}: " + mapObjects.objects?.Count + " Objects received",
                                ConsoleColor.Green);

                            List<Models.Object> crystalMines = mapObjects.objects
                                .Where(x => x.code.ToString() == "20100105").ToList();
                            if (crystalMines.Count >= 1)
                            {
                                CustomConsole.WriteLine(
                                    $"[CMines] c{thisContinent}: " + crystalMines.Count + " received",
                                    ConsoleColor.White);

                                Helpers.ParseObjects("cmines", crystalMines, thisContinent);
                            }

                            List<Object> treasureGoblins = mapObjects.objects
                                .Where(x => x.code.ToString() == "20200104").ToList();
                            if (treasureGoblins.Count >= 1)
                            {
                                CustomConsole.WriteLine(
                                    $"[Goblins] c{thisContinent}: " + treasureGoblins.Count + " received",
                                    ConsoleColor.White);

                                Helpers.ParseObjects("goblins", treasureGoblins, thisContinent);
                            }

                            List<Object> dKs = mapObjects.objects
                                .Where(x => x.code.ToString() == "20200201").ToList();
                            if (dKs.Count >= 1)
                            {
                                CustomConsole.WriteLine(
                                    $"[Deathkar] c{thisContinent}: " + dKs.Count + " received",
                                    ConsoleColor.White);

                                Helpers.ParseObjects("deathkar", dKs, thisContinent);
                            }

                            var kingdoms = mapObjects.objects.Where(x => x.code == 20300101).ToList();

                            if (kingdoms.Count >= 1 && continent == 15)
                            {
                                Helpers.ParseKingdoms(kingdoms);
                            }
                        }
                    }

                    if (message.Contains("/field/enter/v3"))
                    {
                        var enc = message.Split(",")[1];
                        var toDecode = enc.Substring(1, enc.Length - 3);
                        var dec = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(toDecode));

                        _c15Timer = new Timer(
                            _ => SendRequest(client, thisContinent, _c15Timer, exitEvent),
                            null,
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(3));
                    }

                    if (message.Contains("{"))
                    {
                        json = Helpers.ExtractJson(message[message.IndexOf("{", StringComparison.Ordinal)..]);
                        parse = JObject.Parse(json);
                    }
                });
                client.Start();

                exitEvent.WaitOne();
            }
            catch (Exception ex)
            {
                DiscordWebhooks.logError("c15", ex);
            }
        }

        private void SendRequest(WebsocketClient client, int continent, Timer timer, ManualResetEvent exitEvent)
        {
            int count = 9;
            string zones = "";
            Random rand = new();
            var leaveZoneCommand = "42[\"/zone/leave/list/v2\", {\"world\":" + continent + ", \"zones\":\"[" + _leaveZones + "]\"}]";
            Task.Run(() => client.Send(leaveZoneCommand));
            //CustomConsole.WriteLine($"[LeaveZones] {continent}: {leaveZoneCommand}", ConsoleColor.Yellow);

            for (int i = 0; i < count; i++)
            {
                int number = runningCount++;
                zones += $"{number},";
                if (runningCount >= 4080) runningCount = 1;
            }

            zones = zones.Substring(0, zones.Length - 1);
            _leaveZones = zones;
            var stringToEncode = "{\"world\":" + continent + ", \"zones\":\"[" + zones + "]\"}";
            var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(stringToEncode));
            var command = $"42[\"/zone/enter/list/v3\", \"{base64}\"]";

            Task.Run(() => client.Send(command));

            CustomConsole.WriteLine($"[Requested] {continent}: {zones}", ConsoleColor.Yellow);
        }
    }
}