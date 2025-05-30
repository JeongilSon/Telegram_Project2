using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelebotButtonTest.Models
{
    public class UserModel
    {
        public string Chat_ID { get; set; }

        public string Telegram_ID { get; set; }

        public string NickName { get; set; }

        public string User_Question { get; set; }

        public int Link_Move { get; set; }

        public int Channel_Move { get; set; }

        public int Mission_Complete { get; set; }
    }
}
