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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnExtracurricular.PrivilegeUserElective;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class ExportExcelUnSubmittedScoreHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IExtracurricularScore _extracurricularScoreApi;
        private readonly IMachineDateTime _dateTime;
        private readonly GetPrivilegeUserElectiveHandler _getPrivilegeUserElectiveHandler;
        public ExportExcelUnSubmittedScoreHandler(
            ISchedulingDbContext dbContext,
            IExtracurricularScore extracurricularScoreApi,
            IMachineDateTime dateTime,
            GetPrivilegeUserElectiveHandler getPrivilegeUserElectiveHandler)
        {
            _dbContext = dbContext;
            _extracurricularScoreApi = extracurricularScoreApi;
            _dateTime = dateTime;
            _getPrivilegeUserElectiveHandler = getPrivilegeUserElectiveHandler;
        }
        protected override async Task<IActionResult> RawHandler()
        {
            //var param = Request.ValidateParams<GetExtracurricularStudentScoreRequest>(nameof(GetExtracurricularStudentScoreRequest.IdAcademicYear), nameof(GetExtracurricularStudentScoreRequest.Semester), nameof(GetExtracurricularStudentScoreRequest.IdExtracurricular));

            var param = await Request.GetBody<GetUnSubmittedScoreRequest>();

            var AcademicYear = _dbContext.Entity<MsAcademicYear>()
                               .Where(a => a.Id == param.IdAcademicYear)
                               .FirstOrDefault();

            var getPrivilegeUserElective = await _getPrivilegeUserElectiveHandler.GetAvailabilityPositionUserElective(new GetPrivilegeUserElectiveRequest
            {
                IdUser = param.IdBinusian,
                IdSchool = AcademicYear.IdSchool,
                IdAcademicYear = param.IdAcademicYear
            });

            var PrivilegeUserElectiveList = getPrivilegeUserElective.Select(x => x.IdExtracurricular).ToList();

            var ElectiveList = await _dbContext.Entity<MsExtracurricular>()
              .Where(a => a.ExtracurricularGradeMappings.Any(b => b.Grade.Level.IdAcademicYear == param.IdAcademicYear))
              .Where(a => PrivilegeUserElectiveList.Any(z => z == a.Id) && a.Status == true)
              .ToListAsync(CancellationToken);

            var GetUnsubmittedScore = await _extracurricularScoreApi.GetUnSubmittedScore(new GetUnSubmittedScoreRequest
            {
                GetAll = true,
                IdAcademicYear = param.IdAcademicYear,
                IdBinusian = param.IdBinusian
                //Semester = param.Semester
            });

            var UnsubmittedScoreList = GetUnsubmittedScore.Payload
                .Select(x => new
                {
                    Extracurricular = new NameValueVm
                    {
                        Id = x.Extracurricular.Id,
                        Name = x.Extracurricular.Description
                    },
                    Supervisor = x.Supervisior,
                    Total = x.Total
                })
                .Where(x => ElectiveList.Select(x => x.Id).Contains(x.Extracurricular.Id))
                .ToList();

            var filteredElectiveList = ElectiveList.Where(x => !UnsubmittedScoreList.Select(y => y.Extracurricular.Id).Contains(x.Id)).ToList();

            var unionElectiveList = UnsubmittedScoreList.Union(filteredElectiveList.Select(x => new
            {
                Extracurricular = new NameValueVm
                {
                    Id = x.Id,
                    Name = x.Name
                },
                Supervisor = (string)null,
                Total = 0
            }));


            var getExtracurricularGradeMapping = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                .Include(x => x.Grade)
                .Where(x => unionElectiveList.Select(x => x.Extracurricular.Id).Contains(x.IdExtracurricular))
                .ToListAsync(CancellationToken);

            var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                .Where(x => unionElectiveList.Select(x => x.Extracurricular.Id).Contains(x.IdExtracurricular))
                .ToListAsync(CancellationToken);


            var exportExcelUnsubmittedScoreList = new List<ExportExcelUnsubmittedScoreResult>();
            foreach (var data in unionElectiveList)
            {
                var exportExcelUnsubmittedScoreData = new ExportExcelUnsubmittedScoreResult();
                var filterExtracurricular = ElectiveList.Where(x => x.Id == data.Extracurricular.Id).FirstOrDefault();

                var getStudentScore = await _extracurricularScoreApi.GetExtracurricularStudentScore(new GetExtracurricularStudentScoreRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = filterExtracurricular.Semester,
                    IdExtracurricular = data.Extracurricular.Id
                });
                var getStudentScoreList = getStudentScore.Payload;

                var extracurricularGradeMappingData = getExtracurricularGradeMapping.Where(x => x.IdExtracurricular == data.Extracurricular.Id).ToList();
                var extracurcciularParticipant = getExtracurricularParticipant.Where(x => x.IdExtracurricular == data.Extracurricular.Id).ToList();


                exportExcelUnsubmittedScoreData.AcademicYear = AcademicYear.Code;
                exportExcelUnsubmittedScoreData.Semester = filterExtracurricular.Semester.ToString();
                exportExcelUnsubmittedScoreData.Extracurricular = new NameValueVm
                {
                    Name = data.Extracurricular.Name,
                    Id = data.Extracurricular.Id
                };
                exportExcelUnsubmittedScoreData.ElectiveGrade = string.Join("; ", extracurricularGradeMappingData.Select(x => x.Grade.Description));

                if(data.Supervisor != null)
                {
                    var unsubmittedScoreData = new ExportExcelUnsubmittedScoreUnsubmittedScore();
                    unsubmittedScoreData.SpvCoach = data.Supervisor;
                    unsubmittedScoreData.TotalEntry = (extracurcciularParticipant.Count() - data.Total).ToString();
                    unsubmittedScoreData.TotalParticipant = extracurcciularParticipant.Count().ToString();
                    exportExcelUnsubmittedScoreData.UnsubmittedScore = unsubmittedScoreData;
                }

                var studentScoreList = new List<ExportExcelUnsubmittedScoreStudentScore>();
                foreach(var item in getStudentScoreList.Body)
                {
                    var studentScoreData = new ExportExcelUnsubmittedScoreStudentScore();
                    studentScoreData.Student = new NameValueVm
                    {
                        Id = item.IdStudent,
                        Name = item.StudentName
                    };
                    studentScoreData.StudentGrade = item.Grade;
                    studentScoreData.Class = item.Homeroom;
                    studentScoreList.Add(studentScoreData);
                }

                exportExcelUnsubmittedScoreData.StudentScore = studentScoreList.OrderBy(x => x.Student.Name).ToList();
                exportExcelUnsubmittedScoreList.Add(exportExcelUnsubmittedScoreData);
            }

            var checkExtracurricularStatus = await _dbContext.Entity<MsExtracurricular>()
                .Where(x => exportExcelUnsubmittedScoreList.Select(y => y.Extracurricular.Id).Contains(x.Id))
                .Where(x => x.Status == true)
                .Select(x => x.Id)
                .ToListAsync(CancellationToken);

            exportExcelUnsubmittedScoreList = exportExcelUnsubmittedScoreList
                .Where(x => checkExtracurricularStatus.Contains(x.Extracurricular.Id))
                .OrderBy(x => x.AcademicYear).ThenBy(x => x.Semester).ThenBy(x => x.Extracurricular.Name).ThenBy(x => x.ElectiveGrade)
                .ToList();

            var title = "UnSubmitted Score";

            if (UnsubmittedScoreList != null)
            {
                var generateExcelByte = GenerateExcel(exportExcelUnsubmittedScoreList, AcademicYear.Description);
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

        public void SetDynamicColumnWidthExcel(int columnComponentCount, ref ISheet excelSheet)
        {
            for (int i = 0; i < columnComponentCount; i++)
            {
                excelSheet.SetColumnWidth(i, 30 * 256);
            }
        }

        public byte[] GenerateExcel(List<ExportExcelUnsubmittedScoreResult> data, string academicYear)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Student Score");

                int columnComponentCount = 7;

                //Create style for header
                ICellStyle headerStyle = workbook.CreateCellStyle();

                //Set border style 
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.VerticalAlignment = VerticalAlignment.Center;
                headerStyle.Alignment = HorizontalAlignment.Center;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsBold = true;
                headerStyle.SetFont(font);

                //Create style for header
                ICellStyle dataStyle = workbook.CreateCellStyle();

                //Set border style 
                dataStyle.BorderBottom = BorderStyle.Thin;
                dataStyle.BorderLeft = BorderStyle.Thin;
                dataStyle.BorderRight = BorderStyle.Thin;
                dataStyle.BorderTop = BorderStyle.Thin;
                dataStyle.VerticalAlignment = VerticalAlignment.Center;
                dataStyle.Alignment = HorizontalAlignment.Left;
                dataStyle.WrapText = true;

                //Set font style
                IFont Datafont = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                Datafont.FontName = "Arial";
                Datafont.FontHeightInPoints = 12;
                dataStyle.SetFont(Datafont);

                //header 
                //IRow row = excelSheet.CreateRow(0);

                //Title 

                IRow row = excelSheet.CreateRow(0);
                var cellTitleRow = row.CreateCell(0);
                cellTitleRow.SetCellValue("Student Score");
                cellTitleRow.CellStyle = headerStyle;
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, 7));

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Academic Year :");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(academicYear);
                row.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 7; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = dataStyle;
                }
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 7));
                            
                row = excelSheet.CreateRow(row.RowNum + 1);

                //Body

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Academic Year");
                row.Cells[0].CellStyle = headerStyle;
                row.CreateCell(1).SetCellValue("Semester");
                row.Cells[1].CellStyle = headerStyle;             
                row.CreateCell(2).SetCellValue("Elective Name");
                row.Cells[2].CellStyle = headerStyle;
                row.CreateCell(3).SetCellValue("Grade");
                row.Cells[3].CellStyle = headerStyle;
                row.CreateCell(4).SetCellValue("Student ID");
                row.Cells[4].CellStyle = headerStyle;
                row.CreateCell(5).SetCellValue("Student Name");
                row.Cells[5].CellStyle = headerStyle;
                row.CreateCell(6).SetCellValue("Class");
                row.Cells[6].CellStyle = headerStyle;
                row.CreateCell(7).SetCellValue("Grade");
                row.Cells[7].CellStyle = headerStyle;


                ISheet excelSheet2 = workbook.CreateSheet("Unsubmitted Score");

                int columnComponentCount2 = 6;

                //Title 
                IRow row2 = excelSheet2.CreateRow(0);
                var cellTitleRow2 = row2.CreateCell(0);
                cellTitleRow2.SetCellValue("Unsubmitted Score");
                cellTitleRow2.CellStyle = headerStyle;
                excelSheet2.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row2.RowNum, row2.RowNum, 0, 6));

                row2 = excelSheet2.CreateRow(row2.RowNum + 1);
                row2.CreateCell(0).SetCellValue("Academic Year :");
                row2.Cells[0].CellStyle = dataStyle;
                row2.CreateCell(1).SetCellValue(academicYear);
                row2.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 6; i++)
                {
                    row2.CreateCell(i).SetCellValue("");
                    row2.Cells[i].CellStyle = dataStyle;
                }
                excelSheet2.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row2.RowNum, row2.RowNum, 1, 6));

                row2 = excelSheet2.CreateRow(row2.RowNum + 1);

                //Body

                row2 = excelSheet2.CreateRow(row2.RowNum + 1);
                row2.CreateCell(0).SetCellValue("Academic Year");
                row2.Cells[0].CellStyle = headerStyle;
                row2.CreateCell(1).SetCellValue("Semester");
                row2.Cells[1].CellStyle = headerStyle;
                row2.CreateCell(2).SetCellValue("Elective Name");
                row2.Cells[2].CellStyle = headerStyle;
                row2.CreateCell(3).SetCellValue("Grade");
                row2.Cells[3].CellStyle = headerStyle;
                row2.CreateCell(4).SetCellValue("Supervisor/Coach");
                row2.Cells[4].CellStyle = headerStyle;
                row2.CreateCell(5).SetCellValue("Total Participant");
                row2.Cells[5].CellStyle = headerStyle;
                row2.CreateCell(6).SetCellValue("Total Entry");
                row2.Cells[6].CellStyle = headerStyle;

                foreach (var item in data)
                {
                    foreach(var studentData in item.StudentScore)
                    {
                        row = excelSheet.CreateRow(row.RowNum + 1);
                        row.CreateCell(0).SetCellValue(item.AcademicYear);
                        row.Cells[0].CellStyle = dataStyle;
                        row.CreateCell(1).SetCellValue(item.Semester);
                        row.Cells[1].CellStyle = dataStyle;
                        row.CreateCell(2).SetCellValue(item.Extracurricular.Name);
                        row.Cells[2].CellStyle = dataStyle;
                        row.CreateCell(3).SetCellValue(item.ElectiveGrade);
                        row.Cells[3].CellStyle = dataStyle;
                        row.CreateCell(4).SetCellValue(studentData.Student.Id);
                        row.Cells[4].CellStyle = dataStyle;
                        row.CreateCell(5).SetCellValue(studentData.Student.Name);
                        row.Cells[5].CellStyle = dataStyle;
                        row.CreateCell(6).SetCellValue(studentData.Class);
                        row.Cells[6].CellStyle = dataStyle;
                        row.CreateCell(7).SetCellValue(studentData.StudentGrade);
                        row.Cells[7].CellStyle = dataStyle;
                    }

                    if(item.UnsubmittedScore != null)
                    {
                        row2 = excelSheet2.CreateRow(row2.RowNum + 1);
                        row2.CreateCell(0).SetCellValue(item.AcademicYear);
                        row2.Cells[0].CellStyle = dataStyle;
                        row2.CreateCell(1).SetCellValue(item.Semester);
                        row2.Cells[1].CellStyle = dataStyle;
                        row2.CreateCell(2).SetCellValue(item.Extracurricular.Name);
                        row2.Cells[2].CellStyle = dataStyle;
                        row2.CreateCell(3).SetCellValue(item.ElectiveGrade);
                        row2.Cells[3].CellStyle = dataStyle;
                        row2.CreateCell(4).SetCellValue(item.UnsubmittedScore.SpvCoach);
                        row2.Cells[4].CellStyle = dataStyle;
                        row2.CreateCell(5).SetCellValue(item.UnsubmittedScore.TotalParticipant);
                        row2.Cells[5].CellStyle = dataStyle;
                        row2.CreateCell(6).SetCellValue(item.UnsubmittedScore.TotalEntry);
                        row2.Cells[6].CellStyle = dataStyle;
                    }
                }
          

                SetDynamicColumnWidthExcel(columnComponentCount, ref excelSheet);
                SetDynamicColumnWidthExcel(columnComponentCount2, ref excelSheet2);

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
