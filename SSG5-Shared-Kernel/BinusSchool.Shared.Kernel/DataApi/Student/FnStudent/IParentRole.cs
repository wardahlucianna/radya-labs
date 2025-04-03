using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ParentRole;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IParentRole : IFnStudent
    {
        [Get("/student/parent-role")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetParentRoles(CollectionRequest query);
    }
}
