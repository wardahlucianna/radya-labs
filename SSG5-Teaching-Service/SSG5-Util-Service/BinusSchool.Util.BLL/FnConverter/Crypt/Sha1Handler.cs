using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Data.Model.Util.FnConverter.Crypt;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace BinusSchool.Util.FnConverter.Crypt
{
    public class Sha1Handler : FunctionsHttpSingleHandler
    {
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<Sha1Request>(nameof(Sha1Request.Name));

            RijndaelResult ReturnResult = new RijndaelResult();

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(param.Name));
                ReturnResult.msg = BitConverter.ToString(hash).Replace("-", "");
            }

            return new JsonResult(ReturnResult, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));
        }

    }
}
