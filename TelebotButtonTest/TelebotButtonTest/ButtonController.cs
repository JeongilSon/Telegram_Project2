using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.WebRequestMethods;

namespace TelebotButtonTest
{
    public class ButtonController
    {
        private readonly ITelegramBotClient _botClient;
        private const string telegramGroup_Url = "https://t.me/+HmEW0q3w2nI5NWU1";
        private const string telegram_Redirect_Url = "https://www.google.com";
        private const string telegram_Group_Chat_Id = "-1002643939224";
        private const string chat_Telegram_Bot_Url = "https://t.me/chatT_e_st_Bot";
        public ButtonController(ITelegramBotClient botClient)
        {
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        }
        public async Task SendWelcomeMessageAsync(long chatId, CancellationToken cancellationToken = default(CancellationToken))
        {
            // 1. 
            string welcomeMessage = "Welcome Test";
            
            if (_botClient != null)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: welcomeMessage,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                string menuMessage = " Test - ";
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: menuMessage,
                    parseMode: ParseMode.Html,
                    replyMarkup: ButtonViewManager.CreateMainMenu(),
                    cancellationToken: cancellationToken);
            }
        }        
        public async Task HandleButtonMessageAsync(long chatId, string messageText, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.MoveWebPageButton]))
            {
                try
                {
                    var trackingParam = $"utm_source=telegrambot&utm_medium=button&utm_campaign=test1&user_id={chatId}";
                    var urlWithTracking = telegram_Redirect_Url;

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] { InlineKeyboardButton.WithUrl("페이지로 이동", urlWithTracking) }
                    });

                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "botleitn",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }                
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.QnAButton]))
            {
                if (_botClient != null)
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "질문 1",
                        cancellationToken: cancellationToken);
                }
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.MoveChannelButton]))
            {
                try
                {
                    // 채널 가입 여부 체크
                    bool isSubscribed = await CheckChannelSubscriptionAsync(chatId, cancellationToken);

                    // 채널 URL 생성
                    string channelUrl = telegramGroup_Url;
                    string text;
                    InlineKeyboardMarkup inlineKeyboard;

                    if (isSubscribed)
                    {
                        // 이미 가입한 경우
                        text = "텔레그램 채널에 이미 가입하셨습니다! 감사합니다.";
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("채널 방문하기", channelUrl) },
                        });
                    }
                    else
                    {
                        // 가입하지 않은 경우
                        text = "아래 버튼을 클릭하여 텔레그램 채널에 가입하세요. \n\n가입 후 '가입 확인하기' 버튼을 눌러주세요!";
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("채널 가입하기", channelUrl) },
                        });
                    }

                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: text,
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.AttendanceCheckButton]))
            {
                if (_botClient != null)
                {
                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "미션 완료 되었습니다.",
                        cancellationToken: cancellationToken);
                }
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.LiveChatButton]))
            {
                if (_botClient != null)
                {
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                          new [] { InlineKeyboardButton.WithUrl("대화 시작하기", chat_Telegram_Bot_Url) },
                    });

                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "관리자 채팅",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                }
            }
        }
        private async Task<bool> CheckChannelSubscriptionAsync(long userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // 채널 멤버십 상태 조회
                var chatMember = await _botClient.GetChatMember(
                    chatId: telegram_Group_Chat_Id,
                    userId: userId,
                    cancellationToken: cancellationToken);

                // 멤버십 상태 확인
                return chatMember.Status == ChatMemberStatus.Administrator ||
                       chatMember.Status == ChatMemberStatus.Creator ||
                       chatMember.Status == ChatMemberStatus.Member;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"채널 구독 확인 중 오류: {ex.Message}");
                return false; // 오류 발생 시 가입하지 않은 것으로 간주
            }
        }
        public async Task HandleCallbackQueryAsync(JToken callbackQuery, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var data = callbackQuery["data"];
                if (data == null)
                {
                    return;
                }

                var dataStr = data.Value<string>();
                if (string.IsNullOrEmpty(dataStr))
                {
                    return;
                }

                var fromToken = callbackQuery["from"];
                if (fromToken == null)
                {
                    return;
                }

                var idToken = fromToken["id"];
                if (idToken == null)
                {
                    return;
                }

                var chatId = idToken.Value<long>();

                // 콜백 데이터 체크 및 처리
                if (dataStr == "check_subscription")
                {
                    // 채널 구독 여부 다시 확인
                    bool isSubscribed = await CheckChannelSubscriptionAsync(chatId, cancellationToken);

                    if (isSubscribed)
                    {
                        // 구독 확인되면 환영 메시지 발송
                        string channelUrl = telegramGroup_Url;
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("채널 방문하기", channelUrl) },
                        });

                        if (_botClient != null)
                        {
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "텔레그램 채널 가입을 확인했습니다! 감사합니다. 혜택을 받으시려면 '혜택 받기' 버튼을 눌러주세요.",
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        // 구독하지 않은 경우 다시 초대 메시지 발송
                        string channelUrl = telegramGroup_Url;
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("채널 가입하기", channelUrl) },
                        });

                        if (_botClient != null)
                        {
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "채널 가입이 확인되지 않았습니다. \n채널에 가입하신 후 '가입 확인하기' 버튼을 다시 눌러주세요.",
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (dataStr == "claim_subscription_reward")
                {
                    // 혜택 제공 로직
                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "채널 가입 혜택을 받으셨습니다! 감사합니다.",
                            cancellationToken: cancellationToken);
                    }
                }
                else if (dataStr.StartsWith("test_"))
                {
                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: $" : {dataStr}",
                            cancellationToken: cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" : {ex.Message}");
            }
        }    
    }
}
