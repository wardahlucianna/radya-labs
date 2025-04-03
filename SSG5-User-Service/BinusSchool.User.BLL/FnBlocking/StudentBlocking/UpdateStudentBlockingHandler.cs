using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.User.FnBlocking.StudentBlocking.Validator;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.UserDb.Entities.Student;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Persistence.UserDb.Entities.School;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class UpdateStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private IDictionary<string, object> _notificationData;

        private IDbContextTransaction _transaction;

        private readonly IUserDbContext _dbContext;

        private readonly IMachineDateTime _dateTime;

        public UpdateStudentBlockingHandler(IUserDbContext userDbContext, IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateStudentBlockingRequest, UpdateStudentBlockingValidator>();

            var data = await _dbContext.Entity<MsStudentBlocking>()
                .Where(x => x.IdStudent == body.IdUser
                && x.IdBlockingCategory == body.IdBlockingCategory
                && x.IdBlockingType == body.IdBlockingType)
                .ToListAsync(CancellationToken); 

            if (data.Count > 0)
            {
                //data.IsBlocked = body.IsBlock;
                foreach (var item in data)
                {
                    item.IsBlocked = body.IsBlock;
                }
            }
            else
            {
                var studentBlocking = new MsStudentBlocking()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdStudent = body.IdUser,
                    IdBlockingCategory = body.IdBlockingCategory,
                    IdBlockingType = body.IdBlockingType,
                    IsBlocked = body.IsBlock
                };

                _dbContext.Entity<MsStudentBlocking>().Add(studentBlocking);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await UpdateHistoryBlocking(body);

            await _transaction.CommitAsync(CancellationToken);

            if (body.IsBlock)
            {
                await ProcessEmailAndNotif(body);
            }
            return Request.CreateApiResult2();
        }

        protected async Task<object> UpdateHistoryBlocking(UpdateStudentBlockingRequest body)
        {
            var history = await _dbContext.Entity<HMsStudentBlocking>()
                            .Where(x => x.IdStudent == body.IdUser
                            && x.IdBlockingCategory == body.IdBlockingCategory
                            && x.IdBlockingType == body.IdBlockingType
                            && x.EndDate == null)
                            .ToListAsync(CancellationToken);

            if (body.IsBlock)
            {
                if (history.Count == 0)
                {
                    var historyStudentBlocking = new HMsStudentBlocking()
                    {
                        IdHMsStudentBlocking = Guid.NewGuid().ToString(),
                        IdStudent = body.IdUser,
                        IdBlockingCategory = body.IdBlockingCategory,
                        IdBlockingType = body.IdBlockingType,
                        StartDate = _dateTime.ServerTime
                    };

                    _dbContext.Entity<HMsStudentBlocking>().Add(historyStudentBlocking);
                }
            }
            else
            {
                if (history.Count > 0)
                {
                    foreach(var item in history)
                    {
                        item.EndDate = _dateTime.ServerTime;
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return history;
        }

        private async Task ProcessEmailAndNotif(UpdateStudentBlockingRequest param)
        {
            var GetParent = await _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Parent)
                            .Where(x => x.IdStudent == param.IdUser)
                            .ToListAsync(CancellationToken);

            var GetStudentBlocking = await (from StudentBlocking in _dbContext.Entity<MsStudentBlocking>()
                                            join Student in _dbContext.Entity<MsStudent>() on StudentBlocking.IdStudent equals Student.Id
                                            join School in _dbContext.Entity<MsSchool>() on Student.IdSchool equals School.Id
                                            join BlockingType in _dbContext.Entity<MsBlockingType>() on StudentBlocking.IdBlockingType equals BlockingType.Id
                                            join BlockingCategory in _dbContext.Entity<MsBlockingCategory>() on StudentBlocking.IdBlockingCategory equals BlockingCategory.Id
                                            join Feature in _dbContext.Entity<MsFeature>() on BlockingType.IdFeature equals Feature.Id into JoinedFeature
                                            from Feature in JoinedFeature.DefaultIfEmpty()
                                            join BlockingTypeSubFeature in _dbContext.Entity<MsBlockingTypeSubFeature>() on BlockingType.Id equals BlockingTypeSubFeature.IdBlockingType into JoinedTypeSubFeature
                                            from BlockingTypeSubFeature in JoinedTypeSubFeature.DefaultIfEmpty()
                                            join SubFeature in _dbContext.Entity<MsFeature>() on BlockingTypeSubFeature.IdSubFeature equals SubFeature.Id into JoinedSubFeature
                                            from SubFeature in JoinedSubFeature.DefaultIfEmpty()
                                            where StudentBlocking.IsBlocked == true && StudentBlocking.IdStudent == param.IdUser && StudentBlocking.IdBlockingCategory == param.IdBlockingCategory
                                            && StudentBlocking.IdBlockingType == param.IdBlockingType
                                            select new GetBlockingNotifResult
                                            {
                                                StudentBlocking = StudentBlocking.Id,
                                                IdStudent = Student.Id,
                                                BlockingTypeCategory = BlockingType.Category,
                                                BlockingCategoryName = BlockingCategory.Name,
                                                IdFeature = BlockingType.IdFeature,
                                                FeatureName = Feature.Description,
                                                IdSubFeature = BlockingTypeSubFeature.IdSubFeature,
                                                SubFeatureName = SubFeature.Description,
                                                StudentName = $"{Student.FirstName} {Student.LastName}",
                                                //DataParents = GetParent.Where(x => x.IdStudent == Student.Id).Select(x => new DataParent
                                                //{
                                                //    EmailParent = x.Parent.PersonalEmailAddress,
                                                //    ParentName = !string.IsNullOrEmpty(x.Parent.FirstName) ? x.Parent.FirstName : x.Parent.LastName,
                                                //}).ToList(),
                                                SchoolName = School.Name,
                                                Action = !string.IsNullOrEmpty(SubFeature.IdParent) ? SubFeature.Action : Feature.Action,
                                                Controller = !string.IsNullOrEmpty(SubFeature.IdParent) ? SubFeature.Controller : Feature.Controller
                                            })
            .ToListAsync(CancellationToken);

            foreach (var data in GetStudentBlocking)
            {
                data.DataParents = GetParent.Where(x => x.IdStudent == data.IdStudent).Select(x => new DataParent
                {
                    EmailParent = x.Parent.PersonalEmailAddress,
                    ParentName = !string.IsNullOrEmpty(x.Parent.FirstName) ? x.Parent.FirstName : x.Parent.LastName,
                }).ToList();
            }

            IDictionary<string, object> paramTemplateNotification = new Dictionary<string, object>();

            if (KeyValues.ContainsKey("studentBlockingData"))
            {
                KeyValues.Remove("studentBlockingData");
            }

            paramTemplateNotification.Add("studentBlockingData", GetStudentBlocking);

            IEnumerable<string> IdRecipients = new string[] { param.IdUser };

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "BS1")
                {
                    IdRecipients = IdRecipients,
                    KeyValues = paramTemplateNotification
                });
                collector.Add(message);
            }
        }
    }
}
