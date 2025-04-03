using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class DownloadSubstitutionReportHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(DownloadSubstitutionReportRequest.IdAcademicYear),
            nameof(DownloadSubstitutionReportRequest.IdLevel),
            nameof(DownloadSubstitutionReportRequest.StartDate),
            nameof(DownloadSubstitutionReportRequest.EndDate),
        };
        private readonly ISchedulingDbContext _dbContext;

        public DownloadSubstitutionReportHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadSubstitutionReportRequest>(nameof(DownloadSubstitutionReportRequest.IdAcademicYear));

            var predicate = PredicateBuilder.Create<TrScheduleRealization>(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            var predicateLesson = PredicateBuilder.Create<MsLessonTeacher>(x => x.Lesson.IdAcademicYear == param.IdAcademicYear);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization>(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            
            if(param.StartDate != null)
            {
                predicate = predicate.And(x => x.ScheduleDate >= param.StartDate);
                predicateSubtitute = predicateSubtitute.And(x => x.ScheduleDate >= param.StartDate);
            }

            if(param.EndDate != null)
            {
                predicate = predicate.And(x => x.ScheduleDate <= param.EndDate);
                predicateSubtitute = predicateSubtitute.And(x => x.ScheduleDate <= param.StartDate);
            }

            if(param.IdUserTeacher != null)
                predicate = predicate.And(x => param.IdUserTeacher.Contains(x.IdBinusian));

            if(param.IdUserSubstituteTeacher != null)
                predicate = predicate.And(x => param.IdUserSubstituteTeacher.Contains(x.IdBinusianSubtitute));
            
            if(!string.IsNullOrWhiteSpace(param.IdLevel))
            {
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
                predicateSubtitute = predicateSubtitute.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            }

            if(param.IdGrade != null)
                {
                    predicate = predicate.And(x => param.IdGrade.Contains(x.Homeroom.IdGrade));
                    predicateLesson = predicateLesson.And(x => param.IdGrade.Contains(x.Lesson.Subject.IdGrade));
                    predicateSubtitute = predicateSubtitute.And(x => param.IdGrade.Contains(x.Homeroom.IdGrade));
                }

            if(param.SessionID != null)
                predicate = predicate.And(x => param.SessionID.Contains(x.SessionID));

            if(param.IdVenue != null)
            {
                predicate = predicate.And(x => param.IdVenue.Contains(x.IdVenue));
                predicateSubtitute = predicateSubtitute.And(x => param.IdVenue.Contains(x.IdVenue));
            }

            var query = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Staff)
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataTeacher = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Include(x => x.Venue)
                                 .Where(predicate);

            var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization>()
                                 .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                 .Where(predicateSubtitute);

            var subjectTeacher = _dbContext.Entity<MsLessonTeacher>()
                                 .Include(x => x.Lesson).ThenInclude(x => x.Subject)
                                 .Where(predicateLesson);
            
            IReadOnlyList<GetListSubstitutionReportResult> items;
            List<GetListSubstitutionReportResult> dataItems;

            items = await query
                .Select(x => new GetListSubstitutionReportResult
                    {
                        
                        Date = x.ScheduleDate,
                        ClassID = x.ClassID,
                        SessionID = x.SessionID,
                        SessionStartTime = x.StartTime,
                        SessionEndTime = x.EndTime,
                        IdVenue = x.IdVenue,
                        VenueName = x.VenueName,
                        IdVenueOld = x.IdVenue,
                        VenueNameOld = x.VenueName,
                        IsCancelClass = x.IsCancel,
                        IsSendEmail = x.IsSendEmail,
                        NotesForSubtitutions = x.NotesForSubtitutions,
                        Status = x.Status
                    }
                )
                .Distinct()
                .OrderBy(x => x.Date).ThenBy(x => x.SessionStartTime)
                .ToListAsync(CancellationToken);

            dataItems = items
                    .Select(x => new GetListSubstitutionReportResult
                    {
                        // Ids = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Id).ToList(),
                        Date = x.Date,
                        ClassID = x.ClassID,
                        SessionID = x.SessionID,
                        SessionStartTime = x.SessionStartTime,
                        SessionEndTime = x.SessionEndTime,
                        DataTeachers =  getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListTeacher
                                        {
                                            Id = y.IdBinusian,
                                            Description = y.TeacherName
                                        }).Distinct().ToList(),
                        DataSubtituteTeachers = x.IsCancelClass == false ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListSubtituteTeacher
                                        {
                                            Id = y.IdBinusianSubtitute,
                                            Description = y.TeacherNameSubtitute
                                        }).Distinct().ToList() :
                                        getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => new DataListSubtituteTeacher
                                        {
                                            Id = y.IdBinusian,
                                            Description = y.TeacherName
                                        }).Distinct().ToList(),
                        IdVenue = x.IsCancelClass ? x.IdVenueOld : x.IdVenue,
                        VenueName = x.IsCancelClass ? x.VenueNameOld : x.VenueName,
                        ChangeVenue = new ItemValueVm(x.IdVenue,x.VenueName),
                        EntryStatusBy = "System",
                        EntryStatusDate = getDataTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.DateIn).FirstOrDefault(),
                        Status = x.IsCancelClass == false ? getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.Date && y.SessionID == x.SessionID && y.ClassID == x.ClassID).Select(y => y.Status).FirstOrDefault() : null,
                        IsCancelClass = x.IsCancelClass,
                        IsSendEmail = x.IsSendEmail,
                        NotesForSubtitutions = x.NotesForSubtitutions
                    }    
                ).ToList();

            var excelSubstitution = GenerateExcel(dataItems);

            return new FileContentResult(excelSubstitution, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Substitution_Report{DateTime.Now.Ticks}.xlsx"
            };
        }

        private byte[] GenerateExcel(List<GetListSubstitutionReportResult> data)
        {
            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var borderCellStyle = workbook.CreateCellStyle();
            borderCellStyle.BorderBottom = BorderStyle.Thin;
            borderCellStyle.BorderLeft = BorderStyle.Thin;
            borderCellStyle.BorderRight = BorderStyle.Thin;
            borderCellStyle.BorderTop = BorderStyle.Thin;

            var headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.CloneStyleFrom(borderCellStyle);
            headerCellStyle.SetFont(fontBold);

            var sheet = workbook.CreateSheet();
            // Acadyear
            var rowHeader = sheet.CreateRow(0);
            var cellDate = rowHeader.CreateCell(0);
            cellDate.SetCellValue("Date");
            cellDate.CellStyle = boldStyle;
            var cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = boldStyle;
            var cellSession = rowHeader.CreateCell(2);
            cellSession.SetCellValue("Session");
            cellSession.CellStyle = boldStyle;
            var cellTeacherName = rowHeader.CreateCell(3);
            cellTeacherName.SetCellValue("Teacher Name");
            cellTeacherName.CellStyle = boldStyle;
            var cellSubstituteTeacher = rowHeader.CreateCell(4);
            cellSubstituteTeacher.SetCellValue("Substitute Teacher");
            cellSubstituteTeacher.CellStyle = boldStyle;
            var cellRegularVenue = rowHeader.CreateCell(5);
            cellRegularVenue.SetCellValue("Regular Venue");
            cellRegularVenue.CellStyle = boldStyle;
            var cellChangeVenue = rowHeader.CreateCell(6);
            cellChangeVenue.SetCellValue("Change Venue");
            cellChangeVenue.CellStyle = boldStyle;
            var cellCancelTheClass = rowHeader.CreateCell(7);
            cellCancelTheClass.SetCellValue("Cancel the Class");
            cellCancelTheClass.CellStyle = boldStyle;
            var cellEmail = rowHeader.CreateCell(8);
            cellEmail.SetCellValue("Email");
            cellEmail.CellStyle = boldStyle;
            var cellNotesForSubstitute = rowHeader.CreateCell(9);
            cellNotesForSubstitute.SetCellValue("Notes for Substitutions");
            cellNotesForSubstitute.CellStyle = boldStyle;
            var cellEntryStatus = rowHeader.CreateCell(10);
            cellEntryStatus.SetCellValue("Entry Status");
            cellEntryStatus.CellStyle = boldStyle;
            var cellStatus = rowHeader.CreateCell(11);
            cellStatus.SetCellValue("Entry Status");
            cellStatus.CellStyle = boldStyle;

            int rowIndex = 1;
            int startColumn = 0;

            foreach(var itemData in data){
                rowHeader = sheet.CreateRow(rowIndex);
                cellDate = rowHeader.CreateCell(0);
                cellDate.SetCellValue(itemData.Date.ToString("dd MMMM yyyy"));
                cellClassID = rowHeader.CreateCell(1);
                cellClassID.SetCellValue(itemData.ClassID);
                cellSession = rowHeader.CreateCell(2);
                cellSession.SetCellValue("Session " + itemData.SessionID + " (" + itemData.SessionStartTime + " - " + itemData.SessionEndTime + ")");
                cellTeacherName = rowHeader.CreateCell(3);
                cellTeacherName.SetCellValue(itemData.DataTeachers.Select(x => x.Description).FirstOrDefault());
                cellSubstituteTeacher = rowHeader.CreateCell(4);
                cellSubstituteTeacher.SetCellValue(itemData.DataSubtituteTeachers.Select(x => x.Description).FirstOrDefault());
                cellRegularVenue = rowHeader.CreateCell(5);
                cellRegularVenue.SetCellValue(itemData.VenueName);
                cellChangeVenue = rowHeader.CreateCell(6);
                cellChangeVenue.SetCellValue(itemData.ChangeVenue.Description);
                cellCancelTheClass = rowHeader.CreateCell(7);
                cellCancelTheClass.SetCellValue(itemData.IsCancelClass == true ? "Yes" : "No");
                cellEmail = rowHeader.CreateCell(8);
                cellEmail.SetCellValue(itemData.IsSendEmail == true ? "Yes" : "No");
                cellNotesForSubstitute = rowHeader.CreateCell(9);
                cellNotesForSubstitute.SetCellValue(itemData.NotesForSubtitutions);
                cellEntryStatus = rowHeader.CreateCell(10);
                cellEntryStatus.SetCellValue("Done");
                cellEntryStatus = rowHeader.CreateCell(11);
                cellEntryStatus.SetCellValue(itemData.Status);
                
                rowIndex++;
                startColumn++;
            }
                
            using var ms = new MemoryStream();
            //ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }
    }
}
