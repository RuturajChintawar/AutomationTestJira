using IronXL;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TestJira.DataModel;
using TestJira.Manager;

namespace TestJira
{
    public partial class Form1 : Form
    {
        string file_path;
        public Form1()
        {
            InitializeComponent();
            IsCrSubTask.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
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

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.Until(webdriver => webdriver.FindElement(By.Id("opsbar-operations_more")));

            List<string> jiraNos = textBox1.Text.Split(',').ToList();
            List<string> time = textBox3.Text.Split(',').ToList();
            for (int i = 0; i < jiraNos.Count; i++)
            {
                driver.Navigate().GoToUrl(url + jiraNos[i]);
                driver.FindElement(By.Id("opsbar-operations_more")).Click();
                driver.FindElement(By.Id("create-subtask")).Click();
                WebDriverWait waita = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                waita.Until(webdriver => webdriver.FindElement(By.Id("issuetype-single-select")));
                driver.FindElement(By.Id("summary")).SendKeys(textBox2.Text);
                SelectElement oSelect = new SelectElement(driver.FindElement(By.Id("customfield_10503")));
                oSelect.SelectByText("TSS Consultancy");
                driver.FindElement(By.Id("components-textarea")).SendKeys("AML");
                driver.FindElement(By.Id("assign-to-me-trigger")).Click();
                driver.FindElement(By.Id("timetracking_originalestimate")).SendKeys(time[i]);
                driver.FindElement(By.Id("customfield_16000-2")).Click();
                driver.FindElement(By.Id("create-issue-submit")).Click();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int size = -1;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                file_path = openFileDialog1.FileName;
                try
                {
                    string text = File.ReadAllText(file_path);
                    size = text.Length;
                    path.Text = Path.GetFileName(file_path);
                }
                catch (IOException)
                {
                }
            }
        }
        private DataTable ReadExcel(string fileName)
        {
            WorkBook workbook = WorkBook.Load(fileName);
            WorkSheet sheet = workbook.DefaultWorkSheet;
            return sheet.ToDataTable(true);
        }

        

        private bool ValidateFileColumns(DataTable fileData)
        {
            List<string> columns = new List<string>() { "jirano", "task", "teammember", "estimateinhours", "storypoint", "taskcreated" , "name", "fullname" };

            HashSet<string> fileColumns = fileData.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToHashSet<string>();
            foreach (string column in columns)
            {
                if (!fileColumns.Contains(column))
                    throw new Exception(column + " not present in file");
            }
            return true;
        }
        public Dictionary<string,string> GetNameList(DataTable fileData)
        {
            Dictionary<String, string> nameDic = new Dictionary<string, string>();
            foreach(DataRow row in fileData.Rows)
            {
                if(row.Field<string>("name") == "Total")
                    break;

                string name = row.Field<string>("name");
                if(!string.IsNullOrWhiteSpace(name)&& !nameDic.ContainsKey(name))
                {
                    nameDic.Add(name, row.Field<string>("fullname"));
                }
            }
            return nameDic;
        }

        private List<MainJiraInfoModel> GetMainJiraModel(DataTable fileData, Dictionary<string,string> nameDic)
        {
            List<MainJiraInfoModel> mainJiraInfoModels = new List<MainJiraInfoModel>();
            HashSet<string> dupCheckMainJira = new HashSet<string>();
            foreach (DataRow row in fileData.Rows)
            {
                string mainJiraNo = row.Field<string>("jirano");
                if (!string.IsNullOrWhiteSpace(mainJiraNo) && !dupCheckMainJira.Contains(mainJiraNo))
                {
                    if (mainJiraNo.ToLower().Replace(" ", "").Substring(0, 3) != "web")
                        continue;
                    MainJiraInfoModel mainJiraInfoModel = new MainJiraInfoModel();
                    mainJiraInfoModel.MainJiraNo = mainJiraNo;
                    mainJiraInfoModel.StoryPoints = row.Field<double>("storypoint");
                    mainJiraInfoModel.DevOwner = nameDic[row.Field<string>("teammember")];
                    mainJiraInfoModels.Add(mainJiraInfoModel);
                    dupCheckMainJira.Add(mainJiraNo);
                }
            }
            return mainJiraInfoModels;
        }
        private List<JiraInfoModel> GetJiraInfoModel(DataTable fileData, Dictionary<string, string> nameDic)
        {
            List<JiraInfoModel> jiraInfoModels = new List<JiraInfoModel>();
            string mainJiraNo = null;
            foreach (DataRow row in fileData.Rows)
            {
                if (!string.IsNullOrEmpty(row.Field<string>("teammember")) && nameDic.ContainsKey(row.Field<string>("teammember")))
                {
                    if (!string.IsNullOrEmpty(row.Field<string>("jirano")))
                    {
                        mainJiraNo = row.Field<string>("jirano").Replace(" ", "").ToLower();
                    }
                    if (mainJiraNo.ToLower().Replace(" ", "").Substring(0, 3) != "web")
                        continue;

                    JiraInfoModel jiraInfoModel = new JiraInfoModel();
                    jiraInfoModel.MainJiraNo = mainJiraNo;
                    jiraInfoModel.SubTaskName = row.Field<string>("task");
                    if (string.IsNullOrEmpty(jiraInfoModel.SubTaskName))
                        jiraInfoModel.SubTaskName = row.Field<string>("jiraname");
                    jiraInfoModel.TeamMember = nameDic[row.Field<string>("teammember")];
                    jiraInfoModel.EstimateInHours = row.Field<Object>("estimateinhours");
                    jiraInfoModel.AlreadyCreated = row.Field<string>("taskcreated");

                    if (jiraInfoModel.AlreadyCreated.ToLower().Replace(" ", "") == "done")
                        continue;
                    jiraInfoModels.Add(jiraInfoModel);
                }
            }
            return jiraInfoModels;
        }
        private string ValidateSprint(string fileName)
        {
            IWebDriver driver = new ChromeDriver();
            string url = "https://jira.tssconsultancy.com/browse/WEB-55784";
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(50);
            driver.Navigate().GoToUrl(url);
            string username = "ruturaj.chintawar@tssconsultancy.com";
            string password = "Sept@123456";
            driver.FindElement(By.Id("login-form-username")).SendKeys(username);
            driver.FindElement(By.Id("login-form-password")).SendKeys(password);
            driver.FindElement(By.Id("login-form-submit")).Click();

            driver.FindElement(By.Id("edit-issue")).Click();
            fileName = Path.GetFileNameWithoutExtension(fileName);
            string sprintName = fileName.Substring(fileName.Length - 10).Replace("_", "/") + "( AML Incredibles )";
            return sprintName;
            //driver.FindElement(By.Id("customfield_10300-field")).SendKeys(sprintName);
            //try
            //{
            //    driver.FindElement(By.Id("customfield_10300-suggestions")).Click();
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("sprint not present");
            //}
            //finally
            //{
            //    driver.Close();
            //}


        }
        private void Create_Click(object sender, EventArgs e)
        {

            try
            {
                // file Name related validation
                if (string.IsNullOrEmpty(file_path))
                {
                    throw new Exception("Please select file path");
                }
                if (Path.GetExtension(path.Text) != ".csv")
                {
                    throw new Exception("Please select .csv extension file path");
                }


                DataTable fileData = ReadExcel(file_path);

                // columnname lowercase and space reduction
                for (int i = 0; i < fileData.Columns.Count; i++)
                {
                    fileData.Columns[i].ColumnName = fileData.Columns[i].ColumnName.Replace(" ", "").ToLower();
                }

                //validate file column
                ValidateFileColumns(fileData);

                // convert into model
                Dictionary<string, string> nameList = GetNameList(fileData);
                List<MainJiraInfoModel> mainJiraInfoModels = GetMainJiraModel(fileData, nameList);
                List<JiraInfoModel> jiraInfoModels = GetJiraInfoModel(fileData, nameList);

                // validate sprint

                //ValidateSprint(path.Text);

                // 
                string fileName = Path.GetFileNameWithoutExtension(path.Text);
                string sprintName = fileName.Substring(fileName.Length - 10).Replace("_", "/") + "( AML Incredibles )";

                AutomationManager automationManager = new AutomationManager();
                automationManager.UpdateJira(mainJiraInfoModels,jiraInfoModels, sprintName,IsCrSubTask.Checked);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        //Update Main

        //Create checkin and code review

    }
}
