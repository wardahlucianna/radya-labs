using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scoring.FnScoring;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.CommonData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.ComponentData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Semarang;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Serpong;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateData.Simprug;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Bekasi;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Semarang;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Serpong;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportData.MasterTemplateDataNatCur.Simprug;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.Util.FnConverter.BnsReportToPdf;
using BinusSchool.Util.FnConverter.BnsReportToPdf.Validator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace BinusSchool.Util.FnConverter.BnsReportToPdf
{
    public class ConvertBnsReportToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IConverter _converter;
        private readonly IReportData _reportData;

        public ConvertBnsReportToPdfHandler(
            IConverter converter, 
            IReportData reportData
            )
        {
            _converter = converter;
            _reportData = reportData;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = await Request.ValidateBody<ConvertBnsReportToPdfRequest, ConvertBnsReportToPdfValidator>();

            var containerName = "";
            var HtmlOutput = "";
            var StorageSetting = "";
            var GenerateStatusList = new List<string>();
            var orientationType = Orientation.Portrait;
            var spacing = 1;

            var fileResult = default(ConvertBnsReportToPdfResult);
            var setFooter = new FooterSettings();
            var setMargin = new MarginSettings();
            var setGlobal = new GlobalSettings();
            var pdfHeader = new CreateHeaderBNSReportRequest();
            var htmlGeneratedHeader = new CreateHeaderFooterReportResult();
            var htmlGeneratedHeader2 = new CreateHeaderBNSReportResult();

            #region Serpong
            if (request.TemplateName == "SerpongELNatcurMaster")
            {
                var htmlGenerated = await _reportData.GetSerpongELNatcurMasterTemplate(new GetSerpongELNatcurMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                GenerateStatusList = htmlGenerated.Payload.GenerateStatus;
                pdfHeader = htmlGenerated.Payload.Header;
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (htmlGenerated.Payload.GlobalSetting.PaperSize != null ? (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize : PaperKind.A4),
                    Outline = false,
                    Margins = setMargin
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                //setFooter = new FooterSettings { FontName = "Arial", FontSize = 8, Line = false, Left = "       Date Printed: " + DateTime.Now.DayOfWeek + ", " + DateTime.Now.ToString("D") + "\n       Page [page] of [toPage]", Spacing = 1 };
            }
            else if (request.TemplateName == "SerpongMSHSNatcurMaster")
            {

                var htmlGenerated = await _reportData.GetSerpongMSHSNatcurMasterTemplate(new GetSerpongMSHSNatcurMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                GenerateStatusList = htmlGenerated.Payload.GenerateStatus;
                pdfHeader = htmlGenerated.Payload.Header;
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (htmlGenerated.Payload.GlobalSetting.PaperSize != null ? (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize : PaperKind.A4),
                    Outline = false,
                    Margins = setMargin
                };
                //setFooter = new FooterSettings { FontName = "Arial", FontSize = 8, Line = false, Left = "       Date Printed: " + DateTime.Now.DayOfWeek + ", " + DateTime.Now.ToString("D") + "\n       Page [page] of [toPage]", Spacing = 1 };

            }
            else if (request.TemplateName == "SerpongMSHSNatcurMaster2023")
            {

                var htmlGenerated = await _reportData.GetSerpongMSHSNatcurMasterTemplate2023(new GetSerpongMSHSNatcurMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                GenerateStatusList = htmlGenerated.Payload.GenerateStatus;
                pdfHeader = htmlGenerated.Payload.Header;
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (htmlGenerated.Payload.GlobalSetting.PaperSize != null ? (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize : PaperKind.A4),
                    Outline = false,
                    Margins = setMargin
                };
                //setFooter = new FooterSettings { FontName = "Arial", FontSize = 8, Line = false, Left = "       Date Printed: " + DateTime.Now.DayOfWeek + ", " + DateTime.Now.ToString("D") + "\n       Page [page] of [toPage]", Spacing = 1 };

            }
            #endregion

            #region Semarang
            else if (request.TemplateName == "SemarangECYMaster")
            {
                var htmlGenerated = await _reportData.GetSemarangECYMasterTemplate(new GetSemarangECYMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                pdfHeader = null;
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header.ContainerName,
                    FileName = htmlGenerated.Payload.Header.FileName,
                    ContentType = htmlGenerated.Payload.Header.ContentType,
                    Location = htmlGenerated.Payload.Header.Location,
                    Html = htmlGenerated.Payload.Header.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize,
                    Outline = false,
                    DPI = 200,
                    Margins = setMargin
                };
            }
            else if (request.TemplateName == "SemarangELMaster")
            {
                var htmlGenerated = await _reportData.GetSemarangELMasterTemplate(new GetSemarangELMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                pdfHeader = null;
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header.ContainerName,
                    FileName = htmlGenerated.Payload.Header.FileName,
                    ContentType = htmlGenerated.Payload.Header.ContentType,
                    Location = htmlGenerated.Payload.Header.Location,
                    Html = htmlGenerated.Payload.Header.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize,
                    Outline = false,
                    DPI = 200,
                    Margins = setMargin
                };
            }
            else if (request.TemplateName == "SemarangMSMaster" || request.TemplateName == "SemarangHSMaster")
            {
                var htmlGenerated = await _reportData.GetSemarangMSHSMasterTemplate(new GetSemarangMSHSMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                pdfHeader = null;
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header.ContainerName,
                    FileName = htmlGenerated.Payload.Header.FileName,
                    ContentType = htmlGenerated.Payload.Header.ContentType,
                    Location = htmlGenerated.Payload.Header.Location,
                    Html = htmlGenerated.Payload.Header.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize,
                    Outline = false,
                    DPI = 200,
                    Margins = setMargin
                };
            }
            else if (request.TemplateName == "SemarangNatCur")
            {
                var htmlGenerated = await _reportData.GetSemarangMasterTemplateNatCur(new GetSemarangMasterTemplateNatCurRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdSchool = request.IdSchool,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                pdfHeader = null;
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header.ContainerName,
                    FileName = htmlGenerated.Payload.Header.FileName,
                    ContentType = htmlGenerated.Payload.Header.ContentType,
                    Location = htmlGenerated.Payload.Header.Location,
                    Html = htmlGenerated.Payload.Header.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize,
                    Outline = false,
                    DPI = 200,
                    Margins = setMargin
                };
                spacing = 5;
            }
            #endregion

            #region Bekasi
            else if (request.TemplateName == "BekasiNatCur")
            {
                var htmlGenerated = await _reportData.GetBekasiMasterTemplateNatCur(new GetBekasiMasterTemplateNatCurRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdSchool = request.IdSchool,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                pdfHeader = null;
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header.ContainerName,
                    FileName = htmlGenerated.Payload.Header.FileName,
                    ContentType = htmlGenerated.Payload.Header.ContentType,
                    Location = htmlGenerated.Payload.Header.Location,
                    Html = htmlGenerated.Payload.Header.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize,
                    Outline = false,
                    DPI = 200,
                    Margins = setMargin
                };
                spacing = 5;
            }
            #endregion

            #region Simprug
            else if (request.TemplateName == "SimprugDPMaster")
            {
                var htmlGenerated = await _reportData.GetSimprugDPMasterTemplate(new GetSimprugDPMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                //containerName = htmlGenerated.Payload.ContainerName;
                orientationType = (htmlGenerated.Payload.TemplateBNSSetting != null ? (htmlGenerated.Payload.TemplateBNSSetting.Orientation == 0 ? Orientation.Landscape : Orientation.Portrait) : Orientation.Landscape);
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                GenerateStatusList = htmlGenerated.Payload.GenerateStatus;
                pdfHeader = htmlGenerated.Payload.Header;
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.TemplateBNSSetting.MarginTop,
                    Bottom = htmlGenerated.Payload.TemplateBNSSetting.MarginBottom,
                    Left = htmlGenerated.Payload.TemplateBNSSetting.MarginLeft,
                    Right = htmlGenerated.Payload.TemplateBNSSetting.MarginRight,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.TemplateBNSSetting.Orientation,
                    PaperSize = (htmlGenerated.Payload.TemplateBNSSetting.PaperSize != null ? (PaperKind)htmlGenerated.Payload.TemplateBNSSetting.PaperSize : PaperKind.Letter),
                    Outline = false,
                    Margins = setMargin
                };
                //setFooter = new FooterSettings { FontName = "Arial", FontSize = 8, Line = false, Left = "       Date Printed: " + DateTime.Now.DayOfWeek + ", " + DateTime.Now.ToString("D") + "\n       Page [page] of [toPage]", Spacing = 1 };
            }
            else if (request.TemplateName == "SimprugPYPMaster")
            {
                var htmlGenerated = await _reportData.GetSimprugPYPMasterTemplate(new GetSimprugPYPMasterTemplateRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                orientationType = (htmlGenerated.Payload.TemplateBNSSetting != null ? (htmlGenerated.Payload.TemplateBNSSetting.Orientation == 0 ? Orientation.Landscape : Orientation.Portrait) : Orientation.Portrait);
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                GenerateStatusList = htmlGenerated.Payload.GenerateStatus;
                pdfHeader = null;
                //pdfHeader = htmlGenerated.Payload.Header;
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.TemplateBNSSetting.MarginTop,
                    Bottom = htmlGenerated.Payload.TemplateBNSSetting.MarginBottom,
                    Left = htmlGenerated.Payload.TemplateBNSSetting.MarginLeft,
                    Right = htmlGenerated.Payload.TemplateBNSSetting.MarginRight,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.TemplateBNSSetting.Orientation,
                    PaperSize = (htmlGenerated.Payload.TemplateBNSSetting.PaperSize != null ? (PaperKind)htmlGenerated.Payload.TemplateBNSSetting.PaperSize : PaperKind.Letter),
                    Outline = false,
                    Margins = setMargin
                };
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header.ContainerName,
                    FileName = htmlGenerated.Payload.Header.FileName,
                    ContentType = htmlGenerated.Payload.Header.ContentType,
                    Location = htmlGenerated.Payload.Header.Location,
                    Html = htmlGenerated.Payload.Header.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
            }
            else if (request.TemplateName == "SimprugNatCur")
            {
                var htmlGenerated = await _reportData.GetSimprugMasterTemplateNatCur(new GetSimprugMasterTemplateNatCurRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    IdSchool = request.IdSchool,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                pdfHeader = null;
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header.ContainerName,
                    FileName = htmlGenerated.Payload.Header.FileName,
                    ContentType = htmlGenerated.Payload.Header.ContentType,
                    Location = htmlGenerated.Payload.Header.Location,
                    Html = htmlGenerated.Payload.Header.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting.Orientation,
                    PaperSize = (PaperKind)htmlGenerated.Payload.GlobalSetting.PaperSize,
                    Outline = false,
                    DPI = 200,
                    Margins = setMargin
                };
                spacing = 5;
            }
            #endregion

            #region Master Generate
            else
            {
                var htmlGenerated = await _reportData.MasterGenerateReportData(new MasterGenerateReportDataRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    //IdSchool = request.IdSchool,
                    IdStudent = request.IdStudent,
                    IdGrade = request.IdGrade,
                    Semester = request.Semester,
                    IdPeriod = request.IdPeriod
                });
                containerName = htmlGenerated.Payload.ContainerName;
                StorageSetting = htmlGenerated.Payload.StorageSetting;
                HtmlOutput = htmlGenerated.Payload.HtmlOutput;
                pdfHeader = null;
                htmlGeneratedHeader.Header = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Header?.ContainerName,
                    FileName = htmlGenerated.Payload.Header?.FileName,
                    ContentType = htmlGenerated.Payload.Header?.ContentType,
                    Location = htmlGenerated.Payload.Header?.Location,
                    Html = htmlGenerated.Payload.Header?.Html,
                };
                htmlGeneratedHeader.Footer = new GetMasterTemplateResult_File
                {
                    ContainerName = htmlGenerated.Payload.Footer?.ContainerName,
                    FileName = htmlGenerated.Payload.Footer?.FileName,
                    ContentType = htmlGenerated.Payload.Footer?.ContentType,
                    Location = htmlGenerated.Payload.Footer?.Location,
                    Html = htmlGenerated.Payload.Footer?.Html,
                };
                setMargin = new MarginSettings
                {
                    Top = htmlGenerated.Payload.Margin.Top,
                    Bottom = htmlGenerated.Payload.Margin.Bottom,
                    Left = htmlGenerated.Payload.Margin.Left,
                    Right = htmlGenerated.Payload.Margin.Right,
                    Unit = Unit.Inches
                };
                setGlobal = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = (Orientation)htmlGenerated.Payload.GlobalSetting?.Orientation,
                    PaperSize = (PaperKind)htmlGenerated.Payload.GlobalSetting?.PaperSize,
                    Outline = false,
                    DPI = 200,
                    Margins = setMargin
                };
                spacing = 5;
            }
            #endregion

            if (pdfHeader != null)
            {
                containerName = request.IdSchool + "/" + pdfHeader.AcademicYear.Code + "/" + pdfHeader.Homeroom.Code + "/" + pdfHeader.Student.Id;

                var htmlGeneratedHeaderV2 = await _reportData.CreateHeaderBNSReport(new CreateHeaderBNSReportRequest
                {
                    IdReportTemplate = request.IdReportTemplate,
                    IdReportType = request.IdReportType,
                    TemplateName = request.TemplateName,
                    ContainerName = containerName,
                    Student = pdfHeader.Student,
                    //School = schoolResult,
                    AcademicYear = pdfHeader.AcademicYear,
                    Homeroom = pdfHeader.Homeroom,
                    IdPeriod = request.IdPeriod,
                    CA = pdfHeader.CA,
                    Principal = pdfHeader.Principal
                });

                htmlGeneratedHeader2 = htmlGeneratedHeaderV2.Payload;
            }

            var blobName = containerName + "/" + $"BNS_Report_{request.IdStudent}_{DateTime.Now.ToString("MMddyyyyHHmmss")}.pdf";

            var serviceClient = new BlobServiceClient(StorageSetting);

            var baseContainer = "bns-report";

            if (request.IsDraft)
                baseContainer = "draft-bns-report";

            var containerClient = serviceClient.GetBlobContainerClient(baseContainer);
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobName);
            var doc = new HtmlToPdfDocument();

            if (setGlobal != null)
            {
                doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = setGlobal,
                    Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = HtmlOutput,
                        WebSettings = { DefaultEncoding = "utf-8"},
                        HeaderSettings = { FontName = "Calibri", FontSize = 11, Line = false, HtmlUrl = htmlGeneratedHeader?.Header?.Location?.AbsoluteUri.ToString()??"", Spacing = spacing },
                        FooterSettings = { FontName = "Calibri", FontSize = 11, Line = false, HtmlUrl = htmlGeneratedHeader?.Footer?.Location?.AbsoluteUri.ToString()??"", Spacing = spacing }
                    }
                }
                };
            }
            else
            {
                doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = orientationType,
                        PaperSize = PaperKind.Letter,
                        Outline = false,
                        Margins = setMargin
                    },
                    Objects = {
                        new ObjectSettings() {
                            PagesCount = true,
                            HtmlContent = HtmlOutput,
                            WebSettings = { DefaultEncoding = "utf-8"},
                            HeaderSettings = { FontName = "Calibri", FontSize = 11, Line = false, HtmlUrl = htmlGeneratedHeader2?.Header.Location.ToString(), Spacing = 1 },
                            FooterSettings = { FontName = "Calibri", FontSize = 11, Line = false, HtmlUrl = htmlGeneratedHeader2?.Footer.Location.ToString(), Spacing = 1 }
                        }
                    }
                };
            }

            var bytes = _converter.Convert(doc);

            // save to storage
            var blobResult = await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true, CancellationToken);
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
            {
                ContentType = "application/pdf",
                CacheControl = "max-age=5"
            });

            var rawBlobResult = blobResult.GetRawResponse();

            if (!(rawBlobResult.Status == StatusCodes.Status200OK || rawBlobResult.Status == StatusCodes.Status201Created))
                throw new Exception(rawBlobResult.ReasonPhrase);

            await blobClient.SetMetadataAsync(new Dictionary<string, string>
            {
                { "lastUpdate", DateTime.MinValue.ToString("O") }
            }, cancellationToken: CancellationToken);

            //generate SAS uri with expire time in 10 minutes
            var sasUri = GenerateSasUri(blobClient);
            fileResult = new ConvertBnsReportToPdfResult
            {
                FileName = blobName,
                ContentType = "application/pdf",
                Location = sasUri.Scheme + @"://" + sasUri.Host + "" + sasUri.AbsolutePath,
                HtmlBody = HtmlOutput,
                HtmlHeader = pdfHeader != null ? (htmlGeneratedHeader2?.Header?.Html ?? "") : (htmlGeneratedHeader?.Header?.Html ?? ""),
                HtmlFooter = pdfHeader != null ? (htmlGeneratedHeader2?.Footer?.Html ?? "") : (htmlGeneratedHeader?.Footer?.Html ?? ""),
                GenerateStatus = GenerateStatusList
            };

            return new JsonResult(fileResult, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));

        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.Add(TimeSpan.FromMinutes(5));

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }
    }
}

