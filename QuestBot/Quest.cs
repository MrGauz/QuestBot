using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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

            JsonSerializerOptions options = new();
            options.PropertyNameCaseInsensitive = true;
            Messages = JsonSerializer.Deserialize<List<TgMessage>>(jsonString, options);
        }
    }

    enum TgMessageType
    {
        Text,
        Image,
        Sticker,
        Voice,
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
        public GeoCoordinate.NetStandard2.GeoCoordinate GetGeoCoordinate
        {
            get { return new GeoCoordinate.NetStandard2.GeoCoordinate(Latitude, Longitude); }
        }
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
        public string Voice { get; set; } = null;
        public Location Geotag { get; set; } = null;
        public string Video { get; set; } = null;
        public Quiz Quiz { get; set; } = null;
        public string[] Keyboard { get; set; } = null;
        public Button[] Buttons { get; set; } = null;

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
                    buttons.Add(new InlineKeyboardButton[]
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

                var buttons = Buttons.Select(button => new InlineKeyboardButton[]
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

                if (Voice != null)
                {
                    return TgMessageType.Voice;
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
            get
            {
                return NextName == null ? null : Quest.Messages.Single(m => m.Name == NextName);
            }
        }
    }
}