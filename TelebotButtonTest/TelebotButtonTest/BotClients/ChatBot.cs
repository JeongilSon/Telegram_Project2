using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using TelebotButtonTest.Models;
namespace TelebotButtonTest.BotClients
{
    public class ChatBot
    {
        private static string ChatBotToken = "";
        private static TelegramBotClient _chatTestBotClient;
        private static readonly string _webApiBaseUrl = "http://localhost:44367"; // Telegram_Project2_Web API URL
        public async Task Inistialize()
        {
            // 텔레그램 봇 클라이언트 초기화
            ChatBotToken = Program._botTokenInfo.Chat_Bot_Token;
            _chatTestBotClient = new TelegramBotClient(ChatBotToken);
            var chatBotMe = await _chatTestBotClient.GetMe();
            Console.WriteLine($"봇 시작: @{chatBotMe.Username}");
        }
        public static void StartChatBotReceiving(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.ChatMember, UpdateType.CallbackQuery },
                Limit = 100
            };
            _chatTestBotClient.StartReceiving(
                updateHandler: HandleChatBotUpdateAsync,
                errorHandler: HandleChatBotPollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );
        }
        // 사용자 메시지 처리 메서드
        private static async Task HandleUserMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id.ToString();
            var text = message.Text;
            var userId = message.From.Id.ToString();
            var username = message.From.Username  + "(" + message.From.LastName + message.From.FirstName+ ")";

            // 웹 서버로 메시지 전달
            await SendMessageToWebAsync(chatId, text, username);

            // 메시지 수신 확인 응답 (옵션)
            await _chatTestBotClient.SendChatAction(
                chatId: message.Chat.Id,
                action: ChatAction.Typing,
                cancellationToken: cancellationToken
            );
        }
        // /start 명령어 처리
        private static async Task HandleStartCommandAsync(Message message, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var startParam = string.Empty;

            // 파라미터 추출 (/start 이후의 텍스트)
            if (message.Text.Length > 6) // "/start" 이후에 텍스트가 있는지 확인
            {
                startParam = message.Text.Substring(7).Trim(); // "/start " 이후의 텍스트
            }

            // 파라미터가 있는 경우 처리 (향후 사용자 식별 등에 사용 가능)
            if (!string.IsNullOrEmpty(startParam))
            {
                Console.WriteLine($"Start 파라미터: {startParam}");
                // 필요한 경우 여기서 파라미터 처리 (예: 사용자 연결 정보 저장)
            }

            // 웰컴 메시지 전송
            string welcomeMessage = "안녕하세요! 실시간 상담이 시작되었습니다. 문의사항을 입력해 주세요.";

            // 실제로는 웹 API에서 웰컴 메시지를 가져오는 것도 좋은 방법입니다.
            // var welcomeMessage = await GetWelcomeMessageFromWebAsync() ?? "안녕하세요! 실시간 상담이 시작되었습니다.";

            await _chatTestBotClient.SendMessage(
                chatId: chatId,
                text: welcomeMessage,
                cancellationToken: cancellationToken
            );
        }
        private static async Task HandleChatBotUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                // 메시지 처리
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                {
                    var message = update.Message;
                    
                    // /start 명령어 처리
                    if (message.Text.StartsWith("/start"))
                    {
                        await HandleStartCommandAsync(message, cancellationToken);
                        return;
                    }
                    
                    // 일반 메시지 처리
                    await HandleUserMessageAsync(message, cancellationToken);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"채팅 봇 처리 중 오류 발생: {ex.Message}");
            }
        }

        private static Task HandleChatBotPollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception is ApiRequestException apiRequestException
                ? $"Telegram API 오류:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}"
                : exception.ToString();

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        // 웹 서버로 메시지 전송
        private static async Task SendMessageToWebAsync(string chatId, string text, string username)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // 웹 서버로 전송할 메시지 데이터
                    var messageData = new LiveChatModel
                    {
                        Chat_ID = chatId,
                        Message_Content = text,
                        UserName = username,
                        Timestamp = DateTime.Now,
                        IsFromAdmin = false,
                        IsRead = false
                    };
                    
                    // JSON 직렬화
                    var content = new StringContent(
                        JsonSerializer.Serialize(messageData),
                        Encoding.UTF8,
                        "application/json"
                    );
                    
                    // 웹 서버 엔드포인트로 전송
                    var response = await client.PostAsync(_webApiBaseUrl + "/api/Livechat/Send", content);
                    
                    // 응답 확인 (선택 사항)
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"웹 서버로 메시지 전송 실패: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"메시지 전송 중 오류 발생: {ex.Message}");
            }
        }
        
        // 관리자 메시지를 텔레그램으로 전송하는 공개 메서드
        public async Task SendAdminMessage(string chatId, string message)
        {
            try
            {
                await _chatTestBotClient.SendMessage(
                    chatId: long.Parse(chatId),
                    text: message,
                    cancellationToken: CancellationToken.None
                );
                
                Console.WriteLine($"관리자 메시지 전송 성공: {chatId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"관리자 메시지 전송 실패: {ex.Message}");
                throw;
            }
        }
    }
}
