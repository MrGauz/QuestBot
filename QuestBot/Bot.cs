using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuestBot
{
    class Bot
    {
        private static ITelegramBotClient _botClient;

        private static readonly ReplyKeyboardMarkup ReplyKeyboardForAdmins = new(
            new[]
            {
                new KeyboardButton[] { "квест ua_artist", "квест alice_cooper", "квест burp" },
                new KeyboardButton[] { "квест memes", "квест motocycles", "квест welldone" },
                new KeyboardButton[] { "муз квиз раунд 1", "муз квиз раунд 2", "муз квиз раунд 3" },
                new KeyboardButton[] { "Как пользоваться админкой?" },
            })
        {
            ResizeKeyboard = true
        };

        public static void Initialize()
        {
            _botClient = new TelegramBotClient(Config.TelegramBotToken);

            using var cancellationTokenSource = new CancellationTokenSource();

            GreetAdmins();

            _botClient.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cancellationTokenSource.Token);
        }

        #region Sending

        private static void GreetAdmins()
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Greeting admins...");
            foreach (var admin in Config.AdminChatIds)
            {
                SendAdminUsage(admin);
            }
        }

        private static void SendAdminUsage(long admin)
        {
            const string usage = @"
Использование:
  <code>/send ID сообщения как в JSON</code>
  <code>/forward текст для отправки Лере</code>
  <code>/keyboard</code> - показать клавиатуру с кнопками
                ";

            _botClient.SendTextMessageAsync(
                chatId: admin,
                text: usage,
                replyMarkup: ReplyKeyboardForAdmins);
        }

        public static async void SendMessage(TgMessage message, long to = 0, bool pin = false)
        {
            if (to == 0)
            {
                to = Config.BdayChatId;
            }

            Message sentMessage;

            switch (message.Type)
            {
                case TgMessageType.Geotag:
                    await _botClient.SendChatActionAsync(to, ChatAction.FindLocation);
                    sentMessage = await _botClient.SendLocationAsync(
                        chatId: to,
                        latitude: (float)message.Geotag.Latitude,
                        longitude: (float)message.Geotag.Longitude,
                        replyMarkup: message.KeyboardMarkup
                    );
                    break;
                case TgMessageType.Image:
                    await _botClient.SendChatActionAsync(to, ChatAction.UploadPhoto);
                    using (var stream = System.IO.File.OpenRead(message.Image))
                    {
                        sentMessage = await _botClient.SendPhotoAsync(
                            chatId: to,
                            photo: stream,
                            caption: message.Text,
                            replyMarkup: message.KeyboardMarkup
                        );
                    }

                    break;
                case TgMessageType.Sticker:
                    await _botClient.SendChatActionAsync(to, ChatAction.Typing);
                    using (var stream = System.IO.File.OpenRead(message.Sticker))
                    {
                        sentMessage = await _botClient.SendStickerAsync(
                            chatId: to,
                            sticker: stream,
                            replyMarkup: message.KeyboardMarkup
                        );
                    }

                    break;
                case TgMessageType.AnimatedSticker:
                    await _botClient.SendChatActionAsync(to, ChatAction.Typing);
                    var sticker = new InputOnlineFile(message.AnimatedSticker);
                    sentMessage = await _botClient.SendStickerAsync(
                        chatId: to,
                        sticker: sticker,
                        replyMarkup: message.KeyboardMarkup
                    );
                    break;
                case TgMessageType.Voice:
                    await _botClient.SendChatActionAsync(to, ChatAction.RecordVoice);
                    using (var stream = System.IO.File.OpenRead(message.Voice))
                    {
                        sentMessage = await _botClient.SendVoiceAsync(
                            chatId: to,
                            voice: stream,
                            replyMarkup: message.KeyboardMarkup
                        );
                    }

                    break;
                case TgMessageType.Audio:
                    await _botClient.SendChatActionAsync(to, ChatAction.RecordVoice);
                    sentMessage = await _botClient.SendAudioAsync(
                        chatId: to,
                        audio: message.Audio,
                        replyMarkup: message.KeyboardMarkup
                    );
                    break;
                case TgMessageType.Video:
                    await _botClient.SendChatActionAsync(to, ChatAction.UploadVideo);
                    using (var stream = System.IO.File.OpenRead(message.Video))
                    {
                        sentMessage = await _botClient.SendVideoAsync(
                            chatId: Config.BdayChatId,
                            video: stream,
                            caption: message.Text,
                            supportsStreaming: true,
                            replyMarkup: message.KeyboardMarkup
                        );
                    }

                    break;
                case TgMessageType.Quiz:
                    await _botClient.SendChatActionAsync(to, ChatAction.Typing);
                    sentMessage = await _botClient.SendPollAsync(
                        chatId: to,
                        question: message.Quiz.Question,
                        options: message.Quiz.Options,
                        type: PollType.Quiz,
                        correctOptionId: message.Quiz.CorrectOptionId,
                        replyMarkup: message.KeyboardMarkup
                    );
                    message.Quiz.PollId = sentMessage.Poll.Id;
                    break;
                case TgMessageType.Buttons:
                    await _botClient.SendChatActionAsync(to, ChatAction.Typing);
                    sentMessage = await _botClient.SendTextMessageAsync(
                        chatId: to,
                        text: message.Text,
                        parseMode: ParseMode.Html,
                        replyMarkup: message.InlineKeyboard);
                    break;
                case TgMessageType.Text:
                    await _botClient.SendChatActionAsync(to, ChatAction.Typing);
                    sentMessage = await _botClient.SendTextMessageAsync(
                        chatId: to,
                        text: message.Text,
                        parseMode: ParseMode.Html,
                        replyMarkup: message.KeyboardMarkup
                    );
                    break;
                default:
                    Console.WriteLine(
                        $"{DateTime.Now:HH:mm:ss} - ERROR - Trying to send unknown message type: {message.Type}");
                    return;
            }

            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Sent \"{message.Name}\"");

            // Pinning messages
            if (pin)
            {
                await _botClient.UnpinAllChatMessages(Config.BdayChatId);
                await _botClient.PinChatMessageAsync(Config.BdayChatId, sentMessage.MessageId, true);
            }

            // Wiretap a.k.a. "Babysitting"
            if (to == Config.BdayChatId)
            {
                foreach (var recipient in Config.AdminChatIds.Concat(Config.ObserversChatIds).ToArray())
                {
                    await _botClient.ForwardMessageAsync(
                        chatId: recipient,
                        fromChatId: sentMessage.Chat.Id,
                        messageId: sentMessage.MessageId
                    );
                }
            }

            // Sending message sequences
            if (message.NextAfter != null)
            {
                Thread.Sleep((int)message.NextAfter * 1000);
                SendMessage(message.NextMessage);
            }
        }

        public static void SendMessage(string message, long to = 0, bool pin = false)
        {
            TgMessage tgMessage = new()
            {
                Name = "manual_message",
                Text = message
            };
            SendMessage(tgMessage, to, pin);
        }

        #endregion

        #region Receiving

        private static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception,
            CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine($"{DateTime.Now:HH:mm:ss} - {errorMessage}");
            return Task.CompletedTask;
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update,
            CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                    case UpdateType.EditedMessage:
                        BotOnMessageReceived(update.Message);
                        break;
                    case UpdateType.CallbackQuery:
                        BotOnCallbackQueryReceived(update.CallbackQuery);
                        break;
                    case UpdateType.Poll:
                        BotOnPollAnswerReceived(update.Poll);
                        break;
                }
            }
            catch (Exception e)
            {
                await HandleErrorAsync(_botClient, e, cancellationToken);
            }
        }

        private static void BotOnMessageReceived(Message message)
        {
            if (message == null) return;

            // Another wiretap
            if (message.From.Id == Config.BdayChatId)
            {
                foreach (var admin in Config.AdminChatIds.Concat(Config.ObserversChatIds).ToArray())
                {
                    _botClient.ForwardMessageAsync(
                        chatId: admin,
                        fromChatId: message.Chat.Id,
                        messageId: message.MessageId
                    );
                }
            }

            switch (message.Type)
            {
                case MessageType.Text:
                    BotOnTextReceived(message);
                    break;
                case MessageType.Location:
                    BotOnLocationReceived(message);
                    break;
                default:
                    return;
            }
        }

        private static void BotOnTextReceived(Message message)
        {
            if (new List<long>(Config.AdminChatIds).Contains(message.From.Id))
            {
                if (message.Text.StartsWith("/send"))
                {
                    SendMessage(Quest.Messages.Single(m => m.Name == message.Text[6..]));
                }
                else if (message.Text.StartsWith("/forward"))
                {
                    SendMessage(message.Text[9..]);
                }
                else if (message.Text.StartsWith("/keyboard"))
                {
                    _botClient.SendTextMessageAsync(
                        chatId: message.From.Id,
                        text: "Вот:",
                        replyMarkup: ReplyKeyboardForAdmins);
                }
                else if (message.Text.StartsWith("муз квиз раунд 2"))
                {
                    SendMessage(Quest.Messages.Single(m => m.Name == "quiz_16_round_2_welcome"));
                }
                else if (message.Text.StartsWith("Как пользоваться админкой?"))
                {
                    SendAdminUsage(message.From.Id);
                }
            }

            // Handling codewords
            if (message.From.Id == Config.BdayChatId && Quest.Messages.Any(m => m.SendOnText == message.Text))
            {
                SendMessage(Quest.Messages.Single(m => m.SendOnText == message.Text));
            }
        }

        private static void BotOnLocationReceived(Message message)
        {
            var sentCoordinate =
                new GeoCoordinate.NetStandard2.GeoCoordinate(message.Location.Latitude, message.Location.Longitude);
            foreach (var tgMessage in Quest.Messages.Where(m => m.SendAtLocation != null))
            {
                var distanceInMeters = (int)sentCoordinate.GetDistanceTo(tgMessage.SendAtLocation.GetGeoCoordinate);
                if (distanceInMeters < 100)
                {
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss} - Location proximity {distanceInMeters} meters");
                    SendMessage(tgMessage);
                    return;
                }
            }
        }

        private static void BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            SendMessage(Quest.Messages.Single(m => m.Name == callbackQuery.Data));
        }

        private static void BotOnPollAnswerReceived(Poll poll)
        {
            foreach (var message in Quest.Messages.Where(m => m.Quiz != null && m.Quiz.PollId == poll.Id))
            {
                foreach (var admin in Config.AdminChatIds.Concat(Config.ObserversChatIds).ToArray())
                {
                    _botClient.SendTextMessageAsync(
                        chatId: admin,
                        text:
                        $"Выбранный ответ на вопрос \"{message.Quiz.Question}\": {poll.Options.Single(o => o.VoterCount > 0).Text}",
                        ParseMode.Html);
                }

                SendMessage(Quest.Messages.Single(m => m.Name == message.Quiz.ReplyName));
            }
        }

        #endregion
    }
}