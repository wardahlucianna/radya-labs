using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Threading.Tasks;
using BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Validator;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FluentEmail.Core;
using BinusSchool.Auth.Authentications.Jwt;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.Extensions.DependencyInjection;
using NPOI.XWPF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class UpdateInvitationBookingSettingVanueOnlyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public UpdateInvitationBookingSettingVanueOnlyHandler(ISchedulingDbContext SchedulingDbContext)
        {
            _dbContext = SchedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateInvitationBookingSettingVanueOnlyRequest, UpdateInvitationBookingSettingVanueOnlyValidator>();

            var getInvitationBooking = await _dbContext.Entity<TrInvitationBookingSetting>()
                        .Include(e => e.InvitationBookingSettingVenueDates).ThenInclude(e => e.InvitationBookingSettingVenueDtl).ThenInclude(e => e.User)
                        .Include(e => e.InvitationBookingSettingVenueDates).ThenInclude(e => e.InvitationBookingSettingVenueDtl).ThenInclude(e => e.Venue)
                        .Include(e => e.AcademicYears)
                        .Where(e => e.Id == body.IdInvitationBookingSetting)
                        .FirstOrDefaultAsync(CancellationToken);

            var listInvitationSchedule = await _dbContext.Entity<TrInvitationBookingSettingSchedule>()
                                            .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting)
                                            .ToListAsync(CancellationToken);

            var listInvitationBooking = await _dbContext.Entity<TrInvitationBooking>()
                                            .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting)
                                            .ToListAsync(CancellationToken);

            var listVanue = await _dbContext.Entity<MsVenue>()
                                .Where(e => e.Building.IdSchool == getInvitationBooking.AcademicYears.IdSchool)
                                .ToListAsync(CancellationToken);

            ENS1Result Ens1Result = new ENS1Result();
            Ens1Result.IdInvitationBookingSetting = body.IdInvitationBookingSetting;
            Ens1Result.InvitationName = getInvitationBooking.InvitationName;
            Ens1Result.InvitationBookingSettingOld = new List<ENS1InvitationBooking>();
            Ens1Result.InvitationBookingSettingNew = new List<ENS1InvitationBooking>();
            Ens1Result.IdRecepient = new List<string>();

            #region Update Invitation Booking Setting Venue
            foreach (var itemVenueDates in getInvitationBooking.InvitationBookingSettingVenueDates)
            {
                foreach (var itemVanueDetail in itemVenueDates.InvitationBookingSettingVenueDtl)
                {
                    var getVenueDtlByBody = body.UserVenueMapping
                                                .Where(e => e.IdInvitationBookingSettingVenueDtl == itemVanueDetail.Id)
                                                .FirstOrDefault();

                    if (getVenueDtlByBody == null)
                        continue;

                    if (getVenueDtlByBody.IdVenue != itemVanueDetail.IdVenue)
                    {
                        itemVanueDetail.IdVenue = getVenueDtlByBody.IdVenue;
                        _dbContext.Entity<TrInvitationBookingSettingVenueDtl>().Update(itemVanueDetail);

                        #region update schedule
                        List<DateTime> DayValue = new List<DateTime>();
                        var SpitDateVanue = itemVenueDates.DateInvitation.Split("||");

                        if (SpitDateVanue.Count() > 0)
                        {
                            foreach (var ItemDate in SpitDateVanue)
                            {
                                DayValue.Add(Convert.ToDateTime(ItemDate));
                            }
                        }

                        var listInvitationScheduleByTeacherDay = listInvitationSchedule
                                                                    .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting
                                                                            && e.IdUserTeacher == itemVanueDetail.IdUserTeacher
                                                                            && DayValue.Contains(e.DateInvitation.Date))
                                                                    .ToList();

                        listInvitationScheduleByTeacherDay.ForEach(e => e.IdVenue = getVenueDtlByBody.IdVenue);
                        _dbContext.Entity<TrInvitationBookingSettingSchedule>().UpdateRange(listInvitationScheduleByTeacherDay);

                        var listInvitationBookingByTeacherDay = listInvitationBooking
                                                        .Where(e => e.IdInvitationBookingSetting == body.IdInvitationBookingSetting
                                                                        && e.IdUserTeacher == itemVanueDetail.IdUserTeacher
                                                                        && DayValue.Contains(e.StartDateInvitation.Date))
                                                                    .ToList();

                        listInvitationBookingByTeacherDay.ForEach(e => e.IdVenue = getVenueDtlByBody.IdVenue);
                        _dbContext.Entity<TrInvitationBooking>().UpdateRange(listInvitationBookingByTeacherDay);
                        #endregion

                        #region ENS1 Email

                        var invitationDate = String.Join(", ", DayValue.Select(e=>e.ToString("dd MMM yyyy")).ToList());
                        var parentBookingPeriod = $"{getInvitationBooking.ParentBookingStartDate.ToString("dd MMM yyyy")} - {getInvitationBooking.ParentBookingEndDate.ToString("dd MMM yyyy")}";
                        var StaffBookingPeriod = getInvitationBooking.StaffBookingStartDate == null
                                                    ? "-"
                                                    : $"{Convert.ToDateTime(getInvitationBooking.StaffBookingStartDate).ToString("dd MMM yyyy")} - {Convert.ToDateTime(getInvitationBooking.ParentBookingEndDate).ToString("dd MMM yyyy")}";

                        var VanueNameNew = listVanue.Where(e => e.Id == getVenueDtlByBody.IdVenue).Select(e => e.Description).FirstOrDefault();
                        var NewTeacherEns1ResultNew = new ENS1InvitationBooking
                        {
                            AcademicYear = getInvitationBooking.AcademicYears.Description,
                            InvitationName = getInvitationBooking.InvitationName,
                            InvitationDate = invitationDate,
                            ParentBookingPeriod = parentBookingPeriod,
                            StaffBookingPeriod = StaffBookingPeriod,
                            TeacherName = itemVanueDetail.User.DisplayName,
                            Venue = VanueNameNew,
                        };

                        var NewTeacherEns1ResultOld = new ENS1InvitationBooking
                        {
                            AcademicYear = getInvitationBooking.AcademicYears.Description,
                            InvitationName = getInvitationBooking.InvitationName,
                            InvitationDate = invitationDate,
                            ParentBookingPeriod = parentBookingPeriod,
                            StaffBookingPeriod = StaffBookingPeriod,
                            TeacherName = itemVanueDetail.User.DisplayName,
                            Venue = itemVanueDetail.Venue.Description,
                        };

                        Ens1Result.InvitationBookingSettingOld.Add(NewTeacherEns1ResultOld);
                        Ens1Result.InvitationBookingSettingNew.Add(NewTeacherEns1ResultNew);

                        if(!Ens1Result.IdRecepient.Contains(getVenueDtlByBody.IdUserTeacher))
                            Ens1Result.IdRecepient.Add(getVenueDtlByBody.IdUserTeacher);
                        #endregion
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            #endregion

            var Notification = ENS1Notification(KeyValues, AuthInfo, Ens1Result);
            return Request.CreateApiResult2();
        }

        public string ENS1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, ENS1Result Ens1Result)
        {
            if (KeyValues.ContainsKey("GetInvitationBookingSettingVenue"))
            {
                KeyValues.Remove("GetInvitationBookingSettingVenue");
            }
            KeyValues.Add("GetInvitationBookingSettingVenue", Ens1Result);

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "ENS1")
                {
                    IdRecipients = Ens1Result.IdRecepient,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
