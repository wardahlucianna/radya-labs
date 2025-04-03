using System;
using System.Collections.Generic;
using System.Threading.Tasks;


using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.District;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IDistrict : IFnStudent
    {
        [Get("/student/district")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetBanks(CollectionRequest query);
    }
}
