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
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class DeleteClassDiaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteClassDiaryHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteClassDiaryRequest, DeteleClassDiaryValidator>();

            var GetClassDiary = await _dbContext.Entity<TrClassDiary>()
                .Include(e => e.ClassDiaryAttachments)
                .Include(e => e.HistoryClassDiaries)
                .Include(e => e.Homeroom).ThenInclude(e => e.HomeroomTeachers)
                .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                .Include(e => e.ClassDiaryTypeSetting)
                .Where(e => e.Id == body.ClassDiaryId).SingleOrDefaultAsync(CancellationToken);
            var GetClassDiaryAttachment = await _dbContext.Entity<TrClassDiaryAttachment>().Where(e => e.IdClassDiary == body.ClassDiaryId).ToListAsync(CancellationToken);

            if (GetClassDiary == null)
                throw new BadRequestException("Class diary is not found.");

            //create History class diary
            var newClassDiary = new HTrClassDiary
            {
                IdHTrClassDiary = Guid.NewGuid().ToString(),
                IdTrClassDiary = GetClassDiary.Id,
                IdClassDiaryTypeSetting = GetClassDiary.IdClassDiaryTypeSetting,
                IdHomeroom = GetClassDiary.IdHomeroom,
                IdLesson = GetClassDiary.IdLesson,
                ClassDiaryDate = GetClassDiary.ClassDiaryDate,
                ClassDiaryTopic = GetClassDiary.ClassDiaryTopic,
                ClassDiaryDescription = GetClassDiary.ClassDiaryDescription,
                Status = "Delete Requested",
                DeleteReason = body.DeleteReason,
            };
            _dbContext.Entity<HTrClassDiary>().Add(newClassDiary);

            //create History class diary attachment
            foreach (var ItemAttachment in GetClassDiaryAttachment)
            {
                var newClassDiaryAttachment = new HTrClassDiaryAttachment
                {
                    IdHTrClassDiary = newClassDiary.IdHTrClassDiary,
                    IdHTrClassDiaryAttachment = Guid.NewGuid().ToString(),
                    OriginalFilename = ItemAttachment.OriginalFilename,
                    Url = ItemAttachment.Url,
                    Filename = ItemAttachment.Filename,
                    Filetype = ItemAttachment.Filetype,
                    Filesize = ItemAttachment.Filesize,
                };
                _dbContext.Entity<HTrClassDiaryAttachment>().Add(newClassDiaryAttachment);
            }

            //update status class diary
            GetClassDiary.Status = "Delete Requested";
            _dbContext.Entity<TrClassDiary>().Update(GetClassDiary);

            await _dbContext.SaveChangesAsync(CancellationToken);

            //send notification
            KeyValues.Add("ClassDiary", GetClassDiary);
            KeyValues.Add("CreatorName", AuthInfo.UserName);
            KeyValues.Add("SchoolName", AuthInfo.Tenants.FirstOrDefault().Name);

            var positionReceiptment = _dbContext.Entity<MsTeacherPosition>()
                .Include(x => x.Position)
                .Where(x => GetClassDiary.Homeroom.HomeroomTeachers.Select(x => x.IdTeacherPosition).Contains(x.Id) && x.Position.Code == "VP")
                .Select(x => x.Id);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "CD5")
                {
                    IdRecipients = GetClassDiary.Homeroom.HomeroomTeachers.Where(x => positionReceiptment.Contains(x.IdTeacherPosition)).Select(x => x.IdBinusian),
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return Request.CreateApiResult2();
        }
    }
}
