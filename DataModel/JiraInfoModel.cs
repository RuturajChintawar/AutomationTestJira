using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestJira.DataModel
{
    public class JiraInfoModel
    {
        public string SubTaskName { get; set; }
        public string TeamMember { get; set; }
        public Object EstimateInHours { get; set; }
        public string AlreadyCreated { get; set; }
    }
}
