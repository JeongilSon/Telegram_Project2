using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelebotButtonTest;
using System.Linq;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using System.Collections.Concurrent;

namespace TelegramCasinoBot
{
    class Program
    {
        // 텔레그램 봇 토큰
        private static readonly string MainBotToken = "7608424452:AAFymaN8L0pG7dE7Zzge7_zqrCockARnxUM";
        private static readonly string ChatTestBotToken = "7648846172:AAHxVxLX0mts20EwIx3WLyTCgrRQxse65Cs";
        // 봇 클라이언트 인스턴스
        private static TelegramBotClient _mainBotClient;
        private static TelegramBotClient _chatTestBotClient;

        // 버튼 컨트롤러 인스턴스
        private static ButtonController _buttonController;
        
        // 사용자 상태 관리 Dictionary
        private static Dictionary<long, UserState> _userStates = new Dictionary<long, UserState>();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("텔레그램 봇 시작 중...");

            // 봇 초기화 및 시작
            InitializeBot();

            // 봇 정보 출력
            var mainBotMe = await _mainBotClient.GetMe();
            var chatBotMe = await _chatTestBotClient.GetMe();
            Console.WriteLine($"봇 시작: @{mainBotMe.Username}");
            Console.WriteLine($"봇 시작: @{chatBotMe.Username}");

            // 봇 실행
            using (var cts = new CancellationTokenSource())
            {
                StartReceivingUpdates(cts.Token);
                
                Console.WriteLine("봇이 실행 중입니다. 종료하려면 아무 키나 누르세요...");
                Console.ReadKey();
                
                // 봇 중지
                cts.Cancel();
            }
        }

        // 봇 초기화
        private static void InitializeBot()
        {
            _mainBotClient = new TelegramBotClient(MainBotToken);
            _chatTestBotClient = new TelegramBotClient(ChatTestBotToken);
            _buttonController = new ButtonController(_mainBotClient);
        }

        // 봇 업데이트 수신 시작
        private static void StartReceivingUpdates(CancellationToken cancellationToken)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
                Limit = 100
            };

            _mainBotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );

            //_chatTestBotClient.StartReceiving(
            //        updateHandler: HandleUpdateAsync,
            //        errorHandler: HandlePollingErrorAsync,
            //        receiverOptions: receiverOptions,
            //        cancellationToken: cancellationToken
            //    );
        }

        // 업데이트 처리 핸들러
        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                {
                    await HandleMessageAsync(update.Message, cancellationToken);
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    await HandleCallbackAsync(update.CallbackQuery, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"메시지 처리 중 오류 발생: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        // 콜백 쿼리 처리
        private static async Task HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            // 빠른 UI 응답을 위해 즉시 콜백 응답
            await _mainBotClient.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                cancellationToken: cancellationToken);

            // 콜백 쿼리 처리를 위임
            var callbackQueryJson = JObject.FromObject(callbackQuery);
            await _buttonController.HandleCallbackQueryAsync(callbackQueryJson, cancellationToken);
        }

        // 폴링 오류 처리 핸들러
        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

            // 사용자 상태 관리
            EnsureUserState(chatId);

            if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                // 시작 명령어 처리
                _userStates[chatId].CurrentStep = "welcomed";
                await _buttonController.SendWelcomeMessageAsync(chatId, cancellationToken);
            }
            else if (IsButtonMessage(messageText))
            {
                // 버튼 메시지 처리
                await _buttonController.HandleButtonMessageAsync(chatId, messageText, cancellationToken);
            }
            else
            {
                // 기타 메시지 처리
                await _mainBotClient.SendMessage(
                    chatId: chatId,
                    text: "명령을 실행하려면 /start를 입력하세요.",
                    cancellationToken: cancellationToken);
            }
        }

        // 사용자 상태 확인
        private static void EnsureUserState(long chatId)
        {
            if (!_userStates.ContainsKey(chatId))
            {
                _userStates[chatId] = new UserState();
            }
            _userStates[chatId].LastActivity = DateTime.Now;
        }

        // 버튼 메시지인지 확인
        private static bool IsButtonMessage(string messageText)
        {
            return ButtonViewManager.ButtonTitles.Any(b => b.Value == messageText);
        }
    }

    public class UserState
    {
        public string CurrentStep { get; set; } = "";
        public DateTime LastActivity { get; set; } = DateTime.Now;
    }
}