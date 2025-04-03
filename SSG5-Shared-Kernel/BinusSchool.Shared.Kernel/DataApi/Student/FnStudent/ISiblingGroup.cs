using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SiblingGroup;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ISiblingGroup : IFnStudent
    {
        [Get("/student/sibling_group")]
        Task<ApiErrorResult<IEnumerable<GetSiblingGroupResult>>> GetSiblingGroups(GetSiblingGroupRequest query);

        [Get("/student/sibling_group/{id}")]
        Task<ApiErrorResult<IEnumerable<GetSiblingGroupDetailResult>>> GetSiblingGroupDetail(string id);

        [Put("/student/sibling_group")]
        Task<ApiErrorResult> AddSiblingGroup([Body] AddSiblingGroupRequest body);

        [Delete("/student/sibling_group")]
        Task<ApiErrorResult> DeleteSiblingGroup([Body] AddSiblingGroupRequest body);
    }
}
