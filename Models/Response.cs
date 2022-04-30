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
        ACCESS_DENIED = 465,
        // action not allowed in a context, e.g. adding user to private chat, returns operation
        OPERATION_NOT_ALLOWED = 466
    }

    public static class WPResponse
    {
        public static IActionResult Custom(object result, ReturnCode returnCode = ReturnCode.SUCCESS)
        {
            var objRes = new ObjectResult(new Dictionary<string, object> {
                { "reason", ReturnCodeToString(returnCode) },
                { "result", result },
            });
            objRes.StatusCode = (int)returnCode;
            return objRes;
        }

        public static IActionResult Custom(object key, object value, ReturnCode returnCode = ReturnCode.SUCCESS)
        {
            var objRes = new ObjectResult(new Dictionary<object, object> {
                { "reason", ReturnCodeToString(returnCode) },
                { "result", null },
                { key, value }
            });
            objRes.StatusCode = (int)returnCode;
            return objRes;
        }

        public static IActionResult Custom(ReturnCode returnCode = ReturnCode.SUCCESS)
        {
            return Custom(null, returnCode);
        }

        public static IActionResult InternalError()
        {
            return Custom(ReturnCode.INTERNAL_ERROR);
        }

        public static IActionResult ArgumentInvalid(string argumentName)
        {
            return Custom("argument_name", argumentName, ReturnCode.INVALID_ARGUMENT);
        }

        public static IActionResult AccessDenied(string itemName)
        {
            return Custom("item_name", itemName, ReturnCode.ACCESS_DENIED);
        }

        public static IActionResult AuthenticationInvalid()
        {
            return Custom(ReturnCode.AUTHENTICATION_INVALID);
        }

        public static IActionResult ArgumentAlreadyExists(string argumentName)
        {
            return Custom("argument_name", argumentName, ReturnCode.ARGUMENT_ALREADY_EXISTS);
        }

        public static IActionResult ArgumentDoesNotExist(string argumentName)
        {
            return Custom("argument_name", argumentName, ReturnCode.ARGUMENT_DOES_NOT_EXIST);
        }

        public static IActionResult OperationNotAllowed(string invalidOpertaion)
        {
            return Custom("invalid_operation", invalidOpertaion, ReturnCode.OPERATION_NOT_ALLOWED);
        }

        private static string ReturnCodeToString(ReturnCode returnCode)
        {
            return Enum.GetName(typeof(ReturnCode), returnCode);
        }
    }
}
