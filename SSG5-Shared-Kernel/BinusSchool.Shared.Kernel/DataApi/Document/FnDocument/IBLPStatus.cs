using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BLPStatus;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IBLPStatus : IFnDocument
    {
        [Get("/blp/blp-status")]
        Task<ApiErrorResult<IEnumerable<GetBLPStatusResult>>> GetBLPStatus(GetBLPStatusRequest body);
    }
}
