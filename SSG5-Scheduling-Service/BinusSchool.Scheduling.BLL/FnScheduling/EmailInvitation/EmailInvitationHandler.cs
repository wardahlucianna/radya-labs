using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.EmailInvitation.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Spreadsheet;

namespace BinusSchool.Scheduling.FnSchedule.EmailInvitation
{
    public class EmailInvitationHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public EmailInvitationHandler(ISchedulingDbContext InvotationEmailDbContext, IMachineDateTime DateTime)
        {
            _dbContext = InvotationEmailDbContext;
            _dateTime = DateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            var GetEmailInvitation = await _dbContext.Entity<TrInvitationEmail>()
              .Where(x => ids.Contains(x.Id))
              .ToListAsync(CancellationToken);

            GetEmailInvitation.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrInvitationEmail>().UpdateRange(GetEmailInvitation);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetEmailInvitationRequest>();
            var predicate = PredicateBuilder.Create<TrInvitationEmail>(x => x.UserIn == param.IdUser && x.InvitationBookingSetting.IdAcademicYear == param.IdAcademicYear);
            string[] _columns = { "StudentName", "BinusanId", "InitiateBy", "InvitationName", "InvitationDate", "LastSendEmail" };

            if (!string.IsNullOrEmpty(param.StartDate.ToString()) && !string.IsNullOrEmpty(param.EndDate.ToString()))
                predicate = predicate.And(x => (x.InvitationBookingSetting.InvitationStartDate.Date >= Convert.ToDateTime(param.StartDate) && x.InvitationBookingSetting.InvitationStartDate.Date <= Convert.ToDateTime(param.EndDate))
                    || (x.InvitationBookingSetting.InvitationEndDate.Date >= Convert.ToDateTime(param.StartDate) && x.InvitationBookingSetting.InvitationEndDate.Date <= Convert.ToDateTime(param.EndDate)));

            if (!string.IsNullOrEmpty(param.IdInvitationBookingSetting))
                predicate = predicate.And(x => x.IdInvitationBookingSetting == param.IdInvitationBookingSetting);

            var DateInvitation = await _dbContext.Entity<TrInvitationEmail>()
               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
               .Include(e => e.InvitationBookingSetting)
              .Where(predicate)
               .Select(x => new
               {
                   Id = x.Id,
                   StudentName = x.HomeroomStudent.Student.FirstName + (x.HomeroomStudent.Student.MiddleName == null ? "" : " " + x.HomeroomStudent.Student.MiddleName) + (x.HomeroomStudent.Student.LastName == null ? "" : " " + x.HomeroomStudent.Student.LastName),
                   BinusianId = x.HomeroomStudent.IdStudent,
                   InitiateBy = x.InitiateBy.ToString(),
                   InvitationName = x.InvitationBookingSetting.InvitationName,
                   InvitationDate = x.InvitationBookingSetting.InvitationStartDate.Date.ToString("dd MMM yyyy HH:mm") + "-" + x.InvitationBookingSetting.InvitationEndDate.Date.ToString("dd MMM yyyy HH:mm"),
                   LastSendEmail = x.LastSendEmailInvitation == null ? "-" : Convert.ToDateTime(x.LastSendEmailInvitation).ToString("dd MMM yyyy HH:mm"),
                   InvitationDateEnd = x.InvitationBookingSetting.InvitationEndDate,
                   IdInvitationBookingSetting = x.IdInvitationBookingSetting,
               }).ToListAsync(CancellationToken);

            var query = DateInvitation.Distinct();

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.StudentName.ToLower().Contains(param.Search.ToLower()) || x.BinusianId.ToLower().Contains(param.Search.ToLower()));

            //ordering
            switch (param.OrderBy)
            {
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;

                case "BinusanId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.BinusianId)
                        : query.OrderBy(x => x.BinusianId);
                    break;
                case "InitiateBy":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InitiateBy)
                        : query.OrderBy(x => x.InitiateBy);
                    break;
                case "InvitationName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationName)
                        : query.OrderBy(x => x.InvitationName);
                    break;
                case "InvitationDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.InvitationDate)
                        : query.OrderBy(x => x.InvitationDate);
                    break;
                case "LastSendEmail":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.LastSendEmail)
                        : query.OrderBy(x => x.LastSendEmail);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(x => new GetEmailInvitationResult
                {
                    Id = x.Id,
                    StudentName = x.StudentName,
                    BinusianId = x.BinusianId,
                    InitiateBy = x.InitiateBy,
                    InvitationName = x.InvitationName,
                    InvitationDate = x.InvitationDate,
                    LastSendEmail = x.LastSendEmail,
                    IdInvitationBookingSetting = x.IdInvitationBookingSetting,
                    CanSendEmail = x.InvitationDateEnd.Date >= _dateTime.ServerTime.Date,
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetEmailInvitationResult
                {
                    Id = x.Id,
                    StudentName = x.StudentName,
                    BinusianId = x.BinusianId,
                    InitiateBy = x.InitiateBy,
                    InvitationName = x.InvitationName,
                    InvitationDate = x.InvitationDate,
                    LastSendEmail = x.LastSendEmail,
                    IdInvitationBookingSetting = x.IdInvitationBookingSetting,
                    CanSendEmail = x.InvitationDateEnd.Date >= _dateTime.ServerTime.Date
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));

        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddEmailInvitationRequest, AddEmailInvitationValidator>();

            var DataInvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSetting>()
                .Where(e => e.Id == body.IdInvitationBookingSetting)
                .FirstOrDefaultAsync(CancellationToken);

            if (DataInvitationBookingSetting == null)
            {
                throw new BadRequestException("Invitation booking setting is not found");
            }

            var DataInvitationEmail = await _dbContext.Entity<TrInvitationEmail>()
                .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting && body.IdHomeroomStudent.Contains(e.IdHomeroomStudent))
                .ToListAsync(CancellationToken);

           foreach (var IdHOmeroomStudent in body.IdHomeroomStudent)
           {
                var ExsisInvitationEmailByHomeroomStudent = DataInvitationEmail.Where(e => e.UserIn == AuthInfo.UserId && e.IdHomeroomStudent == IdHOmeroomStudent).Any();

                if (!ExsisInvitationEmailByHomeroomStudent)
                {
                    var NewInvitationEmail = new TrInvitationEmail
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdHomeroomStudent = IdHOmeroomStudent,
                        IdInvitationBookingSetting = body.IdInvitationBookingSetting,
                        InitiateBy = body.InitiateBy,
                        LastSendEmailInvitation = body.IsSendEmail == true ? _dateTime.ServerTime : (DateTime?)null,
                    };
                    _dbContext.Entity<TrInvitationEmail>().Add(NewInvitationEmail);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Sand Email
            if (body.IsSendEmail)
            {
                var GetInvitationEmail = await _dbContext.Entity<TrInvitationEmail>()
                                            .Include(e => e.InvitationBookingSetting)
                                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                                            .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting && body.IdHomeroomStudent.Contains(e.IdHomeroomStudent) && e.UserIn== AuthInfo.UserId)
                                            .ToListAsync(CancellationToken);

                foreach (var IdHOmeroomStudent in body.IdHomeroomStudent)
                {
                    var GetInvitationEmailByHomeroomStudent = GetInvitationEmail.Where(e => e.IdHomeroomStudent == IdHOmeroomStudent).FirstOrDefault();

                    if (GetInvitationEmailByHomeroomStudent != null)
                    {
                        var TeacherName = await _dbContext.Entity<MsUser>()
                                            .Where(e => e.Id == GetInvitationEmailByHomeroomStudent.UserIn)
                                            .Select(e => e.DisplayName)
                                            .FirstOrDefaultAsync(CancellationToken);

                        EmailInvitationResult EmailInvitatin = new EmailInvitationResult
                        {
                            StudentName = GetInvitationEmailByHomeroomStudent.HomeroomStudent.Student.FirstName
                                            + (GetInvitationEmailByHomeroomStudent.HomeroomStudent.Student.MiddleName == null ? "" : " " + GetInvitationEmailByHomeroomStudent.HomeroomStudent.Student.MiddleName)
                                            + (GetInvitationEmailByHomeroomStudent.HomeroomStudent.Student.LastName == null ? "" : " " + GetInvitationEmailByHomeroomStudent.HomeroomStudent.Student.LastName),
                            BinusianId = GetInvitationEmailByHomeroomStudent.HomeroomStudent.IdStudent,
                            InvitationName = GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.InvitationName,
                            EarlyBook = GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.StaffBookingStartDate == null ? "" : Convert.ToDateTime(GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.StaffBookingStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.StaffBookingEndDate).ToString("dd MMM yyyy HH:mm"),
                            ParentBook = Convert.ToDateTime(GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.ParentBookingStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.ParentBookingEndDate).ToString("dd MMM yyyy HH:mm"),
                            Teacher = TeacherName,
                            IdTeacher = GetInvitationEmailByHomeroomStudent.UserIn,
                            IdParent = "P" + GetInvitationEmailByHomeroomStudent.HomeroomStudent.IdStudent,
                            IdInvitationBookingSetting = GetInvitationEmailByHomeroomStudent.IdInvitationBookingSetting,
                            IsSchedulingSameTime = GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.SchedulingSiblingSameTime,
                            DateInvitation = Convert.ToDateTime(GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.InvitationStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(GetInvitationEmailByHomeroomStudent.InvitationBookingSetting.InvitationEndDate).ToString("dd MMM yyyy HH:mm"),
                            IdSchool = GetInvitationEmailByHomeroomStudent.HomeroomStudent.Homeroom.AcademicYear.IdSchool
                        };

                        if(KeyValues.ContainsKey("EmailInvitatin"))
                        {
                            KeyValues.Remove("EmailInvitatin");
                        }
                        KeyValues.Add("EmailInvitatin", EmailInvitatin);
                        var Notification = APP18Notification(KeyValues, AuthInfo);
                    }
                }
            }
            #endregion
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<SendEmailInvitationRequest, SendEmailInvitationValidator>();

            var GetInvitationEmail = await _dbContext.Entity<TrInvitationEmail>()
                                        .Include(e => e.InvitationBookingSetting)
                                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                        .Where(e => body.IdInvitationEmail.Contains(e.Id))
                                        .ToListAsync(CancellationToken);

            foreach (var Item in GetInvitationEmail)
            {
                Item.LastSendEmailInvitation=  _dateTime.ServerTime;
                _dbContext.Entity<TrInvitationEmail>().Update(Item);
                await _dbContext.SaveChangesAsync();

                var TeacherName = await _dbContext.Entity<MsUser>()
                                    .Where(e => e.Id == Item.UserIn)
                                    .Select(e => e.DisplayName)
                                    .FirstOrDefaultAsync(CancellationToken);

                EmailInvitationResult EmailInvitatin = new EmailInvitationResult
                {
                    StudentName = Item.HomeroomStudent.Student.FirstName
                                    + (Item.HomeroomStudent.Student.MiddleName == null ? "" : " " + Item.HomeroomStudent.Student.MiddleName)
                                    + (Item.HomeroomStudent.Student.LastName == null ? "" : " " + Item.HomeroomStudent.Student.LastName),
                    BinusianId = Item.HomeroomStudent.IdStudent,
                    InvitationName = Item.InvitationBookingSetting.InvitationName,
                    EarlyBook = Item.InvitationBookingSetting.StaffBookingStartDate == null ? "" : Convert.ToDateTime(Item.InvitationBookingSetting.StaffBookingStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(Item.InvitationBookingSetting.StaffBookingEndDate).ToString("dd MMM yyyy HH:mm"),
                    ParentBook = Convert.ToDateTime(Item.InvitationBookingSetting.ParentBookingStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(Item.InvitationBookingSetting.ParentBookingEndDate).ToString("dd MMM yyyy HH:mm"),
                    Teacher = TeacherName,
                    IdTeacher = Item.UserIn,
                    IdParent = "P" + Item.HomeroomStudent.IdStudent,
                    IdInvitationBookingSetting = Item.IdInvitationBookingSetting,
                    IsSchedulingSameTime = Item.InvitationBookingSetting.SchedulingSiblingSameTime,
                    DateInvitation = Convert.ToDateTime(Item.InvitationBookingSetting.InvitationStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(Item.InvitationBookingSetting.InvitationEndDate).ToString("dd MMM yyyy HH:mm"),
                };

                if(KeyValues.ContainsKey("EmailInvitatin"))
                {
                    KeyValues.Remove("EmailInvitatin");
                }
                KeyValues.Add("EmailInvitatin", EmailInvitatin);
                var Notification = APP18Notification(KeyValues, AuthInfo);
            }
            return Request.CreateApiResult2();
        }

        public static string APP18Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailInvitatin").Value;
            var EmailInvitation = JsonConvert.DeserializeObject<EmailInvitationResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP18")
                {
                    IdRecipients = new List<string>()
                    {
                        EmailInvitation.IdParent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
