using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AsyncEngine.Test
{
    public partial class Default : AsyncPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public MyData GetData()
        {
            return new MyData()
            {
                Name = "Test",
                IsEnabled = true,
                arrarString = new string[] { "aItem 1", "aItem 2" },
                listString = new List<string>() { "lItem 1", "lItem 2" }
            };
        }
    }

    public class MyData
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public string[] arrarString { get; set; }
        public List<string> listString { get; set; }
    }
}