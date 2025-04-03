using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Finance.FnPayment;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.SendEmail;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.SchedulingDb;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Notification
{
    public class JobDeleteStudentExtracurricularNotification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private readonly ISchedulingDbContext _dbContext;
        private readonly ILogger<JobDeleteStudentExtracurricularNotification> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IMasterParticipant _extracurricularMasterParticipantApi;
        private readonly IExtracurricularInvoice _extracurricularInvoiceApi;
        private readonly IOnlineRegistration _scpOnlineRegistrationApi;
        private readonly Data.Api.Scheduling.FnExtracurricular.IEmailNotification _extracurricularEmailNotification;

        public JobDeleteStudentExtracurricularNotification(
            INotificationManager notificationManager,
            DbContextOptions<SchedulingDbContext> options,
            ILogger<JobDeleteStudentExtracurricularNotification> logger,
            IConfiguration configuration,
            IDictionary<string, object> notificationData,
            IMasterParticipant extracurricularMasterParticipantApi,
            IExtracurricularInvoice extracurricularInvoiceApi,
            IOnlineRegistration scpOnlineRegistrationApi,
            Data.Api.Scheduling.FnExtracurricular.IEmailNotification extracurricularEmailNotification,
            IMachineDateTime dateTime) :
            base("", notificationManager, configuration, logger, skipGetTemplate: true)
        {

            _dbContext = new SchedulingDbContext(options);
            _logger = logger;
            _notificationData = notificationData;
            _dateTime = dateTime;
            _extracurricularInvoiceApi = extracurricularInvoiceApi;
            _scpOnlineRegistrationApi = scpOnlineRegistrationApi;
            _extracurricularMasterParticipantApi = extracurricularMasterParticipantApi;
            _extracurricularEmailNotification = extracurricularEmailNotification;
        }

        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = false,
                    Web = false
                }
            };

            return Task.CompletedTask;
        }

        protected override async Task Prepare()
        {
            try
            {
                #region Init for avoiding error (copy this)
                IdUserRecipients = new List<string> { "" };
                GeneratedTitle = "";
                GeneratedContent = "";
                NotificationTemplate = new NotificationTemplate();
                _notificationData = new Dictionary<string, object>();
                #endregion

                var school = await _dbContext.Entity<MsSchool>()
                                    .Where(x => x.Id == IdSchool)
                                    .FirstOrDefaultAsync(CancellationToken);

                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                // check all extracurricular rule
                var extracurricularRuleList = await _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                                                    .Include(ergm => ergm.ExtracurricularRule)
                                                    .ThenInclude(er => er.AcademicYear)
                                                    .ThenInclude(ay => ay.School)
                                                    .Where(x => 
                                                    // get extracurricular rule between 10 hours before now until DateTime.Now
                                                    (x.ExtracurricularRule.ReviewDate.HasValue ? (x.ExtracurricularRule.ReviewDate >= _dateTime.ServerTime.AddHours(-10) && x.ExtracurricularRule.ReviewDate <= _dateTime.ServerTime) : false) &&
                                                    x.ExtracurricularRule.AcademicYear.School.Id == IdSchool &&
                                                                x.ExtracurricularRule.Status == true
                                                            )
                                                    .ToListAsync(CancellationToken);

                if (!extracurricularRuleList.Any())
                    return;

                var extracurricularList = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                                                    .Include(egm => egm.Extracurricular)
                                                    .Where(x => extracurricularRuleList.Select(y => y.IdGrade).Any(y => y == x.IdGrade) 
                                                                //x.Extracurricular.Price > 0
                                                                )
                                                    .Distinct()
                                                    .ToListAsync(CancellationToken);

                var idExtracurricularList = extracurricularList.Select(x => x.IdExtracurricular).Distinct().ToList();

                // get max extracurricular participant
                var allCapacityExtracurricularParticipantList = extracurricularList
                                                            .Select(x => new
                                                            {
                                                                IdExtracurricular = x.Extracurricular.Id,
                                                                MaxParticipant = x.Extracurricular.MaxParticipant,
                                                                MinParticipant = x.Extracurricular.MinParticipant
                                                            })
                                                            .Distinct()
                                                            .ToList();

                // get extracurricular participant count
                var allExtracurricularParticipantCountList = _dbContext.Entity<MsExtracurricularParticipant>()
                                                                .Where(x => idExtracurricularList.Any(y => y == x.IdExtracurricular))
                                                                .ToList()
                                                                .GroupBy(x => x.IdExtracurricular)
                                                                .Select(x => new
                                                                {
                                                                    IdExtracurricular = x.Key,
                                                                    TotalParticipant = x.Select(x => x.IdStudent).Distinct().Count()
                                                                })
                                                                .Distinct()
                                                                .ToList();

                
                var validDeleteIdExtracurricular = new List<string>();
                var sendEmailParamList = new List<SendEmailCancelExtracurricularToParentRequest>();

                foreach (var IdExtracurricular in idExtracurricularList)
                {
                    int? minExtracurricularParticipant = allCapacityExtracurricularParticipantList
                                                        .Where(x => x.IdExtracurricular == IdExtracurricular)
                                                        .Select(x => x.MinParticipant)
                                                        .FirstOrDefault();

                    // get total participants
                    int? extracurricularParticipantCount = allExtracurricularParticipantCountList
                                                            .Where(x => x.IdExtracurricular == IdExtracurricular)
                                                            .Select(x => x.TotalParticipant)
                                                            .FirstOrDefault();

                    if (minExtracurricularParticipant == null && extracurricularParticipantCount == null)
                        throw new BadRequestException(null);

                    // only delete if min participant of the excul does not meet the real condition of the extracurricular on the review date
                    if (extracurricularParticipantCount < minExtracurricularParticipant)
                    {
                        validDeleteIdExtracurricular.Add(IdExtracurricular);
                    }
                }

                var deleteAllStudentParticipantApi = await _extracurricularMasterParticipantApi.DeleteAllStudentParticipant(new DeleteAllStudentParticipantRequest
                {
                    IdExtracurricularList = validDeleteIdExtracurricular
                });

                if(deleteAllStudentParticipantApi.Payload == null || deleteAllStudentParticipantApi.Payload.Count() == 0)
                    throw new BadRequestException(null);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public void Dispose()
        {
            (_dbContext as SchedulingDbContext)?.Dispose();
        }

        protected override Task SendPushNotification()
        {
            throw new NotImplementedException();
        }

        protected override async Task SendEmailNotification()
        {
            Task completedTask = Task.CompletedTask;
            await Task.WhenAll(completedTask);
        }
    }
}
