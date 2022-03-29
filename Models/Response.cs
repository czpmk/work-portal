using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WorkPortalAPI.Models
{
    public enum ReturnCode
    {
        SUCCESS = 0,
        INVALID_LOGIN_OR_PASSWORD = 1,
        INVALID_SESSION_TOKEN = 2,
        INTERNAL_ERROR = 3,
    }

    public class Response
    {
        public Response()
        {
            this.ReturnCode = ReturnCode.SUCCESS;
            this.Data = new Object();
        }
        public Response(Object _data)
        {
            this.ReturnCode = ReturnCode.SUCCESS;
            this.Data = _data;
        }

        public Response(ReturnCode _ec, Object _data)
        {
            this.ReturnCode = _ec;
            this.Data = _data;
        }

        public ReturnCode ReturnCode { get; set; }

        public Object Data { get; set; }
    }
}
