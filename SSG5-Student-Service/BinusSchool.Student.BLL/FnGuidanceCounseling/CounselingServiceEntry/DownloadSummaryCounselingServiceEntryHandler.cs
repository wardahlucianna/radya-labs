using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class DownloadSummaryCounselingServiceEntryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _machineDateTime;

        public DownloadSummaryCounselingServiceEntryHandler(IStudentDbContext dbContext, IMachineDateTime machineDateTime)
        {
            _dbContext = dbContext;
            _machineDateTime = machineDateTime;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<DownloadSummaryCounselingServiceEntryRequest>();

            var title = "CounselingReport";

            var generateExcelByte = GenerateExcel(title, param);

            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{title}_{_machineDateTime.ServerTime.ToString("ddMMyyyy")}.xlsx"
            };
        }

        public byte[] GenerateExcel(string sheetTitle, DownloadSummaryCounselingServiceEntryRequest param)
        {
            var query = _dbContext.Entity<TrCounselingServicesEntry>()
                        .Include(f => f.CounselingServicesEntryConcern)
                        .Include(f => f.CounselingServicesEntryConcern).ThenInclude(f => f.ConcernCategory)
                        .Include(f => f.CounselingServicesEntryAttachment)
                        .Include(f => f.CounselingCategory)
                        .Include(f => f.Counselor)
                        .Include(f => f.Counselor).ThenInclude(f => f.User)
                        .Include(f => f.AcademicYear)
                        .Include(f => f.Student)
                        .Include(f => f.Student).ThenInclude(f => f.StudentParents).ThenInclude(f => f.Parent)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                        .Include(f => f.Student).ThenInclude(f => f.MsHomeroomStudents).ThenInclude(f => f.Homeroom).ThenInclude(x => x.MsGradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                        .Select(x => new DownloadSummaryCounselingServiceEntryResult
                        {
                            AcademicYear = new CodeWithIdVm
                            {
                                Id = x.AcademicYear.Id,
                                Code = x.AcademicYear.Code,
                                Description = x.AcademicYear.Description
                            },
                            CounselorName = x.Counselor.User.DisplayName,
                            StudentName = (x.Student.FirstName == null ? "" : x.Student.FirstName) + (x.Student.MiddleName == null ? "" : " " + x.Student.MiddleName) + (x.Student.LastName == null ? "" : " " + x.Student.LastName)/*NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)*/,
                            IdBinusian = x.Student.IdBinusian,
                            Level = new CodeWithIdVm
                            {
                                Id = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Id).FirstOrDefault(),
                                Code = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Code).FirstOrDefault(),
                                Description = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.Description).FirstOrDefault(),
                            },
                            Grade = new CodeWithIdVm
                            {
                                Id = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id).FirstOrDefault(),
                                Code = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code).FirstOrDefault(),
                                Description = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Description).FirstOrDefault(),
                            },
                            HomeRoom = new ItemValueVm
                            {
                                Id = x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.Id).FirstOrDefault(),
                                Description = $"{x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code).FirstOrDefault()}{x.Student.MsHomeroomStudents.Where(y => y.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear).Select(z => z.Homeroom.MsGradePathwayClassroom.Classroom.Code).FirstOrDefault()}"
                            },
                            CounselingCategory = new NameValueVm
                            {
                                Id = x.CounselingCategory.Id,
                                Name = x.CounselingCategory.CounselingCategoryName
                            },
                            ConcernCategory = x.CounselingServicesEntryConcern.Select(a => a.ConcernCategory).Select(b => new NameValueVm
                            {
                                Id = b.Id,
                                Name = b.ConcernCategoryName
                            }).ToList(),
                            RefredBy = x.ReferredBy,
                            BriefReport = x.BriefReport,
                            FollowUp = x.FollowUp,
                            CounselingWith = x.CounselingWith.GetDescription()
                        }).AsQueryable();

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
            {
                query = query.Where(x => EF.Functions.Like(x.AcademicYear.Id, param.IdAcademicYear));
            }
            if (!string.IsNullOrEmpty(param.IdLevel))
            {
                query = query.Where(x => EF.Functions.Like(x.Level.Id, param.IdLevel));
            }
            if (!string.IsNullOrEmpty(param.IdGrade))
            {
                query = query.Where(x => EF.Functions.Like(x.Grade.Id, param.IdGrade));
            }
            if (!string.IsNullOrEmpty(param.IdHomeRoom))
            {
                query = query.Where(x => EF.Functions.Like(x.HomeRoom.Id, param.IdHomeRoom));
            }
            if (!string.IsNullOrEmpty(param.IdCounselingCategory))
            {
                query = query.Where(x => EF.Functions.Like(x.CounselingCategory.Id, param.IdCounselingCategory));
            }
            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => x.StudentName.Contains(param.Search)
                || x.IdBinusian.Contains(param.Search)
                || x.CounselingCategory.Name.Contains(param.Search));
            }

            var data = query.ToList();

            var result = new byte[0];

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
                var fontBold = workbook.CreateFont();
                fontBold.IsBold = true;
                var boldStyle = workbook.CreateCellStyle();
                boldStyle.SetFont(fontBold);

                //header information
                IRow rowHeader = excelSheet.CreateRow(0);
                var cellParticipant = rowHeader.CreateCell(0);
                //Row1
                rowHeader = excelSheet.CreateRow(0);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Counseling Report");
                cellParticipant.CellStyle = boldStyle;

                //Row2
                rowHeader = excelSheet.CreateRow(1);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Generate Date");


                cellParticipant = rowHeader.CreateCell(1);
                cellParticipant.SetCellValue(DateTime.Now.ToString("dd/MMM/yyyy"));

                //Row3
                rowHeader = excelSheet.CreateRow(2);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("");

                //Row4
                rowHeader = excelSheet.CreateRow(3);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("AY :");

                cellParticipant = rowHeader.CreateCell(1);
                var dataAY = !string.IsNullOrEmpty(param.IdAcademicYear) ? query.Select(x => x.AcademicYear.Description).FirstOrDefault() : "All";
                cellParticipant.SetCellValue(dataAY);

                //Row5
                rowHeader = excelSheet.CreateRow(4);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Level :");

                cellParticipant = rowHeader.CreateCell(1);
                var dataLevel = !string.IsNullOrEmpty(param.IdLevel) ? query.Select(x => x.Level.Code).FirstOrDefault() : "All";
                cellParticipant.SetCellValue(dataLevel);

                //Row6
                rowHeader = excelSheet.CreateRow(5);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Grade :");

                cellParticipant = rowHeader.CreateCell(1);
                var dataGrade = !string.IsNullOrEmpty(param.IdGrade) ? query.Select(x => x.Grade.Description).FirstOrDefault() : "All";
                cellParticipant.SetCellValue(dataGrade);

                //Row7
                rowHeader = excelSheet.CreateRow(6);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Class/HomeRoom :");

                cellParticipant = rowHeader.CreateCell(1);
                var dataHomeRoom = !string.IsNullOrEmpty(param.IdHomeRoom) ? query.Select(x => x.HomeRoom.Description).FirstOrDefault() : "All";
                cellParticipant.SetCellValue(dataHomeRoom);

                //Row8
                rowHeader = excelSheet.CreateRow(7);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue("Counseling Category :");

                cellParticipant = rowHeader.CreateCell(1);
                var dataCategory = !string.IsNullOrEmpty(param.IdCounselingCategory) ? query.Select(x => x.CounselingCategory.Name).FirstOrDefault() : "All";
                cellParticipant.SetCellValue(dataCategory);

                //row data
                rowHeader = excelSheet.CreateRow(9);
                var cellAcademicYear = rowHeader.CreateCell(0);
                cellAcademicYear.SetCellValue("Academic Year");
                cellAcademicYear.CellStyle = boldStyle;

                var cellStudentName = rowHeader.CreateCell(1);
                cellStudentName.SetCellValue("Student Name");
                cellStudentName.CellStyle = boldStyle;

                var cellBinusianId = rowHeader.CreateCell(2);
                cellBinusianId.SetCellValue("Binusian Id");
                cellBinusianId.CellStyle = boldStyle;

                var cellLevel = rowHeader.CreateCell(3);
                cellLevel.SetCellValue("Level");
                cellLevel.CellStyle = boldStyle;

                var cellGrade = rowHeader.CreateCell(4);
                cellGrade.SetCellValue("Grade");
                cellGrade.CellStyle = boldStyle;

                var cellHomeroom = rowHeader.CreateCell(5);
                cellHomeroom.SetCellValue("Homeroom");
                cellHomeroom.CellStyle = boldStyle;

                var cellCounselingCategory = rowHeader.CreateCell(6);
                cellCounselingCategory.SetCellValue("Counseling Category");
                cellCounselingCategory.CellStyle = boldStyle;

                var cellCounselorName = rowHeader.CreateCell(7);
                cellCounselorName.SetCellValue("Counselor Name");
                cellCounselorName.CellStyle = boldStyle;

                var cellCounselingWith = rowHeader.CreateCell(8);
                cellCounselingWith.SetCellValue("Counseling With");
                cellCounselingWith.CellStyle = boldStyle;

                var cellReferedBy = rowHeader.CreateCell(9);
                cellReferedBy.SetCellValue("Refered By");
                cellReferedBy.CellStyle = boldStyle;

                var cellConcernCategory = rowHeader.CreateCell(10);
                cellConcernCategory.SetCellValue("Concern Category");
                cellConcernCategory.CellStyle = boldStyle;

                var cellBriefReport = rowHeader.CreateCell(11);
                cellBriefReport.SetCellValue("Brief Report");
                cellBriefReport.CellStyle = boldStyle;

                var cellFollowUp = rowHeader.CreateCell(12);
                cellFollowUp.SetCellValue("Follow Up");
                cellFollowUp.CellStyle = boldStyle;

                //value
                int rowIndex = 10;
                int startColumn = 0;
                foreach (var itemData in data)
                {
                    rowHeader = excelSheet.CreateRow(rowIndex);
                    cellParticipant = rowHeader.CreateCell(0);
                    cellParticipant.SetCellValue(itemData.AcademicYear.Description);
                    excelSheet.AutoSizeColumn(0);

                    cellParticipant = rowHeader.CreateCell(1);
                    cellParticipant.SetCellValue(itemData.StudentName);
                    excelSheet.AutoSizeColumn(1);

                    cellParticipant = rowHeader.CreateCell(2);
                    cellParticipant.SetCellValue(itemData.IdBinusian);
                    excelSheet.AutoSizeColumn(2);

                    cellParticipant = rowHeader.CreateCell(3);
                    cellParticipant.SetCellValue(itemData.Level.Description);
                    excelSheet.AutoSizeColumn(3);

                    cellParticipant = rowHeader.CreateCell(4);
                    cellParticipant.SetCellValue(itemData.Grade.Description);
                    excelSheet.AutoSizeColumn(3);

                    cellParticipant = rowHeader.CreateCell(5);
                    cellParticipant.SetCellValue(itemData.HomeRoom.Description);
                    excelSheet.AutoSizeColumn(4);

                    cellParticipant = rowHeader.CreateCell(6);
                    cellParticipant.SetCellValue(itemData.CounselingCategory.Name);
                    excelSheet.AutoSizeColumn(5);

                    cellParticipant = rowHeader.CreateCell(7);
                    cellParticipant.SetCellValue(itemData.CounselorName);
                    excelSheet.AutoSizeColumn(6);

                    cellParticipant = rowHeader.CreateCell(8);
                    cellParticipant.SetCellValue(itemData.CounselingWith);
                    excelSheet.AutoSizeColumn(7);

                    cellParticipant = rowHeader.CreateCell(9);
                    cellParticipant.SetCellValue(itemData.RefredBy);
                    excelSheet.AutoSizeColumn(8);

                    cellParticipant = rowHeader.CreateCell(10);
                    cellParticipant.SetCellValue(string.Join(",", itemData.ConcernCategory.Select(x => x.Name)));
                    excelSheet.AutoSizeColumn(9);

                    cellParticipant = rowHeader.CreateCell(11);
                    cellParticipant.SetCellValue(itemData.BriefReport);
                    excelSheet.AutoSizeColumn(10);

                    cellParticipant = rowHeader.CreateCell(12);
                    cellParticipant.SetCellValue(itemData.FollowUp);
                    excelSheet.AutoSizeColumn(11);


                    rowIndex++;
                    startColumn++;
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
