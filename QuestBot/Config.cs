using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace QuestBot;

public class SmtpConfig
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string From { get; set; }
}

public class Configs
{
    public string TelegramBotToken { get; set; }
    public long BdayChatId { get; set; }
    public long[] AdminChatIds { get; set; }
    public long BroadcastChatId { get; set; }
    public SmtpConfig Smtp { get; set; }
    public string OpenAiApiKey { get; set; } = null;
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
    public static IEnumerable<long> AdminChatIds => Configs.Instance.AdminChatIds;
    public static long BroadcastChatId => Configs.Instance.BroadcastChatId;
    public static SmtpConfig Smtp => Configs.Instance.Smtp;
    public static string OpenAiApiKey => Configs.Instance.OpenAiApiKey;
    public static string Locale => Configs.Locale;
}