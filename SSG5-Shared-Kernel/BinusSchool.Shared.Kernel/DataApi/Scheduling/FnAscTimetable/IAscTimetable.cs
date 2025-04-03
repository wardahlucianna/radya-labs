using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnAscTimetable.AscTimeTables;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnAscTimetable
{
    public interface IAscTimetable : IFnAscTimetable
    {
        [Multipart]
        [Post("/Scheduling/asc-timetable/upload-xml")]
        Task<HttpResponseMessage> UploadAscTimetable(
        string name,
        string idSchoolAcademicyears,
        string idSessionSet,
        [AliasAs("CodeGradeForAutomaticGenerateClassId")] string CodeGradeForAutomaticGenerateClassId,
        string FormatIdClass,
        bool AutomaticGenerateClassId,
        [AliasAs("file")] StreamPart file,
        string idSchool,
        bool IsCreateSessionSetFromXml,
        string IdGradepathwayforCreateSession,
        string SessionSetName);

        [Multipart]
        [Post("/Scheduling/asc-timetable/reupload-xml")]
        Task<HttpResponseMessage> ReUploadAscTimetbaleXML(
        string IdAscTimeTable,
        string IdGradePathway,
        string FormatIdClass,
        bool AutomaticGenerateClassId,
        string IdSchool,
        [AliasAs("file")] StreamPart file);


        [Post("/Scheduling/asc-timetable/save-data-xml")]
        Task<ApiErrorResult> SaveDataXml([Body] AddDataAscTimeTableAfterUploadRequest model);


        [Post("/Scheduling/asc-timetable/save-reupload-data-xml")]
        Task<ApiErrorResult> SaveReUploadDataXml([Body] AddReUploadXmlRequest model);


        [Get("/Scheduling/asc-timetable")]
        Task<ApiErrorResult<IEnumerable<AscTimetableGetListResult>>> GetlistAsc([Query] AscTimetableGetListRequest model);

        [Get("/Scheduling/asc-timetable/{id}")]
        Task<ApiErrorResult<AscTimetableGetDetailResult>> GetAscById(string id);

        [Multipart]
        [Post("/Scheduling/asc-timetable/save-file-xml")]
        Task<ApiErrorResult> SaveFileXml(string IdAscTimetable,
        string Type,
        [AliasAs("file")] StreamPart file);

        [Get("/Scheduling/asc-timetable/check/is-there-job-run")]
        Task<ApiErrorResult> CheckIsJobRunning([Query] StartAscTimetableProcessRequest model);

        [Post("/Scheduling/asc-timetable/start-process")]
        Task<ApiErrorResult<string>> StartProcess([Body] StartAscTimetableProcessRequest model);

        [Put("/Scheduling/asc-timetable/finish-process")]
        Task<ApiErrorResult> FinishProcess([Body] FinishAscTimetableProcessRequest model);
    }
}
