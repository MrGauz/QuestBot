using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuestBot
{
    class Quest
    {
        public static readonly List<TgMessage> Messages;

        static Quest()
        {
            // Load messages from json
            var jsonString = File.ReadAllText("messages.json");
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };
            Messages = JsonSerializer.Deserialize<List<TgMessage>>(jsonString, options);
        }
    }

    enum TgMessageType
    {
        Text,
        Image,
        Sticker,
        AnimatedSticker,
        Voice,
        Audio,
        Geotag,
        Video,
        Quiz,
        Buttons
    }

    class Button
    {
        public string Text { get; set; }
        public string CallbackData { get; set; }
    }

    class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [JsonIgnore]
        public GeoCoordinate.NetStandard2.GeoCoordinate GetGeoCoordinate => new(Latitude, Longitude);
    }

    class Time
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
    }

    class Quiz
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int CorrectOptionId { get; set; }
        public string ReplyName { get; set; }
        [JsonIgnore] public string PollId { get; set; } = null;
    }

    class NpcMessage
    {
        public string NpcCodename { get; set; }
        public string MessageName { get; set; }
    }

    class TgMessage
    {
        public string Name { get; set; }
        public Time SendAt { get; set; } = null;
        public Location SendAtLocation { get; set; } = null;
        public string SendOnText { get; set; } = null;
        public int? NextAfter { get; set; } = null;
        public string NextName { get; set; } = null;
        public string Text { get; set; } = null;
        public string Image { get; set; } = null;
        public string Sticker { get; set; } = null;
        public string AnimatedSticker { get; set; } = null;
        public string Voice { get; set; } = null;
        public string Audio { get; set; } = null;
        public Location Geotag { get; set; } = null;
        public string Video { get; set; } = null;
        public Quiz Quiz { get; set; } = null;
        public string[] Keyboard { get; set; } = null;
        public Button[] Buttons { get; set; } = null;
        public NpcMessage[] NotifyNpc { get; set; } = null;

        [JsonIgnore]
        public InlineKeyboardMarkup KeyboardMarkup
        {
            get
            {
                if (Keyboard == null)
                {
                    return null;
                }

                var buttons = new List<InlineKeyboardButton[]>();
                for (var i = 0; i < Keyboard.Length; i++)
                {
                    var callbackData = $"{Name}_{i}";
                    buttons.Add(new[]
                        { InlineKeyboardButton.WithCallbackData(Keyboard[i], callbackData) });
                }

                return new InlineKeyboardMarkup(buttons);
            }
        }

        [JsonIgnore]
        public InlineKeyboardMarkup InlineKeyboard
        {
            get
            {
                if (Buttons == null)
                {
                    return null;
                }

                var buttons = Buttons.Select(button => new[]
                    { InlineKeyboardButton.WithCallbackData(button.Text, button.CallbackData) }).ToList();

                return new InlineKeyboardMarkup(buttons);
            }
        }

        [JsonIgnore]
        public TgMessageType Type
        {
            get
            {
                if (Geotag != null)
                {
                    return TgMessageType.Geotag;
                }

                if (Image != null)
                {
                    return TgMessageType.Image;
                }

                if (Sticker != null)
                {
                    return TgMessageType.Sticker;
                }

                if (AnimatedSticker != null)
                {
                    return TgMessageType.AnimatedSticker;
                }

                if (Voice != null)
                {
                    return TgMessageType.Voice;
                }

                if (Audio != null)
                {
                    return TgMessageType.Audio;
                }

                if (Video != null)
                {
                    return TgMessageType.Video;
                }

                if (Quiz != null)
                {
                    return TgMessageType.Quiz;
                }

                if (Buttons != null)
                {
                    return TgMessageType.Buttons;
                }

                return TgMessageType.Text;
            }
        }

        [JsonIgnore]
        public TgMessage NextMessage
        {
            get { return NextName == null ? null : Quest.Messages.Single(m => m.Name == NextName); }
        }

        [JsonIgnore]
        public Dictionary<long, TgMessage> NpcMessages
        {
            get
            {
                var messages = new Dictionary<long, TgMessage>();
                foreach (var npcMessage in NotifyNpc)
                {
                    if (npcMessage.NpcCodename == null || npcMessage.MessageName == null)
                    {
                        Log.Warning("Incomplete NPC message: {MessageName}", Name);
                        continue;
                    }

                    if (!Config.Npc.Any(n => n.Codename == npcMessage.NpcCodename))
                    {
                        Log.Warning("NPC '{Codename}' not found in message {MessageName}", npcMessage.NpcCodename,
                            Name);
                        continue;
                    }

                    var npcId = Config.Npc.Single(n => n.Codename == npcMessage.NpcCodename).ChatId;
                    messages.Add(npcId, Quest.Messages.Single(m => m.Name == npcMessage.MessageName));
                }

                return messages;
            }
        }
    }
}