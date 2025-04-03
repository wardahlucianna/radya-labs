using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ICurrentAcademicYear : IFnSchool
    {
        [Post("/school/academicyears/GetActiveAcademicYear")]
        Task<ApiErrorResult<GetActiveAcademicYearResult>> GetActiveAcademicYear([Body] GetActiveAcademicYearRequest body);
    }
}
