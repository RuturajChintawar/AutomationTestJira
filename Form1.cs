//using IronXL;
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
using System.Data.OleDb;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TestJira
{
    public partial class Form1 : Form
    {
        string file_path;
        public Form1()
        {
            InitializeComponent();
            IsCrSubTask.Checked = false;
            textBox1.Text = "ruturaj.chintawar@tssconsultancy.com";
            textBox2.Text = "Sept@123456";
            textBox4.Text = "Faisal.Shaikh";
            textBox5.Text = "Aml";
            textBox3.Text = "( AML Incredibles )";
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
            
            for(int i = 0; i < fileData.Rows.Count; i++)
            {
                DataRow row = fileData.Rows[i];
                string mainJiraNo;
                if (string.IsNullOrWhiteSpace(row.Field<string>("jirano")) || row.Field<string>("jirano").ToLower().Replace(" ", "").Substring(0, 3) != "web" || dupCheckMainJira.Contains(row.Field<string>("jirano")))
                    continue;
                else
                    mainJiraNo = row.Field<string>("jirano").Replace(" ", "").ToLower();

                MainJiraInfoModel mainJiraInfoModel = new MainJiraInfoModel();
                mainJiraInfoModel.MainJiraNo = mainJiraNo;
                mainJiraInfoModel.StoryPoints = row.Field<int?>("storypoint");
                mainJiraInfoModel.DevOwner = nameDic[row.Field<string>("teammember")];
                mainJiraInfoModel.BranchName = mainJiraNo.Substring(4);

                mainJiraInfoModel.jiraInfoModels = new List<JiraInfoModel>();

                while (i < fileData.Rows.Count  && !string.IsNullOrEmpty(fileData.Rows[i].Field<string>("teammember")))
                {
                    JiraInfoModel jiraInfoModel = new JiraInfoModel();
                    jiraInfoModel.SubTaskName = fileData.Rows[i].Field<string>("task");
                    jiraInfoModel.TeamMember = nameDic[fileData.Rows[i].Field<string>("teammember")];
                    jiraInfoModel.EstimateInHours = fileData.Rows[i].Field<Object>("estimateinhours");
                    jiraInfoModel.AlreadyCreated = fileData.Rows[i].Field<string>("taskcreated");
                    i++;
                    if (jiraInfoModel.AlreadyCreated != null && jiraInfoModel.AlreadyCreated.ToLower().Replace(" ", "") == "done")
                        continue;

                    mainJiraInfoModel.jiraInfoModels.Add(jiraInfoModel);
                }
                dupCheckMainJira.Add(mainJiraNo);
                mainJiraInfoModel.NoOfSubTask = mainJiraInfoModel.jiraInfoModels.Count;
                mainJiraInfoModels.Add(mainJiraInfoModel);
            }

            return mainJiraInfoModels;
        }
        private DataTable GetDataTableFromCsv(string path)
        {
            string header = 1 == 1 ? "Yes" : "No";

            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties=\"Text;HDR=" + header + "\""))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
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
        public List<List<MainJiraInfoModel>> partition(List<MainJiraInfoModel> values)
        {
            List<List<MainJiraInfoModel>> listOfMainJiraInfoModel = new List<List<MainJiraInfoModel>>();
            List<MainJiraInfoModel> list1 = new List<MainJiraInfoModel>();
            List<MainJiraInfoModel> list2 = new List<MainJiraInfoModel>();
            List<MainJiraInfoModel> list3 = new List<MainJiraInfoModel>();
            List<MainJiraInfoModel> list4 = new List<MainJiraInfoModel>();
            for (int i = 0; i < values.Count; i++)
            {
                if(i % 4==0)
                    list1.Add(values[i]);
                if(i % 4 == 1)
                    list2.Add(values[i]);
                if(i % 4 == 2)
                    list3.Add(values[i]);
                if (i % 4 == 3)
                    list4.Add(values[i]);

            }
            listOfMainJiraInfoModel.Add(list1);
            listOfMainJiraInfoModel.Add(list2);
            listOfMainJiraInfoModel.Add(list3);
            listOfMainJiraInfoModel.Add(list4);
            return listOfMainJiraInfoModel;
        }
        private async void Create_Click(object sender, EventArgs e)
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


                DataTable fileData = GetDataTableFromCsv(file_path);

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

                string fileName = Path.GetFileNameWithoutExtension(path.Text);
                string sprintName = fileName.Substring(fileName.Length - 10).Replace("_", "/") + textBox3.Text;


                List<List<MainJiraInfoModel>> partitions = partition(mainJiraInfoModels);
                AutomationManager automationManager = new AutomationManager();

                var Task1 = Task.Run(() => new AutomationManager().UpdateJira(partitions[0], sprintName, IsCrSubTask.Checked, textBox1.Text, textBox2.Text, textBox4.Text, textBox5.Text, textBox3.Text));
                var Task2 = Task.Run(() => new AutomationManager().UpdateJira(partitions[1], sprintName, IsCrSubTask.Checked, textBox1.Text, textBox2.Text, textBox4.Text, textBox5.Text, textBox3.Text));
                var Task3 = Task.Run(() => new AutomationManager().UpdateJira(partitions[2], sprintName, IsCrSubTask.Checked, textBox1.Text, textBox2.Text, textBox4.Text, textBox5.Text, textBox3.Text));
                var Task4 = Task.Run(() => new AutomationManager().UpdateJira(partitions[3], sprintName, IsCrSubTask.Checked, textBox1.Text, textBox2.Text, textBox4.Text, textBox5.Text, textBox3.Text));

                await Task1;
                await Task2;
                await Task3;
                await Task4;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

    }
}
