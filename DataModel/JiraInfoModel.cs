using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestJira.DataModel
{
    public class JiraInfoModel
    {
        public string MainJiraNo { get; set; }
        public string SubTaskName { get; set; }
        public string TeamMember { get; set; }
        public Object EstimateInHours { get; set; }
        public string AlreadyCreated { get; set; }
        public string SubTaskJiraNo { get; set; }
    }
}
