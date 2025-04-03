using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using FluentEmail.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class DeleteSchoolEventHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteSchoolEventHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var ids = (await GetIdsFromBody()).Distinct();

            var GetEvent = await _dbContext.Entity<TrEvent>()
               .Include(x => x.EventIntendedFor).ThenInclude(e=>e.EventIntendedForAttendanceStudents)
               .Include(x => x.EventApprovers)
               .Include(x => x.EventAttachments)
               .Include(x => x.EventAwardApprovers)
               .Include(x => x.EventBudgets)
               .Include(x => x.EventDetails)
               .Include(x => x.EventCoordinators)
               .Include(x => x.EventApprovals)
               .Include(x => x.EventSchedules)
               .Where(x => ids.Contains(x.Id))
               .ToListAsync(CancellationToken);


            if (!GetEvent.Any())
            {
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Event"], "Id", ids));
            }

            GetEvent.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrEvent>().UpdateRange(GetEvent);
            List<string> listIdSchedule = new List<string>();
            foreach (var itemEvent in GetEvent)
            {
                itemEvent.EventApprovers.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventApprover>().UpdateRange(itemEvent.EventApprovers);

                itemEvent.EventCoordinators.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventCoordinator>().UpdateRange(itemEvent.EventCoordinators);

                itemEvent.EventApprovers.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventApprover>().UpdateRange(itemEvent.EventApprovers);

                itemEvent.EventAttachments.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventAttachment>().UpdateRange(itemEvent.EventAttachments);

                itemEvent.EventAwardApprovers.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventAwardApprover>().UpdateRange(itemEvent.EventAwardApprovers);

                itemEvent.EventBudgets.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventBudget>().UpdateRange(itemEvent.EventBudgets);

                itemEvent.EventDetails.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventDetail>().UpdateRange(itemEvent.EventDetails);

                itemEvent.EventApprovals.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<HTrEventApproval>().UpdateRange(itemEvent.EventApprovals);

                itemEvent.EventSchedules.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventSchedule>().UpdateRange(itemEvent.EventSchedules);
            }

            //================================================================================
            var GetEventActivity = await _dbContext.Entity<TrEventActivity>()
                .Include(e => e.EventActivityRegistrants)
                .Include(e => e.EventActivityPICs)
                .Include(e => e.EventActivityAwards)
               .Where(x => ids.Contains(x.IdEvent))
               .ToListAsync(CancellationToken);

            foreach (var itemActivity in GetEventActivity)
            {
                itemActivity.IsActive = false;
                itemActivity.EventActivityRegistrants.ToList().ForEach(e => e.IsActive = false);
                itemActivity.EventActivityPICs.ToList().ForEach(e => e.IsActive = false);
                itemActivity.EventActivityAwards.ToList().ForEach(e => e.IsActive = false);
                itemActivity.IsActive = false;
                _dbContext.Entity<TrEventActivity>().Update(itemActivity);

                if (itemActivity.EventActivityRegistrants.Any())
                {
                    itemActivity.EventActivityRegistrants.ToList().ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityRegistrant>().UpdateRange(itemActivity.EventActivityRegistrants);
                }

                if (itemActivity.EventActivityAwards.Any())
                {
                    itemActivity.EventActivityAwards.ToList().ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityAward>().UpdateRange(itemActivity.EventActivityAwards);
                }

                if (itemActivity.EventActivityPICs.Any())
                {
                    itemActivity.EventActivityPICs.ToList().ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityPIC>().UpdateRange(itemActivity.EventActivityPICs);
                }
            }

            var GetEventIntendedFor = await _dbContext.Entity<TrEventIntendedFor>()
               .Include(x => x.EventIntendedForDepartments)
               .Include(x => x.EventIntendedForGradeStudents)
               .Include(x => x.EventIntendedForLevelStudents)
               .Include(x => x.EventIntendedForPersonals)
               .Include(x => x.EventIntendedForPersonalParents)
               .Include(x => x.EventIntendedForPersonalStudents)
               .Include(x => x.EventIntendedForPositions)
               .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
               .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdPICStudents)
               .Include(x => x.EventIntendedForGradeParents)
            .Where(x => ids.Contains(x.IdEvent))
            .ToListAsync(CancellationToken);

            foreach (var ItemEventIntendedFor in GetEventIntendedFor)
            {
                ItemEventIntendedFor.IsActive = false;
                _dbContext.Entity<TrEventIntendedFor>().Update(ItemEventIntendedFor);

                ItemEventIntendedFor.EventIntendedForDepartments.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForDepartment>().UpdateRange(ItemEventIntendedFor.EventIntendedForDepartments);

                ItemEventIntendedFor.EventIntendedForGradeStudents.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForGradeStudent>().UpdateRange(ItemEventIntendedFor.EventIntendedForGradeStudents);

                ItemEventIntendedFor.EventIntendedForLevelStudents.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForLevelStudent>().UpdateRange(ItemEventIntendedFor.EventIntendedForLevelStudents);

                ItemEventIntendedFor.EventIntendedForPersonals.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForPersonal>().UpdateRange(ItemEventIntendedFor.EventIntendedForPersonals);

                ItemEventIntendedFor.EventIntendedForPersonalParents.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForPersonalParent>().UpdateRange(ItemEventIntendedFor.EventIntendedForPersonalParents);

                ItemEventIntendedFor.EventIntendedForPersonalStudents.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForPersonalStudent>().UpdateRange(ItemEventIntendedFor.EventIntendedForPersonalStudents);

                ItemEventIntendedFor.EventIntendedForPositions.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForPosition>().UpdateRange(ItemEventIntendedFor.EventIntendedForPositions);

                foreach (var ItemAttendanceStudents in ItemEventIntendedFor.EventIntendedForAttendanceStudents)
                {
                    ItemAttendanceStudents.IsActive = false;
                    ItemAttendanceStudents.EventIntendedForAtdCheckStudents.ToList().Select(e => e.IsActive = false);
                    ItemAttendanceStudents.EventIntendedForAtdPICStudents.ToList().Select(e => e.IsActive = false);

                    _dbContext.Entity<TrEventIntendedForAttendanceStudent>().Update(ItemAttendanceStudents);

                    ItemAttendanceStudents.EventIntendedForAtdCheckStudents.ToList().ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventIntendedForAtdCheckStudent>().UpdateRange(ItemAttendanceStudents.EventIntendedForAtdCheckStudents);

                    ItemAttendanceStudents.EventIntendedForAtdPICStudents.ToList().ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventIntendedForAtdPICStudent>().UpdateRange(ItemAttendanceStudents.EventIntendedForAtdPICStudents);
                }

                ItemEventIntendedFor.EventIntendedForGradeParents.ToList().ForEach(e => e.IsActive = false);
                _dbContext.Entity<TrEventIntendedForGradeParent>().UpdateRange(ItemEventIntendedFor.EventIntendedForGradeParents);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            var lastStatusEvent = await _dbContext.Entity<TrEvent>()
                    .Where(x => ids.Contains(x.Id))
                    .IgnoreQueryFilters() 
                    .FirstOrDefaultAsync(CancellationToken);

            if(lastStatusEvent != null)
            {
                if(lastStatusEvent.StatusEvent == "Approved" && lastStatusEvent.StatusEventAward == "Approved")
                {
                    var JobsEventUpdateGenerate = new CodeWithIdVm
                    {
                        Id = ids.First(),
                        Code = "Delete"
                    };

                    if (KeyValues.ContainsKey("JobsEventUpdateGenerate"))
                    {
                        KeyValues.Remove("JobsEventUpdateGenerate");
                    }
                    KeyValues.Add("JobsEventUpdateGenerate", JobsEventUpdateGenerate);
                    var Notification = QueueEventUpdateGenerate(KeyValues, AuthInfo);
                }
            }

            return Request.CreateApiResult2();
        }

        public static string QueueEventUpdateGenerate(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "JobsEventUpdateGenerate").Value;
            var JobsEventUpdateGenerate = JsonConvert.DeserializeObject<CodeWithIdVm>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "UGBE")
                {
                    KeyValues = KeyValues,
                });
                collector.Add(message);
            }
            
            return "";
        }

    }
}
