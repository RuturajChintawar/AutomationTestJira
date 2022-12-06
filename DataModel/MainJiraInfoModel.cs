using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestJira.DataModel
{
    public class MainJiraInfoModel
    {
        public string MainJiraNo { get; set; }
        public int? StoryPoints { get;set; }
        public string BranchName { get; set; }
        public int NoOfSubTask { get; set; }
        public string DevOwner { get; set; }
        public List<JiraInfoModel> jiraInfoModels { get; set; }
    }
}
