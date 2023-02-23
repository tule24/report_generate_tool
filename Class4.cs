using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PAVE_PRIORITY
{
    public class tire
    {
        public string HANDLE { get; set; }
        public string COMPLETE { get; set; }
        public string ID { get; set; } 
        public string IMAGE { get; set; }
        public string AGENT_NAME { get; set; }
        public string CONDITION_RP { get; set; }
        public string INSPECT { get; set; }
    }
    class Class4
    {
        public static tire ds_tire(string data, string[] input)
        {

            Regex pattern_id = new Regex(@"sessionID:(?<id>[^,]+)");
            Regex pattern_handle = new Regex(@"username:(?<handle>[^,]+),action:HANDLE");
            Regex pattern_complete = new Regex(@"username:(?<complete>[^,]+),action:COMPLETE-INSPECTION");
            Regex pattern_img = new Regex(@"image_cap:(?<image>[^,]+)");
            Regex pattern_name_admin = new Regex(@"username:(?<name>[^,]+)");
            Regex pattern_name_agent = new Regex(@"username:(?<name>[^,]+),e");
            Regex pattern_tire = new Regex(@"damage_group:TIRE,damage_name:(?<tire>[^,]+)|tire_status:(?<tire>[^,]+)");

            tire element = new tire();
            element.ID = pattern_id.Match(data).Groups["id"].ToString();
            element.HANDLE = pattern_handle.Match(data).Groups["handle"].ToString();
            element.COMPLETE = pattern_complete.Match(data).Groups["complete"].ToString();
            foreach (string j in input)
            {
                if (j.Contains("photoCode:06,status:FINISHED,type:QC"))
                {
                    Match img = pattern_img.Match(j);
                    Match name_agent = pattern_name_agent.Match(j);
                    Match cr = pattern_tire.Match(j);
                    //if (img.Groups["image"].ToString().Contains("06"))
                    //{
                        element.IMAGE = img.Groups["image"].ToString();
                        element.AGENT_NAME = name_agent.Groups["name"].ToString();
                        element.CONDITION_RP = cr.Groups["tire"].ToString();
                        if (j.Contains("adminInspect"))
                        {
                            element.INSPECT = pattern_name_admin.Match(j).NextMatch().Groups["name"].ToString();
                        }
                  //  }
                }
            }
            return element;
        }
    }
}
