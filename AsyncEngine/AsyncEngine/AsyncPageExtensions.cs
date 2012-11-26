using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Reflection;

namespace AsyncEngine
{
    public static class AsyncPageExtensions
    {
        public static object CallMethod<T>(this AsyncPage obj, string method, object[] args)
        {
            object result = null;

            Type type = typeof(T);
            MethodInfo mi = type.GetMethod(method);
            if (mi != null)
                result = mi.Invoke(obj, args);

            return result;
        }
    }
}
