using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ParentRelationship;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IParentRelationship : IFnStudent
    {
        [Get("/student/parent-relationship")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetParentRelationship(CollectionRequest query);
    }
}
