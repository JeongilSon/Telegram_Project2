using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TelebotButtonTest.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelebotButtonTest;
using static System.Net.WebRequestMethods;
using TelebotButtonTest.BotClients;

namespace TelebotButtonTest
{
    public class ButtonController
    {
        private readonly ITelegramBotClient _botClient;
        private const string chat_Telegram_Bot_Url = "https://t.me/chatT_e_st_Bot";
        public bool Qna_Mode { get; set; } = false;

        public ButtonController(ITelegramBotClient botClient)
        {           
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        }
        public async Task SendWelcomeMessageAsync(long chatId, CancellationToken cancellationToken = default(CancellationToken))
        {
            // 1. 
            string welcomeMessage = Program._botMessageList.Welcome_Message;
            
            if (_botClient != null)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: welcomeMessage,
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
                    var urlWithTracking = Program._linkList.First();
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                            new [] { InlineKeyboardButton.WithCallbackData("페이지 방문하기", $"visit_page_{chatId}") }
                    });
                    APIs.Instance.UpdateLinkMove(Convert.ToString(chatId), true);
                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: urlWithTracking.Link_Chat_Content,
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
                        text: Program._botMessageList.Question_Message,
                        cancellationToken: cancellationToken);
                }

                Qna_Mode = true; // QnA 모드 활성화
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.MoveChannelButton]))
            {
                try
                {
                    // 채널 URL 생성
                    var channel_Info =  Program._channelList;
                    // 채널 가입 여부 체크
                    bool isSubscribed = await CheckChannelSubscriptionAsync(chatId, channel_Info, cancellationToken);
                    InlineKeyboardMarkup inlineKeyboard;

                    if (isSubscribed)
                    {
                        // 이미 가입한 경우                        
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("채널 방문하기", channel_Info.Url) },
                        });
                    }
                    else
                    {
                        var current_User = Program._userList.FirstOrDefault(u => u.Chat_ID == Convert.ToString(chatId));
                        // 가입하지 않은 경우
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("채널 가입하기", channel_Info.Url) },
                        });
                    }

                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: channel_Info.Chat_Content,
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.MissonCheckButton]))
            {
                // 미션 체크 버튼 클릭 시 처리
                // 예시로 미션 완료 메시지를 전송                
                var missionInfo = Program._missionList;
                var current_User = Program._userList.FirstOrDefault(u => u.Chat_ID == Convert.ToString(chatId));
                if(current_User.Mission_Complete == 0)
                {
                    if (_botClient != null)
                    {
                        APIs.Instance.UpdateMissionComplete(current_User.Chat_ID, true);
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: Program._missionList.Mission_Chat_Content,
                            cancellationToken: cancellationToken);
                    }
                }
                else
                {
                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "이미 미션 수행이 완료되었습니다.",
                            cancellationToken: cancellationToken);
                    }
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
        private async Task<bool> CheckChannelSubscriptionAsync(long userId, ChannelInfoModel channel, CancellationToken cancellationToken = default)
        {
            try
            {
                // 채널 멤버십 상태 조회
                var chatMember = await _botClient.GetChatMember(
                    chatId: channel.Code,
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
        public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, JObject callbackQueryJson, CancellationToken cancellationToken)
        {
            string callbackData = callbackQueryJson["Data"].ToString();

            if (callbackData.StartsWith("visit_page_"))
            {
                string userChatId = callbackData.Replace("visit_page_", "");
                long chatId = long.Parse(callbackQueryJson["Message"]["Chat"]["Id"].ToString());

                // 상태 업데이트
                APIs.Instance.UpdateLinkMove(userChatId, true);

                // 링크 보내기
                var urlWithTracking = Program._linkList.First();
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithWebApp("페이지로 이동", new WebAppInfo { Url = urlWithTracking.Link_Url }) }
                });
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "페이지로 이동합니다.",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
