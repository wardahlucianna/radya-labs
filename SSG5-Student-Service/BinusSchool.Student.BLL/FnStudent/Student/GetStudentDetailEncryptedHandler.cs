using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Api.Student.FnStudent;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentDetailEncryptedHandler : FunctionsHttpSingleHandler
    {
        private readonly string _codeCrypt = "j8S^cS/";
        private readonly IStudent _studentApi;

        public GetStudentDetailEncryptedHandler(
            IStudent studentApi
        )
        {
            _studentApi =  studentApi;
        }


        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetStudentDetailRequest>(nameof(GetStudentDetailRequest.IdStudent));

            var split = SplitIdDatetimeUtil.Split(EncryptStringUtil.Decrypt(HttpUtility.UrlDecode(param.IdStudent), _codeCrypt));

            if (!split.IsValid)
            {
                throw new BadRequestException(split.ErrorMessage);
            }

            var StudentId = split.Id;
            if (StudentId.Where(char.IsDigit).ToArray().Length != 10)
            {
                throw new BadRequestException("Student ID incorrect format");
            }


            var retVal = await _studentApi.GetStudentDetail(StudentId);

            return Request.CreateApiResult2(retVal.Payload as object);
        }

    }
}
