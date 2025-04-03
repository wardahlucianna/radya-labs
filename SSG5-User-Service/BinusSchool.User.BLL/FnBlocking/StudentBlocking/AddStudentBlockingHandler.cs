using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using BinusSchool.User.FnBlocking.StudentBlocking.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class AddStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        private readonly IMachineDateTime _dateTime;

        public AddStudentBlockingHandler(IUserDbContext userDbContext, IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;

            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddStudentBlockingRequest, AddStudentBlockingValidator>();

            var updateStudentBlocking = new List<MsStudentBlocking>();
            var newStudentBlocking = new List<MsStudentBlocking>();
            var listUserBLocking = new List<string>();
            var listUserBLockingCategory = new List<string>();
            var listUserBLockingType = new List<string>();
            foreach (var item in body.StudentBlocking.Select(e=> new { e.IdCategory, e.IdType }).Distinct().ToList())
            {

                var BodyIdStudentByTypeCategory = body.StudentBlocking
                                        .Where(e=>e.IdType==item.IdType && e.IdCategory==item.IdCategory && e.IsBlock)
                                        .ToList();

                //update
                var UpdateStudentBlock = await (_dbContext.Entity<MsStudentBlocking>()
                                                .Where(e => e.IdBlockingCategory == item.IdCategory
                                                    && e.IdBlockingType == item.IdType
                                                    && BodyIdStudentByTypeCategory.Select(e=>e.IdStudent).ToList().Contains(e.IdStudent))
                                                ).ToListAsync(CancellationToken);


                UpdateStudentBlock.ForEach(x => 
                                x.IsBlocked = BodyIdStudentByTypeCategory.Where(e => e.IdStudent == x.IdStudent).Any() 
                                            ? BodyIdStudentByTypeCategory.Where(e => e.IdStudent == x.IdStudent).SingleOrDefault().IsBlock
                                            : x.IsBlocked) ;

                updateStudentBlocking.AddRange(UpdateStudentBlock);

                _dbContext.Entity<MsStudentBlocking>().UpdateRange(UpdateStudentBlock);
                
                //insert 
                newStudentBlocking = BodyIdStudentByTypeCategory
                    .Where(e=> !UpdateStudentBlock.Select(e=>e.IdStudent).ToList().Contains(e.IdStudent) && e.IsBlock)
                    .Select(e => new MsStudentBlocking
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdStudent = e.IdStudent,
                        IdBlockingCategory = e.IdCategory,
                        IdBlockingType = e.IdType,
                        IsBlocked= e.IsBlock
                    })
                    .ToList();

                _dbContext.Entity<MsStudentBlocking>().AddRange(newStudentBlocking);
                updateStudentBlocking.AddRange(newStudentBlocking);
                await UpdateHistoryBlocking(updateStudentBlocking);
                await _dbContext.SaveChangesAsync(CancellationToken);
                foreach (var dataBlock in updateStudentBlocking)
                {
                    await ProcessEmailAndNotif(dataBlock.IdStudent,item.IdCategory,item.IdType);
                }

            }

            return Request.CreateApiResult2();
        }

        protected async Task<object> UpdateHistoryBlocking(List<MsStudentBlocking> body)
        {
            var historyStudentBlocking = new HMsStudentBlocking();
            var listStudent = body.Where(y=>y.IsBlocked).Select(y => y.IdStudent).ToList();
            var historyDB = await _dbContext.Entity<HMsStudentBlocking>()
                .Where(x => listStudent.Contains(x.IdStudent))
                .ToListAsync(CancellationToken);

            foreach (var item in body)
            {
                var history = historyDB.Where(x => x.IdStudent == item.IdStudent
                                && x.IdBlockingCategory == item.IdBlockingCategory
                                && x.IdBlockingType == item.IdBlockingType
                                && x.EndDate == null)
                                .FirstOrDefault();

                if (history == null)
                {
                    historyStudentBlocking = new HMsStudentBlocking()
                    {
                        IdHMsStudentBlocking = Guid.NewGuid().ToString(),
                        IdStudent = item.IdStudent,
                        IdBlockingCategory = item.IdBlockingCategory,
                        IdBlockingType = item.IdBlockingType,
                        StartDate = _dateTime.ServerTime
                    };

                    _dbContext.Entity<HMsStudentBlocking>().Add(historyStudentBlocking);
                }
            }

            return historyStudentBlocking;
        }

        private async Task ProcessEmailAndNotif(string idStudent, string IdBlockingCategory, string IdBlockingType)
        {
            var GetParent = await _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Parent)
                            .Where(x => x.IdStudent == idStudent)
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
                                            where StudentBlocking.IsBlocked == true && StudentBlocking.IdStudent == idStudent && StudentBlocking.IdBlockingCategory == IdBlockingCategory
                                            && StudentBlocking.IdBlockingType == IdBlockingType
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

            IEnumerable<string> IdRecipients = new string[] { idStudent };

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
