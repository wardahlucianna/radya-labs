using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IExemplaryCharacter : IFnStudent
    {
        [Post("/exemplary-character/get-list-exemplary-character-summary")]
        Task<ApiErrorResult<GetListExemplaryCharacterSummaryResult>> GetListExemplaryCharacterSummary(GetListExemplaryCharacterSummaryRequest query);

        [Get("/exemplary-character/get-list-exemplary-category-settings")]
        Task<ApiErrorResult<IEnumerable<GetListExemplaryCategorySettingsResult>>> GetListExemplaryCategorySettings(GetListExemplaryCategorySettingsRequest query);
        
        [Get("/exemplary-character/get-list-exemplary-value-settings")]
        Task<ApiErrorResult<IEnumerable<GetListExemplaryValueSettingsResult>>> GetListExemplaryValueSettings(GetListExemplaryValueSettingsRequest query);
        
        [Post("/exemplary-character/get-detail-exemplary-category-settings")]
        Task<ApiErrorResult<GetDetailExemplaryCategorySettingsResult>> GetDetailExemplaryCategorySettings(GetDetailExemplaryCategorySettingsRequest query);

        [Delete("/exemplary-character/delete-exemplary-character")]
        Task<ApiErrorResult> DeleteExemplaryCharacter([Body] DeleteExemplaryCharacterRequest query);

        [Post("/exemplary-character/create-exemplary-category")]
        Task<ApiErrorResult> CreateExemplaryCategorySettings([Body] CreateExemplaryCategorySettingsRequest query);

        [Post("/exemplary-character/create-exemplary-value")]
        Task<ApiErrorResult> CreateExemplaryValueSettings([Body] CreateExemplaryValueSettingsRequest query);

        [Put("/exemplary-character/update-exemplary-category")]
        Task<ApiErrorResult> UpdateExemplaryCategorySettings([Body] UpdateExemplaryCategorySettingsRequest query);

        [Put("/exemplary-character/update-exemplary-value")]
        Task<ApiErrorResult> UpdateExemplaryValueSettings([Body] UpdateExemplaryValueSettingsRequest query);

        [Delete("/exemplary-character/delete-exemplary-category")]
        Task<ApiErrorResult> DeleteExemplaryCategorySettings([Body] DeleteExemplaryCategorySettingsRequest query);

        [Delete("/exemplary-character/delete-exemplary-value")]
        Task<ApiErrorResult> DeleteExemplaryValueSettings([Body] DeleteExemplaryValueSettingsRequest query);

        [Get("/exemplary-character/exemplary/{id}")]
        Task<ApiErrorResult<GetDetailExemplaryCharacterResult>> GetExemplaryCharacterById(string id);

        [Post("/exemplary-character/exemplary")]
        Task<ApiErrorResult> SaveExemplaryCharacter([Body] List<SaveExemplaryCharacterRequest> query);

        //[Delete("/exemplary-character/exemplary")]
        //Task<ApiErrorResult> DeleteExemplaryCharacter([Body] IEnumerable<string> ids);

        [Get("/exemplary-character/get-view-exemplary-character")]
        Task<ApiErrorResult<IEnumerable<GetExemplaryCharacterViewResult>>> GetExemplaryCharacterView(GetExemplaryCharacterViewRequest query);

        [Put("/exemplary-character/update-exemplary-like")]
        Task<ApiErrorResult<UpdateExemplaryLikeResult>> UpdateExemplaryLike([Body] UpdateExemplaryLikeRequest query);

        [Get("/exemplary-character/get-list-exemplary-category-used")]
        Task<ApiErrorResult<IEnumerable<GetListExemplaryValueSettingsResult>>> GetExemplaryCategoryUsed(GetExemplaryCategoryUsedRequest query);

        [Get("/exemplary-character/get-list-student-for-exemplary")]
        Task<ApiErrorResult<IEnumerable<GetListStudentByAySmtLvlGrdHrmForExemplaryResult>>> GetListStudentByAySmtLvlGrdHrmForExemplary(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest query);
    }
}
