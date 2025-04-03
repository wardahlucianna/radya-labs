using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.HandbookManagement;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IHandbookManagement : IStudent
    {
        [Get("/student/handbook-management")]
        Task<ApiErrorResult<IEnumerable<GetHandbookManagementResult>>> GetListHandbookManagement(GetHandbookManagementRequest query);

        [Get("/student/handbook-management/detail/{id}")]
        Task<ApiErrorResult<GetHandbookManagementDetailResult>> GetListHandbookManagementDetail(string id);

        [Post("/student/handbook-management")]
        Task<ApiErrorResult> AddHandbookManagement([Body] AddHandbookManagementRequest body);

        [Put("/student/handbook-management")]
        Task<ApiErrorResult> UpdateHandbookManagement([Body] UpdateHandbookManagementRequest body);

        [Delete("/student/handbook-management")]
        Task<ApiErrorResult> DeleteHandbookManagement([Body] IEnumerable<string> ids);
    }
}
