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
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class DownloadRecapHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListTeacherByInvitationRequest.IdInvitationBookingSetting),
        };
        private static readonly string[] _columns = { "StudentName", "TeacherName" };
        private readonly ISchedulingDbContext _dbContext;

        public DownloadRecapHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        private CodeWithIdVm GetClass(string IdHomeroomStudent)
        {
            if (IdHomeroomStudent == null)
                return null;

            var dataStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                                .Include(x => x.Homeroom).ThenInclude(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                                .Where(x => x.Id == IdHomeroomStudent)
                                .Select(x => new CodeWithIdVm
                                {
                                    Id = x.Id,
                                    Code = x.Homeroom.Grade.Description,
                                    Description = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.Classroom.Code
                                })
                                .FirstOrDefault();

            return dataStudent;
        }
        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetListRecapRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBooking>(x => x.IsActive == true);

            predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting);

            // if (!string.IsNullOrWhiteSpace(param.Search))
            //     predicate = predicate.And(x
            //         => EF.Functions.Like(x.UserTeacher.DisplayName, $"%{param.Search}%"));

            if (!string.IsNullOrWhiteSpace(param.IdUserTeacher))
                predicate = predicate.And(x => x.IdUserTeacher == param.IdUserTeacher);

            if (param.Status == InvitationBookingStatus.Default)
            {
                predicate = predicate.And(x => x.Status == InvitationBookingStatus.Default);
            }
            else if (param.Status == InvitationBookingStatus.Present)
            {
                predicate = predicate.And(x => x.Status == InvitationBookingStatus.Present);
            }
            else if (param.Status == InvitationBookingStatus.Absent)
            {
                predicate = predicate.And(x => x.Status == InvitationBookingStatus.Absent);
            }
            else if (param.Status == InvitationBookingStatus.Postponed)
            {
                predicate = predicate.And(x => x.Status == InvitationBookingStatus.Postponed);
            }
            else
            {

            }

            var dataQuery = _dbContext.Entity<TrInvitationBooking>()
                .Include(x => x.InvitationBookingDetails).ThenInclude(x => x.HomeroomStudent).ThenInclude(x => x.Student)
                .Include(x => x.UserTeacher)
                .Include(x => x.Venue)
                .Where(predicate);

            var query = dataQuery
               .Select(x => new
               {
                   Id = x.Id,
                   StudentName = x.InvitationBookingDetails.Select(x => (x.HomeroomStudent.Student.FirstName == null ? "" : "" + x.HomeroomStudent.Student.FirstName) + (x.HomeroomStudent.Student.MiddleName == null ? "" : " " + x.HomeroomStudent.Student.MiddleName) + (x.HomeroomStudent.Student.LastName == null ? "" : " " + x.HomeroomStudent.Student.LastName)).ToList(),
                   BinusianId = x.InvitationBookingDetails.Select(x => x.HomeroomStudent.Student.Id).ToList(),
                   IdHomeroomStudent = x.InvitationBookingDetails.Select(x => x.HomeroomStudent.Id).ToList(),
                   IdInvitationBooking = x.Id,
                   IdInvitationBookingSetting = x.IdInvitationBookingSetting,
                   InitiateBy = x.InitiateBy,
                   IdUserTeaacher = x.IdUserTeacher,
                   TeacherName = x.UserTeacher.DisplayName,
                   Venue = x.Venue.Description,
                   StartDateInvitation = x.StartDateInvitation,
                   EndtDateInvitation = x.EndDateInvitation,
                   Status = x.Status,
                   Note = x.Note,
                   CanCancel = true,
                   CanReschedule = true
               });

            List<GetListRecapResult> data;
            var result = await query
                .ToListAsync(CancellationToken);

            data = result.Select(x => new GetListRecapResult
            {
                Id = x.Id,
                StudentName = ConvertString(x.StudentName),
                BinusianID = ConvertString(x.BinusianId),
                Grade = ConvertGetGrade(x.IdHomeroomStudent),
                Class = ConvertGetClass(x.IdHomeroomStudent),
                InitiateBy = x.InitiateBy,
                Teacher = new CodeWithIdVm(x.IdUserTeaacher, x.IdUserTeaacher, x.TeacherName),
                Venue = x.Venue,
                StartDateInvitation = x.StartDateInvitation,
                EndDateInvitation = x.EndtDateInvitation,
                Status = x.Status,
                Note = x.Note,
                CanCancel = x.CanCancel,
                CanReschedule = x.CanReschedule,
                HomeroomStudentId = ConvertString(x.IdHomeroomStudent),
            }).OrderBy(x => x.StudentName).ToList();

            if (data.Any())
            {
                var dataEmailInvitation = await _dbContext.Entity<TrInvitationEmail>()
                                            .Where(x => x.IdInvitationBookingSetting == result.Select(x => x.IdInvitationBookingSetting).First())
                                            .ToListAsync(CancellationToken);

                if (dataEmailInvitation.Any())
                {
                    foreach (var item in data)
                    {
                        item.InitiateBy = dataEmailInvitation.Any(x => x.IdHomeroomStudent == item.HomeroomStudentId)
                             == false ? item.InitiateBy : dataEmailInvitation.Where(x => x.IdHomeroomStudent == item.HomeroomStudentId)
                            .Select(x => x.InitiateBy).FirstOrDefault();
                    }
                }
            }

            var excelRecap = GenerateExcel(data);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Recap{DateTime.Now.Ticks}.xlsx"
            };
        }

        private string ConvertString(List<string> List)
        {
            var ValueStirng = "";

            foreach (var item in List)
            {
                ValueStirng += ValueStirng == "" ? item : ", " + item;
            }

            return ValueStirng;
        }

        private string ConvertGetGrade(List<string> List)
        {
            var ValueStirng = "";

            foreach (var item in List)
            {
                ValueStirng += ValueStirng == "" ? GetClass(item).Code : ", " + GetClass(item).Code;
            }

            return ValueStirng;
        }

        private string ConvertGetClass(List<string> List)
        {
            var ValueStirng = "";

            foreach (var item in List)
            {
                ValueStirng += ValueStirng == "" ? GetClass(item).Description : ", " + GetClass(item).Description;
            }

            return ValueStirng;
        }

        private byte[] GenerateExcel(List<GetListRecapResult> data)
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
            cellParticipant.SetCellValue("Student Name");
            cellParticipant.CellStyle = boldStyle;
            var cellBinusianid = rowHeader.CreateCell(1);
            cellBinusianid.SetCellValue("Binusian ID");
            cellBinusianid.CellStyle = boldStyle;
            var cellGrade = rowHeader.CreateCell(2);
            cellGrade.SetCellValue("Grade");
            cellGrade.CellStyle = boldStyle;
            var cellClass = rowHeader.CreateCell(3);
            cellClass.SetCellValue("Class");
            cellClass.CellStyle = boldStyle;
            var cellLevel = rowHeader.CreateCell(4);
            cellLevel.SetCellValue("Initiate by");
            cellLevel.CellStyle = boldStyle;
            var cellTeacher = rowHeader.CreateCell(5);
            cellTeacher.SetCellValue("Teacher Name");
            cellTeacher.CellStyle = boldStyle;
            var cellHomeroom = rowHeader.CreateCell(6);
            cellHomeroom.SetCellValue("Venue");
            cellHomeroom.CellStyle = boldStyle;
            var cellEventname = rowHeader.CreateCell(7);
            cellEventname.SetCellValue("Date");
            cellEventname.CellStyle = boldStyle;
            var cellPlace = rowHeader.CreateCell(8);
            cellPlace.SetCellValue("Time");
            cellPlace.CellStyle = boldStyle;
            var cellActivity = rowHeader.CreateCell(9);
            cellActivity.SetCellValue("Status");
            cellActivity.CellStyle = boldStyle;
            var cellAward = rowHeader.CreateCell(10);
            cellAward.SetCellValue("Note");
            cellAward.CellStyle = boldStyle;

            int rowIndex = 1;
            int startColumn = 0;

            foreach (var itemData in data)
            {
                rowHeader = sheet.CreateRow(rowIndex);
                cellParticipant = rowHeader.CreateCell(0);
                cellParticipant.SetCellValue(itemData.StudentName);
                cellParticipant = rowHeader.CreateCell(1);
                cellParticipant.SetCellValue(itemData.BinusianID);
                cellParticipant = rowHeader.CreateCell(2);
                cellParticipant.SetCellValue(itemData.Grade);
                cellParticipant = rowHeader.CreateCell(3);
                cellParticipant.SetCellValue(itemData.Class);
                cellParticipant = rowHeader.CreateCell(4);
                cellParticipant.SetCellValue(itemData.InitiateBy.ToString());
                cellParticipant = rowHeader.CreateCell(5);
                cellParticipant.SetCellValue(itemData.Teacher.Description);
                cellParticipant = rowHeader.CreateCell(6);
                cellParticipant.SetCellValue(itemData.Venue);
                cellParticipant = rowHeader.CreateCell(7);
                cellParticipant.SetCellValue(itemData.StartDateInvitation.ToString("dd MMMM yyyy"));
                cellParticipant = rowHeader.CreateCell(8);
                cellParticipant.SetCellValue(itemData.StartDateInvitation.ToString("HH:mm"));
                cellParticipant = rowHeader.CreateCell(9);
                cellParticipant.SetCellValue(itemData.Status.ToString());
                cellParticipant = rowHeader.CreateCell(10);
                cellParticipant.SetCellValue(itemData.Note);

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
