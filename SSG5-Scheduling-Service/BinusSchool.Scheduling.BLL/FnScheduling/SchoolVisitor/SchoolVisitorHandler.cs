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
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolVisitor;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.SchoolVisitor.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolVisitor
{
    public class SchoolVisitorHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public SchoolVisitorHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetVisitorSchool = await _dbContext.Entity<TrVisitorSchool>()
                                     .Where(e => ids.Contains(e.Id))
                                     .ToListAsync(CancellationToken);

            GetVisitorSchool.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrVisitorSchool>().UpdateRange(GetVisitorSchool);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var Items = await _dbContext.Entity<TrVisitorSchool>()
                                     .Where(e => e.Id == id)
                                     .Select(e => new DetailSchoolVisitorResult
                                     {
                                         Id = e.Id,
                                         BookFullName = e.IdUserBook == null ? e.BookName : e.UserBook.DisplayName,
                                         BookBinusianId = e.IdUserBook,
                                         BookUserName = e.UserBook.Username,
                                         VisitorFullName = e.IdUserVisitor == null ? e.VisitorName + " (Without Binusian ID)" : e.UserVisitor.DisplayName,
                                         VisitorDate = e.VisitorDate,
                                         Vanue = new ItemValueVm
                                         {
                                             Id = e.IdVenue,
                                             Description = e.Venue.Description
                                         },
                                         Description = e.Description
                                     })
                                     .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(Items as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetSchoolVisitorRequest>();
            string[] _columns = { "VisitorName", "VisitorDate", "Venue", "UserBook", "VisitType", "Description", "Time" };

            List<GetSchoolVisitorResult> DataVisitor = new List<GetSchoolVisitorResult>();

            var GetVisitorInvitationBooking = await (from InvitationBookingDetail in _dbContext.Entity<TrInvitationBookingDetail>()
                                                     join InvitationBooking in _dbContext.Entity<TrInvitationBooking>() on InvitationBookingDetail.IdInvitationBooking equals InvitationBooking.Id
                                                     join InvitationBookingSetting in _dbContext.Entity<TrInvitationBookingSetting>() on InvitationBooking.IdInvitationBookingSetting equals InvitationBookingSetting.Id
                                                     join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on InvitationBookingDetail.IdHomeroomStudent equals HomeroomStudent.Id
                                                     join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                                                     join Venue in _dbContext.Entity<MsVenue>() on InvitationBooking.IdVenue equals Venue.Id
                                                     join UserTeacher in _dbContext.Entity<MsUser>() on InvitationBooking.IdUserTeacher equals UserTeacher.Id
                                                     join UserCreate in _dbContext.Entity<MsUser>() on InvitationBookingDetail.UserIn equals UserCreate.Id into JoinedCreate
                                                     from UserCreate in JoinedCreate.DefaultIfEmpty()
                                                     where InvitationBooking.StartDateInvitation.Date == param.VisitDate.Date && InvitationBookingSetting.IdAcademicYear == param.IdAcademicYear
                                                     select new GetSchoolVisitorResult
                                                     {
                                                         Id = InvitationBooking.Id,
                                                         VisitorName = InvitationBooking.InitiateBy == InvitationBookingInitiateBy.Staff || InvitationBooking.InitiateBy == InvitationBookingInitiateBy.Teacher
                                                            ? Student.FirstName + (string.IsNullOrEmpty(Student.MiddleName) ? "" : " " + Student.MiddleName) + (string.IsNullOrEmpty(Student.LastName) ? "" : " " + Student.LastName)
                                                            : UserCreate.DisplayName,
                                                         VisitorDate = InvitationBooking.StartDateInvitation,
                                                         Venue = Venue.Description,
                                                         UserBook = UserTeacher.DisplayName,
                                                         VisitType = "Invitation Booking",
                                                         Description = InvitationBooking.Note,
                                                         DisabledButton = true,
                                                         Time = InvitationBooking.StartDateInvitation.TimeOfDay.ToString(@"hh\:mm"),
                                                     }).ToListAsync(CancellationToken);
            DataVisitor.AddRange(GetVisitorInvitationBooking);

            var GetVisitorPersenalInvitation = await (from PersonalInvitation in _dbContext.Entity<TrPersonalInvitation>()
                                                      join Student in _dbContext.Entity<MsStudent>() on PersonalInvitation.IdStudent equals Student.Id
                                                      join Venue in _dbContext.Entity<MsVenue>() on PersonalInvitation.IdVenue equals Venue.Id
                                                      join UserTeacher in _dbContext.Entity<MsUser>() on PersonalInvitation.IdUserTeacher equals UserTeacher.Id
                                                      join UserCreate in _dbContext.Entity<MsUser>() on PersonalInvitation.UserIn equals UserCreate.Id into JoinedCreate
                                                      from UserCreate in JoinedCreate.DefaultIfEmpty()
                                                      where PersonalInvitation.InvitationDate.Date == param.VisitDate.Date && PersonalInvitation.IdAcademicYear == param.IdAcademicYear
                                                      select new GetSchoolVisitorResult
                                                      {
                                                          Id = PersonalInvitation.Id,
                                                          VisitorName = !PersonalInvitation.IsStudent && !PersonalInvitation.IsFather && !PersonalInvitation.IsMother
                                                                         ? UserCreate.DisplayName
                                                                         : Student.FirstName + (string.IsNullOrEmpty(Student.MiddleName) ? "" : " " + Student.MiddleName) + (string.IsNullOrEmpty(Student.LastName) ? "" : " " + Student.LastName),
                                                          VisitorDate = PersonalInvitation.InvitationDate,
                                                          Venue = Venue.Description,
                                                          UserBook = UserTeacher.DisplayName,
                                                          VisitType = "Personal Invitation",
                                                          Description = PersonalInvitation.Description,
                                                          DisabledButton = true,
                                                          Time = PersonalInvitation.InvitationStartTime.ToString(@"hh\:mm"),
                                                      }).ToListAsync(CancellationToken);

            DataVisitor.AddRange(GetVisitorPersenalInvitation);

            var GetVisitorSchool = await (from VisitorSchool in _dbContext.Entity<TrVisitorSchool>()
                                          join Venue in _dbContext.Entity<MsVenue>() on VisitorSchool.IdVenue equals Venue.Id
                                          join UserBook in _dbContext.Entity<MsUser>() on VisitorSchool.IdUserBook equals UserBook.Id into JoinedUserBook
                                          from UserBook in JoinedUserBook.DefaultIfEmpty()
                                          where VisitorSchool.VisitorDate.Date == param.VisitDate.Date && VisitorSchool.IdAcademicYear == param.IdAcademicYear
                                          select new GetSchoolVisitorResult
                                          {
                                              Id = VisitorSchool.Id,
                                              VisitorName = VisitorSchool.VisitorName,
                                              VisitorDate = VisitorSchool.VisitorDate,
                                              Venue = Venue.Description,
                                              UserBook = UserBook.DisplayName,
                                              VisitType = "General Visitor",
                                              Description = VisitorSchool.Description,
                                              DisabledButton = false,
                                              Time = VisitorSchool.VisitorDate.TimeOfDay.ToString(@"hh\:mm"),
                                          }).ToListAsync(CancellationToken);

            DataVisitor.AddRange(GetVisitorSchool);

            var query = DataVisitor.Distinct();
            //search
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.VisitorName.ToLower().Contains(param.Search.ToLower())
                                        || x.UserBook.ToLower().Contains(param.Search.ToLower())
                                        || x.Description.ToLower().Contains(param.Search.ToLower()));

            if (!string.IsNullOrEmpty(param.VisitorType))
                query = query.Where(x => x.VisitType.ToLower().Contains(param.Search.ToLower()));

            //ordering
            switch (param.OrderBy)
            {
                case "VisitorName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.VisitorName)
                        : query.OrderBy(x => x.VisitorName);
                    break;
                case "VisitorDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.VisitorDate)
                        : query.OrderBy(x => x.VisitorDate);
                    break;
                case "Venue":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Venue)
                        : query.OrderBy(x => x.Venue);
                    break;
                case "UserBook":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.UserBook)
                        : query.OrderBy(x => x.UserBook);
                    break;
                case "VisitType":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.VisitType)
                        : query.OrderBy(x => x.VisitType);
                    break;
                case "Description":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
                case "Time":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Description)
                        : query.OrderBy(x => x.Description);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(e => new GetSchoolVisitorResult
                {
                    Id = e.Id,
                    VisitorName = e.VisitorName,
                    VisitorDate = e.VisitorDate,
                    Venue = e.Venue,
                    UserBook = e.UserBook,
                    VisitType = e.VisitType,
                    Description = e.Description,
                    DisabledButton = e.DisabledButton,
                    Time = e.Time,
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(e => new GetSchoolVisitorResult
                {
                    Id = e.Id,
                    VisitorName = e.VisitorName,
                    VisitorDate = e.VisitorDate,
                    Venue = e.Venue,
                    UserBook = e.UserBook,
                    VisitType = e.VisitType,
                    Description = e.Description,
                    DisabledButton = e.DisabledButton,
                    Time = e.Time,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddSchoolVisitorRequest, AddSchoolVisitorValidator>();

            var AddVisitorSchool = new TrVisitorSchool
            {
                Id = Guid.NewGuid().ToString(),
                IdUserBook = string.IsNullOrEmpty(body.IdUserBook) ? null : body.IdUserBook,
                BookName = body.NameBook,
                IdUserVisitor = string.IsNullOrEmpty(body.IdUserVisitor) ? null : body.IdUserVisitor,
                VisitorName = body.NameVisitor,
                IdVenue = body.IdVenue,
                Description = body.Description,
                VisitorDate = body.VisitorDate,
                IdAcademicYear = body.IdAcademicYear,
            };

            _dbContext.Entity<TrVisitorSchool>().Add(AddVisitorSchool);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateSchoolVisitorRequest, UpdateSchoolVisitorValidator>();

            var GetSchoolVisitor = await _dbContext.Entity<TrVisitorSchool>()
                                     .Where(e => e.Id == body.Id).FirstOrDefaultAsync(CancellationToken);

            if (GetSchoolVisitor == null)
                throw new BadRequestException($"Id TrVisitorBooking:{body.Id} is not found");

            GetSchoolVisitor.IdVenue = body.IdVenue;
            GetSchoolVisitor.VisitorDate = body.VisitorDate;
            GetSchoolVisitor.Description = body.Description;

            _dbContext.Entity<TrVisitorSchool>().Update(GetSchoolVisitor);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
