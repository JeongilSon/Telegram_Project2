using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelebotButtonTest.Models;
using System.Runtime.CompilerServices;
using System.Configuration;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TelebotButtonTest
{    
    public class APIs
    {
        public static APIs Instance = new APIs();
        private static readonly string host_Url = "http://localhost:44367";
        public ChannelInfoModel GetAllChannels()
        {
            string request_Url = host_Url + "/api/Channel";
            ChannelInfoModel channels = new ChannelInfoModel();

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                JObject json = JObject.Parse(JArray.Parse(reader.ReadToEnd()).First().ToString());
                                // JSON 응답 파싱
                                return new ChannelInfoModel
                                {
                                    Code = Convert.ToString(json.SelectToken("code")),
                                    Name = Convert.ToString(json.SelectToken("name")),
                                    Url = Convert.ToString(json.SelectToken("url")),
                                    Chat_Content = Convert.ToString(json.SelectToken("content")),
                                };
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)ex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = reader.ReadToEnd();
                                Console.WriteLine($"API 오류: {errorResponse.StatusCode}, 상세: {error}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"채널 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"채널 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                }

                Thread.Sleep(1000);
            }

            return channels; // 실패 시 빈 리스트 반환
        }
        public List<LinkInfoModel> GetAllLinks()
        {
            string request_Url = host_Url + "/api/Link";
            List<LinkInfoModel> links = new List<LinkInfoModel>();

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                string json = reader.ReadToEnd();

                                // JSON 응답 파싱
                                links = JsonConvert.DeserializeObject<List<LinkInfoModel>>(json);
                                return links;
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)ex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = reader.ReadToEnd();
                                Console.WriteLine($"API 오류: {errorResponse.StatusCode}, 상세: {error}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"링크 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"링크 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                }

                Thread.Sleep(1000);
            }

            return links; // 실패 시 빈 리스트 반환
        }
        public MissionInfoModel GetMissions()
        {
            string request_Url = host_Url + "/api/Mission";
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                JObject json = JObject.Parse(JArray.Parse(reader.ReadToEnd()).First().ToString());
                                // JSON 응답 파싱
                                return new MissionInfoModel
                                {
                                    MissionName = Convert.ToString(json.SelectToken("mission_Name")),
                                    MissionType = (MissionTypeEnum)Convert.ToInt32(json.SelectToken("mission_Type")),
                                    Mission_Rewords = Convert.ToInt32(json.SelectToken("mission_Rewords")),
                                    Mission_Chat_Content = Convert.ToString(json.SelectToken("mission_Chat_Content")),
                                };
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)ex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = reader.ReadToEnd();
                                Console.WriteLine($"API 오류: {errorResponse.StatusCode}, 상세: {error}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"미션 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"미션 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                }

                Thread.Sleep(1000);
            }

            return null; // 실패 시 빈 리스트 반환
        }

        public BotTokenModel GetBotTokenInfo()
        {
            string request_Url = host_Url + "/api/Bot/tokens";
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                string json = reader.ReadToEnd();
                                return JsonConvert.DeserializeObject<BotTokenModel>(json);
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.ToString(), retry, "봇 토큰 정보");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"봇 토큰 정보 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                }
                Thread.Sleep(1000);
            }
            return null;
        }
        public BotMessageInfoModel GetBotMessages()
        {
            string request_Url = host_Url + "/api/BotMessage";
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                JObject json = JObject.Parse(reader.ReadToEnd().ToString());
                                // JSON 응답 파싱
                                return new BotMessageInfoModel
                                {
                                    Welcome_Message = Convert.ToString(json.SelectToken("welcome_message")),
                                    Question_Message = Convert.ToString(json.SelectToken("question_message")),
                                };
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        using (var errorResponse = (HttpWebResponse)ex.Response)
                        {
                            using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                            {
                                string error = reader.ReadToEnd();
                                Console.WriteLine($"API 오류: {errorResponse.StatusCode}, 상세: {error}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"봇 메세지 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"봇 메세지 데이터 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                }

                Thread.Sleep(1000);
            }

            return null; // 실패 시 빈 리스트 반환
        }
        #region UserTable 관련 api
        // 모든 사용자 정보 가져오기
        public List<UserModel> GetAllUsers()
        {
            string request_Url = host_Url + "/api/User";
            List<UserModel> users = new List<UserModel>();

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                string json = reader.ReadToEnd();
                                users = JsonConvert.DeserializeObject<List<UserModel>>(json);
                                return users;
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.ToString(), retry, "사용자 목록");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"사용자 목록 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                }
                Thread.Sleep(1000);
            }
            return users;
        }
        // 특정 사용자 정보 가져오기
        public UserModel GetUser(string chatId)
        {
            string request_Url = host_Url + $"/api/User/{chatId}";

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                string json = reader.ReadToEnd();
                                Console.WriteLine($"API 응답: {json}");
                                return JsonConvert.DeserializeObject<UserModel>(json);
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.ToString(), retry, "사용자 정보");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"사용자 정보 가져오기 실패 (시도 {retry + 1}/3): {ex.Message}");
                }
                Thread.Sleep(1000);
            }
            return null;
        }
        // 미션 완료 상태 업데이트
        public UserModel UpdateMissionComplete(string chatId, bool isComplete)
        {
            string request_Url = host_Url + $"/api/User/{chatId}/MissionComplete";
            return SendPostRequest(request_Url, isComplete, "미션 완료 상태");
        }
        // 채널 이동 상태 업데이트
        public UserModel UpdateChannelMove(string chatId, bool isMoved)
        {
            string request_Url = host_Url + $"/api/User/{chatId}/ChannelMove";
            return SendPostRequest(request_Url, isMoved, "채널 이동 상태");
        }
        // 링크 이동 상태 업데이트
        public UserModel UpdateLinkMove(string chatId, bool isMoved)
        {
            string request_Url = host_Url + $"/api/User/{chatId}/LinkMove";
            return SendPostRequest(request_Url, isMoved, "링크 이동 상태");
        }
        public UserModel RegisterUser(string chatId, string telegramId, string nickname, string userQuestion)
        {
            if(string.IsNullOrEmpty(userQuestion))
            {
                userQuestion = "test";
            }
            // 등록할 사용자 정보 준비
            var user = new UserModel
            {
                Chat_ID = chatId,
                Telegram_ID = telegramId ?? "unknown",
                NickName = nickname ?? "Anonymous",
                User_Question = userQuestion ?? "test",
                Link_Move = 0,
                Channel_Move = 0,
                Mission_Complete = 0
            };

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    string request_Url = host_Url + $"/api/User/Register";
                    return SendPostRequest(request_Url, user, "유저 등록");
                }
                catch 
                {
                    Thread.Sleep(1000);
                }                                
            }
            return null;
        }
        // 기존 SendPostRequest 함수 수정 - Created 응답 코드도 처리하도록
        private UserModel SendPostRequest<T>(string url, T data, string operationName)
        {
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Accept = "application/json";

                    // 요청 본문 작성
                    string jsonData = JsonConvert.SerializeObject(data);
                    byte[] byteData = Encoding.UTF8.GetBytes(jsonData);
                    request.ContentLength = byteData.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(byteData, 0, byteData.Length);
                    }

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        // OK(200)와 Created(201) 모두 처리
                        if (response.StatusCode == HttpStatusCode.OK ||
                            response.StatusCode == HttpStatusCode.Created)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                string json = reader.ReadToEnd();
                                Console.WriteLine($"API 응답: {json}");
                                return JsonConvert.DeserializeObject<UserModel>(json);
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.ToString(), retry, operationName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{operationName} 작업 실패 (시도 {retry + 1}/3): {ex.Message}");
                }
                Thread.Sleep(1000);
            }
            return null;
        }
        #endregion
        
        #region LiveChat 관련 API
        // 처리되지 않은 관리자 메시지 가져오기
        public List<LiveChatModel> GetUnprocessedAdminMessages()
        {
            string request_Url = host_Url + "/api/Livechat/UnprocessedAdminMessages";
            List<LiveChatModel> messages = new List<LiveChatModel>();

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "GET";
                    request.Accept = "application/json";

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var reader = new StreamReader(response.GetResponseStream()))
                            {
                                string json = reader.ReadToEnd();
                                messages = JsonConvert.DeserializeObject<List<LiveChatModel>>(json);
                                return messages ?? new List<LiveChatModel>();
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    if (retry == 2) // 마지막 시도에서만 로그 출력
                    {
                        Console.WriteLine($"관리자 메시지 가져오기 실패: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (retry == 2) // 마지막 시도에서만 로그 출력
                    {
                        Console.WriteLine($"관리자 메시지 가져오기 실패: {ex.Message}");
                    }
                }
                Thread.Sleep(500);
            }
            return messages;
        }

        // 관리자 메시지 처리 완료 표시
        public bool MarkAdminMessageAsProcessed(int messageId)
        {
            string request_Url = host_Url + $"/api/Livechat/MarkAsProcessed/{messageId}";

            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(request_Url);
                    request.Method = "PUT";
                    request.Accept = "application/json";
                    // Content-Length를 0으로 설정 (빈 본문)
                    request.ContentLength = 0;

                    // 또는 Content-Type도 함께 설정
                    request.ContentType = "application/json";
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        return response.StatusCode == HttpStatusCode.OK;
                    }
                }
                catch (WebException ex)
                {
                    if (retry == 2) // 마지막 시도에서만 로그 출력
                    {
                        Console.WriteLine($"메시지 처리 완료 표시 실패: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (retry == 2) // 마지막 시도에서만 로그 출력
                    {
                        Console.WriteLine($"메시지 처리 완료 표시 실패: {ex.Message}");
                    }
                }
                Thread.Sleep(500);
            }
            return false;
        }
        #endregion
    }
}