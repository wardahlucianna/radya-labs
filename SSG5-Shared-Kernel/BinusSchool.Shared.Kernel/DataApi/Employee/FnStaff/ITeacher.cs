using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.Teacher;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface ITeacher : IFnStaff
    {
        [Get("/employee/teacher-information/{id}")]
        Task<ApiErrorResult<GetTeacherDetailResult>> GetTeacherDetail(string id);

        [Put("/employee/teacher-update-expatriate-formalities")]
        Task<ApiErrorResult> UpdateExpatriateFormalities([Body] UpdateExpatriateFormalitiesRequest body);
    }
}
