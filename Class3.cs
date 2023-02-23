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
    public class damage
    {
        public string agent_name { get; set; }
        public string damage_group { get; set; }
        public string component { get; set; }
        public string damage_name { get; set; }
        public string user_response { get; set; }
        public string source { get; set; }
        public string photo_code { get; set; }
        public string cropped_code { get; set; }
    }

    public class user
    {
        public string DATE { get; set; }
        public string AGENT { get; set; }
        public string ID { get; set; }
        public string IMAGE { get; set; }
        public string TASK { get; set; }
        public string TOPIC { get; set; }
        public string CONTENT { get; set; }

    }
    class Class3
    {
        public static List<string> inDsAgent(string data) // hàm in ds tên agent
        {
            List<string> dsAgent = new List<string>();
            Regex pattern = new Regex(@"username:(?<agent>(\w+\.\w+){1,2}),email");
            Match value = pattern.Match(data);
            int i = 0;
            do
            {
                i++;
                if (i > 1)
                {
                    dsAgent.Add(value.Groups["agent"].ToString());
                }
                value = value.NextMatch();
            } while (value != Match.Empty);
            return dsAgent;
        }
        public static List<user> taoDsDamage(List<string> agent, string report, string id, string date) // hàm in ds tên damage
        {

            List<user> dsUser = new List<user>();
            Regex pattern = new Regex(@"damage_group:(?<group>\w+)|component:(?<component>\w+)|damage_name:(?<name>\w+)|user_response:(?<response>\w+)|source:(?<source>\w+)|{code:(?<photo>\d+)|cropped_code:(?<cropped>\d+|\w+)}");
            Match value = pattern.Match(report);
            value = value.NextMatch();
            int count_crop = 0;
            string photo_temp = "04";
            do
            {
                count_crop++;
                damage temp = new damage();
                user element = new user();
                temp.damage_group = value.Groups["group"].ToString();
                if (temp.damage_group == "TIRE")
                {
                    break;
                }
                value = value.NextMatch();
                temp.component = value.Groups["component"].ToString();
                value = value.NextMatch();
                temp.damage_name = value.Groups["name"].ToString();
                value = value.NextMatch();
                temp.user_response = value.Groups["response"].ToString();
                value = value.NextMatch();
                temp.source = value.Groups["source"].ToString();
                value = value.NextMatch();
                temp.photo_code = value.Groups["photo"].ToString();
                if (photo_temp != temp.photo_code) { count_crop = 1; }
                photo_temp = temp.photo_code;
                value = value.NextMatch();
                temp.cropped_code = "\n    (" + value.Groups["cropped"].ToString() + ")";
                if (temp.cropped_code.Contains("null") && temp.source.Contains("inspect")) { temp.cropped_code = "\n    (" + count_crop + ")"; }
                if (temp.source == "user" || temp.user_response == "reject")
                {
                    if (temp.photo_code == "04")
                    {
                        if (temp.component.Contains("FRONT") || temp.component.Contains("MIRROR"))
                        {
                            if (temp.damage_group == "BODY") temp.agent_name = agent[2]; else temp.agent_name = agent[0];
                        }
                        else
                        {
                            if (temp.damage_group == "BODY") temp.agent_name = agent[3]; else temp.agent_name = agent[1];
                        }
                    }
                    else if (temp.photo_code == "05")
                    {
                        if (temp.damage_group == "BODY") temp.agent_name = agent[5]; else temp.agent_name = agent[4];
                    }
                    else if (temp.photo_code == "07")
                    {
                        if (temp.component.Contains("FRONT"))
                        {
                            if (temp.damage_group == "BODY") temp.agent_name = agent[8]; else temp.agent_name = agent[6];
                        }
                        else
                        {
                            if (temp.damage_group == "BODY") temp.agent_name = agent[9]; else temp.agent_name = agent[7];
                        }
                    }
                    else
                    {
                        if (temp.damage_group == "BODY") temp.agent_name = agent[11]; else temp.agent_name = agent[10];
                    }
                    if (temp.source == "user") temp.source = "Add-Damages"; else temp.source = "Reject-Damages";
                    bool flag = true;
                    for (int i = 0; i < dsUser.Count; i++)
                    {
                        if (dsUser[i].AGENT == temp.agent_name &&
                            dsUser[i].IMAGE == temp.photo_code &&
                            dsUser[i].TASK.Contains(temp.damage_group) &&
                            dsUser[i].TOPIC == temp.source)
                        {
                            flag = false;
                            if (dsUser[i].CONTENT.Contains(temp.component))
                                dsUser[i].CONTENT = dsUser[i].CONTENT + temp.cropped_code + temp.damage_name;
                            else
                                dsUser[i].CONTENT = dsUser[i].CONTENT + "\n(" + temp.damage_group + ")" + temp.component + " " + temp.cropped_code + temp.damage_name;
                            i = dsUser.Count;
                        }
                    }
                    if (flag)
                    {
                        element = addElement(temp, date, id);
                        dsUser.Add(element);
                    }
                }
                value = value.NextMatch();
            } while (value != Match.Empty);
            return dsUser;

        }

        public static user addElement(damage input, string date, string id) // hàm add ds damage
        {
            user temp = new user();
            temp.DATE = date;
            temp.ID = id;
            temp.AGENT = input.agent_name;
            temp.IMAGE = input.photo_code;
            temp.TASK = "DAMAGE-" + input.damage_group;
            temp.CONTENT = "(" + input.damage_group + ")" + input.component + " " + input.cropped_code + input.damage_name;
            temp.TOPIC = input.source;
            return temp;
        }
    }
}
