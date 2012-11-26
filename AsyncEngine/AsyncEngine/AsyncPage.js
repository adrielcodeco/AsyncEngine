//http://www.phpied.com/3-ways-to-define-a-javascript-class/
/// <reference path="jquery-1_7_1.js" />
/// <reference path="jquery_json-2_3.js" />

IsNullOrEmpty = function (value) {
    return value == null || value == '';
};

var Functions = { EACallMethod: 0 };

var ParameterTransfer = function () {
    Function = Functions.EACallMethod;
    Method = "";
    Args = {};
};

var AsyncPage = new function () {
    var calls = new Array();
    var sleep = false;
    var _completed = function (args) { };

    this.Call = function (args, context, completed) {
        if (!sleep) {
            _completed = completed;
            CallServer($.toJSON(args), context);
        }
        else
            calls.push({ a: args, ctx: context, comp: completed });
    };

    var callNext = function (args) {
        var _args = $.evalJSON(args);
        _completed(_args);
        if (calls.length == 0)
            sleep = false;
        else {
            _completed = calls[0].comp;
            CallServer($.toJSON(calls[0].a), calls[0].ctx);
            calls.reverse();
            calls.pop();
            calls.reverse();
        }
    };

    this.ServerResponse = function (args, context) {
        if (IsNullOrEmpty(args)) {
            callNext();
            return;
        }

        callNext(args);
    };
};