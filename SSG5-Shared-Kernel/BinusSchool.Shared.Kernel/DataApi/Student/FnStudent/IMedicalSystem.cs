using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem;
using Refit;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport;
using System.Net.Http;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.EmergencyStudentContact;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMedicalSystem : IFnStudent
    {
        #region Medical Data
        [Get("/student/medical-system/get-medical-item-type")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetMedicalItemType(CollectionRequest param);

        [Get("/student/medical-system/get-medicine-type")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetMedicineType(CollectionRequest param);

        [Get("/student/medical-system/get-medicine-category")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetMedicineCategory(CollectionRequest param);

        [Get("/student/medical-system/get-dosage-type")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetDosageType(CollectionRequest param);

        [Get("/student/medical-system/get-list-medical-item")]
        Task<ApiErrorResult<IEnumerable<GetListMedicalItemResult>>> GetListMedicalItem(MedicalDataSchoolRequest param);

        [Post("/student/medical-system/save-medical-item")]
        Task<ApiErrorResult> SaveMedicalItem([Body] SaveMedicalItemRequest body);

        [Get("/student/medical-system/get-detail-medical-item")]
        Task<ApiErrorResult<GetDetailMedicalItemResult>> GetDetailMedicalItem(IdCollection param);

        [Delete("/student/medical-system/delete-medical-item")]
        Task<ApiErrorResult> DeleteMedicalItem([Body] IdCollection body);

        [Get("/student/medical-system/get-list-medical-treatment")]
        Task<ApiErrorResult<IEnumerable<GetListMedicalTreatmentResult>>> GetListMedicalTreatment(MedicalDataSchoolRequest param);

        [Post("/student/medical-system/save-medical-treatment")]
        Task<ApiErrorResult> SaveMedicalTreatment([Body] SaveMedicalTreatmentRequest body);

        [Get("/student/medical-system/get-detail-medical-treatment")]
        Task<ApiErrorResult<ItemValueVm>> GetDetailMedicalTreatment(IdCollection param);

        [Delete("/student/medical-system/delete-medical-treatment")]
        Task<ApiErrorResult> DeleteMedicalTreatment([Body] IdCollection body);

        [Get("/student/medical-system/get-list-medical-condition")]
        Task<ApiErrorResult<IEnumerable<GetListMedicalConditionResult>>> GetListMedicalCondition(MedicalDataSchoolRequest param);

        [Post("/student/medical-system/save-medical-condition")]
        Task<ApiErrorResult> SaveMedicalCondition([Body] SaveMedicalConditionRequest body);

        [Get("/student/medical-system/get-detail-medical-condition")]
        Task<ApiErrorResult<GetDetailMedicalConditionResult>> GetDetailMedicalCondition(IdCollection param);

        [Delete("/student/medical-system/delete-medical-condition")]
        Task<ApiErrorResult> DeleteMedicalCondition([Body] IdCollection body);

        [Get("/student/medical-system/get-list-medical-vaccine")]
        Task<ApiErrorResult<IEnumerable<GetListMedicalVaccineResult>>> GetListMedicalVaccine(MedicalDataSchoolRequest param);

        [Post("/student/medical-system/save-medical-vaccine")]
        Task<ApiErrorResult> SaveMedicalVaccine([Body] SaveMedicalVaccineRequest body);

        [Get("/student/medical-system/get-detail-medical-vaccine")]
        Task<ApiErrorResult<GetDetailMedicalVaccineResult>> GetDetailMedicalVaccine(IdCollection param);

        [Delete("/student/medical-system/delete-medical-vaccine")]
        Task<ApiErrorResult> DeleteMedicalVaccine([Body] IdCollection body);

        [Get("/student/medical-system/get-list-medical-hospital")]
        Task<ApiErrorResult<IEnumerable<GetListMedicalHospitalResult>>> GetListMedicalHospital(MedicalDataSchoolRequest param);

        [Get("/student/medical-system/get-detail-medical-hospital")]
        Task<ApiErrorResult<GetDetailMedicalHospitalResult>> GetDetailMedicalHospital(GetDetailMedicalHospitalRequest param);

        [Post("/student/medical-system/save-medical-hospital")]
        Task<ApiErrorResult> SaveMedicalHospital([Body] SaveMedicalHospitalRequest body);

        [Post("/student/medical-system/delete-medical-hospital")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> DeleteMedicalHospital([Body] DeleteMedicalHospitalRequest body);

        [Get("/student/medical-system/get-list-medical-doctor")]
        Task<ApiErrorResult<IEnumerable<GetListMedicalDoctorResult>>> GetListMedicalDoctor(MedicalDataSchoolRequest param);

        [Post("/student/medical-system/save-medical-doctor")]
        Task<ApiErrorResult> SaveMedicalDoctor([Body] SaveMedicalDoctorRequest body);

        [Get("/student/medical-system/get-detail-medical-doctor")]
        Task<ApiErrorResult<GetDetailMedicalDoctorResult>> GetDetailMedicalDoctor(IdCollection param);

        [Delete("/student/medical-system/delete-medical-doctor")]
        Task<ApiErrorResult> DeleteMedicalDoctor([Body] IdCollection body);
        #endregion

        #region Medical Tapping System
        [Get("/student/medical-system/get-medical-tapping-system-patient-list")]
        Task<ApiErrorResult<IEnumerable<GetMedicalTappingSystemPatientListResponse>>> GetMedicalTappingSystemPatientList(GetMedicalTappingSystemPatientListRequest request);

        [Get("/student/medical-system/get-medical-tapping-system-patient-list")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetPatientList(GetMedicalTappingSystemPatientListRequest request);

        [Post("/student/medical-system/update-medical-tapping-system-patient-status")]
        Task<ApiErrorResult<UpdateMedicalTappingSystemPatientStatusResponse>> UpdateMedicalTappingSystemPatientStatus([Body] UpdateMedicalTappingSystemPatientStatusRequest request);

        [Get("/student/medical-system/get-medical-tapping-system-patient-card")]
        Task<ApiErrorResult<UpdateMedicalTappingSystemPatientStatusResponse>> GetMedicalTappingSystemPatientCard(GetMedicalTappingSystemPatientCardRequest request);
        #endregion

        #region Medical Record Entry
        [Get("/student/medical-system/get-medical-record-entry-student-list")]
        Task<ApiErrorResult<IEnumerable<GetMedicalRecordEntryStudentListResponse>>> GetMedicalRecordEntryStudentList(GetMedicalRecordEntryStudentListRequest request);

        [Get("/student/medical-system/get-medical-record-entry-staff-list")]
        Task<ApiErrorResult<IEnumerable<GetMedicalRecordEntryStaffListResponse>>> GetMedicalRecordEntryStaffList(GetMedicalRecordEntryStaffListRequest request);

        [Get("/student/medical-system/get-medical-record-entry-other-patient-list")]
        Task<ApiErrorResult<IEnumerable<GetMedicalRecordEntryOtherPatientListResponse>>> GetMedicalRecordEntryOtherPatientList(GetMedicalRecordEntryOtherPatientListRequest request);

        [Post("/student/medical-system/add-medical-record-entry-other-patient")]
        Task<ApiErrorResult> AddMedicalRecordEntryOtherPatient([Body] AddMedicalRecordEntryOtherPatientRequest request);

        [Get("/student/medical-system/get-medical-record-entry-detail")]
        Task<ApiErrorResult<GetMedicalRecordEntryDetailResponse>> GetMedicalRecordEntryDetail(GetMedicalRecordEntryDetailRequest request);

        [Get("/student/medical-system/get-medical-record-entry-clinic-visit-history")]
        Task<ApiErrorResult<IEnumerable<GetMedicalRecordEntryClinicVisitHistoryResponse>>> GetMedicalRecordEntryClinicVisitHistory(GetMedicalRecordEntryClinicVisitHistoryRequest request);

        [Get("/student/medical-system/print-medical-record-entry-student-pass")]
        Task<ApiErrorResult<PrintMedicalRecordEntryStudentPassResponse>> PrintMedicalRecordEntryStudentPass(PrintMedicalRecordEntryStudentPassRequest request);
        #endregion

        #region Clinic Daily Report
        [Get("/student/medical-system/get-clinic-daily-report-visit-data")]
        Task<ApiErrorResult<GetClinicDailyReportVisitDataResponse>> GetClinicDailyReportVisitData(ClinicDailyReportDataRequest request);

        [Get("/student/medical-system/get-clinic-daily-report-of-leave-school-early-student")]
        Task<ApiErrorResult<IEnumerable<GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse>>> GetClinicDailyReportOfLeaveSchoolEarlyStudent(ClinicDailyReportDataRequest request);

        [Get("/student/medical-system/get-clinic-daily-report-injury-visit")]
        Task<ApiErrorResult<GetClinicDailyReportInjuryVisitResponse>> GetClinicDailyReportInjuryVisit(ClinicDailyReportDataRequest request);

        [Get("/student/medical-system/get-clinic-daily-report-exclude-injury-visit")]
        Task<ApiErrorResult<GetClinicDailyReportExcludeInjuryVisitReponse>> GetClinicDailyReportExcludeInjuryVisit(ClinicDailyReportDataRequest request);

        [Get("/student/medical-system/export-excel-clinic-daily-report")]
        Task<HttpResponseMessage> ExportExcelClinicDailyReport(ClinicDailyReportDataRequest request);
        #endregion

        #region Medical Personal Data
        [Get("/student/medical-system/get-general-medical-information")]
        Task<ApiErrorResult<GetGeneralMedicalInformationResponse>> GetGeneralMedicalInformation(GetGeneralMedicalInformationRequest request);

        [Post("/student/medical-system/save-general-medical-information")]
        Task<ApiErrorResult> SaveGeneralMedicalInformation([Body] SaveGeneralMedicalInformationRequest request);

        [Get("/student/medical-system/get-medical-document")]
        Task<ApiErrorResult<IEnumerable<GetMedicalDocumentResponse>>> GetMedicalDocument(GetMedicalDocumentRequest request);

        [Multipart]
        [Post("/student/medical-system/save-medical-document")]
        Task<ApiErrorResult> SaveMedicalDocument(
            string Id,
            string Mode,
            string? IdDocument,
            string DocumentName,
            [AliasAs("file")] StreamPart file);

        [Delete("/student/medical-system/delete-medical-document")]
        Task<ApiErrorResult> DeleteMedicalDocument([Body] DeleteMedicalDocumentRequest request);

        [Get("/student/medical-system/get-general-physical-measurement")]
        Task<ApiErrorResult<IEnumerable<GetGeneralPhysicalMeasurementResponse>>> GetGeneralPhysicalMeasurement(GetGeneralPhysicalMeasurementRequest request);

        [Post("/student/medical-system/save-general-physical-measurement")]
        Task<ApiErrorResult> SaveGeneralPhysicalMeasurement([Body] SaveGeneralPhysicalMeasurementRequest request);

        [Post("/student/medical-system/get-treatment-and-medic-item-by-condition")]
        Task<ApiErrorResult<GetTreatmentAndMedicItemByConditionResponse>> GetTreatmentAndMedicItemByCondition([Body] GetTreatmentAndMedicItemByConditionRequest request);
        #endregion

        #region Emergency Student Contact
        [Get("/student/medical-system/get-emergency-student-contact")]
        Task<ApiErrorResult<IEnumerable<GetEmergencyStudentContactResult>>> GetEmergencyStudentContact(IdCollection param);
        #endregion

        #region Details Condition Treatment & Medication Entry
        [Get("/student/medical-system/get-details-condition-data")]
        Task<ApiErrorResult<GetDetailsCondtionDataResult>> GetDetailsConditionData(GetDetailsConditionDataRequest param);

        [Post("/student/medical-system/save-details-condition-data")]
        Task<ApiErrorResult> SaveDetailsConditionData([Body] SaveDetailsConditionDataRequest body);
        #endregion
    }
}
