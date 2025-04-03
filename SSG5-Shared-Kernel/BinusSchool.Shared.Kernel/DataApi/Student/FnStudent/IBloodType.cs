using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.BloodType;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IBloodType : IFnStudent
    {
        [Get("/student/blood-type")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetBloodTypes(CollectionRequest query);
    }
}
