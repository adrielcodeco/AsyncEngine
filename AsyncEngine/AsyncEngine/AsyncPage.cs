using System.Web;
using System.Web.UI;
using System;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using AsyncEngine.Dynamic;

//http://code.google.com/p/jquery-json/downloads/list

[assembly: System.Web.UI.WebResource("AsyncEngine.jquery-1_7_2_min.js", "text/javascript", PerformSubstitution = true)]
[assembly: System.Web.UI.WebResource("AsyncEngine.jquery_json-2_3_min.js", "text/javascript", PerformSubstitution = true)]
[assembly: System.Web.UI.WebResource("AsyncEngine.AsyncPage.js", "text/javascript", PerformSubstitution = true)]

namespace AsyncEngine
{
    public class AsyncPage : Page, ICallbackEventHandler
    {
        bool RegisterScripts;

        public AsyncPage()
            : base()
        {
            RegisterScripts = true;
        }

        public AsyncPage(bool registerScripts)
            : this()
        {
            RegisterScripts = registerScripts;
        }

        protected override void OnInit(System.EventArgs e)
        {
            if (RegisterScripts)
            {
                RegisterScriptFromResource<AsyncPage>("jquery", "AsyncEngine.jquery-1_7_2_min.js");
                RegisterScriptFromResource<AsyncPage>("jquery.json", "AsyncEngine.jquery_json-2_3_min.js");
                RegisterScriptFromResource<AsyncPage>("AsyncPage", "AsyncEngine.AsyncPage.js");
            }
            Response.CacheControl = "no-cache";
            Response.AddHeader("Pragma", "no-store");
            Response.Expires = -1;

            base.OnInit(e);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            if (!base.IsPostBack && !base.IsCallback)
            {
                string cbReference = CurrentClientScript.GetCallbackEventReference(this, "args", "AsyncPage.ServerResponse", "context");
                string callbackScript = "function CallServer(args, context) {" + cbReference + ";}";
                RegisterScriptBlock<AsyncPage>("CallServer", callbackScript);
            }

            base.OnLoad(e);
        }

        public static Page CurrentPage
        {
            get { return (HttpContext.Current.CurrentHandler as Page); }
        }

        public static ClientScriptManager CurrentClientScript
        {
            get { return CurrentPage.ClientScript; }
        }

        public static void RegisterScriptFromResource<T>(string key, string name)
        {
            CurrentClientScript.RegisterClientScriptInclude(key, CurrentClientScript.GetWebResourceUrl(typeof(T), name));
        }

        public static void RegisterScriptBlock<T>(string key, string script)
        {
            CurrentClientScript.RegisterClientScriptBlock(typeof(T), key, script, true);
        }

        #region [ ICallbackEventHandler ]

        protected string result = string.Empty;

        public const string EACallMethod = "CallMethod";

        private dynamic Deserialize(string arg)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Dictionary<string, object> dic = jss.DeserializeObject(arg) as Dictionary<string, object>;
            return DeserializeDictionary(dic);
        }

        private object DeserializeDictionary(Dictionary<string, object> dic)
        {
            List<DynamicProperty> properties = new List<DynamicProperty>();

            foreach (KeyValuePair<string, object> kvp in dic)
            {
                double d = 0d;
                Guid g = Guid.Empty;
                if (kvp.Value is object[] ||
                    kvp.Value is Dictionary<string, object>)
                    properties.Add(new DynamicProperty(kvp.Key, typeof(object)));
                else if (double.TryParse(kvp.Value.ToString(), out d))
                    properties.Add(new DynamicProperty(kvp.Key, typeof(double)));
                else if (Guid.TryParse(kvp.Value.ToString(), out g))
                    properties.Add(new DynamicProperty(kvp.Key, typeof(Guid)));
                else
                    properties.Add(new DynamicProperty(kvp.Key, typeof(string)));
            }

            Type type = DynamicExpression.CreateClass(properties.ToArray());
            object obj = type.GetConstructor(new Type[] { }).Invoke(new object[] { });

            foreach (KeyValuePair<string, object> kvp in dic)
            {
                var v = kvp.Value;

                if (kvp.Value is object[])
                    v = DeserializeArray(kvp.Value as object[]);

                if (kvp.Value is Dictionary<string, object>)
                    v = DeserializeDictionary(kvp.Value as Dictionary<string, object>);

                double d = 0d;
                Guid g = Guid.Empty;

                if (double.TryParse(kvp.Value.ToString(), out d))
                    v = d;

                if (Guid.TryParse(kvp.Value.ToString(), out g))
                    v = g;

                type.GetProperty(kvp.Key)
                               .GetSetMethod()
                                   .Invoke(obj, new object[] { v });
            }

            return obj;
        }

        private object[] DeserializeArray(object[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                object v = array[i];
                double d = 0d;
                Guid g = Guid.Empty;
                if (v is object[])
                    array[i] = DeserializeArray(v as object[]);
                else if (v is Dictionary<string, object>)
                    array[i] = DeserializeDictionary(v as Dictionary<string, object>);
                else if (double.TryParse(v.ToString(), out d))
                    array[i] = d;
                else if (Guid.TryParse(v.ToString(), out g))
                    array[i] = g;
            }

            return array;
        }

        private string Serialize(object obj)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(obj);
        }

        public virtual string GetCallbackResult()
        {
            return result;
        }

        public virtual void RaiseCallbackEvent(string eventArgument)
        {
            try
            {
                dynamic pt = Deserialize(eventArgument);

                if (pt.Function == AsyncPage.EACallMethod)
                    result = Serialize(this.CallMethod<AsyncPage>(pt.Method as string, pt.Args as object[]));
            }
            catch { }
        }

        #endregion [ ICallbackEventHandler ]
    }
}
