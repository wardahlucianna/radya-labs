using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.PreviousSchoolNew;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IPreviousSchoolNew : IFnStudent
    {
        [Get("/student/previous-school-new")]
        Task<ApiErrorResult<IEnumerable<GetPreviousSchoolNewResult>>> GetCountries(CollectionRequest query);
    }
}
