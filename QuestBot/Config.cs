﻿using System.IO;
using Newtonsoft.Json;

namespace QuestBot;

public class Configs
{
    public string TelegramBotToken { get; set; }
    public long BdayChatId { get; set; }
    public long[] AdminChatIds { get; set; }
    public long[] ObserversChatIds { get; set; }
    public static string Locale => "de-DE";

    public static Configs Instance { get; private set; }

    static Configs()
    {
        var jsonString = File.ReadAllText("config.json");
        Instance = JsonConvert.DeserializeObject<Configs>(jsonString);
    }
}

public static class Config
{
    public static string TelegramBotToken => Configs.Instance.TelegramBotToken;
    public static long BdayChatId => Configs.Instance.BdayChatId;
    public static long[] AdminChatIds => Configs.Instance.AdminChatIds;
    public static long[] ObserversChatIds => Configs.Instance.ObserversChatIds;
    public static string Locale => Configs.Locale;
}