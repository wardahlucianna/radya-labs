using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using Refit;

namespace BinusSchool.Data.Api.School.FnPeriod
{
    public interface IDay : IFnPeriod
    {
        [Get("/school/day")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetDays(CollectionSchoolRequest query);
    }
}
