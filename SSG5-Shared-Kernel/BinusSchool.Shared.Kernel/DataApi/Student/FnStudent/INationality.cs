using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Nationality;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface INationality : IFnStudent
    {
        [Get("/student/nationality")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> Getnationalities(CollectionRequest query);
    }
}