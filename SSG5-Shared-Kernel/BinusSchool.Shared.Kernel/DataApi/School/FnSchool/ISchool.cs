using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.School;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ISchool : IFnSchool
    {
        [Get("/school/sch")]
        Task<ApiErrorResult<IEnumerable<GetSchoolResult>>> GetSchools(CollectionRequest param);

        [Get("/school/sch/{id}")]
        Task<ApiErrorResult<GetSchoolDetailResult>> GetSchoolDetail(string id);
    }
}
