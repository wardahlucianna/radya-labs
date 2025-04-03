using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using System.Net.Http;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IAchievement : IFnStudent
    {
        #region My Achievement
        [Get("/student/achievement")]
        Task<ApiErrorResult<IEnumerable<GetAchievementResult>>> GetAchievement(GetAchievementRequest query);

        [Post("/student/achievement")]
        Task<ApiErrorResult> AddAchievement([Body]AddAchievementRequest body);

        [Get("/student/achievement/{id}")]
        Task<ApiErrorResult<DetailAchievementResult>> DetailAchievement(string id);

        [Put("/student/achievement")]
        Task<ApiErrorResult> UpdateAchievement([Body] UpdateAchievementRequest body);

        [Get("/student/achievement-user-by-position")]
        Task<ApiErrorResult<IEnumerable<GetUserByPositionResult>>> GetUserByPosition(GetUserByPositionRequest query);

        [Get("/student/achievement-focus-area")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetFosusArea();

        [Post("/student/achievement-download")]
        Task<HttpResponseMessage> DownloadAchievement([Body] GetAchievementRequest body);
        #endregion

        #region approval Achievement
        [Post("/student/achievement-delete-approval")]
        Task<ApiErrorResult> ApprovalAndDeleteAchievement([Body] ApprovalAndDeleteAchievementRequest body);

        
        #endregion
    }
}
