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
using System.Linq;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using System.Collections.Concurrent;
using TelebotButtonTest.Models;
using System.Runtime.Remoting.Channels;
using TelebotButtonTest.BotClients;

namespace TelebotButtonTest
{
    class Program
    {

        public static List<UserModel> _userList = new List<UserModel>();
        public static ChannelInfoModel _channelList = new ChannelInfoModel();
        public static List<LinkInfoModel> _linkList = new List<LinkInfoModel>();
        public static MissionInfoModel _missionList = new MissionInfoModel();
        public static BotMessageInfoModel _botMessageList = new BotMessageInfoModel();
        public static MainBot _mainBotClient = new MainBot();
        public static ChatBot _chatTestBotClient = new ChatBot();
        public static BotTokenModel _botTokenInfo = new BotTokenModel();
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("텔레그램 봇 시작 중...");
            _botTokenInfo = APIs.Instance.GetBotTokenInfo();
            // 봇 초기화 및 시작
            InitializeBot();
            // db 데이터 주기적으로 업데이트
            var userListUpdateTask = Task.Run(UpdateUserListPeriodically);
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
        // 유저 리스트를 주기적으로 업데이트하는 메서드
        private static async Task UpdateUserListPeriodically()
        {
            // 종료 신호를 받기 위한 CancellationTokenSource 추가 (선택 사항)
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            // 유저 리스트 업데이트
                            _userList = APIs.Instance.GetAllUsers();
                            _channelList = APIs.Instance.GetAllChannels();
                            _linkList = APIs.Instance.GetAllLinks();
                            _missionList = APIs.Instance.GetMissions();
                            _botMessageList = APIs.Instance.GetBotMessages();
                            // 관리자 메시지 처리 추가
                            await ProcessAdminMessages();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"유저 리스트 업데이트 중 오류 발생: {ex.Message}");
                        }

                        // 1초 대기
                        await Task.Delay(1000, cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // 작업 취소됨
                    Console.WriteLine("유저 리스트 업데이트 작업이 취소되었습니다.");
                }
            }
        }
        
        // 관리자 메시지 처리 메서드
        private static async Task ProcessAdminMessages()
        {
            try
            {
                // DB에서 처리되지 않은 관리자 메시지 가져오기
                var adminMessages = APIs.Instance.GetUnprocessedAdminMessages();
                
                foreach (var message in adminMessages)
                {
                    try
                    {
                        // 텔레그램으로 메시지 전송
                        await _chatTestBotClient.SendAdminMessage(message.Chat_ID, message.Message_Content);
                        
                        // 처리 완료 표시
                        APIs.Instance.MarkAdminMessageAsProcessed(message.Id);
                        
                        Console.WriteLine($"관리자 메시지 전송 완료: {message.Chat_ID} -> {message.Message_Content}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"관리자 메시지 전송 실패: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"관리자 메시지 처리 중 오류: {ex.Message}");
            }
        }
        // 봇 초기화
        private static void InitializeBot()
        {
            Task _ = _mainBotClient.Inistialize();
            Task __ = _chatTestBotClient.Inistialize();            
        }

        // 봇 업데이트 수신 시작
        private static void StartReceivingUpdates(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_botTokenInfo.Main_Bot_Token))
            {
                MainBot.StartMainBotReceiving(cancellationToken);
            }

            if (!string.IsNullOrEmpty(_botTokenInfo.Chat_Bot_Token) &&
                _botTokenInfo.Chat_Bot_Token != _botTokenInfo.Main_Bot_Token)
            {
                ChatBot.StartChatBotReceiving(cancellationToken);
            }
        }
    }
}