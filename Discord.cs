using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;

namespace lok_wss
{
    public static class DiscordWebhooks
    {
        private const double D33Px = 953;
        private const double D33Py = 1296;

        public static async void PostToDiscordCmine(int continent, string name, string coords, string level, string value, bool underKingdom)
        {
            var continentUrl = "";
            switch (continent)
            {
                case 24:
                    continentUrl = "https://discord.com/api/webhooks/948937096461156432/-EPALnQkk0Ow9KKA7pcqgh0yZOYvDP249ePeOBiNV4ixG6ks_SV65uK-QIHsu8Vgd5Lw";
                    break;
                case 15:
                    continentUrl = "https://discord.com/api/webhooks/948951511591960657/YsaTGH9ffYRMHgut6QX1tfjJJjB2Z9d9M_ACVXDiLaouF47yHMsvXt9jWT89Eg4VshmA";
                    break;
                case 100002:
                    continentUrl = "https://discord.com/api/webhooks/952324290328543322/Bc-jclcKlot84Sf2_lHPQokkmSy0yb6lNyvyDoEU6KqkAAm_Kne85iBmP1jCHmJNjcGK";
                    break;
            }
            try
            {
                using (DiscordWebhookClient client = new(
                continentUrl)) //holdnut uoa
                {
                    EmbedBuilder embed = new()
                    {
                        Title = $"L{level} CMine",
                        Description = coords,
                        ThumbnailUrl = "https://i.imgur.com/d9ICitd.png",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Found at {DateTime.UtcNow:HH:mm} UTC"
                        }
                    };


                    EmbedFieldBuilder detailFieldLevel = new() { IsInline = true, Name = "Level", Value = level };
                    EmbedFieldBuilder detailFieldCoords = new() { IsInline = true, Name = "Value", Value = value };
                    if (underKingdom) return;
                    switch (continent)
                    {
                        case 15:
                            {
                                var distance =
                                    $"{Helpers.GetDistance(D33Px, D33Py, double.Parse(coords.Split(":")[0]), double.Parse(coords.Split(":")[1])):0}";
                                EmbedFieldBuilder detailFieldDistance = new() { IsInline = true, Name = "Distance from AC", Value = distance + "km" };
                                embed.AddField(detailFieldLevel).AddField(detailFieldCoords).AddField(detailFieldDistance);
                                break;
                            }
                        case 24:
                            {
                                var distance = string.Format("{0:0}", Helpers.GetDistance(406, 1408, double.Parse(coords.Split(":")[0]),
                                    double.Parse(coords.Split(":")[1])));
                                EmbedFieldBuilder detailFieldDistance = new() { IsInline = true, Name = "Distance from A", Value = distance + "km" };
                                embed.AddField(detailFieldLevel).AddField(detailFieldCoords).AddField(detailFieldDistance);
                                break;
                            }
                        
                    }

                    if (continent == 100002 && (int.Parse(coords.Split(":")[0]) > 400 || int.Parse(coords.Split(":")[1]) < 1600))
                    {
                        return;
                    }
                    await client.SendMessageAsync("", embeds: new[] { embed.Build() });

                    // Webhooks are able to send multiple embeds per message
                    // As such, your embeds must be passed as a collection.



                }
            }
            catch (Exception e)
            {
                logError("postDiscordCMiness", e);
            }
        }

        public static async void PostToDiscordMagdar(int continent, string name, string coords, string level, string value)
        {
            var continentUrl = "";
            switch (continent)
            {
                case 100002:
                    continentUrl = "https://discord.com/api/webhooks/956520018248630352/vgU1tztTL1yK1hlI_hhHvaVdy7k6HAGFQCTL0GHoT_dQPfMrtG1teCJ5DgOeGPQ_93Jt";
                    break;
            }
            try
            {
                using (DiscordWebhookClient client = new(
                continentUrl)) //holdnut uoa
                {
                    EmbedBuilder embed = new()
                    {
                        Title = $"L{level} Magdar",
                        Description = coords,
                        ThumbnailUrl = "https://i.imgur.com/d9ICitd.png",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Found at {DateTime.UtcNow:HH:mm} UTC"
                        }
                    };


                    EmbedFieldBuilder detailFieldLevel = new() { IsInline = true, Name = "Level", Value = level };
                    EmbedFieldBuilder detailFieldHealth = new() { IsInline = true, Name = "Health", Value = value };

                   
   

                    if (continent == 100002 && (int.Parse(coords.Split(":")[0]) > 1200 || int.Parse(coords.Split(":")[1]) < 1600))
                    {
                        return;
                    }
                    await client.SendMessageAsync("", embeds: new[] { embed.Build() });


                }
            }
            catch (Exception e)
            {
                logError("postDiscordMagdar", e);
            }
        }

        public static async void PostToDiscordGoblins(int continent, string name, string coords, string level, string value)
        {
            try
            {
                var continentUrl = "";
                switch (continent)
                {
                    case 24:
                        continentUrl = "https://discord.com/api/webhooks/948940283305918544/yiKPAUA1DH_2miukIJ77Nt5866aALYt2RbOv68WFOvY_Zh3AJDNf5tHr-4EWSwPAX3Sf";
                        break;
                    case 15:
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
                        Title = $"L{level} Treasure Goblin",
                        Description = coords,
                        ThumbnailUrl = "https://miro.medium.com/max/1000/1*5eGAz1A3AWuHBX9M-u7gng.png",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Found at {DateTime.UtcNow:HH:mm} UTC"
                        }
                    }
                        .AddField(detailFieldLevel)
                        .AddField(detailFieldCoords);

                    // Webhooks are able to send multiple embeds per message
                    // As such, your embeds must be passed as a collection.
                    await client.SendMessageAsync("", embeds: new[] { embed.Build() });
                }
                //
            }
            catch (Exception e)
            {
                logError("postDiscordGoblins", e);
            }
        }

        public static async void PostToDiscordDeathkar(int continent, string name, string coords, string level, string value, bool underKingdom)
        {
            try
            {
                var continentUrl = "";
                switch (continent)
                {
                    case 24:
                        return;
                    case 15:
                        continentUrl = "https://discord.com/api/webhooks/978927835777478666/4zMOUipUe-oN0HNLS-JrsxB7P3-RRkkILmpszEQQZv7a4_Ss7ufMb9R468c5paDL9h71";
                        break;
                }

                using (DiscordWebhookClient client = new(
                    continentUrl)) 
                {
                    EmbedFieldBuilder detailFieldLevel = new() { IsInline = true, Name = "Level", Value = level };
                    EmbedFieldBuilder detailFieldCoords = new() { IsInline = true, Name = "Health", Value = value };
                    var distance = $"{Helpers.GetDistance(D33Px, D33Py, double.Parse(coords.Split(":")[0]), double.Parse(coords.Split(":")[1])):0}";
                    EmbedFieldBuilder detailFieldDistance = new() { IsInline = true, Name = "Distance from AC", Value = distance + "km" };
                   

                    if (int.Parse(distance) >= 600) return;
                    if (underKingdom) return; 

                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = $"L{level} Deathkar",
                        Description = coords,
                        ThumbnailUrl = "https://miro.medium.com/max/1000/1*xRAZGJ6OBustqna2SvsNpg.png",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Found at {DateTime.UtcNow:HH:mm} UTC"
                        }
                    }
                        .AddField(detailFieldLevel)
                        .AddField(detailFieldCoords)
                        .AddField(detailFieldDistance);

                    // Webhooks are able to send multiple embeds per message
                    // As such, your embeds must be passed as a collection.
                    await client.SendMessageAsync("", embeds: new[] { embed.Build() });
                }
                //
            }
            catch (Exception e)
            {
                logError("postDiscordGoblins", e);
            }
        }



        public static async void PostToDiscordUnknown(string continent, string name, string coords, string level, string value)
        {
            try
            {
                var continentUrl = "https://discord.com/api/webhooks/955230201296597032/aOX-vm8vbqMFlCKEUq77N1Hxhpb6WGgiyLO0ZD1IDUHzMcu9lWefnla-HenEucuE-Ixr";


                using (DiscordWebhookClient client = new(
                    continentUrl)) 
                {
                    EmbedFieldBuilder detailFieldLevel = new() { IsInline = true, Name = "Level", Value = level };
                    EmbedFieldBuilder detailFieldCoords = new() { IsInline = true, Name = "Health", Value = value };

                    EmbedBuilder embed = new EmbedBuilder
                    {
                        Title = $"L{level} {name}",
                        Description = coords,
                        ThumbnailUrl = "https://i.imgur.com/d9ICitd.png",
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"Found at {DateTime.UtcNow:HH:mm} UTC"
                        }
                    }
                        .AddField(detailFieldLevel)
                        .AddField(detailFieldCoords);

                    // Webhooks are able to send multiple embeds per message
                    // As such, your embeds must be passed as a collection.
                    await client.SendMessageAsync("", embeds: new[] { embed.Build() });
                }
                //
            }
            catch (Exception e)
            {
                logError("postDiscordGoblins", e);
            }
        }

        public static void logError(string postType, Exception e)
        {
            try
            {
                //using (DiscordWebhookClient client =
                //    new(
                //        "https://discord.com/api/webhooks/954348870098378782/7Mg9Baz0YniGBcDpVLIyf-szuvy6U4ogSh6sG_bOQrSPRGuopzwEv8h0pRKdtYC3rr-B")) //holdnut uoa
                //{
                //    List<Embed> postEmbeds = new List<Embed>();
                //    EmbedBuilder embed = new()
                //    {
                //        Title = $"{postType}",
                //        Description = e.Message,
                //        Author = new EmbedAuthorBuilder()
                //        {
                //            Name = $"{DateTime.UtcNow:g}"
                //        },
                //    };
                //    postEmbeds.Add(embed.Build());
                //    await client.SendMessageAsync("", embeds: postEmbeds);
                //    Thread.Sleep(5000);
                //}
            }
            catch (Exception)
            {
                Thread.Sleep(5000);
            }
        }
    }
}
