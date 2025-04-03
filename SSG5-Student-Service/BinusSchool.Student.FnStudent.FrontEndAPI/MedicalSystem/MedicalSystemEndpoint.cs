using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalData;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalData.MedicalVaccine;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalTappingSystem;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry;
using Newtonsoft.Json;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.ClinicDailyReport;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.EmergencyStudentContact;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.EmergencyStudentContact;
using BinusSchool.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry;
using BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry;

namespace BinusSchool.Student.FnStudent.MedicalSystem
{
    public class MedicalSystemEndpoint
    {
        private const string _route = "student/medical-system";
        private const string _tag = "Student Medical System";

        #region Medical Data
        private readonly MedicalItemTypeHandler _medicalItemTypeHandler;
        private readonly MedicineTypeHandler _medicineTypeHandler;
        private readonly MedicineCategoryHandler _medicineCategoryHandler;
        private readonly DosageTypeHandler _dosageTypeHandler;

        private readonly GetListMedicalItemHandler _getListMedicalItemHandler;
        private readonly SaveMedicalItemHandler _saveMedicalItemHandler;
        private readonly GetDetailMedicalItemHandler _getDetailMedicalItemHandler;
        private readonly DeleteMedicalItemHandler _deleteMedicalItemHandler;

        private readonly GetListMedicalTreatmentHandler _getListMedicalTreatmentHandler;
        private readonly SaveMedicalTreatmentHandler _saveMedicalTreatmentHandler;
        private readonly GetDetailMedicalTreatmentHandler _getDetailMedicalTreatmentHandler;
        private readonly DeleteMedicalTreatmentHandler _deleteMedicalTreatmentHandler;

        private readonly GetListMedicalConditionHandler _getListMedicalConditionHandler;
        private readonly SaveMedicalConditionHandler _saveMedicalConditionHandler;
        private readonly GetDetailMedicalConditionHandler _getDetailMedicalConditionHandler;
        private readonly DeleteMedicalConditionHandler _deleteMedicalConditionHandler;

        private readonly GetListMedicalVaccineHandler _getListMedicalVaccineHandler;
        private readonly SaveMedicalVaccineHandler _saveMedicalVaccineHandler;
        private readonly GetDetailMedicalVaccineHandler _getDetailMedicalVaccineHandler;
        private readonly DeleteMedicalVaccineHandler _deleteMedicalVaccineHandler;

        private readonly GetListMedicalHospitalHandler _getListMedicalHospitalHandler;
        private readonly GetDetailMedicalHospitalHandler _getDetailMedicalHospitalHandler;
        private readonly DeleteMedicalHospitalHandler _deleteMedicalHospitalHandler;
        private readonly SaveMedicalHospitalHandler _saveMedicalHospitalHandler;

        private readonly GetListMedicalDoctorHandler _getListMedicalDoctorHandler;
        private readonly SaveMedicalDoctorHandler _saveMedicalDoctorHandler;
        private readonly GetDetailMedicalDoctorHandler _getDetailMedicalDoctorHandler;
        private readonly DeleteMedicalDoctorHandler _deleteMedicalDoctorHandler;
        #endregion

        #region Medical Tapping System
        private readonly GetMedicalTappingSystemPatientListHandler _getMedicalTappingSystemPatientListHandler;
        private readonly UpdateMedicalTappingSystemPatientStatusHandler _updateMedicalTappingSystemPatientStatusHandler;
        private readonly GetMedicalTappingSystemPatientCardHandler _getMedicalTappingSystemPatientCardHandler;
        #endregion

        #region Medical Record Entry
        private readonly GetMedicalRecordEntryStudentListHandler _getMedicalRecordEntryStudentListHandler;
        private readonly GetMedicalRecordEntryStaffListHandler _getMedicalRecordEntryStaffListHandler;
        private readonly GetMedicalRecordEntryOtherPatientListHandler _getMedicalRecordEntryOtherPatientListHandler;
        private readonly AddMedicalRecordEntryOtherPatientHandler _addMedicalRecordEntryOtherPatientHandler;
        private readonly GetMedicalRecordEntryDetailHandler _getMedicalRecordEntryDetailHandler;
        private readonly GetMedicalRecordEntryClinicVisitHistoryHandler _getMedicalRecordEntryClinicVisitHistoryHandler;
        private readonly PrintMedicalRecordEntryStudentPassHandler _printMedicalRecordEntryStudentPassHandler;
        #endregion

        #region Clinic Daily Report
        private readonly GetClinicDailyReportVisitDataHandler _getClinicDailyReportVisitDataHandler;
        private readonly GetClinicDailyReportOfLeaveSchoolEarlyStudentHandler _getClinicDailyReportOfLeaveSchoolEarlyStudentHandler;
        private readonly GetClinicDailyReportInjuryVisitHandler _getClinicDailyReportInjuryVisitHandler;
        private readonly GetClinicDailyReportExcludeInjuryVisitHandler _getClinicDailyReportExcludeInjuryVisitHandler;
        private readonly ExportExcelClinicDailyReportHandler _exportExcelClinicDailyReportHandler;
        #endregion

        #region Medical Personal Data
        private readonly GetGeneralMedicalInformationHandler _getGeneralMedicalInformationHandler;
        private readonly SaveGeneralMedicalInformationHandler _saveGeneralMedicalInformationHandler;
        private readonly GetMedicalDocumentHandler _getMedicalDocumentHandler;
        private readonly SaveMedicalDocumentHandler _saveMedicalDocumentHandler;
        private readonly DeleteMedicalDocumentHandler _deleteMedicalDocumentHandler;
        private readonly GetGeneralPhysicalMeasurementHandler _getGeneralPhysicalMeasurementHandler;
        private readonly SaveGeneralPhysicalMeasurementHandler _saveGeneralPhysicalMeasurementHandler;
        private readonly GetTreatmentAndMedicItemByConditionHandler _getTreatmentAndMedicItemByConditionHandler;
        #endregion

        #region Emergency Student Contact
        private readonly GetEmergencyStudentContactHandler _getEmergencyStudentContactHandler;
        #endregion

        #region Details Condition Treatment & Medication Entry
        private readonly GetDetailsCondtionDataHandler _getDetailsCondtionDataHandler;
        private readonly SaveDetailsConditionDataHandler _saveDetailsConditionDataHandler;
        #endregion

        public MedicalSystemEndpoint(
            MedicalItemTypeHandler medicalItemTypeHandler,
            MedicineTypeHandler medicineTypeHandler,
            MedicineCategoryHandler medicineCategoryHandler,
            DosageTypeHandler dosageTypeHandler,
            GetListMedicalItemHandler getListMedicalItemHandler,
            SaveMedicalItemHandler saveMedicalItemHandler,
            GetDetailMedicalItemHandler getDetailMedicalItemHandler,
            DeleteMedicalItemHandler deleteMedicalItemHandler,
            GetListMedicalTreatmentHandler getListMedicalTreatmentHandler,
            SaveMedicalTreatmentHandler saveMedicalTreatmentHandler,
            GetDetailMedicalTreatmentHandler getDetailMedicalTreatmentHandler,
            DeleteMedicalTreatmentHandler deleteMedicalTreatmentHandler,
            GetListMedicalConditionHandler getListMedicalConditionHandler,
            SaveMedicalConditionHandler saveMedicalConditionHandler,
            GetDetailMedicalConditionHandler getDetailMedicalConditionHandler,
            DeleteMedicalConditionHandler deleteMedicalConditionHandler,
            GetListMedicalVaccineHandler getListMedicalVaccineHandler,
            SaveMedicalVaccineHandler saveMedicalVaccineHandler,
            GetDetailMedicalVaccineHandler getDetailMedicalVaccineHandler,
            DeleteMedicalVaccineHandler deleteMedicalVaccineHandler,
            GetListMedicalHospitalHandler getListMedicalHospitalHandler,
            GetDetailMedicalHospitalHandler getDetailMedicalHospitalHandler,
            DeleteMedicalHospitalHandler deleteMedicalHospitalHandler,
            SaveMedicalHospitalHandler saveMedicalHospitalHandler,
            GetListMedicalDoctorHandler getListMedicalDoctorHandler,
            SaveMedicalDoctorHandler saveMedicalDoctorHandler,
            GetDetailMedicalDoctorHandler getDetailMedicalDoctorHandler,
            DeleteMedicalDoctorHandler deleteMedicalDoctorHandler,

            // Medical Tapping System
            GetMedicalTappingSystemPatientListHandler getMedicalTappingSystemPatientListHandler,
            UpdateMedicalTappingSystemPatientStatusHandler updateMedicalTappingSystemPatientStatusHandler,
            GetMedicalTappingSystemPatientCardHandler getMedicalTappingSystemPatientCardHandler,

            // Medical Record Entry
            GetMedicalRecordEntryStudentListHandler getMedicalRecordEntryStudentListHandler,
            GetMedicalRecordEntryStaffListHandler getMedicalRecordEntryStaffListHandler,
            GetMedicalRecordEntryOtherPatientListHandler getMedicalRecordEntryOtherPatientListHandler,
            AddMedicalRecordEntryOtherPatientHandler addMedicalRecordEntryOtherPatientHandler,
            GetMedicalRecordEntryDetailHandler getMedicalRecordEntryDetailHandler,
            GetMedicalRecordEntryClinicVisitHistoryHandler getMedicalRecordEntryClinicVisitHistoryHandler,
            PrintMedicalRecordEntryStudentPassHandler printMedicalRecordEntryStudentPassHandler,

            // Clinic Daily Report
            GetClinicDailyReportVisitDataHandler getClinicDailyReportVisitDataHandler,
            GetClinicDailyReportOfLeaveSchoolEarlyStudentHandler getClinicDailyReportOfLeaveSchoolEarlyStudentHandler,
            GetClinicDailyReportInjuryVisitHandler getClinicDailyReportInjuryVisitHandler,
            GetClinicDailyReportExcludeInjuryVisitHandler getClinicDailyReportExcludeInjuryVisitHandler,
            ExportExcelClinicDailyReportHandler exportExcelClinicDailyReportHandler,

            // Medical Personal Data
            GetGeneralMedicalInformationHandler getGeneralMedicalInformationHandler,
            SaveGeneralMedicalInformationHandler saveGeneralMedicalInformationHandler,
            GetMedicalDocumentHandler getMedicalDocumentHandler,
            SaveMedicalDocumentHandler saveMedicalDocumentHandler,
            DeleteMedicalDocumentHandler deleteMedicalDocumentHandler,
            GetGeneralPhysicalMeasurementHandler getGeneralPhysicalMeasurementHandler,
            SaveGeneralPhysicalMeasurementHandler saveGeneralPhysicalMeasurementHandler,
            GetTreatmentAndMedicItemByConditionHandler getTreatmentAndMedicItemByConditionHandler,

            // Emergency Student Contact
            GetEmergencyStudentContactHandler getEmergencyStudentContactHandler,

            // Details Condition Treatment & Medication Entry
            GetDetailsCondtionDataHandler getDetailsCondtionDataHandler,
            SaveDetailsConditionDataHandler saveDetailsConditionDataHandler
        )
        {
            _medicalItemTypeHandler = medicalItemTypeHandler;
            _medicineTypeHandler = medicineTypeHandler;
            _medicineCategoryHandler = medicineCategoryHandler;
            _dosageTypeHandler = dosageTypeHandler;
            _getListMedicalItemHandler = getListMedicalItemHandler;
            _saveMedicalItemHandler = saveMedicalItemHandler;
            _getDetailMedicalItemHandler = getDetailMedicalItemHandler;
            _deleteMedicalItemHandler = deleteMedicalItemHandler;
            _getListMedicalTreatmentHandler = getListMedicalTreatmentHandler;
            _saveMedicalTreatmentHandler = saveMedicalTreatmentHandler;
            _getDetailMedicalTreatmentHandler = getDetailMedicalTreatmentHandler;
            _deleteMedicalTreatmentHandler = deleteMedicalTreatmentHandler;
            _getListMedicalConditionHandler = getListMedicalConditionHandler;
            _saveMedicalConditionHandler = saveMedicalConditionHandler;
            _getDetailMedicalConditionHandler = getDetailMedicalConditionHandler;
            _deleteMedicalConditionHandler = deleteMedicalConditionHandler;
            _getListMedicalVaccineHandler = getListMedicalVaccineHandler;
            _saveMedicalVaccineHandler = saveMedicalVaccineHandler;
            _getDetailMedicalVaccineHandler = getDetailMedicalVaccineHandler;
            _deleteMedicalVaccineHandler = deleteMedicalVaccineHandler;
            _getListMedicalHospitalHandler = getListMedicalHospitalHandler;
            _getDetailMedicalHospitalHandler = getDetailMedicalHospitalHandler;
            _deleteMedicalHospitalHandler = deleteMedicalHospitalHandler;
            _saveMedicalHospitalHandler = saveMedicalHospitalHandler;
            _getListMedicalDoctorHandler = getListMedicalDoctorHandler;
            _saveMedicalDoctorHandler = saveMedicalDoctorHandler;
            _getDetailMedicalDoctorHandler = getDetailMedicalDoctorHandler;
            _deleteMedicalDoctorHandler = deleteMedicalDoctorHandler;

            // Medical Tapping System
            _getMedicalTappingSystemPatientListHandler = getMedicalTappingSystemPatientListHandler;
            _updateMedicalTappingSystemPatientStatusHandler = updateMedicalTappingSystemPatientStatusHandler;
            _getMedicalTappingSystemPatientCardHandler = getMedicalTappingSystemPatientCardHandler;

            // Medical Record Entry
            _getMedicalRecordEntryStudentListHandler = getMedicalRecordEntryStudentListHandler;
            _getMedicalRecordEntryStaffListHandler = getMedicalRecordEntryStaffListHandler;
            _getMedicalRecordEntryOtherPatientListHandler = getMedicalRecordEntryOtherPatientListHandler;
            _addMedicalRecordEntryOtherPatientHandler = addMedicalRecordEntryOtherPatientHandler;
            _getMedicalRecordEntryDetailHandler = getMedicalRecordEntryDetailHandler;
            _getMedicalRecordEntryClinicVisitHistoryHandler = getMedicalRecordEntryClinicVisitHistoryHandler;
            _printMedicalRecordEntryStudentPassHandler = printMedicalRecordEntryStudentPassHandler;

            // Clinic Daily Report
            _getClinicDailyReportVisitDataHandler = getClinicDailyReportVisitDataHandler;
            _getClinicDailyReportOfLeaveSchoolEarlyStudentHandler = getClinicDailyReportOfLeaveSchoolEarlyStudentHandler;
            _getClinicDailyReportInjuryVisitHandler = getClinicDailyReportInjuryVisitHandler;
            _getClinicDailyReportExcludeInjuryVisitHandler = getClinicDailyReportExcludeInjuryVisitHandler;
            _exportExcelClinicDailyReportHandler = exportExcelClinicDailyReportHandler;

            // Medical Personal Data
            _getGeneralMedicalInformationHandler = getGeneralMedicalInformationHandler;
            _saveGeneralMedicalInformationHandler = saveGeneralMedicalInformationHandler;
            _getMedicalDocumentHandler = getMedicalDocumentHandler;
            _saveMedicalDocumentHandler = saveMedicalDocumentHandler;
            _deleteMedicalDocumentHandler = deleteMedicalDocumentHandler;
            _getGeneralPhysicalMeasurementHandler = getGeneralPhysicalMeasurementHandler;
            _saveGeneralPhysicalMeasurementHandler = saveGeneralPhysicalMeasurementHandler;
            _getTreatmentAndMedicItemByConditionHandler = getTreatmentAndMedicItemByConditionHandler;

            // Emergency Student Contact
            _getEmergencyStudentContactHandler = getEmergencyStudentContactHandler;

            // Details Condition Treatment & Medication Entry
            _getDetailsCondtionDataHandler = getDetailsCondtionDataHandler;
            _saveDetailsConditionDataHandler = saveDetailsConditionDataHandler;
        }

        #region Medical Data
        [FunctionName(nameof(GetMedicalItemType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Item Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetMedicalItemType(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-item-type")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _medicalItemTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicineType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medicine Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetMedicineType(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medicine-type")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _medicineTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicineCategory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medicine Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetMedicineCategory(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medicine-category")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _medicineCategoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDosageType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Dosage Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetDosageType(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-dosage-type")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _dosageTypeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListMedicalItem))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Medical Item")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListMedicalItemResult>))]
        public Task<IActionResult> GetListMedicalItem(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-medical-item")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListMedicalItemHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMedicalItem))]
        [OpenApiOperation(tags: _tag, Summary = "Save Medical Item")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMedicalItemRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveMedicalItem(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-medical-item")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveMedicalItemHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailMedicalItem))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Medical Item")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMedicalItemResult))]
        public Task<IActionResult> GetDetailMedicalItem(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-medical-item")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailMedicalItemHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMedicalItem))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Medical Item")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IdCollection), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMedicalItem(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-medical-item")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteMedicalItemHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListMedicalTreatment))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Medical Treatment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListMedicalTreatmentResult>))]
        public Task<IActionResult> GetListMedicalTreatment(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-medical-treatment")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListMedicalTreatmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMedicalTreatment))]
        [OpenApiOperation(tags: _tag, Summary = "Save Medical Treatment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMedicalTreatmentRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveMedicalTreatment(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-medical-treatment")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveMedicalTreatmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailMedicalTreatment))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Medical Treatment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetDetailMedicalTreatment(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-medical-treatment")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailMedicalTreatmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMedicalTreatment))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Medical Treatment")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IdCollection), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMedicalTreatment(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-medical-treatment")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteMedicalTreatmentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListMedicalCondition))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Medical Condition")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListMedicalConditionResult>))]
        public Task<IActionResult> GetListMedicalCondition(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-medical-condition")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListMedicalConditionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMedicalCondition))]
        [OpenApiOperation(tags: _tag, Summary = "Save Medical Condition")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMedicalConditionRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveMedicalCondition(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-medical-condition")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveMedicalConditionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailMedicalCondition))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Medical Condition")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMedicalConditionResult))]
        public Task<IActionResult> GetDetailMedicalCondition(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-medical-condition")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailMedicalConditionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMedicalCondition))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Medical Condition")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IdCollection), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMedicalCondition(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-medical-condition")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteMedicalConditionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListMedicalVaccine))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Medical Vaccine")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListMedicalVaccineResult>))]
        public Task<IActionResult> GetListMedicalVaccine(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-medical-vaccine")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListMedicalVaccineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMedicalVaccine))]
        [OpenApiOperation(tags: _tag, Summary = "Save Medical Vaccine")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMedicalVaccineRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveMedicalVaccine(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-medical-vaccine")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveMedicalVaccineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailMedicalVaccine))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Medical Vaccine")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMedicalVaccineResult))]
        public Task<IActionResult> GetDetailMedicalVaccine(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-medical-vaccine")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailMedicalVaccineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMedicalVaccine))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Medical Vaccine")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IdCollection), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMedicalVaccine(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-medical-vaccine")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteMedicalVaccineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListMedicalHospital))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Medical Hospital")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListMedicalHospitalResult>))]
        public Task<IActionResult> GetListMedicalHospital(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-medical-hospital")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListMedicalHospitalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailMedicalHospital))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Medical Hospital")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailMedicalHospitalRequest.IdHospital), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMedicalHospitalResult))]
        public Task<IActionResult> GetDetailMedicalHospital(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-medical-hospital")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailMedicalHospitalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMedicalHospital))]
        [OpenApiOperation(tags: _tag, Summary = "Save Medical Hospital")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMedicalHospitalRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveMedicalHospital(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-medical-hospital")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveMedicalHospitalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMedicalHospital))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Medical Hospital")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMedicalHospitalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMedicalHospital(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-medical-hospital")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteMedicalHospitalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetListMedicalDoctor))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Medical Doctor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(MedicalDataSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListMedicalDoctorResult>))]
        public Task<IActionResult> GetListMedicalDoctor(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-medical-doctor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListMedicalDoctorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMedicalDoctor))]
        [OpenApiOperation(tags: _tag, Summary = "Save Medical Doctor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMedicalDoctorRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveMedicalDoctor(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-medical-doctor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveMedicalDoctorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetDetailMedicalDoctor))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Medical Doctor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMedicalDoctorResult))]
        public Task<IActionResult> GetDetailMedicalDoctor(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-medical-doctor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailMedicalDoctorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMedicalDoctor))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Medical Doctor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IdCollection), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMedicalDoctor(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-medical-doctor")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteMedicalDoctorHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Medical Tapping System
        [FunctionName(nameof(GetMedicalTappingSystemPatientList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Tapping System Patient List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]        
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMedicalTappingSystemPatientListResponse>))]
        public Task<IActionResult> GetMedicalTappingSystemPatientList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-tapping-system-patient-list")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalTappingSystemPatientListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UpdateMedicalTappingSystemPatientStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Update Medical Tapping System Patient Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMedicalTappingSystemPatientStatusRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UpdateMedicalTappingSystemPatientStatusResponse))]
        public Task<IActionResult> UpdateMedicalTappingSystemPatientStatus(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-medical-tapping-system-patient-status")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _updateMedicalTappingSystemPatientStatusHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicalTappingSystemPatientCard))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Tapping System Patient Card")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalTappingSystemPatientCardRequest.TagId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UpdateMedicalTappingSystemPatientStatusResponse))]
        public Task<IActionResult> GetMedicalTappingSystemPatientCard(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-tapping-system-patient-card")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalTappingSystemPatientCardHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Medical Record Entry
        [FunctionName(nameof(GetMedicalRecordEntryStudentList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Record Entry Student List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStudentListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMedicalRecordEntryStudentListResponse>))]
        public Task<IActionResult> GetMedicalRecordEntryStudentList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-record-entry-student-list")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalRecordEntryStudentListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicalRecordEntryStaffList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Record Entry Staff List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.IdDesignation), In = ParameterLocation.Query, Type = typeof(int?))]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryStaffListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMedicalRecordEntryStaffListResponse>))]
        public Task<IActionResult> GetMedicalRecordEntryStaffList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-record-entry-staff-list")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalRecordEntryStaffListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicalRecordEntryOtherPatientList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Record Entry Other Patient List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryOtherPatientListRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMedicalRecordEntryOtherPatientListResponse>))]
        public Task<IActionResult> GetMedicalRecordEntryOtherPatientList(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-record-entry-other-patient-list")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalRecordEntryOtherPatientListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AddMedicalRecordEntryOtherPatient))]
        [OpenApiOperation(tags: _tag, Summary = "Add Medical Record Entry Other Patient")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMedicalRecordEntryOtherPatientRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMedicalRecordEntryOtherPatient(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-medical-record-entry-other-patient")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _addMedicalRecordEntryOtherPatientHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicalRecordEntryDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Record Entry Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryDetailRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryDetailRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryDetailRequest.Mode), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMedicalRecordEntryDetailResponse))]
        public Task<IActionResult> GetMedicalRecordEntryDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-record-entry-detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalRecordEntryDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicalRecordEntryClinicVisitHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Record Entry Clinic Visit History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryClinicVisitHistoryRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryClinicVisitHistoryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalRecordEntryClinicVisitHistoryRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMedicalRecordEntryClinicVisitHistoryResponse>))]
        public Task<IActionResult> GetMedicalRecordEntryClinicVisitHistory(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-record-entry-clinic-visit-history")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalRecordEntryClinicVisitHistoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PrintMedicalRecordEntryStudentPass))]
        [OpenApiOperation(tags: _tag, Summary = "Print Medical Record Entry Student Pass")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(PrintMedicalRecordEntryStudentPassRequest.IdMedicalRecordEntry), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(PrintMedicalRecordEntryStudentPassResponse))]
        public Task<IActionResult> PrintMedicalRecordEntryStudentPass(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/print-medical-record-entry-student-pass")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _printMedicalRecordEntryStudentPassHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Clinic Daily Report
        [FunctionName(nameof(GetClinicDailyReportVisitData))]
        [OpenApiOperation(tags: _tag, Summary = "Get Clinic Daily Report Visit Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClinicDailyReportVisitDataResponse))]
        public Task<IActionResult> GetClinicDailyReportVisitData(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-clinic-daily-report-visit-data")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getClinicDailyReportVisitDataHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetClinicDailyReportOfLeaveSchoolEarlyStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Clinic Daily Report Of Leave School Early Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetClinicDailyReportOfLeaveSchoolEarlyStudentResponse>))]
        public Task<IActionResult> GetClinicDailyReportOfLeaveSchoolEarlyStudent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-clinic-daily-report-of-leave-school-early-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getClinicDailyReportOfLeaveSchoolEarlyStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetClinicDailyReportInjuryVisit))]
        [OpenApiOperation(tags: _tag, Summary = "Get Clinic Daily Report Injury Visit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClinicDailyReportInjuryVisitResponse))]
        public Task<IActionResult> GetClinicDailyReportInjuryVisit(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-clinic-daily-report-injury-visit")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getClinicDailyReportInjuryVisitHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetClinicDailyReportExcludeInjuryVisit))]
        [OpenApiOperation(tags: _tag, Summary = "Get Clinic Daily Report Exclude Injury Visit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClinicDailyReportExcludeInjuryVisitReponse))]
        public Task<IActionResult> GetClinicDailyReportExcludeInjuryVisit(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-clinic-daily-report-exclude-injury-visit")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getClinicDailyReportExcludeInjuryVisitHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExportExcelClinicDailyReport))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Clinic Daily Report")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ClinicDailyReportDataRequest.Date), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelClinicDailyReport(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/export-excel-clinic-daily-report")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _exportExcelClinicDailyReportHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Medical Personal Data
        [FunctionName(nameof(GetGeneralMedicalInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Get General Medical Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGeneralMedicalInformationRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGeneralMedicalInformationRequest.Mode), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGeneralMedicalInformationResponse))]
        public Task<IActionResult> GetGeneralMedicalInformation(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-general-medical-information")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getGeneralMedicalInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveGeneralMedicalInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Save General Medical Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveGeneralMedicalInformationRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveGeneralMedicalInformation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-general-medical-information")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveGeneralMedicalInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetMedicalDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Get Medical Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMedicalDocumentRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMedicalDocumentRequest.Mode), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMedicalDocumentResponse>))]
        public Task<IActionResult> GetMedicalDocument(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-medical-document")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getMedicalDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveMedicalDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Save Medical Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveMedicalDocumentRequest), Required = true)]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveMedicalDocument(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-medical-document")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveMedicalDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DeleteMedicalDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Medical Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMedicalDocumentRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMedicalDocument(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-medical-document")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _deleteMedicalDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetGeneralPhysicalMeasurement))]
        [OpenApiOperation(tags: _tag, Summary = "Get General Physical Measurement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGeneralPhysicalMeasurementRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGeneralPhysicalMeasurementRequest.Mode), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetGeneralPhysicalMeasurementResponse>))]
        public Task<IActionResult> GetGeneralPhysicalMeasurement(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-general-physical-measurement")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getGeneralPhysicalMeasurementHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveGeneralPhysicalMeasurement))]
        [OpenApiOperation(tags: _tag, Summary = "Save General Physical Measurement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveGeneralPhysicalMeasurementRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveGeneralPhysicalMeasurement(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-general-physical-measurement")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveGeneralPhysicalMeasurementHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GetTreatmentAndMedicItemByCondition))]
        [OpenApiOperation(tags: _tag, Summary = "Get Treatment And Medic Item By Condition")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTreatmentAndMedicItemByConditionRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTreatmentAndMedicItemByConditionResponse))]
        public Task<IActionResult> GetTreatmentAndMedicItemByCondition(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-treatment-and-medic-item-by-condition")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getTreatmentAndMedicItemByConditionHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Emergency Student Contact
        [FunctionName(nameof(GetEmergencyStudentContact))]
        [OpenApiOperation(tags: _tag, Summary = "Get Emergency Student Contact")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(IdCollection.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetEmergencyStudentContactResult>))]

        public Task<IActionResult> GetEmergencyStudentContact(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-emergency-student-contact")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getEmergencyStudentContactHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Details Condition Treatment & Medication Entry
        [FunctionName(nameof(GetDetailsConditionData))]
        [OpenApiOperation(tags: _tag, Summary = "Get Details Condition Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailsConditionDataRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailsConditionDataRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailsConditionDataRequest.CheckInDate), In = ParameterLocation.Query, Type=typeof(DateTime))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailsCondtionDataResult))]
        public Task<IActionResult> GetDetailsConditionData(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-details-condition-data")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDetailsCondtionDataHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SaveDetailsConditionData))]
        [OpenApiOperation(tags: _tag, Summary = "Save Details Condition Data")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveDetailsConditionDataRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveDetailsConditionData(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-details-condition-data")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _saveDetailsConditionDataHandler.Execute(req, cancellationToken);
        }

        #endregion
    }
}
