using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables.UploadXmlModel;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ILesson : IFnSchedule
    {
        [Get("/schedule/lesson")]
        Task<ApiErrorResult<IEnumerable<GetLessonResult>>> GetLessons(GetLessonRequest query);

        [Get("/schedule/lesson/download")]
        Task<HttpResponseMessage> GetDownloadLessons(GetLessonRequest query);

        [Get("/schedule/lesson/{id}")]
        Task<ApiErrorResult<GetLessonDetailResult>> GetLessonDetail(string id);

        [Post("/schedule/lesson")]
        Task<ApiErrorResult> AddLesson([Body] AddLessonRequest body);

        [Put("/schedule/lesson")]
        Task<ApiErrorResult> UpdateLesson([Body] UpdateLessonRequest body);

        [Delete("/schedule/lesson")]
        Task<ApiErrorResult> DeleteLesson([Body] IEnumerable<string> ids);

        [Get("/schedule/lesson/get/teacher")]
        Task<ApiErrorResult<IEnumerable<DataModelGeneral>>> GetTeacherByLesson(string IdLesson);

        [Get("/schedule/lesson/get/week")]
        Task<ApiErrorResult<IEnumerable<DataModelGeneral>>> GetWeekByLesson(GetWeekByLessonRequest query);

        [Get("/schedule/lesson-byteacherid")]
        Task<ApiErrorResult<IEnumerable<GetLessonByTeacherIDResult>>> GetLessonByTeacherID(GetLessonByTeacherIDRequest query);

        [Get("/schedule/lesson-validate-classid")]
        Task<ApiErrorResult<ValidateGeneratedClassIdResult>> GenerateClassId(ValidateGeneratedClassIdRequest query);

        [Post("/schedule/lesson-copy")]
        Task<ApiErrorResult> AddLessonCopy([Body] AddLessonCopyRequest body);

    }
}
