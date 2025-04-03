using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Religion;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IReligion : IFnStudent
    {
        [Get("/student/religion")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetReligions(CollectionRequest query);
    }
}
