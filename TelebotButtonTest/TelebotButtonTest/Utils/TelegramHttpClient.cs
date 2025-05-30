using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace TelebotButtonTest
{    
    public class TelegramHttpClient
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _botToken;
        private readonly JsonSerializerSettings _jsonSettings;

        public TelegramHttpClient(string botToken)
        {
            _botToken = botToken;
            
            // JSON 직렬화 설정 - 텔레그램 API는 camelCase를 예상함
            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
            
            _botClient = new TelegramBotClient(botToken);
        }
        
        public async Task<JObject> SendMessageAsync(
            long chatId,
            string text,
            string parseMode = null,
            ReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var message = await _botClient.SendMessage(
                    chatId: chatId,
                    text: text,
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken);

                return JObject.FromObject(message, JsonSerializer.Create(_jsonSettings));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telegram API Error: {ex.Message}");
                return new JObject();
            }
        }
    }
}