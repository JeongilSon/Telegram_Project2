using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelebotButtonTest.Models
{
    public enum MissionTypeEnum
    {
        Daily,
        Mission,
        Event
    }
    public class MissionInfoModel
    {
        public string MissionName { get; set; }
        public MissionTypeEnum MissionType { get; set; } // Daily, Mission, Event
        public int Mission_Rewords { get; set; } = 0;
        public string Mission_Chat_Content { get; set; }

    }
}
