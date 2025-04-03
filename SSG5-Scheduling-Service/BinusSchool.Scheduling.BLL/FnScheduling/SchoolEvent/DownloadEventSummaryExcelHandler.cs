using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class DownloadEventSummaryExcelHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetSchoolEventSummaryRequest.StartDate),
            nameof(GetSchoolEventSummaryRequest.EndDate),
            nameof(GetSchoolEventSummaryRequest.IntendedFor),
        };
        private readonly ISchedulingDbContext _dbContext;

        public DownloadEventSummaryExcelHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetSchoolEventSummaryRequest>(_requiredParams);

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where a.Id == param.IdUser

                                   select new LtRole
                                   {
                                       IdRoleGroup = rg.IdRoleGroup
                                   }).FirstOrDefaultAsync(CancellationToken);

            if (CheckRole == null)
                throw new BadRequestException($"User in this role not found");

            var data = new List<GetSchoolEventSummary2Result>();

            if (param.IntendedFor == "STUDENT")
            {
                var predicate = PredicateBuilder.Create<TrEventActivityAward>(x => param.IdSchool.Any(y => y == x.EventActivity.Event.EventType.AcademicYear.IdSchool));
                predicate = predicate.And(x => x.EventActivity.Event.IsStudentInvolvement == false && x.EventActivity.Event.StatusEvent == "Approved" && x.EventActivity.Event.EventDetails.Any(y
                    => y.StartDate == param.StartDate || y.EndDate == param.EndDate
                    || (y.StartDate < param.StartDate
                        ? (y.EndDate > param.StartDate && y.EndDate < param.EndDate) || y.EndDate > param.EndDate
                        : (param.EndDate > y.StartDate && param.EndDate < y.EndDate) || param.EndDate > y.EndDate)));
                var query = _dbContext.Entity<TrEventActivityAward>()
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x => x.EventDetails)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityPICs)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityRegistrants)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Activity)
                    .Include(x => x.HomeroomStudent)
                        .ThenInclude(x => x.Student)
                    .Include(x => x.HomeroomStudent)
                        .ThenInclude(x => x.Homeroom)
                            .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                    .Include(x => x.Award)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(param.IdEvent))
                    query = query.Where(x => x.EventActivity.IdEvent == param.IdEvent);

                if (!string.IsNullOrEmpty(param.IdActivity))
                    query = query.Where(x => x.EventActivity.IdActivity == param.IdActivity);

                if (!string.IsNullOrEmpty(param.IdAward))
                    query = query.Where(x => x.IdAward == param.IdAward);

                    query = query.Where(x => x.EventActivity.Event.IsStudentInvolvement == false && x.EventActivity.Event.StatusEvent == "Approved");
            
                switch (param.OrderBy)
                {
                    case "EventName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                            : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                    case "StudentName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.HomeroomStudent.Student.FirstName)
                            : query.OrderBy(x => x.HomeroomStudent.Student.FirstName);
                        break;
                    case "Activity":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Activity.Description)
                            : query.OrderBy(x => x.EventActivity.Activity.Description);
                        break;
                    case "Award":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.Award.Description)
                            : query.OrderBy(x => x.Award.Description);
                        break;
                    default:
                        query = param.OrderType == OrderType.Desc
                                                 ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                                                 : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                }
                data = await query
                    .Where(predicate)
                    .Select(x => new GetSchoolEventSummary2Result
                    {
                        Activity = x.EventActivity.Activity.Description,
                        BinusianID = x.HomeroomStudent.Student.Id,
                        EventDates = x.EventActivity.Event.EventDetails.Select(x => new EventDate
                        {
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                        }).ToList(),
                        EventName = x.EventActivity.Event.Name,
                        Grade = x.HomeroomStudent.Homeroom.Grade.Description,
                        Homeroom = x.HomeroomStudent.Homeroom.Grade.Code + x.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                        Involvement = x.Award.Description,
                        IdEventActivityAward = x.Id,
                        Level = x.HomeroomStudent.Homeroom.Grade.Level.Code,
                        ParticipantName = x.HomeroomStudent.Student.MiddleName != null ? x.HomeroomStudent.Student.FirstName.Trim() + " " + x.HomeroomStudent.Student.MiddleName.Trim() + x.HomeroomStudent.Student.LastName.Trim() : x.HomeroomStudent.Student.FirstName.Trim() + " " + x.HomeroomStudent.Student.LastName.Trim(),
                        PIC = x.EventActivity.EventActivityPICs.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Registratior = x.EventActivity.EventActivityRegistrants.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Place = x.EventActivity.Event.Place
                    })
                    .ToListAsync();
            }
            else
            {
                var query = _dbContext.Entity<TrEventActivityAwardTeacher>()
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Event)
                            .ThenInclude(x => x.EventDetails)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityPICs)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.EventActivityRegistrants)
                            .ThenInclude(x => x.User)
                    .Include(x => x.EventActivity)
                        .ThenInclude(x => x.Activity)
                    .Include(x => x.Award)
                    .AsQueryable();
                if (!string.IsNullOrEmpty(param.IdEvent))
                    query = query.Where(x => x.EventActivity.IdEvent == param.IdEvent);

                if (!string.IsNullOrEmpty(param.IdActivity))
                    query = query.Where(x => x.EventActivity.IdActivity == param.IdActivity);

                if (!string.IsNullOrEmpty(param.IdAward))
                    query = query.Where(x => x.IdAward == param.IdAward);

                    query = query.Where(x => x.EventActivity.Event.IsStudentInvolvement == false && x.EventActivity.Event.StatusEvent == "Approved");

                switch (param.OrderBy)
                {
                    case "EventName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                            : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                    case "StudentName":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.Staff.FirstName)
                            : query.OrderBy(x => x.Staff.FirstName);
                        break;
                    case "Activity":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.EventActivity.Activity.Description)
                            : query.OrderBy(x => x.EventActivity.Activity.Description);
                        break;
                    case "Award":
                        query = param.OrderType == OrderType.Desc
                            ? query.OrderByDescending(x => x.Award.Description)
                            : query.OrderBy(x => x.Award.Description);
                        break;
                    default:
                        query = param.OrderType == OrderType.Desc
                                                 ? query.OrderByDescending(x => x.EventActivity.Event.Name)
                                                 : query.OrderBy(x => x.EventActivity.Event.Name);
                        break;
                }
                data = await query
                    .Select(x => new GetSchoolEventSummary2Result
                    {
                        Activity = x.EventActivity.Activity.Description,
                        BinusianID = x.Staff.IdBinusian,
                        EventDates = x.EventActivity.Event.EventDetails.Select(x => new EventDate
                        {
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                        }).ToList(),
                        EventName = x.EventActivity.Event.Name,
                        Grade = null,
                        Homeroom = null,
                        Involvement = x.Award.Description,
                        IdEventActivityAward = x.Id,
                        Level = null,
                        ParticipantName = x.Staff.FirstName,
                        PIC = x.EventActivity.EventActivityPICs.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Registratior = x.EventActivity.EventActivityRegistrants.Select(x => new User
                        {
                            IdBinusian = x.User.Id,
                            Name = x.User.DisplayName
                        }).ToList(),
                        Place = x.EventActivity.Event.Place
                    })
                    .ToListAsync();
            }

            var excelSchedules = GenerateExcel(data);

            return new FileContentResult(excelSchedules, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"SummaryEventOf_Student{DateTime.Now.Ticks}.xlsx"
            };
        }

        private byte[] GenerateExcel(List<GetSchoolEventSummary2Result> data)
        {
            // var trEvent = _dbContext.Entity<TrEvent>().Select(x => new TrEvent{
            //     Id = x.Id,
            //     Name = x.Name
            // }).ToList();
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
            var cellParticipant = rowHeader.CreateCell(0);
            cellParticipant.SetCellValue("Participant Name");
            cellParticipant.CellStyle = boldStyle;
            var cellBinusianid = rowHeader.CreateCell(1);
            cellBinusianid.SetCellValue("Binusian ID");
            cellBinusianid.CellStyle = boldStyle;
            var cellLevel = rowHeader.CreateCell(2);
            cellLevel.SetCellValue("Level");
            cellLevel.CellStyle = boldStyle;
            var cellGrade = rowHeader.CreateCell(3);
            cellGrade.SetCellValue("Grade");
            cellGrade.CellStyle = boldStyle;
            var cellHomeroom = rowHeader.CreateCell(4);
            cellHomeroom.SetCellValue("Homeroom");
            cellHomeroom.CellStyle = boldStyle;
            var cellEventname = rowHeader.CreateCell(5);
            cellEventname.SetCellValue("Event Name");
            cellEventname.CellStyle = boldStyle;
            var cellPlace = rowHeader.CreateCell(6);
            cellPlace.SetCellValue("Place");
            cellPlace.CellStyle = boldStyle;
            var cellActivity = rowHeader.CreateCell(7);
            cellActivity.SetCellValue("Activity");
            cellActivity.CellStyle = boldStyle;
            var cellAward = rowHeader.CreateCell(8);
            cellAward.SetCellValue("Involvement / Award");
            cellAward.CellStyle = boldStyle;
            var cellPIC = rowHeader.CreateCell(9);
            cellPIC.SetCellValue("PIC");
            cellPIC.CellStyle = boldStyle;
            var cellRegistrator = rowHeader.CreateCell(10);
            cellRegistrator.SetCellValue("Registrator");
            cellRegistrator.CellStyle = boldStyle;
            var cellStartdate = rowHeader.CreateCell(11);
            cellStartdate.SetCellValue("Event Date");
            cellStartdate.CellStyle = boldStyle;

            int rowIndex = 1;
            int startColumn = 0;

            foreach(var itemData in data){
                rowHeader = sheet.CreateRow(rowIndex);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue(itemData.ParticipantName);
                cellParticipant = rowHeader.CreateCell(1);
                cellParticipant.SetCellValue(itemData.BinusianID);
                cellParticipant = rowHeader.CreateCell(2);
                cellParticipant.SetCellValue(itemData.Level);
                cellParticipant = rowHeader.CreateCell(3);
                cellParticipant.SetCellValue(itemData.Grade);
                cellParticipant = rowHeader.CreateCell(4);
                cellParticipant.SetCellValue(itemData.Homeroom);
                cellParticipant = rowHeader.CreateCell(5);
                cellParticipant.SetCellValue(itemData.EventName);
                cellParticipant = rowHeader.CreateCell(6);
                cellParticipant.SetCellValue(itemData.Place);
                cellParticipant = rowHeader.CreateCell(7);
                cellParticipant.SetCellValue(itemData.Activity);
                cellParticipant = rowHeader.CreateCell(8);
                cellParticipant.SetCellValue(itemData.Involvement);
                cellParticipant = rowHeader.CreateCell(9);
                cellParticipant.SetCellValue(String.Join(",",itemData.PIC.Select(x => x.Name).ToList()));
                cellParticipant = rowHeader.CreateCell(10);
                cellParticipant.SetCellValue(String.Join(",",itemData.Registratior.Select(x => x.Name).ToList()));
                cellParticipant = rowHeader.CreateCell(11);
                var eventDate = string.Empty;
                var sb = new StringBuilder();
                foreach (var dateEvent in itemData.EventDates)
                {
                    sb.Append($"{dateEvent.StartDate.ToString("dd MMMM yyyy")} - {dateEvent.EndDate.ToString("dd MMMM yyyy")}");
                    sb.Append($"{Environment.NewLine}");
                }
                eventDate = sb.ToString();
                cellParticipant.SetCellValue(eventDate);
                
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
