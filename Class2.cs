using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PAVE_PRIORITY
{
    public class timeout
    {
        public string DATE { get; set; }
        public string AGENT { get; set; }
        public string ID { get; set; }
        public string IMAGE { get; set; }
        public string TASK { get; set; }
    }

    public class qc
    {
        public string DATE { get; set; }
        public string ADMIN { get; set; }
        public string AGENT { get; set; }
        public string ID { get; set; }
        public string IMAGE { get; set; }
        public string REASON { get; set; }
        public string CONTENT { get; set; }
    }
    class Class2
    {
        public static List<timeout> ds_timeout(string[] flow_time, string date, string id)
        {
            List<timeout> dsTimeOut = new List<timeout>();
            Regex pattern_time = new Regex(@"nameAgent:(?<agent>[^,]+)|inspectName:(?<task>[^}]+)|listKickAgent");
            Regex pattern_photo = new Regex(@"photoCode:(?<photo>\d+)");
            foreach (string i in flow_time)
            {
                if (i.Contains("nameAgent"))
                {
                    Match temp = pattern_time.Match(i);
                    string photo = pattern_photo.Match(i).Groups["photo"].ToString();
                    while (!temp.ToString().Contains("listKickAgent"))
                    {
                        timeout element = new timeout();
                        element.IMAGE = photo;
                        element.AGENT = temp.Groups["agent"].ToString();
                        temp = temp.NextMatch();
                        if (!temp.ToString().Contains("nameAgent") && !temp.ToString().Contains("listKickAgent"))
                        {
                            element.TASK = temp.Groups["task"].ToString().ToUpper();
                            temp = temp.NextMatch();
                        }
                        else
                        {
                            element.TASK = "QC";
                        }
                        element.ID = id;
                        element.DATE = date;
                        dsTimeOut.Add(element);
                    }
                }
            }
            return dsTimeOut;
        }

        public static List<qc> ds_qc(string[] flow_time, string date, string id)
        {
            List<qc> ds_qc = new List<qc>();
            Regex pattern_qc = new Regex(@"photoCode:(?<photo>\d+),[^,]+,type:QC|username:(?<agent>[^,]+),e|messageSent:{(?<reason>[^:]+)|username:(?<admin>[^,]+),action:(?<status>\w+)");

            foreach (string i in flow_time)
            {
                if (i.Contains("action:ACCEPT") || i.Contains("action:REJECT"))
                {
                    qc temp_qc = new qc();
                    temp_qc.DATE = date;
                    temp_qc.ID = id;
                    Match temp = pattern_qc.Match(i);
                    temp_qc.CONTENT = temp.Groups["reason"].ToString();
                    while (!temp.ToString().Contains("QC"))
                    {
                        temp = temp.NextMatch();
                    }
                    temp_qc.IMAGE = temp.Groups["photo"].ToString();
                    temp = temp.NextMatch();
                    temp_qc.AGENT = temp.Groups["agent"].ToString();
                    temp = temp.NextMatch();
                    temp_qc.ADMIN = temp.Groups["admin"].ToString();
                    temp_qc.REASON = temp.Groups["status"].ToString();
                    temp = temp.NextMatch();
                    while (temp.NextMatch() != Match.Empty)
                    {
                        if (temp.Groups["status"].ToString() != temp_qc.REASON)
                        {
                            temp_qc.ADMIN = "";
                            temp = temp.NextMatch();
                        } else
                        {
                            temp_qc.ADMIN = temp.Groups["admin"].ToString();
                            temp = temp.NextMatch();
                        }
                    }
                    if (temp_qc.REASON.Contains("ACCEPT")) { temp_qc.REASON = "REJECT WRONG IMAGE"; temp_qc.CONTENT += " => Accept"; }
                    else { temp_qc.REASON = "ACCEPT WRONG IMAGE"; temp_qc.CONTENT = "It's " + temp_qc.CONTENT; }
                    if (temp_qc.ADMIN != "") { ds_qc.Add(temp_qc); }
                }
            }
            return ds_qc;
        }
    }
}
