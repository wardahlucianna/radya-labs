using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using CodeView = BinusSchool.Data.Model.Teaching.FnAssignment.Timetable.CodeView;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class TimeTablePreferencesExportHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(ExportExcelTimeTableRequest.IdSchool),
            nameof(ExportExcelTimeTableRequest.IdSchoolAcadyears)
        });
        
        private readonly ITeachingDbContext _dbContext;

        public TimeTablePreferencesExportHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<ExportExcelTimeTableRequest>(_requiredParams.Value);

            var timeTable = await _dbContext.Entity<MsSubjectCombination>()
                .Include(x => x.Subject).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                .Include(x => x.Subject).ThenInclude(x => x.Department)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails).ThenInclude(x => x.Pathway)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.Childs)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.TeachingLoads).ThenInclude(x => x.User)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.Venue).ThenInclude(x => x.Building)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.Period)
                .Include(x => x.TimeTablePrefHeader).ThenInclude(x => x.TimetablePrefDetails).ThenInclude(x => x.Division)
                .Where(x 
                    => x.Subject.Grade.Level.IdAcademicYear == param.IdSchoolAcadyears
                    && x.TimeTablePrefHeader != null
                    && x.TimeTablePrefHeader.IdParent == null
                    && x.TimeTablePrefHeader.Status)
                .OrderBy(x => x.Subject.SubjectID)
                .ToListAsync(CancellationToken);
            var idUsers = timeTable.SelectMany(x => x.TimeTablePrefHeader.TimetablePrefDetails
                .Where(y => y.TeachingLoads != null)
                .SelectMany(y => y.TeachingLoads.Select(z => z.IdUser)));
            
            var employee = await _dbContext.Entity<MsStaff>().Where(x => idUsers.Any(p => p == x.IdBinusian)).ToListAsync(CancellationToken);
            var data =
                (
                    from _timeTablePref in timeTable

                    where
                        (string.IsNullOrEmpty(param.IdLevel) || _timeTablePref.Subject.Grade.IdLevel == param.IdLevel)
                    select new GetListTimeTableResult
                    {
                        Id = _timeTablePref.TimeTablePrefHeader.Id,
                        IsMarge = _timeTablePref.TimeTablePrefHeader.IsMerge,
                        ClassroomDivision = "-",
                        AcadYear = new CodeView
                        {
                            Id = _timeTablePref.Subject.Grade.Level.IdAcademicYear,
                            Code = _timeTablePref.Subject.Grade.Level.AcademicYear.Code,
                            Description = _timeTablePref.Subject.Grade.Level.AcademicYear.Description
                        },
                        Level = new CodeView
                        {
                            Id = _timeTablePref.Subject.Grade.IdLevel,
                            Code = _timeTablePref.Subject.Grade.Level.Code,
                            Description = _timeTablePref.Subject.Grade.Level.Description
                        },
                        Grade = new CodeView
                        {
                            Id = _timeTablePref.Subject.IdGrade,
                            Code = _timeTablePref.Subject.Grade.Code,
                            Description = _timeTablePref.Subject.Grade.Description
                        },
                        Class = new CodeView
                        {
                            Id = _timeTablePref.GradePathwayClassroom.Id,
                            Code = _timeTablePref.GradePathwayClassroom.GradePathway.Grade.Code +
                             _timeTablePref.GradePathwayClassroom.Classroom.Code,
                            Description = _timeTablePref.GradePathwayClassroom.Classroom.Description
                        },
                        Status = _timeTablePref.TimeTablePrefHeader.Status,
                        Subject = new SubjectVm
                        {
                            Id = _timeTablePref.Subject.Id,
                            SubjectId = _timeTablePref.Subject.SubjectID,
                            SubjectName = _timeTablePref.Subject.Code,
                            Description = _timeTablePref.Subject.Description
                        },
                        TotalSession = _timeTablePref.Subject.MaxSession,
                        Department = new CodeView
                        {
                            Id = _timeTablePref.Subject.IdDepartment,
                            Code = _timeTablePref.Subject.Department.Code,
                            Description = _timeTablePref.Subject.Department.Description
                        },
                        Streaming = new CodeView
                        {
                            Id = string.Join(",", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Id).Distinct()),
                            Code = string.Join(" & ", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Code).Distinct()),
                            Description = string.Join(" & ", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Description).Distinct())
                        },
                        IsParent = _timeTablePref.TimeTablePrefHeader.IsParent,
                        TimeTableDetail = _timeTablePref.TimeTablePrefHeader.TimetablePrefDetails.Select(x => new TimeTableDetailVm
                        {
                            Id = x.Id,
                            Week = x.Week,
                            Count = x.Count,
                            Lenght = x.Length,
                            Department = _timeTablePref.Subject.Department.Description,
                            Streaming = string.Join(" & ", _timeTablePref.GradePathwayClassroom.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Description).Distinct()),
                            BuildingVanue = new BuildingVenueVm
                            {
                                Id = x.IdVenue,
                                Code = x.Venue.Code,
                                Description = x.Venue.Description,
                                BuildingCode = x.Venue?.Building?.Code,
                                BuildingDesc = x.Venue?.Building?.Description
                            },
                            Division = new CodeView
                            {
                                Id = x.IdDivision,
                                Code = x.Division?.Code,
                                Description = x.Division?.Description
                            },
                            Term = new CodeView
                            {
                                Id = x.IdPeriod,
                                Code = x.Period?.Code,
                                Description = x.Period?.Description
                            },
                            Level = _timeTablePref.Subject.Grade.IdLevel,
                            TotalLoad = 0,//teacherLoads.Where(y => y.IdUser == x.TeachingLoads.Where(y => y.IdTimetablePrefDetail == x.Id).FirstOrDefault()?.IdUser).Select(x => x.Load).FirstOrDefault(),
                            Teacher = new TeacherVm
                            {
                                BinusianId = employee.FirstOrDefault(e => e.IdBinusian == (x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser))?.IdBinusian,
                                Code = employee.FirstOrDefault(e => e.IdBinusian == (x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser))?.ShortName,
                                Description = employee.FirstOrDefault(e => e.IdBinusian == (x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser))?.FirstName,
                                Id = x.TeachingLoads.FirstOrDefault(y => y.IdTimetablePrefDetail == x.Id)?.IdUser,
                                TotalLoad = 0//teacherLoads.Where(y => y.IdUser == x.TeachingLoads.Where(y => y.IdTimetablePrefDetail == x.Id).FirstOrDefault()?.IdUser).Select(x => x.Load).FirstOrDefault()
                            }
                        }).ToList()
                    }).ToList();
            var generateExcelByte = GenerateExcel(data);
            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"ExportTimeTable_{DateTime.Now.Ticks}.xlsx"
            };
        }

        public byte[] GenerateExcel(List<GetListTimeTableResult> data)
        {
            var result = new byte[0];
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("TimeTable Data");

                // //Create style
                ICellStyle style = workbook.CreateCellStyle();

                // //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                // // style.BottomBorderColor = HSSFColor.Yellow.Index;

                // //Set font style
                //IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                //font.FontName = "Arial";
                //font.FontHeight = 13;
                //font.IsItalic = true;
                //style.SetFont(font);

                #region "Text Center"
                ICellStyle textcenter = workbook.CreateCellStyle();
                textcenter.Alignment = HorizontalAlignment.Center;
                textcenter.VerticalAlignment = VerticalAlignment.Top;
                textcenter.WrapText = true; //wrap the text in the cell
                #endregion
                //header 
                IRow row = excelSheet.CreateRow(0);

                // //subject
                var Judul = row.CreateCell(0);
                Judul.SetCellValue("SUBJECTS");
                CellRangeAddress subject = new CellRangeAddress(0, 0, 0, 1);
                excelSheet.AddMergedRegion(subject);



                //teacher
                var Judul1 = row.CreateCell(3);
                Judul1.SetCellValue("TEACHERS");
                CellRangeAddress teacher = new CellRangeAddress(0, 0, 3, 5);
                excelSheet.AddMergedRegion(teacher);

                //venue
                var Judul2 = row.CreateCell(9);
                Judul2.SetCellValue("VENUES");
                CellRangeAddress venue = new CellRangeAddress(0, 0, 9, 11);
                excelSheet.AddMergedRegion(venue);
                //class
                var Judul3 = row.CreateCell(13);
                Judul3.SetCellValue("CLASS");
                CellRangeAddress classes = new CellRangeAddress(0, 0, 13, 15);
                excelSheet.AddMergedRegion(classes);

                row.GetCell(0).CellStyle = textcenter;
                row.GetCell(3).CellStyle = textcenter;
                row.GetCell(9).CellStyle = textcenter;
                row.GetCell(13).CellStyle = textcenter;

                row = excelSheet.CreateRow(2);

                //subject
                var header1 = row.CreateCell(0);
                header1.SetCellValue("Short");
                header1.CellStyle = style;

                var header2 = row.CreateCell(1);
                header2.SetCellValue("Name");
                header2.CellStyle = style;


                //teacher
                var header3 = row.CreateCell(3);
                header3.SetCellValue("Name");
                header3.CellStyle = style;

                var header4 = row.CreateCell(4);
                header4.SetCellValue("Binusian id");
                header4.CellStyle = style;

                var header5 = row.CreateCell(5);
                header5.SetCellValue("Initial");
                header5.CellStyle = style;


                //venue
                var header6 = row.CreateCell(9);
                header6.SetCellValue("Name");
                header6.CellStyle = style;

                var header7 = row.CreateCell(10);
                header7.SetCellValue("Short");
                header7.CellStyle = style;

                var header8 = row.CreateCell(11);
                header8.SetCellValue("Building");
                header8.CellStyle = style;


                //class
                var header9 = row.CreateCell(13);
                header9.SetCellValue("Name");
                header9.CellStyle = style;

                var header10 = row.CreateCell(14);
                header10.SetCellValue("Short");
                header10.CellStyle = style;

                var header11 = row.CreateCell(15);
                header11.SetCellValue("Division");
                header11.CellStyle = style;

                for (int i = 0; i < data.Count(); i++)
                {
                    foreach (var item in data[i].TimeTableDetail)
                    {
                        row = excelSheet.CreateRow(row.RowNum + 1);

                        row.CreateCell(0).SetCellValue(data[i].Subject.SubjectName);
                        row.Cells[0].CellStyle = style;

                        row.CreateCell(1).SetCellValue(data[i].Subject.Description);
                        row.Cells[1].CellStyle = style;


                        if (item.Teacher != null)
                        {
                            row.CreateCell(3).SetCellValue(item.Teacher.Description);
                            row.Cells[2].CellStyle = style;

                            row.CreateCell(4).SetCellValue(item.Teacher.BinusianId);
                            row.Cells[3].CellStyle = style;

                            row.CreateCell(5).SetCellValue(item.Teacher.Code);
                            row.Cells[4].CellStyle = style;

                        }


                        row.CreateCell(9).SetCellValue(item.BuildingVanue.Code + " - " + item.BuildingVanue.Description);
                        row.Cells[5].CellStyle = style;

                        row.CreateCell(10).SetCellValue(item.BuildingVanue.Code);
                        row.Cells[6].CellStyle = style;

                        row.CreateCell(11).SetCellValue(item.BuildingVanue.BuildingDesc);
                        row.Cells[7].CellStyle = style;


                        row.CreateCell(13).SetCellValue(!string.IsNullOrEmpty(data[i].Class.Description) ? data[i].Class.Description : data[i].Class.Code);
                        row.Cells[8].CellStyle = style;

                        row.CreateCell(14).SetCellValue(data[i].Class.Code);
                        row.Cells[9].CellStyle = style;

                        row.CreateCell(15).SetCellValue(item.Division.Description);
                        row.Cells[10].CellStyle = style;
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

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }
    }
}
