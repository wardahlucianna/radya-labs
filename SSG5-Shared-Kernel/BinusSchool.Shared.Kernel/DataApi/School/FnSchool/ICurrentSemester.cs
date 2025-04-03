using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Data.Model.School.FnSchool.GetActiveSemester;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ICurrentSemester : IFnSchool
    {
        [Post("/school/Semester/GetActiveSemester")]
        Task<ApiErrorResult<GetActiveSemesterResult>> GetActiveSemester([Body] GetActiveSemesterRequest body);
    }
}
