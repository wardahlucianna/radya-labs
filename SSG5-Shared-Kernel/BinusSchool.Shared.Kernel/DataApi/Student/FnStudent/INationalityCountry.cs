using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.NationalityCountry;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface INationalityCountry : IFnStudent
    {
        [Get("/student/nationality-country")]
        Task<ApiErrorResult<IEnumerable<GetNationalityCountryResult>>> GetNationalitiesCountry(GetNationalityCountryRequest query);
    }
}