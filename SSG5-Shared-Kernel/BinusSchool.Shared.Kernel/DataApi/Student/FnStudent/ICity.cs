using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.City;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ICity : IFnStudent
    {
        [Get("/student/city")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetCities(GetCityRequest query);
    }
}
