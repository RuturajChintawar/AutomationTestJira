using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestJira.DataModel;
using System.Threading;
using OpenQA.Selenium.Support.UI;

namespace TestJira.Manager
{
    public class AutomationManager
    {
        public void UpdateJira(List<MainJiraInfoModel> mainJiraInfoModels, string sprintName, bool IsCrCreated,string username,string password,string codeReview2,string Components,string Client)
        {
            IWebDriver driver = new ChromeDriver();
            string url = "https://jira.tssconsultancy.com/browse/";
            driver.Navigate().GoToUrl(url + "WEB-55784");
            driver.FindElement(By.Id("login-form-username")).SendKeys(username);
            driver.FindElement(By.Id("login-form-password")).SendKeys(password);
            driver.FindElement(By.Id("login-form-submit")).Click();

            //Dictionary<string, List<JiraInfoModel>> alreadyCreatedSubTask = new Dictionary<string, List<JiraInfoModel>>();
            foreach (MainJiraInfoModel mainJiraInfoModel in mainJiraInfoModels)
            {
                driver.Navigate().GoToUrl(url + mainJiraInfoModel.MainJiraNo);

                //alreadyCreatedSubTask.Add(mainJiraInfoModel.MainJiraNo, GetAlreadyCreatedJiraList(driver));

                driver.FindElement(By.Id("edit-issue")).Click();

                Thread.Sleep(4000);

                IWebElement componentElement = driver.FindElement(By.Id("components-textarea"));
                AddInInputString(componentElement, Components);

                IWebElement assigneeNameElement = driver.FindElement(By.Id("assignee-field"));
                AddInAutoSuggestionDropDown(assigneeNameElement, mainJiraInfoModel.DevOwner);

                IWebElement originalEsitimate = driver.FindElement(By.Id("timetracking_originalestimate"));
                AddInInputString(originalEsitimate, "1h");

                IWebElement sprintNameElement = driver.FindElement(By.Id("customfield_10300-field"));
                AddInAutoSuggestionDropDown(sprintNameElement, sprintName);

                IWebElement devOwnerElement = driver.FindElement(By.Id("customfield_12601"));
                AddInAutoSuggestionDropDown(devOwnerElement, mainJiraInfoModel.DevOwner);

                IWebElement branchNameElement = driver.FindElement(By.Id("customfield_14100"));
                AddInInputString(branchNameElement, mainJiraInfoModel.BranchName);

                IWebElement storyPointsElement = driver.FindElement(By.Id("customfield_10004"));
                AddInInputString(storyPointsElement, mainJiraInfoModel.StoryPoints == null ? "" : mainJiraInfoModel.StoryPoints.ToString());

                //team name
                driver.FindElement(By.Id("customfield_16000-2")).Click();

                driver.FindElement(By.Id("edit-issue-submit")).Click();

                Thread.Sleep(4000);

                if (IsCrCreated)
                {
                    driver.FindElement(By.Id("opsbar-operations_more")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("create-subtask")).Click();
                    Thread.Sleep(4000);

                    IWebElement summaryElement = driver.FindElement(By.Id("summary"));
                    AddInInputString(summaryElement, "Code Review 2 and Checkin");

                    SelectElement oSelect = new SelectElement(driver.FindElement(By.Id("customfield_10503")));
                    oSelect.SelectByText("TSS Consultancy");

                    IWebElement componentElement1 = driver.FindElement(By.Id("components-textarea"));
                    AddInInputString(componentElement1, Components);

                    IWebElement assigneeNameElement1 = driver.FindElement(By.Id("assignee-field"));
                    AddInAutoSuggestionDropDown(assigneeNameElement1, codeReview2);

                    IWebElement originalEsitimate1 = driver.FindElement(By.Id("timetracking_originalestimate"));
                    AddInInputString(originalEsitimate1, "1h");

                    driver.FindElement(By.Id("customfield_16000-2")).Click();
                    driver.FindElement(By.Id("create-issue-submit")).Click();
                    Thread.Sleep(4000);
                }
                foreach (JiraInfoModel jiraModel in mainJiraInfoModel.jiraInfoModels)
                {

                    //if (IsSubTaskPresent(jiraModel, alreadyCreatedSubTask[jiraModel.MainJiraNo]))
                    //    continue;

                    driver.FindElement(By.Id("opsbar-operations_more")).Click();
                    Thread.Sleep(2000);
                    driver.FindElement(By.Id("create-subtask")).Click();
                    Thread.Sleep(4000);

                    IWebElement summaryElement1 = driver.FindElement(By.Id("summary"));
                    AddInInputString(summaryElement1, jiraModel.SubTaskName);

                    SelectElement oSelect = new SelectElement(driver.FindElement(By.Id("customfield_10503")));
                    oSelect.SelectByText("TSS Consultancy");

                    IWebElement componentElement2 = driver.FindElement(By.Id("components-textarea"));
                    AddInInputString(componentElement2, "AML");

                    IWebElement assigneeNameElement2 = driver.FindElement(By.Id("assignee-field"));
                    AddInAutoSuggestionDropDown(assigneeNameElement2, jiraModel.TeamMember);

                    IWebElement originalEsitimate2 = driver.FindElement(By.Id("timetracking_originalestimate"));
                    AddInInputString(originalEsitimate2, jiraModel.EstimateInHours ==  null ? "": jiraModel.EstimateInHours.ToString() + "h");

                    driver.FindElement(By.Id("customfield_16000-2")).Click();
                    driver.FindElement(By.Id("create-issue-submit")).Click();
                    Thread.Sleep(4000);
                }
            }
            driver.Quit();
        }
        public void AddInAutoSuggestionDropDown(IWebElement entity,String entityValue)
        {
            entity.Clear();
            entity.Click();
            Thread.Sleep(2000);
            entity.SendKeys(entityValue);
            Thread.Sleep(2000);
            entity.SendKeys(OpenQA.Selenium.Keys.ArrowDown);
            entity.SendKeys(OpenQA.Selenium.Keys.Enter);
        }
        public void AddInInputString(IWebElement entity, String entityValue)
        {
            entity.Clear();
            entity.SendKeys(entityValue);
        }
        public List<JiraInfoModel> GetAlreadyCreatedJiraList(IWebDriver driver)
        {
            IWebElement tableElement = driver.FindElement(By.Id("issuetable"));
            List<IWebElement> rowsElement =  new List<IWebElement>(tableElement.FindElements(By.ClassName(".issuerow.issue-table-draggable")));
            List<JiraInfoModel> alreadyCreatedJiraList =  new List<JiraInfoModel>();
            List<string> dd = new List<string>();
            foreach (IWebElement rowElement in rowsElement)
            {
                List<IWebElement> colsElement = new List<IWebElement>(rowElement.FindElements(By.TagName("td")));
                JiraInfoModel jiraInfoModel = new JiraInfoModel();

                for(int i = 0; i < colsElement.Count; i++) 
                {
                    dd.Add(colsElement[i].Text);
                    if(i == 1)
                        jiraInfoModel.TeamMember = colsElement[i].Text;
                    
                    if(i == 4)
                        jiraInfoModel.SubTaskName = colsElement[i].Text;
                   //if () 
                }
                alreadyCreatedJiraList.Add(jiraInfoModel);
            }
            return alreadyCreatedJiraList;
        }
        public bool IsSubTaskPresent(JiraInfoModel jiraInfoModel,List<JiraInfoModel> alreadyCreatedJiras)
        {
            foreach(JiraInfoModel alreadyCreatedJira in alreadyCreatedJiras)
            {
                if(alreadyCreatedJira.TeamMember.Replace(" ",".").ToLower() == jiraInfoModel.TeamMember.ToLower()
                    && alreadyCreatedJira.SubTaskName.Replace(" ", "").ToLower() == jiraInfoModel.SubTaskName.Replace(" ","").ToLower()
                    ) {
                    return true;
                }
            }
            return false;
        }

    }
}
