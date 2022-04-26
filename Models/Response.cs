using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WorkPortalAPI.Models
{
    public enum ReturnCode
    {
        SUCCESS = 200,
        INTERNAL_ERROR = 500,
        // credentials or session token invalid; returns (invalid) "argument_name"
        AUTHENTICATION_INVALID = 461,
        // returns (invalid) "argument_name"
        INVALID_ARGUMENT = 462,
        ARGUMENT_ALREADY_EXISTS = 463,
        ARGUMENT_DOES_NOT_EXIST = 464,
        // user does not have access to the item; returns "item_name" to which access has been denied
        ACCESS_DENIED = 465
    }

    public static class WPResponse
    {
        public static IActionResult Create(object result, ReturnCode returnCode = ReturnCode.SUCCESS)
        {
            var objRes = new ObjectResult(new Dictionary<string, object> {
                { "reason", ReturnCodeToString(returnCode) },
                { "result", result },
            });
            objRes.StatusCode = (int)returnCode;
            return objRes;
        }

        public static IActionResult Create(object key, object value, ReturnCode returnCode = ReturnCode.SUCCESS)
        {
            var objRes = new ObjectResult(new Dictionary<object, object> {
                { "reason", ReturnCodeToString(returnCode) },
                { "result", null },
                { key, value }
            });
            objRes.StatusCode = (int)returnCode;
            return objRes;
        }

        public static IActionResult Create(ReturnCode returnCode = ReturnCode.SUCCESS)
        {
            return Create(null, returnCode);
        }

        public static IActionResult CreateArgumentInvalidResponse(string argumentName)
        {
            return Create("argument_name", argumentName, ReturnCode.INVALID_ARGUMENT);
        }

        public static IActionResult CreateAccessDeniedResponse(string itemName)
        {
            return Create("item_name", itemName, ReturnCode.ACCESS_DENIED);
        }

        public static string ReturnCodeToString(ReturnCode returnCode)
        {
            return Enum.GetName(typeof(ReturnCode), returnCode);
        }
    }
}
