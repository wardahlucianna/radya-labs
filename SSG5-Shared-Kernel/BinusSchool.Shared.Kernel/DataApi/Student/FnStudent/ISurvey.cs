using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Survey;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{    
    public interface ISurvey : IFnStudent
    {
        [Get("/student/survey/GetSurveyList")]
        Task<ApiErrorResult<IEnumerable<GetSurveyResult>>> GetSurvey(GetSurveyRequest param);
    }
}
