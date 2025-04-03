using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByLesson;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IStudentScoreViewByLesson : IFnScoring
    {
        [Post("/studentscore/view-subject-bylesson")]
        Task<ApiErrorResult<GetStudentScoreSubjectByLessonResult>> GetStudentScoreViewSubjectByLesson ([Body] GetStudentScoreSubjectByLessonRequest query);

        [Post("/studentscore/exportexcel-studentscoresubject-bylesson")]
        Task<HttpResponseMessage> ExportExcelStudentScoreSubjectByLesson([Body] GetStudentScoreSubjectByLessonRequest body);

    }
}
