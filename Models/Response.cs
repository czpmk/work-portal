using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public enum ErrorCode
    {
        SUCCESS = 0,
        INVALID_LOGIN_OR_PASSWORD = 1,
    }

    public class Response
    {
        //if no error code is provided then it's success
        Response(Object _data)
        {
            this.ErrorCode = ErrorCode.SUCCESS;
            this.Data = _data;
        }

        Response(ErrorCode _ec, Object _data)
        {
            this.ErrorCode = _ec;
            this.Data = _data;
        }

        public ErrorCode ErrorCode { get; set; }

        public Object Data { get; set; }
    }
}
