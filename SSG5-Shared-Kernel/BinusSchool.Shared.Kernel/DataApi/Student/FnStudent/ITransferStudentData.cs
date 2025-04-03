using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ITransferStudentData : IFnStudent
    {
        [Put("/student/transfer-student/prev-master-school")]
        Task<ApiErrorResult> TransferPrevMasterSchool([Body] TransferPrevMasterSchoolRequest body);

        [Put("/student/transfer-student/occupation-type")]
        Task<ApiErrorResult> TransferOccupationType([Body] TransferOccupationTypeRequest body);

        [Put("/student/transfer-student/student-parent")]
        Task<ApiErrorResult> TransferStudent([Body] TransferStudentRequest body);
        
        [Post("/student/transfer-student/master-document")]
        Task<ApiErrorResult> TransferMasterDocument([Body] TransferMasterDocumentRequest body);

        [Post("/student/transfer-student/master-country")]
        Task<ApiErrorResult> TransferMasterCountry([Body] TransferMasterCountryRequest body);

        [Post("/student/transfer-student/master-district")]
        Task<ApiErrorResult> TransferMasterDistrict([Body] TransferMasterDistrictRequest body);

        [Post("/student/transfer-student/master-nationality")]
        Task<ApiErrorResult> TransferMasterNationality([Body] TransferMasterNationalityRequest body);
    }
}
