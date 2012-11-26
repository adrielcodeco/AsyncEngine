<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AsyncEngine.Test.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script>
        window.onload = function () {
            var pt = new ParameterTransfer();
            pt.Function = "CallMethod";
            pt.Method = "GetData";
            pt.Args = [{ a: 1, b: 2 }, { a: 3, b: 4}];
            AsyncPage.Call(pt, 1, (function (args) {
                alert(args);
            }));
        };
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    </div>
    </form>
</body>
</html>
