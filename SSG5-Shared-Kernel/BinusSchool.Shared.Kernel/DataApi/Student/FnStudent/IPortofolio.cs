using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IPortofolio : IFnStudent
    {

        #region Learning Goals
        [Get("/student/portfolio/learning-goals")]
        Task<ApiErrorResult<IEnumerable<GetLearningGoalsOnGoingResult>>> GetLearningGoals(GetLearningGoalsRequest query);

        [Get("/student/portfolio/learning-goals/{id}")]
        Task<ApiErrorResult<GetDetailLearningGoalsResult>> GetDetailLearningGoals(string id);

        [Post("/student/portfolio/learning-goals")]
        Task<ApiErrorResult> AddLearningGoals([Body] AddLearningGoalsRequest body);

        [Put("/student/portfolio/learning-goals")]
        Task<ApiErrorResult> UpdateLearningGoals([Body] UpdateLearningGoalsRequest body);

        [Delete("/student/portfolio/learning-goals")]
        Task<ApiErrorResult> DeleteLearningGoals([Body] IEnumerable<string> body);

        [Get("/student/portfolio/learning-goals-archieved")]
        Task<ApiErrorResult<IEnumerable<GetLearningGoalsOnGoingResult>>> GetLearningAchieved(GetLearningGoalsRequest query);

        [Post("/student/portfolio/learning-goals-archieved")]
        Task<ApiErrorResult> UpdateLearningAchieved([Body] UpdateLearningAchievedRequest body);

        [Get("/student/portfolio/learning-goals-button")]
        Task<ApiErrorResult<GetLearningGoalsButtonResult>> GetLearningGoalsButton(GetLearningGoalsButtonRequest query);
        #endregion

        #region Reflection
        [Get("/student/portfolio/reflection")]
        Task<ApiErrorResult<IEnumerable<GetReflectionResult>>> GetReflection(GetReflectionRequest query);

        [Get("/student/portfolio/reflection/{id}")]
        Task<ApiErrorResult<GetDetailReflectionResult>> GetDetailReflection(string id);

        [Post("/student/portfolio/reflection")]
        Task<ApiErrorResult> AddReflection([Body] AddReflectionRequest body);

        [Put("/student/portfolio/reflection")]
        Task<ApiErrorResult> UpdateReflection([Body] UpdateReflectionRequest body);

        [Delete("/student/portfolio/reflection")]
        Task<ApiErrorResult> DeleteReflection([Body] IEnumerable<string> body);

        [Put("/student/portfolio/reflection-content")]
        Task<ApiErrorResult> UpdateReflectionContent([Body] UpdateReflectionContentRequest body);
        #endregion

        #region Reflection Comment
        [Get("/student/portfolio/reflection-comment/{id}")]
        Task<ApiErrorResult<GetDetailReflectionCommentResult>> GetDetailReflectionComment(string id);

        [Post("/student/portfolio/reflection-comment")]
        Task<ApiErrorResult> AddReflectionComment([Body] AddReflectionCommentRequest body);

        [Put("/student/portfolio/reflection-comment")]
        Task<ApiErrorResult> UpdateReflectionComment([Body] UpdateReflectionCommentRequest body);

        [Delete("/student/portfolio/reflection-comment")]
        Task<ApiErrorResult> DeleteReflectionComment([Body] IEnumerable<string> body);
        #endregion

        #region Coursework
        [Get("/student/portfolio/coursework")]
        Task<ApiErrorResult<IEnumerable<GetListCourseworkResult>>> GetListCoursework(GetListCourseworkRequest query);

        [Get("/student/portfolio/coursework/{id}")]
        Task<ApiErrorResult<GetDetailCourseworkResult>> GetDetailCoursework(string id);

        [Post("/student/portfolio/coursework")]
        Task<ApiErrorResult> AddCoursework([Body] AddCourseworkRequest body);

        [Put("/student/portfolio/coursework")]
        Task<ApiErrorResult> UpdateCoursework([Body] UpdateCourseworkRequest body);

        [Delete("/student/portfolio/coursework")]
        Task<ApiErrorResult> DeleteCoursework([Body] IEnumerable<string> body);
        [Get("/student/portfolio/data-uoi")]
        Task<ApiErrorResult<IEnumerable<GetListUOIResult>>> GetListUOI(GetListUOIRequest query);
        #endregion

        #region Coursework Comment

        [Post("/student/portfolio/coursework-comment")]
        Task<ApiErrorResult> AddCourseworkComment([Body] AddCourseworkCommentRequest body);

        [Put("/student/portfolio/coursework-comment")]
        Task<ApiErrorResult> UpdateCourseworkComment([Body] UpdateCourseworkCommentRequest body);

        [Delete("/student/portfolio/coursework-comment")]
        Task<ApiErrorResult> DeleteCourseworkComment([Body] IEnumerable<string> body);
        #endregion

        #region Coursework Seen By

        [Post("/student/portfolio/coursework-seen-by")]
        Task<ApiErrorResult> AddCourseworkSeenBy([Body] AddCourseworkSeenByRequest body);

        #endregion
    }
}
