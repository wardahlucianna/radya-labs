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


namespace BinusSchool.Util.FnConverter.Crypt
{
    public class RijndaelHandler : FunctionsHttpSingleHandler
    {
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }
        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<RijndaelRequest>(nameof(RijndaelRequest.Type), nameof(RijndaelRequest.Name), nameof(RijndaelRequest.Key));

            RijndaelResult ReturnResult = new RijndaelResult();
            if (param.Type.ToLower() == "decrypt")
            {
                ReturnResult.msg = EncryptStringUtil.Decrypt(param.Name, param.Key);
            }
            else
            {
                ReturnResult.msg = EncryptStringUtil.Encrypt(param.Name, param.Key);
            }

            return new JsonResult(ReturnResult, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));
        }
    }
}
