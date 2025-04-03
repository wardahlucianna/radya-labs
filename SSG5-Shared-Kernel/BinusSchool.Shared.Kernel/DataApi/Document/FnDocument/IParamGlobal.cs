using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.ParamGlobal;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IParamGlobal : IFnDocument
    {
        [Get("/document/param-global")]
        Task<ApiErrorResult<IEnumerable<SelectManualResult>>> Get(SelectManualRequest param);
    }
}
