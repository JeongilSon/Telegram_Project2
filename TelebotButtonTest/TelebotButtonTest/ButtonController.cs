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
                        new [] { InlineKeyboardButton.WithUrl("�������� �̵�", urlWithTracking) }
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
                        text: "���� 1",
                        cancellationToken: cancellationToken);
                }
            }
            else if (messageText.Equals(ButtonViewManager.ButtonTitles[ButtonType.MoveChannelButton]))
            {
                try
                {
                    // ä�� ���� ���� üũ
                    bool isSubscribed = await CheckChannelSubscriptionAsync(chatId, cancellationToken);

                    // ä�� URL ����
                    string channelUrl = telegramGroup_Url;
                    string text;
                    InlineKeyboardMarkup inlineKeyboard;

                    if (isSubscribed)
                    {
                        // �̹� ������ ���
                        text = "�ڷ��׷� ä�ο� �̹� �����ϼ̽��ϴ�! �����մϴ�.";
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("ä�� �湮�ϱ�", channelUrl) },
                        });
                    }
                    else
                    {
                        // �������� ���� ���
                        text = "�Ʒ� ��ư�� Ŭ���Ͽ� �ڷ��׷� ä�ο� �����ϼ���. \n\n���� �� '���� Ȯ���ϱ�' ��ư�� �����ּ���!";
                        inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("ä�� �����ϱ�", channelUrl) },
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
                        text: "�̼� �Ϸ� �Ǿ����ϴ�.",
                        cancellationToken: cancellationToken);
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
        private async Task<bool> CheckChannelSubscriptionAsync(long userId, CancellationToken cancellationToken = default)
        {
            try
            {
                // ä�� ����� ���� ��ȸ
                var chatMember = await _botClient.GetChatMember(
                    chatId: telegram_Group_Chat_Id,
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

                // �ݹ� ������ üũ �� ó��
                if (dataStr == "check_subscription")
                {
                    // ä�� ���� ���� �ٽ� Ȯ��
                    bool isSubscribed = await CheckChannelSubscriptionAsync(chatId, cancellationToken);

                    if (isSubscribed)
                    {
                        // ���� Ȯ�εǸ� ȯ�� �޽��� �߼�
                        string channelUrl = telegramGroup_Url;
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("ä�� �湮�ϱ�", channelUrl) },
                        });

                        if (_botClient != null)
                        {
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "�ڷ��׷� ä�� ������ Ȯ���߽��ϴ�! �����մϴ�. ������ �����÷��� '���� �ޱ�' ��ư�� �����ּ���.",
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        // �������� ���� ��� �ٽ� �ʴ� �޽��� �߼�
                        string channelUrl = telegramGroup_Url;
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            new [] { InlineKeyboardButton.WithUrl("ä�� �����ϱ�", channelUrl) },
                        });

                        if (_botClient != null)
                        {
                            await _botClient.SendMessage(
                                chatId: chatId,
                                text: "ä�� ������ Ȯ�ε��� �ʾҽ��ϴ�. \nä�ο� �����Ͻ� �� '���� Ȯ���ϱ�' ��ư�� �ٽ� �����ּ���.",
                                replyMarkup: inlineKeyboard,
                                cancellationToken: cancellationToken);
                        }
                    }
                }
                else if (dataStr == "claim_subscription_reward")
                {
                    // ���� ���� ����
                    if (_botClient != null)
                    {
                        await _botClient.SendMessage(
                            chatId: chatId,
                            text: "ä�� ���� ������ �����̽��ϴ�! �����մϴ�.",
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
