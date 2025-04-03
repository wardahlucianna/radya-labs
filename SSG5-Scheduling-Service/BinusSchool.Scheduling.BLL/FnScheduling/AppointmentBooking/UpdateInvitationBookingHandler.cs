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
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class UpdateInvitationBookingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public UpdateInvitationBookingHandler(ISchedulingDbContext schedulingDbContext, IMachineDateTime dateTime)
        {
            _dbContext = schedulingDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateInvitationBookingRequest, UpdateInvitationBookingRequestValidator>();

            try
            {
                if (_dateTime.ServerTime > body.StartDateTimeInvitation)
                    throw new BadRequestException("Cannot choose availability time less than the current time");

                List<EmailRescheduleResult> Email = new List<EmailRescheduleResult>();
                var DataInvitationBooking = await _dbContext.Entity<TrInvitationBooking>()
                    .Include(e => e.UserTeacher)
                    .Include(e => e.Venue)
                    .Include(e => e.InvitationBookingSetting).ThenInclude(e => e.AcademicYears)
                    .Include(e => e.InvitationBookingDetails).ThenInclude(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                    .Where(e => e.Id == body.Id)
                    .SingleOrDefaultAsync(CancellationToken);

                var NamaParent = await _dbContext.Entity<MsUser>()
                                    .Where(e => e.Id == ("P" + DataInvitationBooking.InvitationBookingDetails.Select(e => e.HomeroomStudent.IdStudent).FirstOrDefault()))
                                    .Select(e => e.DisplayName)
                                    .FirstOrDefaultAsync(CancellationToken);

                Email.Add(new EmailRescheduleResult
                {
                    IdInvitationBookingSetting = DataInvitationBooking.InvitationBookingSetting.Id,
                    AcademicYear = DataInvitationBooking.InvitationBookingSetting.AcademicYears.Description,
                    InvitationName = DataInvitationBooking.InvitationBookingSetting.InvitationName,
                    InvitationDate = Convert.ToDateTime(DataInvitationBooking.InvitationBookingSetting.InvitationStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(DataInvitationBooking.InvitationBookingSetting.InvitationEndDate).ToString("dd MMM yyyy HH:mm"),
                    //StudentName = ConvertString(DataInvitationBooking.InvitationBookingDetails.Select(x => x.HomeroomStudent.Student.FirstName
                    //                + (x.HomeroomStudent.Student.MiddleName == null ? "" : " " + x.HomeroomStudent.Student.MiddleName)
                    //                + (x.HomeroomStudent.Student.LastName == null ? "" : " " + x.HomeroomStudent.Student.LastName)).ToList()),
                    StudentName = ConvertString(DataInvitationBooking.InvitationBookingDetails.Select(x => x.HomeroomStudent.Student.FirstName != null ? $"{x.HomeroomStudent.Student.FirstName.Trim()} {x.HomeroomStudent.Student.LastName.Trim()}" : $"{x.HomeroomStudent.Student.LastName.Trim()}").ToList()),
                    BinusianId = DataInvitationBooking.InvitationBookingDetails.Count() > 1 ? "-" : DataInvitationBooking.InvitationBookingDetails.Select(e => e.HomeroomStudent.IdStudent).FirstOrDefault(),
                    Venue = DataInvitationBooking.Venue.Description,
                    Date = DataInvitationBooking.StartDateInvitation.ToString("dd MMM yyyy"),
                    Time = DataInvitationBooking.StartDateInvitation.ToString("HH:mm"),
                    Type = "Old",
                    IdUserTeacher = DataInvitationBooking.IdUserTeacher,
                    TeacherName = DataInvitationBooking.UserTeacher.DisplayName,
                    BookByParent = NamaParent,
                    IdUserParent = DataInvitationBooking.InvitationBookingDetails.Select(e => "P" + e.HomeroomStudent.IdStudent).FirstOrDefault(),
                    IdSchool = DataInvitationBooking.InvitationBookingSetting.AcademicYears.IdSchool,
                    IdInvitationBooking = DataInvitationBooking.Id
                });


                if (DataInvitationBooking == null)
                {
                    throw new BadRequestException($"Id invitation booking {body.Id} is not found");
                }

                var IdHomeroomStudents = await _dbContext.Entity<TrInvitationBookingDetail>()
                    .Where(e => e.Id == body.Id)
                    .Select(e => e.IdHomeroomStudent)
                    .ToListAsync(CancellationToken);

                var dataInvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                    .Where(e => e.IdUserTeacher == body.IdUserTeacher
                        && e.IdInvitationBookingSetting == DataInvitationBooking.IdInvitationBookingSetting
                        && e.DateInvitation.Date == DataInvitationBooking.StartDateInvitation.Date
                        && e.StartTime == DataInvitationBooking.StartDateInvitation.TimeOfDay
                        && (e.IsPriority == true || e.IsPriority == null))
                    .FirstOrDefaultAsync(CancellationToken);

                var GetInvitationBooking = await _dbContext.Entity<TrInvitationBooking>()
                   .Where(e => e.IdUserTeacher == body.IdUserTeacher
                       && e.IdInvitationBookingSetting == DataInvitationBooking.IdInvitationBookingSetting
                       && e.StartDateInvitation.Date == DataInvitationBooking.StartDateInvitation.Date)
                   .ToListAsync(CancellationToken);

                if (!body.IsCancel)
                {
                    var DataInvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                  .Where(e => e.IdUserTeacher == body.IdUserTeacher
                      && e.IdInvitationBookingSetting == DataInvitationBooking.IdInvitationBookingSetting
                      && e.DateInvitation.Date == Convert.ToDateTime(body.StartDateTimeInvitation).Date
                      && e.StartTime == Convert.ToDateTime(body.StartDateTimeInvitation).TimeOfDay
                      && (e.IsPriority == true || e.IsPriority == null)
                      && !e.IsFixedBreak
                      )
                  .FirstOrDefaultAsync(CancellationToken);

                    if (DataInvitationBookingSetting == null)
                    {
                        throw new BadRequestException("Rescheduling Failed. The invitation schedule has been changed, please rebook the invitation");
                    }

                    var ScheduleBookingVip = await _dbContext.Entity<TrInvitationEmail>()
                               .Include(e => e.InvitationBookingSetting)
                              .Where(e => e.IdInvitationBookingSetting == DataInvitationBookingSetting.IdInvitationBookingSetting
                                       && IdHomeroomStudents.Contains(e.IdHomeroomStudent)
                                       && _dateTime.ServerTime >= e.InvitationBookingSetting.StaffBookingStartDate && _dateTime.ServerTime <= e.InvitationBookingSetting.StaffBookingEndDate
                                   )
                              .ToListAsync(CancellationToken);

                    var ScheduleBooking = await _dbContext.Entity<TrInvitationBookingSetting>()
                                   .Where(e => e.Id == DataInvitationBookingSetting.IdInvitationBookingSetting)
                                   .ToListAsync(CancellationToken);

                    foreach (var data in ScheduleBooking)
                    {
                        if (data.StaffBookingStartDate == null)
                            data.StaffBookingStartDate = data.ParentBookingStartDate;

                        if (data.StaffBookingEndDate == null)
                            data.StaffBookingEndDate = data.ParentBookingEndDate;
                    }

                    if (body.Role == "PARENT")
                    {
                        ScheduleBooking = ScheduleBooking
                       .Where(e => (_dateTime.ServerTime >= e.ParentBookingStartDate && _dateTime.ServerTime <= e.ParentBookingEndDate
                                || _dateTime.ServerTime >= e.StaffBookingStartDate && _dateTime.ServerTime <= e.ParentBookingEndDate)
                            )
                       .ToList();
                    }
                    else
                    {
                        ScheduleBooking = ScheduleBooking
                        .Where(e => (_dateTime.ServerTime >= e.ParentBookingStartDate && _dateTime.ServerTime <= e.ParentBookingEndDate
                                || _dateTime.ServerTime >= e.StaffBookingStartDate && _dateTime.ServerTime <= e.StaffBookingEndDate
                                || _dateTime.ServerTime >= e.ParentBookingStartDate && _dateTime.ServerTime <= e.StaffBookingEndDate
                                || _dateTime.ServerTime >= e.StaffBookingStartDate && _dateTime.ServerTime <= e.ParentBookingEndDate)
                            )
                        .ToList();
                    }


                    if (!ScheduleBookingVip.Any() && !ScheduleBooking.Any())
                        throw new BadRequestException("Rescheduling failed. Booking period has been ended");

                    var CountInvitationBooking = await _dbContext.Entity<TrInvitationBooking>()
                       .Where(e => e.IdInvitationBookingSetting == DataInvitationBooking.IdInvitationBookingSetting
                            && e.IdUserTeacher == body.IdUserTeacher
                            && e.StartDateInvitation == body.StartDateTimeInvitation)
                       .CountAsync(CancellationToken);

                    if (DataInvitationBookingSetting.QuotaSlot == CountInvitationBooking)
                    {
                        throw new BadRequestException("Rescheduling failed. The slot is full, please select another schedule");
                    }

                    DataInvitationBooking.IdVenue = body.IdVenue;
                    DataInvitationBooking.IdUserTeacher = body.IdUserTeacher;
                    DataInvitationBooking.StartDateInvitation = Convert.ToDateTime(body.StartDateTimeInvitation);
                    DataInvitationBooking.EndDateInvitation = Convert.ToDateTime(body.EndDateTimeInvitation);
                    _dbContext.Entity<TrInvitationBooking>().Update(DataInvitationBooking);

                    //update invitation booking setting
                    var dataInvitationBookingSettingNew = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                   .Where(e => e.IdUserTeacher == body.IdUserTeacher
                       && e.IdInvitationBookingSetting == DataInvitationBooking.IdInvitationBookingSetting
                       && e.DateInvitation.Date == Convert.ToDateTime(body.StartDateTimeInvitation).Date
                       && e.StartTime == Convert.ToDateTime(body.StartDateTimeInvitation).TimeOfDay
                       && (e.IsPriority == true || e.IsPriority == null))
                   .FirstOrDefaultAsync(CancellationToken);

                    if (dataInvitationBookingSettingNew.IsPriority == null)
                    {
                        dataInvitationBookingSettingNew.IsPriority = true;
                        dataInvitationBookingSettingNew.IsFlexibleBreak = false;
                        dataInvitationBookingSettingNew.IsDisabledPriority = false;
                        dataInvitationBookingSettingNew.IsDisabledFlexible = true;
                        dataInvitationBookingSettingNew.IsDisabledAvailable = true;
                        dataInvitationBookingSettingNew.IdUserSetPriority = dataInvitationBookingSetting.IdUserSetPriority == null ? DataInvitationBooking.UserIn : dataInvitationBookingSetting.IdUserSetPriority;
                        _dbContext.Entity<TrInvitationBookingSettingSchedule>().UpdateRange(dataInvitationBookingSetting);
                    }

                    Email.Add(new EmailRescheduleResult
                    {
                        IdInvitationBookingSetting = DataInvitationBooking.InvitationBookingSetting.Id,
                        AcademicYear = DataInvitationBooking.InvitationBookingSetting.AcademicYears.Description,
                        InvitationName = DataInvitationBooking.InvitationBookingSetting.InvitationName,
                        InvitationDate = Convert.ToDateTime(DataInvitationBooking.InvitationBookingSetting.InvitationStartDate).ToString("dd MMM yyyy HH:mm") + " - " + Convert.ToDateTime(DataInvitationBooking.InvitationBookingSetting.InvitationEndDate).ToString("dd MMM yyyy HH:mm"),
                        StudentName = ConvertString(DataInvitationBooking.InvitationBookingDetails.Select(x => x.HomeroomStudent.Student.FirstName
                                        + (x.HomeroomStudent.Student.MiddleName == null ? "" : " " + x.HomeroomStudent.Student.MiddleName)
                                        + (x.HomeroomStudent.Student.LastName == null ? "" : " " + x.HomeroomStudent.Student.LastName)).ToList()),
                        BinusianId = DataInvitationBooking.InvitationBookingDetails.Count() > 1 ? "-" : DataInvitationBooking.InvitationBookingDetails.Select(e => e.HomeroomStudent.IdStudent).FirstOrDefault(),
                        Venue = DataInvitationBooking.Venue.Description,
                        Date = DataInvitationBooking.StartDateInvitation.ToString("dd MMM yyyy"),
                        Time = DataInvitationBooking.StartDateInvitation.ToString("HH:mm"),
                        Type = "New",
                        IdUserTeacher = DataInvitationBooking.IdUserTeacher,
                        TeacherName = DataInvitationBooking.UserTeacher.DisplayName,
                        BookByParent = NamaParent,
                        IdUserParent = DataInvitationBooking.InvitationBookingDetails.Select(e => "P" + e.HomeroomStudent.IdStudent).FirstOrDefault(),
                        IdSchool = DataInvitationBooking.InvitationBookingSetting.AcademicYears.IdSchool,
                        IdInvitationBooking = DataInvitationBooking.Id
                    });

                }
                else
                {
                    if (DataInvitationBooking.EndDateInvitation < _dateTime.ServerTime)
                        throw new BadRequestException("You can't cancel invitation booking. The invitation date has been passed");

                    DataInvitationBooking.IsActive = false;
                    _dbContext.Entity<TrInvitationBooking>().Update(DataInvitationBooking);

                    var InvitationBookingDetail = await _dbContext.Entity<TrInvitationBookingDetail>()
                                                    .Where(e => e.IdInvitationBooking == DataInvitationBooking.Id)
                                                    .ToListAsync(CancellationToken);

                    InvitationBookingDetail.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrInvitationBookingDetail>().UpdateRange(InvitationBookingDetail);
                }

                //update invitationBookingSetting
                if (GetInvitationBooking.Count() - 1 == 0)
                {
                    dataInvitationBookingSetting.IdUserSetPriority = dataInvitationBookingSetting.IdUserSetPriority == DataInvitationBooking.IdUserTeacher
                                            ? DataInvitationBooking.IdUserTeacher
                                            : null;

                    dataInvitationBookingSetting.IsPriority = null;
                    dataInvitationBookingSetting.IsFlexibleBreak = false;
                    dataInvitationBookingSetting.IsDisabledPriority = false;
                    dataInvitationBookingSetting.IsDisabledFlexible = false;
                    dataInvitationBookingSetting.IsDisabledAvailable = false;
                    _dbContext.Entity<TrInvitationBookingSettingSchedule>().UpdateRange(dataInvitationBookingSetting);
                }

                var exsisInvitationBookingSetting = await _dbContext.Entity<TrInvitationBookingSetting>()
                                                            .Where(e => e.Id == DataInvitationBooking.IdInvitationBookingSetting)
                                                            .AnyAsync(CancellationToken);

                if (!exsisInvitationBookingSetting)
                    throw new BadRequestException("The request cannot be processed. The invitation has been deleted by staff");

                await _dbContext.SaveChangesAsync(CancellationToken);

                #region Notification
                if (!body.IsCancel)
                {
                    if (RoleConstant.Parent == body.Role)
                    {

                        foreach (var IdUserTeacher in Email.Where(e => e.Type == "Old").Select(e => e.IdUserTeacher).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var Notification = APP20Notification(KeyValues, AuthInfo, IdUserTeacher);
                        }

                        foreach (var IdUserParent in Email.Where(e => e.Type == "Old").Select(e => e.IdUserParent).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var Notification = APP28Notification(KeyValues, AuthInfo, IdUserParent);
                        }

                        //foreach (var IdUserTeacher in Email.Where(e => e.Type == "New").Select(e => e.IdUserTeacher).Distinct().ToList())
                        //{
                        //    if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                        //    {
                        //        KeyValues.Remove("RescheduleInvitationBookingEmail");
                        //    }
                        //    KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                        //    var Notification = APP26Notification(KeyValues, AuthInfo, IdUserTeacher);
                        //}
                    }
                    else if (RoleConstant.Staff == body.Role)
                    {

                        foreach (var IdUserTeacher in Email.Select(e => e.IdUserTeacher).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var Notification = APP8Notification(KeyValues, AuthInfo, IdUserTeacher);
                        }

                        foreach (var IdUserParent in Email.Select(e => e.IdUserParent).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var Notification = APP10Notification(KeyValues, AuthInfo, IdUserParent);
                        }
                    }
                }
                else
                {
                    if (RoleConstant.Parent == body.Role)
                    {
                        foreach (var IdUserTeacher in Email.Select(e => e.IdUserTeacher).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var Notification = APP21Notification(KeyValues, AuthInfo, IdUserTeacher);
                        }

                        foreach (var IdUserParent in Email.Select(e => e.IdUserParent).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var NotificationCancelParent = APP29Notification(KeyValues, AuthInfo, IdUserParent);
                        }

                    }
                    else if (RoleConstant.Staff == body.Role)
                    {


                        foreach (var IdUserTeacher in Email.Select(e => e.IdUserTeacher).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var Notification = APP11Notification(KeyValues, AuthInfo, IdUserTeacher);
                        }

                        foreach (var IdUserParent in Email.Select(e => e.IdUserParent).Distinct().ToList())
                        {
                            if (KeyValues.ContainsKey("RescheduleInvitationBookingEmail"))
                            {
                                KeyValues.Remove("RescheduleInvitationBookingEmail");
                            }
                            KeyValues.Add("RescheduleInvitationBookingEmail", Email);

                            var Notification = APP12Notification(KeyValues, AuthInfo, IdUserParent);
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw new BadRequestException("The request cannot be processed. The invitation has been deleted by staff");
                else
                    throw new BadRequestException(ex.Message);
            }

            return Request.CreateApiResult2();
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

        public static string APP20Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserTeacher)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP20")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserTeacher,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP26Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserTeacher)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP26")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserTeacher,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP21Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserTeacher)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP21")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserTeacher,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP8Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserTeacher)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP8")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserTeacher,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP10Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserParent)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP10")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserParent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP11Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserTeacher)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP11")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserTeacher,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP12Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserParent)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP12")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserParent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP29Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserParent)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP29")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserParent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public static string APP28Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdUserParent)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var EmailInvitation = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));

                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "APP28")
                {
                    IdRecipients = new List<string>()
                    {
                        IdUserParent,
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }


}
