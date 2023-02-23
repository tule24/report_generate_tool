using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PAVE_PRIORITY
{
    public class priority
    {
        public string admin { get; set; }
        public string agent { get; set; }
        public string photo { get; set; }
        public string action { get; set; }
        public string task { get; set; }
        public string component { get; set; }
        public string damage { get; set; }
        public string location { get; set; }
        public string material { get; set; }
    }
    public class priority_temp
    {
        public string DATE { get; set; }
        public string ADMIN { get; set; }
        public string STATUS { get; set; }
        public string AGENT { get; set; }
        public string ID { get; set; }
        public string IMAGE { get; set; }
        public string TASK { get; set; }
        public string TOPIC { get; set; }
        public string CONTENT { get; set; }

    }
    class Class1
    {
        public static List<priority_temp> ds_damage(string[] temp_0, string date, string id, string report)
        {
            List<string> temp_1 = new List<string>();
            List<string> temp_2 = new List<string>();
            List<string> temp_3 = new List<string>();
            List<priority> ds_damage = new List<priority>();
            List<priority_temp> ds_priority = new List<priority_temp>();

            Regex pattern_material = new Regex(@"^GMF|^CMA|^UCB");
            bool checkMaterial = pattern_material.IsMatch(id);

            foreach (string i in temp_0)
            {
                if (i.Contains("annotated_images"))
                {
                    temp_1.Add(i);
                }
                else if (i.Contains("status:FINISHED"))
                {
                    if (i.Contains("select") && (i.Contains("photoCode:01") || i.Contains("photoCode:02") || i.Contains("photoCode:03") || i.Contains("photoCode:06") || i.Contains("photoCode:15")))
                    {
                        temp_3.Add(i);
                    } else if(i.Contains("admin_response"))
                    {
                        temp_2.Add(i);
                    }
                }   
            }

            foreach (string i in temp_2)
            {
                string[] ds_admin = i.Split(new string[] { "},{_" },StringSplitOptions.None);
                foreach (string j in ds_admin)
                {
                    if (j.Contains("admin_response") && j.Contains("damage_group"))
                    {
                        string[] damage_temp = j.Split(new string[] { "original_image:" }, StringSplitOptions.None);
                        Regex pattern_photoagent = new Regex(@"photoCode:(?<image>\d+)|username:(?<agent>[^,}]+),r");
                        Regex pattern_damage = new Regex(@"component:(?<component>[^,]+),damage_group:(?<task>[^,]+),damage_location:{(?<location>[^}]+)|damage_type:(?<damage>[^,]+)|damage_typeAgent:(?<damageAgent>[^,]+)|admin_response:(?<action>[^,]+),username:(?<admin>[^,}]+)|material:(?<material>[^,]+)");
                        Match temp_agent = pattern_photoagent.Match(j);
                        string image = temp_agent.Groups["image"].ToString();
                        string agent = temp_agent.NextMatch().Groups["agent"].ToString();
                        //admin agent action task component damage location
                        Regex pattern_check = new Regex(@",action:ACCEPT");
                        if(pattern_check.Match(j) != Match.Empty)
                        {
                            continue;
                        }
                        for (int k = 1; k < damage_temp.Length; k++)
                        {
                            priority priority = new priority();
                            if (damage_temp[k].Contains("admin_response"))
                            {
                                Match temp_damage = pattern_damage.Match(damage_temp[k]);
                                priority.component = temp_damage.Groups["component"].ToString();
                                priority.task = temp_damage.Groups["task"].ToString();
                                if (priority.task == "WHEEL") { priority.task = "PART"; }
                                priority.location = temp_damage.Groups["location"].ToString();
                                temp_damage = temp_damage.NextMatch();
                                if (!checkMaterial) {
                                    priority.material = " (" + temp_damage.Groups["material"].ToString() + ")";
                                    temp_damage = temp_damage.NextMatch();
                                }                           
                                priority.damage = temp_damage.Groups["damage"].ToString();
                                temp_damage = temp_damage.NextMatch();
                                if (temp_damage.ToString().Contains("damage_typeAgent"))
                                {
                                    if(temp_damage.Groups["damageAgent"].ToString() != priority.damage)
                                    {
                                        priority.damage = temp_damage.Groups["damageAgent"].ToString() + " => " + priority.damage;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    temp_damage = temp_damage.NextMatch();
                                }
                                priority.action = temp_damage.Groups["action"].ToString();
                                priority.admin = temp_damage.Groups["admin"].ToString();
                                priority.photo = image;
                                priority.agent = agent;
                                priority.component += priority.material;
                                ds_damage.Add(priority);
                            }
                        }
                    }
                    else{
                        if (j.Contains("ext_id"))
                        {
                            ext_color(ref ds_priority, j, date, id, report);
                        }
                        if (j.Contains("name:trim"))
                        {
                            trim_id(ref ds_priority, j, date, id, report);
                        }
                    }
                }
            }

            foreach (string i in temp_1)
            {
                if (i.Contains("admin_response"))
                {
                    Regex pattern_stt = new Regex(@"damage_location:{(?<location>[^}]+)");
                    int count = 1;
                    foreach (Match k in pattern_stt.Matches(i))
                    {
                        foreach (priority j in ds_damage)
                        {
                            if (k.Groups["location"].ToString() == j.location)
                            {
                                j.damage = "(" + count.ToString() + ")" + j.damage;
                                break;
                            }
                        }
                        count++;
                    }
                }
            }

            foreach (priority i in ds_damage)
            {
                priority_temp temp_prior = new priority_temp();
                bool flag = true;
                for (int j = 0; j < ds_priority.Count; j++)
                {                
                    if (ds_priority[j].AGENT == i.agent &&
                        ds_priority[j].IMAGE == i.photo &&
                        ds_priority[j].TOPIC.Contains(i.action)
                        )
                    //&& ds_priority[j].TASK.Contains(i.task) 
                    {
                        flag = false;
                        if (!ds_priority[j].ADMIN.Contains(i.admin))
                        {
                            ds_priority[j].ADMIN = ds_priority[j].ADMIN + " &\n" + i.admin;
                        }
                        if (ds_priority[j].CONTENT.Contains(i.component))
                            ds_priority[j].CONTENT += "\n   " + i.damage;
                        else
                            ds_priority[j].CONTENT += "\n" + i.component + "\n   " + i.damage;
                        j = ds_priority.Count;
                    }
                }
                if (flag)
                {
                    temp_prior.DATE = date;
                    temp_prior.ADMIN = i.admin;
                    temp_prior.AGENT = i.agent;
                    temp_prior.ID = id;
                    temp_prior.TASK = (i.task != "GLASS") ? "DAMAGE_FULL" : "DAMAGE_GLASS";
                    temp_prior.TOPIC = "Admin " + i.action;
                    temp_prior.CONTENT = i.component + "\n   " + i.damage;
                    temp_prior.IMAGE = i.photo;
                    temp_prior.STATUS = "To Be Reviewed";
                    ds_priority.Add(temp_prior);
                }
            }

            for (int i = 0; i < ds_priority.Count - 1; i++)
            {
                if (ds_priority[i].AGENT == ds_priority[i + 1].AGENT &&
                   ds_priority[i].IMAGE == ds_priority[i + 1].IMAGE)
                // && ds_priority[i].TASK == ds_priority[i + 1].TASK
                {
                    ds_priority[i].CONTENT += "\n\n" + ds_priority[i + 1].TOPIC + "\n" + ds_priority[i + 1].CONTENT;
                    ds_priority.RemoveAt(i + 1);
                }
            }

            foreach (string i in temp_3)
            {
                if (i.Contains("dataAdmin"))
                {
                    string[] temp = i.Split(new string[] { "inspection:" }, StringSplitOptions.None);
                    foreach (string j in temp)
                    {
                        if (j.Contains("dataAdmin") && !j.Contains("action:ACCEPT") && j.Contains("action:EDIT"))
                        {
                            if (j.Contains("photoCode:01"))
                            {
                                ext_color(ref ds_priority, j, date, id, report);
                            }              
                            if (j.Contains("photoCode:02"))
                            {
                                ext_color(ref ds_priority, j, date, id, report);
                                status_trans(ref ds_priority, j, date, id, report);
                            }
                            if (j.Contains("photoCode:03"))
                            {
                                odo(ref ds_priority, j, date, id);
                            }
                        }
                    }
                }
                else if (i.Contains("admin_response") && i.Contains("select")) {
                    string photo = i.Contains("photoCode:06") ? "06" : "15";
                    tire_status(ref ds_priority, i, date, id, photo);
                }
            }
            return ds_priority;
        }

        public static void ext_color(ref List<priority_temp> ds_priority, string ext, string date, string id, string report)
        {
            Regex pattern_ext_photo = new Regex(@"photoCode:(?<photo>\d+)");
            Regex pattern_ext_option = new Regex(@"ext_id:(?<color_id>\d+)");
            Regex pattern_ext_ds = new Regex(@"id:(?<color_id>\d+),color_name:(?<color>[^,]+),([^,]+,){0,1}color_code:(?<color_code>\w+)");
            Regex pattern_check = new Regex(@"inspectData:(?<check>[^,]{0,}),action");
            Match temp_check = pattern_check.Match(ext);
            Match temp_option = pattern_ext_option.Match(ext);
            Match temp_ds = pattern_ext_ds.Match(report);
            Regex pt_agent_name = new Regex(@"username:(?<name>[^,}]+),r");
            Regex pt_admin_name = new Regex(@"username:(?<name>[^,}]+)}}");
            priority_temp element = new priority_temp();

            element.IMAGE = pattern_ext_photo.Match(ext).Groups["photo"].ToString();
            element.AGENT = pt_agent_name.Match(ext).Groups["name"].ToString();
            element.ADMIN = pt_admin_name.Match(ext).Groups["name"].ToString();
            if (temp_option.ToString() != temp_option.NextMatch().ToString())
            {
                string admin = "";
                string agent = "";
                if (temp_option.NextMatch() == Match.Empty)
                {
                    if (temp_check.Groups["check"].ToString() == "") 
                    { agent = temp_option.Groups["color_id"].ToString(); }
                    else { admin = temp_option.Groups["color_id"].ToString(); }
                } else
                {
                    agent = temp_option.Groups["color_id"].ToString();
                    temp_option = temp_option.NextMatch();
                    admin = temp_option.Groups["color_id"].ToString();
                }
                
                while (temp_ds != Match.Empty)
                {
                    if (temp_ds.Groups["color_id"].ToString() == agent) { agent = temp_ds.Groups["color"].ToString() + "(" + temp_ds.Groups["color_code"] + ")"; }
                    if (temp_ds.Groups["color_id"].ToString() == admin) { admin = temp_ds.Groups["color"].ToString() + "(" + temp_ds.Groups["color_code"] + ")"; }
                    temp_ds = temp_ds.NextMatch();
                }
                if (agent == "") { agent = "null"; }
                if (admin == "") { admin = "null"; }
                element.CONTENT = agent + " => " + admin;
                element.DATE = date;
                element.ID = id;
                if (element.IMAGE == "02") { element.TASK = "INT_ID"; } else { element.TASK = "EXT_ID"; }
                element.TOPIC = "Wrong_Color";
                element.STATUS = "To Be Reviewed";
                ds_priority.Add(element);
            }
        }

        public static void tire_status(ref List<priority_temp> ds_priority, string tire, string date, string id, string photo)
        {
            priority_temp element = new priority_temp();
            Regex pt_agent_name = new Regex(@"username:(?<name>[^,}]+),r");
            Regex pt_admin_name = new Regex(@"username:(?<name>[^,}]+)}}");
            Regex pattern_tire = new Regex(@"damage_name:(?<status>[^,}]+)|admin_response:(?<res>\w+)");
            Match temp_agent = pt_agent_name.Match(tire);
            Match temp_admin = pt_admin_name.Match(tire);
            Match temp_tire = pattern_tire.Match(tire);

            while(temp_tire != Match.Empty)
            {
                if(temp_tire.NextMatch().Groups["res"].ToString() == "add")
                {
                    element.CONTENT = "It's " + temp_tire.Groups["status"].ToString();
                    break;
                }
                temp_tire = temp_tire.NextMatch().NextMatch();
            }

            element.AGENT = temp_agent.Groups["name"].ToString();
            element.ADMIN = temp_admin.Groups["name"].ToString();
            element.IMAGE = photo;
            element.DATE = date;
            element.ID = id;
            element.TOPIC = "Wrong_Rating_Used";
            element.STATUS = "To Be Reviewed";
            element.TASK = "TIRE_STATUS";
            ds_priority.Add(element);
        }

        public static void status_trans(ref List<priority_temp> ds_priority, string trans, string date, string id, string report)
        {
            priority_temp element = new priority_temp();
            Regex pattern_agent = new Regex(@"agentInspect:{int_id:(?<int_agent>\w+),int_status:(?<status_agent>\w+),trans_agent:(?<trans_agent>\w+)}");
            Regex pattern_admin = new Regex(@"inspectData:{int_id:(?<int_admin>\w+),int_status:(?<status_admin>\w+),trans_agent:(?<trans_admin>\w+)},action:EDIT}]");
                 
            Regex pt_agent_name = new Regex(@"username:(?<name>[^,}]+),r");
            Regex pt_admin_name = new Regex(@"username:(?<name>[^,}]+)}}");
            Match temp_agent = pattern_agent.Match(trans);
            Match temp_admin = pattern_admin.Match(trans);

            string agent = pt_agent_name.Match(trans).Groups["name"].ToString();
            string int_agent = temp_agent.Groups["int_agent"].ToString();
            string trans_agent = temp_agent.Groups["trans_agent"].ToString();
            string status_agent = temp_agent.Groups["status_agent"].ToString();

            string admin = pt_admin_name.Match(trans).Groups["name"].ToString();
            string int_admin = temp_admin.Groups["int_admin"].ToString();
            string trans_admin = temp_admin.Groups["trans_admin"].ToString();
            string status_admin = temp_admin.Groups["status_admin"].ToString();

            Regex pattern_ext_ds = new Regex(@"id:(?<color_id>\d+),color_name:(?<color>[^,]+),([^,]+,){0,1}color_code:(?<color_code>\w+)");
            Match temp_ds = pattern_ext_ds.Match(report);
            string contentAgent = "";
            string contentAdmin = "";
            if (int_agent != int_admin)
            {
                while (temp_ds != Match.Empty)
                {
                    if (temp_ds.Groups["color_id"].ToString() == int_agent) { int_agent = temp_ds.Groups["color"].ToString() + "(" + temp_ds.Groups["color_code"] + ")"; }
                    if (temp_ds.Groups["color_id"].ToString() == int_admin) { int_admin = temp_ds.Groups["color"].ToString() + "(" + temp_ds.Groups["color_code"] + ")"; }
                    temp_ds = temp_ds.NextMatch();
                }
                contentAgent += int_agent;
                contentAdmin += int_admin;
            }
            element.IMAGE = "02";
            element.DATE = date;
            element.ID = id;
            element.TOPIC = "Wrong_Rating\nUsed";
            element.STATUS = "To Be Reviewed";
            element.TASK = "INTERIOR";
            element.AGENT = agent;
            element.ADMIN = admin;
            if (status_agent != status_admin)
            {
                contentAgent += "  " + status_agent;
                contentAdmin += "  " + status_admin;
            }
            if (trans_agent != trans_admin)
            {
                contentAgent += "  " + trans_agent;
                contentAdmin += "  " + trans_admin;
            }
            if(contentAdmin != contentAgent)
            {
                element.CONTENT = contentAgent + " => " + contentAdmin;
                ds_priority.Add(element);
            } 
        }

        public static void trim_id(ref List<priority_temp> ds_priority, string trim, string date, string id, string report)
        {
            Regex pattern_trim_ds = new Regex(@"id:(?<id>\d+),([^,]+,){2}drivetrain:(?<drive>[^,]+),transmission:(?<trans>[^,]+),engine_type:(?<engine>[^,]+),([^,]+,){2}name:(?<name>[^,]+)");
            Regex pattern_trim_ds_kbb = new Regex(@"id:(?<id>\d+),([^,]+,){6}name:(?<name>[^,]+),([^,]+,){2}drivetrain:(?<drive>[^,]+),engine:(?<engine>[^,]+)");

            Regex pattern_trim_name = new Regex(@"photoCode:(?<photo>\d+)|username:(?<name>[^,}]+)");
            Regex pattern_trim_option = new Regex(@"agentInspect:{trim_id:(?<id>[^,}]+){0,}|inspectData:{trim_id:(?<id>[^,}]+)");

            Match term_name = pattern_trim_name.Match(trim);
            Match term_option = pattern_trim_option.Match(trim);
            priority_temp element = new priority_temp();

            element.IMAGE = term_name.Groups["photo"].ToString();
            term_name = term_name.NextMatch();
            element.AGENT = term_name.Groups["name"].ToString();
            term_name = term_name.NextMatch();
            element.ADMIN = term_name.Groups["name"].ToString();

            if (term_option.Groups["id"].ToString() != term_option.NextMatch().Groups["id"].ToString())
            {
                string agent = term_option.Groups["id"].ToString();
                string admin = term_option.NextMatch().Groups["id"].ToString();
                Match k = (trim.Contains("kbb")) ? pattern_trim_ds_kbb.Match(report) : pattern_trim_ds.Match(report);

                while (k != Match.Empty)
                {
                    if (k.Groups["id"].ToString() == agent)
                    {
                        agent = k.Groups["name"].ToString() + " " + k.Groups["drive"].ToString() + " " + k.Groups["trans"].ToString() + " " + k.Groups["engine"].ToString() + " => ";
                    }
                    if (k.Groups["id"].ToString() == admin)
                    {
                        admin = k.Groups["name"].ToString() + " " + k.Groups["drive"].ToString() + " " + k.Groups["trans"].ToString() + " " + k.Groups["engine"].ToString();
                    }
                    k = k.NextMatch();
                }
                if (agent == "") { agent = "null => "; }
                if (admin == "") { admin = "null"; }
                element.CONTENT = agent + admin;
                element.DATE = date;
                element.ID = id;
                element.TOPIC = "Wrong_Information\nEntered";
                element.STATUS = "To Be Reviewed";
                element.TASK = "TRIM_ID";
                ds_priority.Add(element);

            }
        }

        public static void odo(ref List<priority_temp> ds_priority, string odo, string date, string id)
        {
            priority_temp element = new priority_temp();
            Regex pattern_name = new Regex(@"username:(?<agent>[^,}]+)");
            Regex pattern_odo = new Regex(@"odom_reading:(?<reading>[^,}]+),odom_unit:(?<unit>[^,}]+)");
            Match temp_name = pattern_name.Match(odo);
            Match temp_odo = pattern_odo.Match(odo);

            element.AGENT = temp_name.Groups["agent"].ToString();
            element.ADMIN = temp_name.NextMatch().Groups["agent"].ToString();

            string agent_reading = temp_odo.Groups["reading"].ToString();
            string agent_unit = temp_odo.Groups["unit"].ToString();

            string contentAgent = "";
            string contentAdmin = "";
            element.IMAGE = "03";
            element.DATE = date;
            element.ID = id;
            element.TOPIC = "Wrong_Information_Entered";
            element.STATUS = "To Be Reviewed";
            element.TASK = "ODOM VIEW";
            temp_odo = temp_odo.NextMatch();
            if (temp_odo != Match.Empty)
            {
                string admin_reading = temp_odo.Groups["reading"].ToString();
                string admin_unit = temp_odo.Groups["unit"].ToString();

                if (agent_reading != admin_reading)
                {
                    contentAgent += agent_reading;
                    contentAdmin += admin_reading;
                }

                if (agent_unit != admin_unit)
                {
                    contentAgent += " " + agent_unit;
                    contentAdmin += " " + admin_unit;
                }
            }
            element.CONTENT = contentAgent + " => " + contentAdmin;
            ds_priority.Add(element);
        }

    }
}
