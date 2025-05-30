using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace TelebotButtonTest.BotClients
{
    public class MainBot
    {
        // 텔레그램 봇 토큰
        public static string MainBotToken = "";
        // 봇 클라이언트 인스턴스
        private static TelegramBotClient _mainBotClient;
        // 버튼 컨트롤러 인스턴스
        private static ButtonController _buttonController;
        public async Task Inistialize()
        {
            // 텔레그램 봇 클라이언트 초기화
            MainBotToken = Program._botTokenInfo.Main_Bot_Token;
            _mainBotClient = new TelegramBotClient(MainBotToken);
            _buttonController = new ButtonController(_mainBotClient);
            var mainBotMe = await _mainBotClient.GetMe();
            Console.WriteLine($"봇 시작: @{mainBotMe.Username}");
        }
        public static void StartMainBotReceiving(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.ChatMember, UpdateType.CallbackQuery },
                Limit = 100
            };

            _mainBotClient.StartReceiving(
                updateHandler: HandleMainBotUpdateAsync,
                errorHandler: HandleMainBotPollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );
        }
        private static async Task HandleChatMemberUpdateAsync(ITelegramBotClient botClient, ChatMemberUpdated chatMemberUpdated, CancellationToken cancellationToken)
        {
            // 채널 ID 또는 사용자명 확인
            var channelId = chatMemberUpdated.Chat.Id;
            var channelUsername = chatMemberUpdated.Chat.Username;

            // 전체 채널 목록 가져오기
            var channels = Program._channelList;
            var userId = chatMemberUpdated.From.Id;

            // 사용자 정보 찾기
            var current_User = Program._userList.FirstOrDefault(u => u.Chat_ID == userId.ToString());

            // 현재 업데이트가 관련된 채널인지 확인 (채널 ID 또는 사용자명으로 확인)
            var matchedChannel = channels.Url.Contains(channelId.ToString());
            if (matchedChannel != null)
            {
                // 이전 상태와 새 상태 확인
                var oldStatus = chatMemberUpdated.OldChatMember.Status;
                var newStatus = chatMemberUpdated.NewChatMember.Status;
                // 가입한 사용자 ID
                if (current_User != null)
                {
                    // 사용자가 채널에 가입한 경우
                    if ((oldStatus == ChatMemberStatus.Left || oldStatus == ChatMemberStatus.Kicked) &&
                        (newStatus == ChatMemberStatus.Member || newStatus == ChatMemberStatus.Administrator))
                    {
                        // 채널 가입 상태 업데이트
                        APIs.Instance.UpdateChannelMove(current_User.Chat_ID, true);

                        try
                        {
                            // 사용자에게 개인 메시지 보내기
                            await botClient.SendMessage(
                                chatId: userId,
                                text: "채널 가입이 확인되었습니다! 감사합니다.",
                                cancellationToken: cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"사용자에게 메시지 전송 실패: {ex.Message}");
                        }

                        Console.WriteLine($"사용자 {userId}가 채널 {channelUsername ?? channelId.ToString()}에 가입했습니다.");
                    }
                    // 사용자가 채널에서 나간 경우
                    else if ((oldStatus == ChatMemberStatus.Member || oldStatus == ChatMemberStatus.Administrator) &&
                                (newStatus == ChatMemberStatus.Left || newStatus == ChatMemberStatus.Kicked))
                    {
                        // 채널 가입 상태 업데이트 (false로 설정)
                        APIs.Instance.UpdateChannelMove(current_User.Chat_ID, false);

                        try
                        {
                            // 사용자에게 개인 메시지 보내기 (선택 사항)
                            await botClient.SendMessage(
                                chatId: userId,
                                text: "채널에서 나가셨습니다. 다시 가입해주시면 감사하겠습니다.",
                                cancellationToken: cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"사용자에게 메시지 전송 실패: {ex.Message}");
                        }

                        Console.WriteLine($"사용자 {userId}가 채널 {channelUsername ?? channelId.ToString()}에서 나갔습니다.");
                    }
                }
            }
        }
        // 업데이트 처리 핸들러
        private static async Task HandleMainBotUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                {
                    await HandleMessageAsync(update.Message, cancellationToken);
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    await HandleCallbackAsync(botClient, update.CallbackQuery, cancellationToken);
                }
                // 채팅 멤버 업데이트 처리 (채널 가입/탈퇴 감지)
                else if (update.ChatMember != null)
                {
                    await HandleChatMemberUpdateAsync(botClient, update.ChatMember, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"메시지 처리 중 오류 발생: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        // 콜백 쿼리 처리
        private static async Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            // 빠른 UI 응답을 위해 즉시 콜백 응답
            await _mainBotClient.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                cancellationToken: cancellationToken);

            // 콜백 쿼리 처리를 위임
            var callbackQueryJson = JObject.FromObject(callbackQuery);
            await _buttonController.HandleCallbackQueryAsync(botClient, callbackQueryJson, cancellationToken);
        }
        // 폴링 오류 처리 핸들러
        private static Task HandleMainBotPollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception is ApiRequestException apiRequestException
                ? $"Telegram API 오류:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}"
                : exception.ToString();

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        // 메시지 처리
        private static async Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var messageText = message.Text;

            Console.WriteLine($"메시지 수신: {messageText} (ID: {chatId})");

            if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                // 시작 명령어 처리
                await _buttonController.SendWelcomeMessageAsync(chatId, cancellationToken);
                if (Program._userList.FirstOrDefault(x => x.Chat_ID == chatId.ToString()) == null)
                {
                    APIs.Instance.RegisterUser(Convert.ToString(message.Chat.Id), message.Chat.Username, message.Chat.LastName + message.Chat.FirstName, "");
                }
            }
            else if (IsButtonMessage(messageText))
            {
                // 버튼 메시지 처리
                await _buttonController.HandleButtonMessageAsync(chatId, messageText, cancellationToken);
            }
            else if(_buttonController.Qna_Mode == true)
            {
                await _mainBotClient.SendMessage(
                chatId: chatId,
                text: Program._userList.FirstOrDefault(x => x.Chat_ID == chatId.ToString()).User_Question,
                cancellationToken: cancellationToken);
            }
        }

        // 버튼 메시지인지 확인
        private static bool IsButtonMessage(string messageText)
        {
            return ButtonViewManager.ButtonTitles.Any(b => b.Value == messageText);
        }
    }
}
