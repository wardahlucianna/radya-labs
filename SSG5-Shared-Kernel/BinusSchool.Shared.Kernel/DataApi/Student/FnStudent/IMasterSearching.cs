using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMasterSearching : IFnStudent
    {
        [Get("/student/MasterSearching/GetFieldDataList")]
        Task<ApiErrorResult<IEnumerable<GetFieldDataListResult>>> GetFieldDataList(GetFieldDataListRequest request);

        [Post("/student/MasterSearching/GetMasterSearchingData")]
        Task<ApiErrorResult<IEnumerable<GetMasterSearchingDataResult>>> GetMasterSearchingData([Body] GetMasterSearchingDataRequest body);

        [Post("/student/MasterSearching/GetMasterSearchingDataTable")]
        Task<ApiErrorResult<GetMasterSearchingDataTableResult>> GetMasterSearchingDataTable([Body] GetMasterSearchingDataTableRequest body);

        [Post("/student/MasterSearching-export")]
        Task<HttpResponseMessage> MasterSearchingexport([Body]ExportToExcelMasterSearchingDataRequest body);
        
        [Post("/student/MasterSearching-exportexcel")]
        Task<HttpResponseMessage> ExportMasterSeachingDataTableToExcel([Body] ExportMasterSeachingDataTableToExcelRequest body);

        [Get("/student/MasterSearching/GetActiveAYandSmt")]
        Task<ApiErrorResult<GetActiveAYandSmtResult>> GetActiveAYandSmt(GetActiveAYandSmtRequest query);

        [Get("/student/MasterSearching/GetListHomeroom")]
        Task<ApiErrorResult<IEnumerable<GetListHomeroomByAySmtLvlGrdResult>>> GetListHomeroomByAySmtLvlGrd(GetListHomeroomByAySmtLvlGrdRequest query);

        [Get("/student/MasterSearching/GetListStudent")]
        Task<ApiErrorResult<IEnumerable<GetListStudentByAySmtLvlGrdHrmResult>>> GetListStudentByAySmtLvlGrdHrm(GetListStudentByAySmtLvlGrdHrmRequest query);
    }
}
