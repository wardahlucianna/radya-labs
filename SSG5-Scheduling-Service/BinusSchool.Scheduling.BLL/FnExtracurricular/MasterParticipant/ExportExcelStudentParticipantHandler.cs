using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class ExportExcelStudentParticipantHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMasterParticipant _masterParticipantApi;
        private readonly IMachineDateTime _dateTime;

        public ExportExcelStudentParticipantHandler(ISchedulingDbContext dbContext, IMasterParticipant masterParticipantApi, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _masterParticipantApi = masterParticipantApi;
            _dateTime = dateTime;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportExcelStudentParticipantRequest, ExportExcelStudentParticipantValidator>();

            // param desc
            var academicYearDesc = _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcademicYear).Select(x => x.Description).FirstOrDefault();

            ExportExcelStudentParticipantResult_ParamDesc paramDesc = new ExportExcelStudentParticipantResult_ParamDesc();
            paramDesc.AcademicYear = academicYearDesc;
            paramDesc.Semester = param.Semester;

            var title = "ElectivesParticipantSummary";

            // result
            var resultList = new List<ExportExcelStudentParticipantResult>();

            var masterParticipantPayload = await _masterParticipantApi.GetMasterParticipant(new GetMasterParticipantRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                Return = CollectionType.Lov
            });

            if (masterParticipantPayload.Payload?.Count() > 0)
            {
                var masterParticipantList = masterParticipantPayload.Payload.ToList();

                masterParticipantList = masterParticipantList.Where(x => param.IdExtracurricular.Contains(x.Extracurricular.Id)).ToList();

                foreach (var masterParticipant in masterParticipantList)
                {
                    if (param.IdExtracurricular.Contains(masterParticipant.Extracurricular.Id))
                    {
                        var studentParticipantDetailPayload = await _masterParticipantApi.GetStudentParticipantByExtracurricular(new GetStudentParticipantByExtracurricularRequest
                        {
                            IdExtracurricular = masterParticipant.Extracurricular.Id,
                            GetAll = true,
                            Return = CollectionType.Lov
                        });

                        var studentParticipantDetailList = new List<GetStudentParticipantByExtracurricularResult>();
                        if (studentParticipantDetailPayload.Payload?.Count() > 0)
                        {
                            studentParticipantDetailList = studentParticipantDetailPayload.Payload.ToList();
                        }
                        else
                        {
                            studentParticipantDetailList = null;
                        }

                        var result = new ExportExcelStudentParticipantResult
                        {
                            Extracurricular = new NameValueVm
                            {
                                Id = masterParticipant.Extracurricular.Id,
                                Name = masterParticipant.Extracurricular.Name
                            },
                            ParticipantMax = masterParticipant.ParticipantMax,
                            ParticipantMin = masterParticipant.ParticipantMin,
                            LevelGradeList = masterParticipant.LevelGradeList,
                            ParticipantList = studentParticipantDetailList
                        };

                        resultList.Add(result);
                    }
                }

                // generate excel
                var generateExcelByte = GenerateExcel(paramDesc, resultList, title);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
                };
            }
            else
            {
                var generateExcelByte = GenerateBlankExcel(title);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
                };
            }
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateBlankExcel(string sheetTitle)
        {
            var result = new byte[0];
            //string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(2);

                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                //foreach (string field in fieldDataList)
                //{
                //    var Judul = row.CreateCell(fieldCount);
                //    Judul.SetCellValue(field);
                //    row.Cells[fieldCount].CellStyle = style;
                //    fieldCount++;
                //}

                #endregion
                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public byte[] GenerateExcel(ExportExcelStudentParticipantResult_ParamDesc paramDesc, List<ExportExcelStudentParticipantResult> dataList, string sheetTitle)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);
                excelSheet.SetColumnWidth(1, 30 * 256);
                excelSheet.SetColumnWidth(2, 30 * 256);
                excelSheet.SetColumnWidth(3, 30 * 256);
                excelSheet.SetColumnWidth(4, 30 * 256);
                excelSheet.SetColumnWidth(5, 30 * 256);
                excelSheet.SetColumnWidth(6, 30 * 256);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();
                style.WrapText = true;

                ICellStyle styleTable = workbook.CreateCellStyle();
                styleTable.BorderBottom = BorderStyle.Thin;
                styleTable.BorderLeft = BorderStyle.Thin;
                styleTable.BorderRight = BorderStyle.Thin;
                styleTable.BorderTop = BorderStyle.Thin;
                styleTable.WrapText = true;

                ICellStyle styleHeader = workbook.CreateCellStyle();
                styleHeader.WrapText = true;

                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;
                styleHeaderTable.WrapText = true;


                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                //font.IsItalic = true;
                font.FontName = "Arial";
                font.FontHeightInPoints = 12;
                style.SetFont(font);

                styleTable.SetFont(font);

                IFont fontHeader = workbook.CreateFont();
                fontHeader.FontName = "Arial";
                fontHeader.FontHeightInPoints = 12;
                fontHeader.IsBold = true;
                styleHeader.SetFont(fontHeader);

                styleHeaderTable.SetFont(fontHeader);

                int rowIndex = 0;
                int startColumn = 1;

                //Title 
                rowIndex++;
                IRow titleRow = excelSheet.CreateRow(rowIndex);
                var cellTitleRow = titleRow.CreateCell(1);
                cellTitleRow.SetCellValue("Electives Participant Summary");
                cellTitleRow.CellStyle = styleHeader;

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[3] { "Academic Year", "Semester", "Generated Date" };
                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue(paramDesc.AcademicYear);
                    if (field == "Semester")
                        cellValueParamRow.SetCellValue(paramDesc.Semester);
                    if (field == "Generated Date")
                        cellValueParamRow.SetCellValue(string.Format("{0:dd/mm/yyyy hh:mm}", _dateTime.ServerTime));
                    rowIndex++;
                }

                //Content
                rowIndex += 1;
                var headerList = new string[9] { "Electives Name", "Grade", "Student ID", "Student Name", "Homeroom", "Join Date", "Price", "Payment Status", "Status" };
                int startColumnHeader = startColumn;

                IRow sumRow2 = excelSheet.CreateRow(rowIndex);
                foreach (string header in headerList)
                {
                    var cellLevelHeader = sumRow2.CreateCell(startColumnHeader);
                    cellLevelHeader.SetCellValue(header);
                    cellLevelHeader.CellStyle = styleHeaderTable;

                    startColumnHeader++;
                }
                rowIndex += 1;
                foreach (var data in dataList)
                {
                    if (data.ParticipantList != null)
                    {
                        foreach (var itemData in data.ParticipantList)
                        {

                            IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                            var cellExtracurricularName = totalRow2.CreateCell(1);
                            cellExtracurricularName.SetCellValue(data.Extracurricular.Name);
                            cellExtracurricularName.CellStyle = styleTable;

                            var cellGrade = totalRow2.CreateCell(2);
                            cellGrade.SetCellValue(string.Join("; ", data.LevelGradeList.Select(x => x.Grade.Description)));
                            cellGrade.CellStyle = styleTable;

                            var cellIdStudent = totalRow2.CreateCell(3);
                            cellIdStudent.SetCellValue(itemData.Student.Id);
                            cellIdStudent.CellStyle = styleTable;

                            var cellStudentName = totalRow2.CreateCell(4);
                            cellStudentName.SetCellValue(itemData.Student.Name);
                            cellStudentName.CellStyle = styleTable;

                            var cellHomeroom = totalRow2.CreateCell(5);
                            cellHomeroom.SetCellValue(itemData.Homeroom.Name);
                            cellHomeroom.CellStyle = styleTable;

                            var cellJoinDate = totalRow2.CreateCell(6);
                            cellJoinDate.SetCellValue(string.Format("{0:dd-MMM-yyyy}", itemData.JoinDate));
                            cellJoinDate.CellStyle = styleTable;

                            var cellPrice = totalRow2.CreateCell(7);
                            cellPrice.SetCellValue(itemData.Price == null ? "-" : itemData.Price.ToString());
                            cellPrice.CellStyle = styleTable;

                            var cellPaymentStatus = totalRow2.CreateCell(8);
                            cellPaymentStatus.SetCellValue(itemData.PaymentStatus == null ? "No Invoice" : (itemData.PaymentStatus == true ? "Paid" : "Unpaid"));
                            cellPaymentStatus.CellStyle = styleTable;

                            var cellStatus = totalRow2.CreateCell(9);
                            cellStatus.SetCellValue(itemData.Status == true ? "Active" : "Inactive");
                            cellStatus.CellStyle = styleTable;

                            rowIndex++;
                        }
                    }
                }

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }
    }
}
