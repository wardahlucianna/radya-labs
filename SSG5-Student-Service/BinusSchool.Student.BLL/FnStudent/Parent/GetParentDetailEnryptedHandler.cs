using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Data.Api.Student.FnStudent;

namespace BinusSchool.Student.FnStudent.Parent
{
    public class GetParentDetailEnryptedHandler : FunctionsHttpSingleHandler
    {
        private readonly IParent _parentApi;
        private readonly string _codeCrypt = "j8S^cS/";
        public GetParentDetailEnryptedHandler(IParent parentApi)
        {
            _parentApi = parentApi;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetParentDetailRequest>(nameof(GetParentDetailRequest.IdParent));

            var split = SplitIdDatetimeUtil.Split(EncryptStringUtil.Decrypt(HttpUtility.UrlDecode(param.IdParent), _codeCrypt));

            if (!split.IsValid)
            {
                throw new BadRequestException(split.ErrorMessage);
            }

            var ParentId = split.Id;

            var retVal = await _parentApi.GetParentDetail(ParentId);

            return Request.CreateApiResult2(retVal.Payload as object);
        }

    }
}
