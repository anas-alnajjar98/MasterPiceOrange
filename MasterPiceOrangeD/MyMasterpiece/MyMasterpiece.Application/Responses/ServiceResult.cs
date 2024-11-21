using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMasterpiece.Application.Responses
{
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        public static ServiceResult Success(string message) => new ServiceResult { IsSuccess = true, Message = message };
        public static ServiceResult Failure(string errorMessage) => new ServiceResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
