using System;
using System.Collections.Generic;
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
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class GetListRecapHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetListTeacherByInvitationRequest.IdInvitationBookingSetting),
        };
        private static readonly string[] _columns = { "StudentName", "TeacherName" };
        private readonly ISchedulingDbContext _dbContext;

        private CodeWithIdVm GetClass(string IdHomeroomStudent)
        {
            if(IdHomeroomStudent == null)
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
        public GetListRecapHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListRecapRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<TrInvitationBooking>(x => x.IsActive==true && x.StatusData== InvitatinBookingStatusData.Success);
            
            predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting);
            
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
                   StudentNameFirst = x.InvitationBookingDetails.Select(x => (x.HomeroomStudent.Student.FirstName == null ? "" : "" + x.HomeroomStudent.Student.FirstName) + (x.HomeroomStudent.Student.MiddleName == null ? "" : " " + x.HomeroomStudent.Student.MiddleName) + (x.HomeroomStudent.Student.LastName == null ? "" : " " + x.HomeroomStudent.Student.LastName)).First(),
                   BinusianId = x.InvitationBookingDetails.Select(x => x.HomeroomStudent.Student.Id).ToList(),
                   BinusianIdFirst = x.InvitationBookingDetails.Select(x => x.HomeroomStudent.Student.Id).First(),
                   IdHomeroomStudent = x.InvitationBookingDetails.Select(x => x.HomeroomStudent.Id).ToList(),
                   IdInvitationBooking = x.Id,
                   InitiateBy = x.InitiateBy,
                   IdUserTeaacher = x.IdUserTeacher,
                   TeacherName = x.UserTeacher.DisplayName,
                   IdVenue = x.Venue.Id,
                   Venue = x.Venue.Description,
                   StartDateInvitation = x.StartDateInvitation,
                   EndtDateInvitation = x.EndDateInvitation,
                   Status = x.Status,
                   Note = x.Note==null?"":x.Note,
                   CanCancel = true,
                   CanReschedule = true,
                   IdInvitationBookingSetting = x.IdInvitationBookingSetting
               });

            var queryData = await query 
                    .ToListAsync(CancellationToken);

            if (!string.IsNullOrWhiteSpace(param.Search))
                queryData = queryData.Where(x => x.StudentNameFirst.Contains(param.Search, StringComparison.OrdinalIgnoreCase)).ToList();

            //ordering
            switch (param.OrderBy)
            {
                case "StudentName":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.StudentNameFirst).ToList()
                        : queryData.OrderBy(x => x.StudentNameFirst).ToList();
                    break;
                case "BinusianID":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.BinusianIdFirst).ToList()
                        : queryData.OrderBy(x => x.BinusianIdFirst).ToList();
                    break;
                case "InitiateBy":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.InitiateBy).ToList()
                        : queryData.OrderBy(x => x.InitiateBy).ToList();
                    break;
                case "TeacherName":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.TeacherName).ToList()
                        : queryData.OrderBy(x => x.TeacherName).ToList();
                    break;
                case "Venue":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.Venue).ToList()
                        : queryData.OrderBy(x => x.Venue).ToList();
                    break;
                case "DateTime":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.StartDateInvitation).ToList()
                        : queryData.OrderBy(x => x.StartDateInvitation).ToList();
                    break;
                case "Status":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.Status).ToList()
                        : queryData.OrderBy(x => x.Status).ToList();
                    break;
                case "Note":
                    queryData = param.OrderType == OrderType.Desc
                        ? queryData.OrderByDescending(x => x.Note).ToList()
                        : queryData.OrderBy(x => x.Note).ToList();
                    break;
            };


            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = queryData;

                items = result.Select(x => new GetListRecapResult
                {
                    Id = x.Id,
                    StudentName = ConvertString(x.StudentName),
                    BinusianID = ConvertString(x.BinusianId),
                    HomeroomStudentId = ConvertString(x.IdHomeroomStudent),
                    Grade = ConvertGetGrade(x.IdHomeroomStudent),
                    Class = ConvertGetClass(x.IdHomeroomStudent),
                    IdInvitationBooking = x.IdInvitationBooking,
                    InitiateBy = x.InitiateBy,
                    Teacher = new CodeWithIdVm(x.IdUserTeaacher,x.IdUserTeaacher,x.TeacherName),
                    IdVenue = x.IdVenue,
                    Venue = x.Venue,
                    StartDateInvitation = x.StartDateInvitation,
                    EndDateInvitation = x.EndtDateInvitation,
                    Status = x.Status,
                    Note = x.Note,
                    CanCancel = x.CanCancel,
                    CanReschedule = x.CanReschedule
                }).ToList();
            }
            else
            {
                var result = queryData
                    .SetPagination(param)
                    .ToList();

                var data = result.Select(x => new GetListRecapResult
                {
                    Id = x.Id,
                    StudentName = ConvertString(x.StudentName),
                    BinusianID = ConvertString(x.BinusianId),
                    HomeroomStudentId = ConvertString(x.IdHomeroomStudent),
                    Grade = ConvertGetGrade(x.IdHomeroomStudent),
                    Class = ConvertGetClass(x.IdHomeroomStudent),
                    IdInvitationBooking = x.IdInvitationBooking,
                    InitiateBy = x.InitiateBy,
                    Teacher = new CodeWithIdVm(x.IdUserTeaacher, x.IdUserTeaacher, x.TeacherName),
                    IdVenue = x.IdVenue,
                    Venue = x.Venue,
                    StartDateInvitation = x.StartDateInvitation,
                    EndDateInvitation = x.EndtDateInvitation,
                    Status = x.Status,
                    Note = x.Note,
                    CanCancel = x.CanCancel,
                    CanReschedule = x.CanReschedule,
                    IdUserTeacher = x.IdUserTeaacher
                }).ToList();
              
                if (data.Any())
                {
                    var dataEmailInvitation = await _dbContext.Entity<TrInvitationEmail>()
                                                .Where(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting )
                                                .ToListAsync(CancellationToken);

                    if (dataEmailInvitation.Any())
                    {
                        foreach (var item in data)
                        {
                            var coba = dataEmailInvitation.Where(x => x.IdHomeroomStudent == item.HomeroomStudentId && x.UserIn == item.IdUserTeacher).ToList();

                            item.InitiateBy = dataEmailInvitation.Any(x=> x.IdHomeroomStudent == item.HomeroomStudentId && x.UserIn == item.IdUserTeacher)
                                 == false ? item.InitiateBy : dataEmailInvitation.Where(x => x.IdHomeroomStudent == item.HomeroomStudentId && x.UserIn == item.IdUserTeacher)
                                .Select(x => x.InitiateBy).FirstOrDefault();
                        }
                    }
                }

                items = data.Select(x => new GetListRecapResult
                {
                    Id = x.Id,
                    StudentName = x.StudentName,
                    BinusianID = x.BinusianID,
                    HomeroomStudentId = x.HomeroomStudentId,
                    Grade = x.Grade,
                    Class = x.Class,
                    IdInvitationBooking = x.IdInvitationBooking,
                    InitiateBy = x.InitiateBy,
                    Teacher = x.Teacher,
                    IdVenue = x.IdVenue,
                    Venue = x.Venue,
                    StartDateInvitation = x.StartDateInvitation,
                    EndDateInvitation = x.EndDateInvitation,
                    Status = x.Status,
                    Note = x.Note,
                    CanCancel = x.CanCancel,
                    CanReschedule = x.CanReschedule
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
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
    }
}
