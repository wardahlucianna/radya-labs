using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IStudentScoreViewByHomeroom : IFnScoring
    {
        [Post("/studentscore/view-byhomeroom")]
        Task<ApiErrorResult<IEnumerable<GetStudentScoreViewByHomeroomResult>>> GetStudentScoreViewByHomeroom([Body] GetStudentScoreByHomeroomRequest query);

        [Post("/studentscore/view-byhomeroom-header")]
        Task<ApiErrorResult<GetStudentScoreViewHeaderByHomeroomResult>> GetStudentScoreViewHeaderByHomeroom([Body] GetStudentScoreByHomeroomRequest query);
    
        [Get("/studentscore/view-byhomeroom-detail")]
        Task<ApiErrorResult<GetStudentScoreViewDetailByHomeroomResult>> GetStudentScoreViewDetailByHomeroom(GetStudentScoreViewDetailByHomeroomRequest query);

        [Post("/studentscore/summary-byhomeroom")]
        Task<ApiErrorResult<IEnumerable<GetStudentScoreSummaryByHomeroomResult>>> GetStudentScoreSummaryByHomeroom([Body] GetStudentScoreByHomeroomRequest query);

        [Post("/studentscore/exportexcel-studentscore-byhomeroom")]
        Task<HttpResponseMessage> ExportExcelStudentScoreSummaryByHomeroom([Body] GetStudentScoreByHomeroomRequest body);


        [Post("/studentscore/view-subject-byhomeroom")]
        Task<ApiErrorResult<GetStudentScoreViewSubjectByHomeroomResult>> GetStudentScoreViewSubjectByHomeroom([Body] GetStudentScoreSubjectByHomeroomRequest query);
        
        [Post("/studentscore/summary-subject-byhomeroom")]
        Task<ApiErrorResult<IEnumerable<GetStudentScoreSummaryByHomeroomResult>>> GetStudentScoreSummarySubjectByHomeroom([Body] GetStudentScoreSubjectByHomeroomRequest query);       

        [Post("/studentscore/exportexcel-studentscoresubject-byhomeroom")]
        Task<HttpResponseMessage> ExportExcelStudentScoreSubjectByHomeroom([Body] GetStudentScoreSubjectByHomeroomRequest body);
    }
}
