using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.Department;
using Refit;

namespace BinusSchool.Data.Api.School.FnSubject
{
    public interface IDepartment : IFnSubject
    {
        [Get("/school/department")]
        Task<ApiErrorResult<IEnumerable<GetDepartmentResult>>> GetDepartments(GetDepartmentRequest query);

        [Get("/school/department/{id}")]
        Task<ApiErrorResult<GetDepartmenDetailResult>> GetDepartmentDetail(string id);

        [Post("/school/department")]
        Task<ApiErrorResult> AddDepartment([Body] AddDepartmentRequest body);

        [Put("/school/department")]
        Task<ApiErrorResult> UpdateDepartment([Body] UpdateDepartmentRequest body);

        [Delete("/school/department")]
        Task<ApiErrorResult> DeleteDepartment([Body] IEnumerable<string> ids);

        [Post("/school/department/copy")]
        Task<ApiErrorResult> CopyDepartment([Body] CopyDepartmentRequest body);
    }
}
