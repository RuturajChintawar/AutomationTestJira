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
        public void UpdateJira(List<MainJiraInfoModel> mainJiraInfoModels,List<JiraInfoModel> jiraInfoModels, string sprintName, bool IsCrCreated)
        {
            IWebDriver driver = new ChromeDriver();
            string url = "https://jira.tssconsultancy.com/browse/";
            string url1 = url + "WEB-55784";
            driver.Navigate().GoToUrl(url1);
            string username = "ruturaj.chintawar@tssconsultancy.com";
            string password = "Sept@123456";
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
                AddInInputString(componentElement, "AML");

                IWebElement assigneeNameElement = driver.FindElement(By.Id("assignee-field"));
                AddInAutoSuggestionDropDown(assigneeNameElement, mainJiraInfoModel.DevOwner);

                IWebElement originalEsitimate = driver.FindElement(By.Id("timetracking_originalestimate"));
                AddInInputString(originalEsitimate, "1m");
                
                IWebElement sprintNameElement = driver.FindElement(By.Id("customfield_10300-field"));
                AddInAutoSuggestionDropDown(sprintNameElement, sprintName);

                IWebElement devOwnerElement = driver.FindElement(By.Id("customfield_12601"));
                AddInAutoSuggestionDropDown(devOwnerElement, mainJiraInfoModel.DevOwner);

                IWebElement branchNameElement = driver.FindElement(By.Id("customfield_14100"));
                AddInInputString(branchNameElement, mainJiraInfoModel.MainJiraNo.Substring(4));

                IWebElement storyPointsElement = driver.FindElement(By.Id("customfield_10004"));
                AddInInputString(storyPointsElement, mainJiraInfoModel.StoryPoints.ToString());

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
                    AddInInputString(componentElement1, "AML");

                    IWebElement assigneeNameElement1 = driver.FindElement(By.Id("assignee-field"));
                    AddInAutoSuggestionDropDown(assigneeNameElement1, "Faisal.Shaikh");

                    IWebElement originalEsitimate1 = driver.FindElement(By.Id("timetracking_originalestimate"));
                    AddInInputString(originalEsitimate1, "1h");

                    driver.FindElement(By.Id("customfield_16000-2")).Click();
                    driver.FindElement(By.Id("create-issue-submit")).Click();
                    Thread.Sleep(4000);
                }
                
            }

            foreach (JiraInfoModel jiraModel in jiraInfoModels)
            {
                driver.Navigate().GoToUrl(url + jiraModel.MainJiraNo);

                //if (IsSubTaskPresent(jiraModel, alreadyCreatedSubTask[jiraModel.MainJiraNo]))
                //    continue;

                driver.FindElement(By.Id("opsbar-operations_more")).Click();
                Thread.Sleep(2000);
                driver.FindElement(By.Id("create-subtask")).Click();
                Thread.Sleep(4000);

                IWebElement summaryElement = driver.FindElement(By.Id("summary"));
                AddInInputString(summaryElement, jiraModel.SubTaskName);

                SelectElement oSelect = new SelectElement(driver.FindElement(By.Id("customfield_10503")));
                oSelect.SelectByText("TSS Consultancy");

                IWebElement componentElement = driver.FindElement(By.Id("components-textarea"));
                AddInInputString(componentElement, "AML");

                IWebElement assigneeNameElement = driver.FindElement(By.Id("assignee-field"));
                AddInAutoSuggestionDropDown(assigneeNameElement, jiraModel.TeamMember);

                IWebElement originalEsitimate = driver.FindElement(By.Id("timetracking_originalestimate"));
                AddInInputString(originalEsitimate, jiraModel.EstimateInHours.ToString()+"h");

                driver.FindElement(By.Id("customfield_16000-2")).Click();
                driver.FindElement(By.Id("create-issue-submit")).Click();
                Thread.Sleep(4000);
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
            entity.SendKeys(Keys.ArrowDown);
            entity.SendKeys(Keys.Enter);
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

        //public static DataTable GetData(string path, string sheet, bool considerDefault)
        //{

        //    using (var mem = new MemoryStream(File.ReadAllBytes(path)))
        //    {
        //        Workbook workBook = Workbook.Load(mem);

        //        //Workbook workbook = Workbook.Load(path);
        //        var worksheet = workBook.Worksheets.Find(w => w.Name == sheet);
        //        if (considerDefault && worksheet == null)
        //            worksheet = workBook.Worksheets[0];
        //        if (worksheet == null)
        //            throw new ApplicationException("Invalid sheet name " + sheet);

        //        DataTable dt = new DataTable();

        //        // add the headers

        //        int cols = worksheet.Cells.LastColIndex;
        //        for (int i = 0; i <= cols; i++)
        //        {
        //            dt.Columns.Add(worksheet.Cells[0, i].StringValue);
        //        }
        //        int rows = worksheet.Cells.LastRowIndex;
        //        for (int i = 1; i <= rows; i++)
        //        {
        //            DataRow row = dt.NewRow();

        //            for (int j = 0; j <= cols; j++)
        //            {
        //                row[j] = worksheet.Cells[i, j].Value ?? DBNull.Value;
        //            }

        //            dt.Rows.Add(row);
        //        }

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {
        //            bool isEmpty = true;
        //            foreach (DataColumn column in dt.Columns)
        //            {
        //                object obj = dt.Rows[i][column];
        //                if (obj != null && obj != DBNull.Value && !ExtraFunctions.IsAllSpaces(obj.ToString()))
        //                    isEmpty = false;
        //            }

        //            if (isEmpty)
        //            {
        //                dt.Rows.RemoveAt(i);
        //                i--;
        //            }
        //        }

        //        return dt;
        //    }
        //}
    }
}
