using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Bekasi;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Semarang;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Serpong;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData.Simprug;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Bekasi;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Semarang;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Serpong;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Simprug;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Bekasi;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Semarang;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Serpong;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Simprug;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IReportData : IFnScoring
    {
        #region General API
        [Post("/BnsReport/ReportData/MasterGenerateReportData")]
        Task<ApiErrorResult<MasterGenerateReportDataResult>> MasterGenerateReportData([Body] MasterGenerateReportDataRequest query);

        [Post("/BnsReport/ReportData/GetStudentMasterData")]
        Task<ApiErrorResult<GetStudentMasterDataResult>> GetStudentMasterData([Body] GetStudentMasterDataRequest query);

        [Post("/BnsReport/ReportData/GetAttendanceReport")]
        Task<ApiErrorResult<GetAttendanceReportResult>> GetAttendanceReport([Body] GetAttendanceReportRequest query);

        [Post("/BnsReport/ReportData/GetElectivesReport")]
        Task<ApiErrorResult<GetElectivesReportResult>> GetElectivesReport([Body] GetElectivesReportRequest query);

        [Post("/BnsReport/ReportData/GetProgressStatusReport")]
        Task<ApiErrorResult<GetProgressStatusReportResult>> GetProgressStatusReport([Body] GetProgressStatusReportRequest query);

        [Post("/BnsReport/ReportData/GetTeacherCommentReport")]
        Task<ApiErrorResult<GetTeacherCommentReportResult>> GetTeacherCommentReport([Body] GetTeacherCommentReportRequest query);

        #endregion

        #region EL Serpong
        [Post("/BnsReport/ReportData/GetSerpongELAcademicSubjectsScore")]
        Task<ApiErrorResult<GetSerpongELAcademicSubjectsScoreResult>> GetSerpongELAcademicSubjectsScore([Body] GetSerpongELAcademicSubjectsScoreRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELAffectiveSubjectsScore")]
        Task<ApiErrorResult<GetSerpongELAffectiveSubjectsScoreResult>> GetSerpongELAffectiveSubjectsScore([Body] GetSerpongELAffectiveSubjectsScoreRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELTeacherComment")]
        Task<ApiErrorResult<GetSerpongELTeacherCommentResult>> GetSerpongELTeacherComment([Body] GetSerpongELTeacherCommentRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELProgressStatus")]
        Task<ApiErrorResult<GetSerpongELProgressStatusResult>> GetSerpongELProgressStatus([Body] GetSerpongELProgressStatusRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELElectives")]
        Task<ApiErrorResult<GetSerpongELElectivesResult>> GetSerpongELElectives([Body] GetSerpongELElectivesRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELSpiritScore")]
        Task<ApiErrorResult<GetSerpongELSpiritScoreResult>> GetSerpongELSpiritScore([Body] GetSerpongELSpiritScoreRequest query);

        #endregion

        #region ECY Serpong
        [Post("/BnsReport/ReportData/GetSerpongECYAcademicSubjectsScore")]
        Task<ApiErrorResult<GetSerpongECYAcademicSubjectsScoreResult>> GetSerpongECYAcademicSubjectsScore([Body] GetSerpongECYAcademicSubjectsScoreRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELTeacherComment")]
        Task<ApiErrorResult<GetSerpongECYTeacherCommentResult>> GetSerpongECYTeacherComment([Body] GetSerpongECYTeacherCommentRequest query);

        [Post("/BnsReport/ReportData/GetSerpongECYProgressStatus")]
        Task<ApiErrorResult<GetSerpongECYProgressStatusResult>> GetSerpongECYProgressStatus([Body] GetSerpongECYProgressStatusRequest query);

        [Post("/BnsReport/ReportData/GetSerpongECYElectives")]
        Task<ApiErrorResult<GetSerpongECYElectivesResult>> GetSerpongECYElectives([Body] GetSerpongECYElectivesRequest query);

        [Post("/BnsReport/ReportData/GetSerpongECYSpiritScore")]
        Task<ApiErrorResult<GetSerpongECYSpiritScoreResult>> GetSerpongECYSpiritScore([Body] GetSerpongECYSpiritScoreRequest query);

        #endregion

        #region MSHS Serpong

        [Post("/BnsReport/ReportData/GetSerpongMSHSCoreSubjectsScore")]
        Task<ApiErrorResult<GetSerpongMSHSCoreSubjectsScoreResult>> GetSerpongMSHSCoreSubjectsScore([Body] GetSerpongMSHSCoreSubjectsScoreRequest query);

        [Post("/BnsReport/ReportData/GetSerpongMSHSSpecialSubjectsScore")]
        Task<ApiErrorResult<GetSerpongMSHSSpecialSubjectsScoreResult>> GetSerpongMSHSSpecialSubjectsScore([Body] GetSerpongMSHSSpecialSubjectsScoreRequest query);

        [Post("/BnsReport/ReportData/GetSerpongMSHSCLA")]
        Task<ApiErrorResult<GetSerpongMSHSCLAScoreResult>> GetSerpongMSHSCLAHandler([Body] GetSerpongMSHSCLAScoreRequest query);

        [Post("/BnsReport/ReportData/GetSerpongMSHSSpiritScore")]
        Task<ApiErrorResult<GetSerpongMSHSSpiritScoreResult>> GetSerpongMSHSSpiritScore([Body] GetSerpongMSHSSpiritScoreRequest query);

        [Post("/BnsReport/ReportData/GetSerpongMSHSElectives")]
        Task<ApiErrorResult<GetSerpongMSHSElectivesResult>> GetSerpongMSHSElectives([Body] GetSerpongMSHSElectivesRequest query);

        [Post("/BnsReport/ReportData/GetSerpongMSHSTeacherComment")]
        Task<ApiErrorResult<GetSerpongMSHSTeacherCommentResult>> GetSerpongMSHSTeacherComment([Body] GetSerpongMSHSTeacherCommentRequest query);

        #endregion

        #region Semarang
        #region ECY Semarang
        [Post("/BnsReport/ReportData/GetSemarangECYExaminableSubjects")]
        Task<ApiErrorResult<GetSemarangECYExaminableSubjectsResult>> GetSemarangECYExaminableSubjects([Body] GetSemarangECYExaminableSubjectsRequest query);

        [Post("/BnsReport/ReportData/GetSemarangSpiritScore")]
        Task<ApiErrorResult<GetSemarangSpiritScoreResult>> GetSemarangSpiritScore([Body] GetSemarangSpiritScoreRequest query);

        [Post("/BnsReport/ReportData/GetSemarangSAPScore")]
        Task<ApiErrorResult<GetSemarangSAPScoreResult>> GetSemarangSAPScore([Body] GetSemarangSAPScoreRequest query);
        #endregion

        #region EL Semarang
        [Post("/BnsReport/ReportData/GetSemarangELExaminableSubject")]
        Task<ApiErrorResult<GetSemarangELExaminableSubjectsResult>> GetSemarangELExaminableSubject([Body] GetSemarangELExaminableSubjectsRequest query);

        [Post("/BnsReport/ReportData/GetSemarangELExaminableCompetenciesSubject")]
        Task<ApiErrorResult<GetSemarangELExaminableCompetenciesSubjectsResult>> GetSemarangELExaminableCompetencieSubject([Body] GetSemarangELExaminableCompetenciesSubjectsRequest query);

        [Post("/BnsReport/ReportData/GetSemarangLifeSkillScore")]
        Task<ApiErrorResult<GetSemarangLifeSkillScoreResult>> GetSemarangLifeSkillScore([Body] GetSemarangLifeSkillScoreRequest query);

        #endregion

        #region Semarang NatCur
        [Post("/BnsReport/ReportData/GetSemarangNatcurK1andK2SubjectScore")]
        Task<ApiErrorResult<GetSemarangNatcurK1andK2Result>> GetSemarangNatcurK1andK2SubjectScore([Body] GetSemarangNatcurK1andK2Request query);

        [Post("/BnsReport/ReportData/GetSemarangNatcurCambridgeSubject")]
        Task<ApiErrorResult<GetSemarangNatcurCambridgeSubjectResult>> GetSemarangNatcurCambridgeSubject([Body] GetSemarangNatcurCambridgeSubjectRequest query);

        [Post("/BnsReport/ReportData/GetSemarangNatCurComponentSubject")]
        Task<ApiErrorResult<GetSemarangNatCurComponentSubjectResult>> GetSemarangNatCurComponentSubject([Body] GetSemarangNatCurComponentSubjectRequest query);
        #endregion
        #endregion

        #region Bekasi
        #region ECY Bekasi
        [Post("/BnsReport/ReportData/GetBekasiECYExaminableSubjects")]
        Task<ApiErrorResult<GetBekasiECYExaminableSubjectsResult>> GetBekasiECYExaminableSubjects([Body] GetBekasiECYExaminableSubjectsRequest query);

        [Post("/BnsReport/ReportData/GetBekasiSpiritScore")]
        Task<ApiErrorResult<GetBekasiSpiritScoreResult>> GetBekasiSpiritScore([Body] GetBekasiSpiritScoreRequest query);

        [Post("/BnsReport/ReportData/GetBekasiSAPScore")]
        Task<ApiErrorResult<GetBekasiSAPScoreResult>> GetBekasiSAPScore([Body] GetBekasiSAPScoreRequest query);
        #endregion

        #region EL Bekasi
        [Post("/BnsReport/ReportData/GetBekasiELExaminableSubject")]
        Task<ApiErrorResult<GetBekasiELExaminableSubjectsResult>> GetBekasiELExaminableSubject([Body] GetBekasiELExaminableSubjectsRequest query);

        #endregion

        #region Bekasi NatCur
        [Post("/BnsReport/ReportData/GetBekasiNatcurK1andK2SubjectScore")]
        Task<ApiErrorResult<GetBekasiNatcurK1andK2Result>> GetBekasiNatcurK1andK2SubjectScore([Body] GetBekasiNatcurK1andK2Request query);

        [Post("/BnsReport/ReportData/GetBekasiNatcurCambridgeSubject")]
        Task<ApiErrorResult<GetBekasiNatcurCambridgeSubjectResult>> GetBekasiNatcurCambridgeSubject([Body] GetBekasiNatcurCambridgeSubjectRequest query);

        [Post("/BnsReport/ReportData/GetBekasiNatCurComponentSubject")]
        Task<ApiErrorResult<GetBekasiNatCurComponentSubjectResult>> GetBekasiNatCurComponentSubject([Body] GetBekasiNatCurComponentSubjectRequest query);
        #endregion
        #endregion

        #region DP Simprug

        [Post("/BnsReport/ReportData/GetSimprugDPSubjectScore")]
        Task<ApiErrorResult<GetSimprugDPSubjectScoreResult>> GetSimprugDPSubjectScore([Body] GetSimprugDPSubjectScoreRequest query);

        [Post("/BnsReport/ReportData/GetSimprugDPProgressStatus")]
        Task<ApiErrorResult<GetSimprugDPProgressStatusResult>> GetSimprugDPProgressStatus([Body] GetSimprugDPProgressStatusRequest query);

        #endregion

        #region MYP Simprug

        [Post("/BnsReport/ReportData/GetSimprugMYPSubjectScore")]
        Task<ApiErrorResult<GetSimprugMYPSubjectScoreResult>> GetSimprugMYPSubjectScore([Body] GetSimprugMYPSubjectScoreRequest query);

        #endregion

        #region PYP Simprug
        [Post("/BnsReport/ReportData/GetSimprugPYPApproachesToLearning")]
        Task<ApiErrorResult<GetSimprugPYPApproachesToLearningResult>> GetSimprugPYPApproachesToLearning([Body] GetSimprugPYPApproachesToLearningRequest query);

        [Post("/BnsReport/ReportData/GetSimprugPYPSubjectScore")]
        Task<ApiErrorResult<GetSimprugPYPSubjectScoreResult>> GetSimprugPYPSubjectScore([Body] GetSimprugPYPSubjectScoreRequest query);

        [Post("/BnsReport/ReportData/GetSimprugPYPInquiryScore")]
        Task<ApiErrorResult<GetSimprugPYPInquiryScoreResult>> GetSimprugPYPInquiryScore([Body] GetSimprugPYPInquiryScoreRequest query);

        [Post("/BnsReport/ReportData/GetSimprugPYPClassTeacherComments")]
        Task<ApiErrorResult<GetSimprugPYPClassTeacherCommentResult>> GetSimprugPYPClassTeacherComments([Body] GetSimprugPYPClassTeacherCommentRequest query);

        #endregion

        #region Simprug NatCur
        [Post("/BnsReport/ReportData/GetSimprugNatcurK1andK2SubjectScore")]
        Task<ApiErrorResult<GetSimprugNatcurK1andK2Request>> GetSimprugNatcurK1andK2SubjectScore([Body] GetSimprugNatcurK1andK2Result query);
        #endregion

        #region Master Template

        [Post("/BnsReport/ReportData/GetSerpongECYMasterTemplate")]
        Task<ApiErrorResult<GetSerpongECYMasterTemplateResult>> GetSerpongECYMasterTemplate([Body] GetSerpongECYMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELMasterTemplate")]
        Task<ApiErrorResult<GetSerpongELMasterTemplateResult>> GetSerpongELMasterTemplate([Body] GetSerpongELMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSerpongMSHSMasterTemplate")]
        Task<ApiErrorResult<GetSerpongMSHSMasterTemplateResult>> GetSerpongMSHSMasterTemplate([Body] GetSerpongMSHSMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSemarangECYMasterTemplate")]
        Task<ApiErrorResult<GetSemarangECYMasterTemplateResult>> GetSemarangECYMasterTemplate([Body] GetSemarangECYMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSemarangELMasterTemplate")]
        Task<ApiErrorResult<GetSemarangELMasterTemplateResult>> GetSemarangELMasterTemplate([Body] GetSemarangELMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSemarangMSHSMasterTemplate")]
        Task<ApiErrorResult<GetSemarangMSHSMasterTemplateResult>> GetSemarangMSHSMasterTemplate([Body] GetSemarangMSHSMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetBekasiECYMasterTemplate")]
        Task<ApiErrorResult<GetBekasiECYMasterTemplateResult>> GetBekasiECYMasterTemplate([Body] GetBekasiECYMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetBekasiELMasterTemplate")]
        Task<ApiErrorResult<GetBekasiELMasterTemplateResult>> GetBekasiELMasterTemplate([Body] GetBekasiELMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetBekasiMSHSMasterTemplate")]
        Task<ApiErrorResult<GetBekasiMSHSMasterTemplateResult>> GetBekasiMSHSMasterTemplate([Body] GetBekasiMSHSMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSimprugDPMasterTemplate")]
        Task<ApiErrorResult<GetSimprugDPMasterTemplateResult>> GetSimprugDPMasterTemplate([Body] GetSimprugDPMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSimprugMYPMasterTemplate")]
        Task<ApiErrorResult<GetSimprugMYPMasterTemplateResult>> GetSimprugMYPMasterTemplate([Body] GetSimprugMYPMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSimprugPYPMasterTemplate")]
        Task<ApiErrorResult<GetSimprugPYPMasterTemplateResult>> GetSimprugPYPMasterTemplate([Body] GetSimprugPYPMasterTemplateRequest query);

        #endregion

        #region Addtional Template

        [Post("/BnsReport/ReportData/CreateSignatureBNSReport")]
        Task<ApiErrorResult<CreateSignatureBNSReportResult>> CreateSignatureBNSReport([Body] CreateSignatureBNSReportRequest query);

        [Post("/BnsReport/ReportData/CreateHeaderBNSReport")]
        Task<ApiErrorResult<CreateHeaderBNSReportResult>> CreateHeaderBNSReport([Body] CreateHeaderBNSReportRequest query);

        [Post("/BnsReport/ReportData/CreateSignatureReport")]
        Task<ApiErrorResult<CreateSignatureReportResult>> CreateSignatureReport([Body] CreateSignatureReportRequest query);

        [Post("/BnsReport/ReportData/CreateHeaderFooterReport")]
        Task<ApiErrorResult<CreateHeaderFooterReportResult>> CreateHeaderFooterReport([Body] CreateHeaderFooterReportRequest query);
        #endregion

        #region Master Template NatCur
        [Post("/BnsReport/ReportData/GetSimprugMasterTemplateNatCur")]
        Task<ApiErrorResult<GetSimprugMasterTemplateNatCurResult>> GetSimprugMasterTemplateNatCur([Body] GetSimprugMasterTemplateNatCurRequest query);

        [Post("/BnsReport/ReportData/GetSemarangMasterTemplateNatCur")]
        Task<ApiErrorResult<GetSemarangMasterTemplateNatCurResult>> GetSemarangMasterTemplateNatCur([Body] GetSemarangMasterTemplateNatCurRequest query);

        [Post("/BnsReport/ReportData/GetBekasiMasterTemplateNatCur")]
        Task<ApiErrorResult<GetBekasiMasterTemplateNatCurResult>> GetBekasiMasterTemplateNatCur([Body] GetBekasiMasterTemplateNatCurRequest query);

        [Post("/BnsReport/ReportData/GetSerpongELNatcurMasterTemplate")]
        Task<ApiErrorResult<GetSerpongELNatcurMasterTemplateResult>> GetSerpongELNatcurMasterTemplate([Body] GetSerpongELNatcurMasterTemplateRequest query);


        [Post("/BnsReport/ReportData/GetSerpongMSHSNatcurMasterTemplate")]
        Task<ApiErrorResult<GetSerpongMSHSNatcurMasterTemplateResult>> GetSerpongMSHSNatcurMasterTemplate([Body] GetSerpongMSHSNatcurMasterTemplateRequest query);

        [Post("/BnsReport/ReportData/GetSerpongMSHSNatcurMasterTemplate2023")]
        Task<ApiErrorResult<GetSerpongMSHSNatcurMasterTemplateResult>> GetSerpongMSHSNatcurMasterTemplate2023([Body] GetSerpongMSHSNatcurMasterTemplateRequest query);


        #endregion
    }
}

