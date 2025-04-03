using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.OccupationType;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IOccupationType : IFnStudent
    {
        [Get("/student/occupation-type")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetOccupationTypes(CollectionRequest query);
    }
}
