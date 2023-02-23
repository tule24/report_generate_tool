using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

namespace PAVE_PRIORITY
{
    public partial class form_main : Form
    {
        List<priority_temp> ds_priority = new List<priority_temp>();
        List<timeout> ds_timeout = new List<timeout>();
        List<qc> ds_qc = new List<qc>();
        List<user> dsUser = new List<user>();
        List<tire> dsTire = new List<tire>();
        public form_main()
        {
            InitializeComponent();
        }

        private void form_main_Load(object sender, EventArgs e)
        {
            bt_run.Enabled = false;
        }

        void main()
        {

            string link = tB_input.Text;
            Regex pattern_link = new Regex(@"{_id:\s""(?<link>[^""]+)|(?<link>[a-zA-Z0-9]+)$");

            foreach (Match i in pattern_link.Matches(link))
            {
                string url = "API" + i.Groups["link"].ToString();
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Accept = "application/json";
                httpRequest.Headers["Authorization"] = "TOKEN"; // API-KEY để trong đoạn này

                string input = "";
                try
                {
                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        input = streamReader.ReadToEnd();
                    }
                    input = input.Replace("\"", string.Empty);
                    //Regex remove_space = new Regex(@"\s{1,}|\t");
                    //input = remove_space.Replace(input, string.Empty);

                    if (input.Contains("reportData") && input.Contains("detected_status") && input.Contains("tasks:") && input.Contains("agentTimeOut"))
                    {
                        string data = input.Split(new string[] { "reportData" }, StringSplitOptions.None)[0];
                        string[] split_data = input.Split(new string[] { "reportData" }, StringSplitOptions.None)[1].Split(new string[] { "tasks:" }, StringSplitOptions.None);
                        string[] tasks = split_data[1].Split(new string[] { "agentTimeOut" }, StringSplitOptions.None);

                        //Regex pattern_agent = new Regex(@"photoCode:04.*");
                        //string[] list_Agent = pattern_agent.Match(split_data[1]).ToString().Split(new string[] { "name:trim_id" },
                        //                      StringSplitOptions.None)[0].Split(new string[] { "agentTimeOut" }, StringSplitOptions.None);

                        string[] adminData = split_data[1].Replace("agentTimeOut", "~").Replace("taskSlots", "~").Split(new string[] { "~" }, StringSplitOptions.None);
                        //string[] adminData = split_data[1].Replace("agentTimeOut", "~").Split(new string[] { "~" }, StringSplitOptions.None);

                        Regex pattern1 = new Regex(@"sessionID:(?<ID>[^,]+)");
                        Regex pattern2 = new Regex(@"updatedAt:(?<date>[\d\-]+)([^,]+,){2}processAt");
                        Match value1 = pattern1.Match(data);
                        Match value2 = pattern2.Match(data);
                        string id = value1.Groups["ID"].ToString();
                        string date = value2.Groups["date"].ToString();
                        //Cắt file 
                        //if (split_data[0].Contains("source:user") || split_data[0].Contains("user_response:reject"))
                        //{
                        //    List<string> dsAgent = new List<string>();
                        //    user user = new user();
                        //    //Khởi tạo ds agent và ds damage

                        //    List<string> temp = new List<string>();
                        //    foreach (string j in list_Agent)
                        //    {
                        //        temp = Class3.inDsAgent(j);
                        //        dsAgent.AddRange(temp);
                        //    }
                        //    dsAgent.RemoveAt(6);
                        //    //Tạo ds agent
                        //    dsUser.AddRange(Class3.taoDsDamage(dsAgent, split_data[0], id, date));
                        //}
                        string test = input.Split(new string[] { "tasks:" }, StringSplitOptions.None)[1];
                        if (test.Contains("photoCode:06,status:FINISHED"))
                        {
                            dsTire.Add(Class4.ds_tire(data, adminData));
                        }

                        if (data.Contains("hasTimeOutInspect:true"))
                        {
                            ds_timeout.AddRange(Class2.ds_timeout(tasks, date, id));
                        }

                        if (split_data[1].Contains("admin_response") || split_data[1].Contains("dataAdmin"))
                        {
                            ds_priority.AddRange(Class1.ds_damage(adminData, date, id, split_data[0]));
                        }

                        if (split_data[1].Contains("action:ACCEPT") || split_data[1].Contains("action:REJECT"))
                        {
                            ds_qc.AddRange(Class2.ds_qc(tasks, date, id));
                        }
                    }
                }
                catch { }
            }

            data_timeout.DataSource = null;
            if (ds_timeout.Count > 0) { data_timeout.DataSource = ds_timeout; }
            data_timeout.AutoResizeColumns();
            data_timeout.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            data_timeout.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(206, 204, 235);
            data_timeout.EnableHeadersVisualStyles = false;
            data_timeout.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            data_timeout.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(68, 62, 171);
            data_timeout.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
            data_timeout.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            data_user.DataSource = null;
            if (dsUser.Count > 0) { data_user.DataSource = dsUser; }
            data_user.AutoResizeColumns();
            data_user.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            data_user.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(206, 204, 235);
            data_user.EnableHeadersVisualStyles = false;
            data_user.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            data_user.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(68, 62, 171);
            data_user.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
            data_user.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            data_priority.DataSource = null;
            if (ds_priority.Count > 0) { data_priority.DataSource = ds_priority; }
            data_priority.AutoResizeColumns();
            data_priority.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            data_priority.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(206, 204, 235);
            data_priority.EnableHeadersVisualStyles = false;
            data_priority.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            data_priority.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(68, 62, 171);
            data_priority.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
            data_priority.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            data_qc.DataSource = null;
            if (ds_qc.Count > 0) { data_qc.DataSource = ds_qc; }
            data_qc.AutoResizeColumns();
            data_qc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            data_qc.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(206, 204, 235);
            data_qc.EnableHeadersVisualStyles = false;
            data_qc.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            data_qc.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(68, 62, 171);
            data_qc.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 12, FontStyle.Bold);
            data_qc.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void tB_input_TextChanged(object sender, EventArgs e)
        {
            if (tB_input.Text == "") { bt_run.Enabled = false; bt_del.Enabled = true; } else { bt_run.Enabled = true; bt_del.Enabled = false; }
        }

        private void bt_run_Click(object sender, EventArgs e)
        {

            main();
            tB_input.ResetText();
        }

        private void bt_del_Click_1(object sender, EventArgs e)
        {

            if (tab_priority.Focus())
            {
                del_event(1);
            }
            if (tab_timeout.Focus())
            {
                del_event(2);
            }
            if (tab_qc.Focus())
            {
                del_event(3);
            }
            if (tab_user.Focus())
            {
                del_event(4);
            }

        }

        private void del_event(int option)
        {
            switch (option)
            {
                case 1:
                    if (ds_priority.Count != 0)
                    {
                        int index_1 = data_priority.CurrentRow.Index;
                        ds_priority.RemoveAt(index_1);
                        data_priority.DataSource = null;
                        data_priority.DataSource = ds_priority;
                    }
                    break;
                case 2:
                    if (ds_timeout.Count != 0)
                    {
                        int index_2 = data_timeout.CurrentRow.Index;
                        ds_timeout.RemoveAt(index_2);
                        data_timeout.DataSource = null;
                        data_timeout.DataSource = ds_timeout;
                    }
                    break;
                case 3:
                    if (ds_qc.Count != 0)
                    {
                        int index_3 = data_qc.CurrentRow.Index;
                        ds_qc.RemoveAt(index_3);
                        data_qc.DataSource = null;
                        data_qc.DataSource = ds_qc;
                    }
                    break;
                case 4:
                    if (dsUser.Count != 0)
                    {
                        int index_4 = data_user.CurrentRow.Index;
                        dsUser.RemoveAt(index_4);
                        data_user.DataSource = null;
                        data_user.DataSource = dsUser;
                    }
                    break;
            }
        }

        private void bt_Add_Click(object sender, EventArgs e)
        {
            if (tab_priority.Focus())
            {
                add_event(1);
            }
            if (tab_timeout.Focus())
            {
                add_event(2);
            }
            if (tab_qc.Focus())
            {
                add_event(3);
            }
            if (tab_user.Focus())
            {
                add_event(4);
            }
        }

        private void add_event(int option)
        {
            switch (option)
            {
                case 1:
                    ds_priority.Add(new priority_temp());
                    data_priority.DataSource = null;
                    data_priority.DataSource = ds_priority;
                    break;
                case 2:
                    ds_timeout.Add(new timeout());
                    data_timeout.DataSource = null;
                    data_timeout.DataSource = ds_timeout;
                    break;
                case 3:
                    ds_qc.Add(new qc());
                    data_qc.DataSource = null;
                    data_qc.DataSource = ds_qc;
                    break;
                case 4:
                    dsUser.Add(new user());
                    data_user.DataSource = null;
                    data_user.DataSource = dsUser;
                    break;
            }
        }

        private void bt_Reset_Click(object sender, EventArgs e)
        {
            DialogResult result = new DialogResult();
            result = MessageBox.Show("Toàn bộ data sẽ bị mất !!!\nBạn có thực sự muốn Reset lại toàn bộ DATA ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if(result == DialogResult.Yes)
            {
                ds_priority.Clear();
                ds_qc.Clear();
                ds_timeout.Clear();
                dsUser.Clear();
                dsTire.Clear();
                data_priority.DataSource = null;
                data_qc.DataSource = null;
                data_timeout.DataSource = null;
                data_user.DataSource = null;
                //data_tire.DataSource = null;
            }
        }
    }
}
