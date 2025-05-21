using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelebotButtonTest
{
    public enum ButtonType
    {
        MoveWebPageButton,
        QnAButton,
        MoveChannelButton,
        AttendanceCheckButton,
        LiveChatButton
    }

    public class ButtonViewManager
    {
        // 버튼 타이틀 정의
        public static readonly Dictionary<ButtonType, string> ButtonTitles = new Dictionary<ButtonType, string>
        {
            { ButtonType.MoveWebPageButton, "가(외부 링크이동, 트래킹)" },
            { ButtonType.QnAButton, "나(질문 메세지 발송, 답변 수집)" },
            { ButtonType.MoveChannelButton, "다(텔레 채널이동, 가입체크)" },
            { ButtonType.AttendanceCheckButton, "라(출석 체크, 미션완료)" },
            { ButtonType.LiveChatButton, "마(라이브 채팅)" }
        };
        
        // 키보드 버튼 생성 메소드
        private static KeyboardButton CreateButton(ButtonType buttonType)
        {
            return new KeyboardButton(ButtonTitles[buttonType]);
        }
        
        // 콜백 데이터에서 버튼 타입 가져오기
        public static ButtonType GetButtonTypeFromCallback(string callbackData)
        {
            foreach (var pair in ButtonTitles)
            {
                if (pair.Value == callbackData)
                    return pair.Key;
            }

            throw new ArgumentException($"콜백 데이터에 해당하는 버튼 타입을 찾을 수 없습니다: {callbackData}");
        }
        
        // 메인 메뉴 키보드 생성
        public static ReplyKeyboardMarkup CreateMainMenu()
        {
            // 버튼 생성 및 레이아웃 구성
            var keyboard = new[]
            {
                new [] { CreateButton(ButtonType.MoveWebPageButton), CreateButton(ButtonType.QnAButton) },
                new [] { CreateButton(ButtonType.MoveChannelButton), CreateButton(ButtonType.AttendanceCheckButton) },
                new [] { CreateButton(ButtonType.LiveChatButton)}
            };            
            return new ReplyKeyboardMarkup(keyboard)
            {
                ResizeKeyboard = true,   // 화면에 맞게 크기 조정
                OneTimeKeyboard = false , // 계속 유지                
            };
        }
    }
}