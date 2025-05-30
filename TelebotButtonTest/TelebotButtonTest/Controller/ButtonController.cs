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
                            new [] { InlineKeyboardButton.WithCallbackData("������ �湮�ϱ�", $"visit_page_{chatId}") }
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

                Qna_Mode = true; // QnA ��� Ȱ��ȭ
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.MoveChannelButton]))
            {
                try
                {
                    // ä�� URL ����
                    var channel_Info =  Program._channelList;
                    // ä�� ���� ���� üũ
                    bool isSubscribed = await CheckChannelSubscriptionAsync(chatId, channel_Info, cancellationToken);
                    InlineKeyboardMarkup inlineKeyboard;

                    if (isSubscribed)
                    {
                        // �̹� ������ ���                        
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("ä�� �湮�ϱ�", channel_Info.Url) },
                        });
                    }
                    else
                    {
                        var current_User = Program._userList.FirstOrDefault(u => u.Chat_ID == Convert.ToString(chatId));
                        // �������� ���� ���
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("ä�� �����ϱ�", channel_Info.Url) },
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
                // �̼� üũ ��ư Ŭ�� �� ó��
                // ���÷� �̼� �Ϸ� �޽����� ����                
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
                            text: "�̹� �̼� ������ �Ϸ�Ǿ����ϴ�.",
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
                          new [] { InlineKeyboardButton.WithUrl("��ȭ �����ϱ�", chat_Telegram_Bot_Url) },
                    });

                    await _botClient.SendMessage(
                        chatId: chatId,
                        text: "������ ä��",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: cancellationToken);
                }
            }
        }
        private async Task<bool> CheckChannelSubscriptionAsync(long userId, ChannelInfoModel channel, CancellationToken cancellationToken = default)
        {
            try
            {
                // ä�� ����� ���� ��ȸ
                var chatMember = await _botClient.GetChatMember(
                    chatId: channel.Code,
                    userId: userId,
                    cancellationToken: cancellationToken);

                // ����� ���� Ȯ��
                return chatMember.Status == ChatMemberStatus.Administrator ||
                       chatMember.Status == ChatMemberStatus.Creator ||
                       chatMember.Status == ChatMemberStatus.Member;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ä�� ���� Ȯ�� �� ����: {ex.Message}");
                return false; // ���� �߻� �� �������� ���� ������ ����
            }
        }
        public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, JObject callbackQueryJson, CancellationToken cancellationToken)
        {
            string callbackData = callbackQueryJson["Data"].ToString();

            if (callbackData.StartsWith("visit_page_"))
            {
                string userChatId = callbackData.Replace("visit_page_", "");
                long chatId = long.Parse(callbackQueryJson["Message"]["Chat"]["Id"].ToString());

                // ���� ������Ʈ
                APIs.Instance.UpdateLinkMove(userChatId, true);

                // ��ũ ������
                var urlWithTracking = Program._linkList.First();
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new [] { InlineKeyboardButton.WithWebApp("�������� �̵�", new WebAppInfo { Url = urlWithTracking.Link_Url }) }
                });
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "�������� �̵��մϴ�.",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken);
            }
        }
    }
}
