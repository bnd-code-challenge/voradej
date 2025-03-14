using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountModel
{
    public class DataResponse<T>
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Result { get; set; }

        public DataResponse()
        {

            StatusCode = (int)HttpStatusCode.OK;
            Message = "OK";
        }

        public DataResponse(T? result)
        {
            StatusCode = (int)HttpStatusCode.OK;
            Message = "OK";
            Result = result;
        }

        public DataResponse(int code, string message, T? result)
        {
            StatusCode = code;
            Message = message;
            Result = result;
        }
    }
}
