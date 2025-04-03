using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ChildStatus;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IChildStatus : IFnStudent
    {
        [Get("/student/child-status")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetChildStatus(CollectionRequest query);
    }
}
