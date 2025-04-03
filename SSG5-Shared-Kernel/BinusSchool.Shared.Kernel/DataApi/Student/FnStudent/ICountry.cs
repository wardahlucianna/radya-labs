using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Country;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ICountry : IFnStudent
    {
        [Get("/student/country")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetCountries(CollectionRequest query);
    }
}
