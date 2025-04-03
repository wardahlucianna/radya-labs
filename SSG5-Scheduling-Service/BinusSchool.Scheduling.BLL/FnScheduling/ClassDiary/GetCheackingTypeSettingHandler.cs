using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetCheackingTypeSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetCheackingTypeSettingHandler(ISchedulingDbContext schoolDbContext, IMachineDateTime dateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<GetCheackingTypeSettingRequest, GetCheackingTypeSettingValidator>();
            List<CheckTypeFeedbackResult> Limit = new List<CheckTypeFeedbackResult>();

            var GetMsClassDiaryLessonExcludeByLesson = await _dbContext.Entity<MsClassDiaryLessonExclude>()
                                                        .Include(e => e.ClassDiaryTypeSetting)
                                                        .Include(e => e.Lesson)
                                                        .Where(e => body.LessoinId.Contains(e.IdLesson)
                                                         && e.ClassDiaryTypeSetting.IdAcademicyear == body.AcademicYearId
                                                         && e.Lesson.IdGrade == body.GradeId
                                                         && e.Lesson.IdSubject == body.SubjectId
                                                         && e.ClassDiaryTypeSetting.Id == body.TypeSettingId)
                                                        .Select(e => new { e.IdLesson })
                                                        .ToListAsync(CancellationToken);

            var GetAllLesson = await _dbContext.Entity<MsLesson>()
                                  .Where(e => !GetMsClassDiaryLessonExcludeByLesson.Select(e => e.IdLesson).Contains(e.Id)
                                  && body.LessoinId.Contains(e.Id)
                                  && e.IdAcademicYear == body.AcademicYearId
                                  && e.IdGrade == body.GradeId
                                  && e.IdSubject == body.SubjectId
                                  && e.Semester == body.Semester
                                  )
                                  .Select(e => new { IdLesson = e.Id })
                                  .ToListAsync(CancellationToken);

            var GetAllHomeroom = await (from hse in _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                  join ls in _dbContext.Entity<MsLesson>() on hse.IdLesson equals ls.Id
                                  join hr in _dbContext.Entity<MsHomeroomStudent>() on hse.IdHomeroomStudent equals hr.Id
                                  where GetAllLesson.Select(e => e.IdLesson).Contains(hse.IdLesson)
                                  select new
                                  {
                                      hr.IdHomeroom
                                  }).Distinct().ToListAsync(CancellationToken);



            var GatClassDiaryTypeSetting = await _dbContext.Entity<MsClassDiaryTypeSetting>()
                                                      .Where(e =>
                                                       e.IdAcademicyear == body.AcademicYearId
                                                       && e.Id == body.TypeSettingId)
                                                      .Select(e => new { e.OccurrencePerDay, e.MinimumStartDay })
                                                      .SingleOrDefaultAsync(CancellationToken);

            var GetTrClassDiary = await _dbContext.Entity<TrClassDiary>()
                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                        .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                        .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                        .Where(e => e.ClassDiaryDate.Date == body.Date.Date
                            && e.IdClassDiaryTypeSetting == body.TypeSettingId
                            && body.HomeroomId.Contains(e.IdHomeroom)
                            // && e.Homeroom.IdAcademicYear == body.AcademicYearId
                            && e.Homeroom.IdAcademicYear == body.AcademicYearId
                            && e.Homeroom.IdGrade == body.GradeId)
                        //&& e.Lesson.IdSubject == body.SubjectId)// ini di comment karena cukup sampe hoomrom id, tidak per subject id
                        .Select(e => new { e.IdLesson,e.IdHomeroom, Homeroom = e.Homeroom.Grade.Description + e.Homeroom.GradePathwayClassroom.Classroom.Description })
                        .ToListAsync(CancellationToken);

            if (GetAllHomeroom.FirstOrDefault() != null)
            {
                if (body.Date.Date <= _dateTime.ServerTime.Date.AddDays(GatClassDiaryTypeSetting.MinimumStartDay - 1))
                    throw new BadRequestException("Start date is " + _dateTime.ServerTime.Date.AddDays(GatClassDiaryTypeSetting.MinimumStartDay).ToString("dd MMM yyyy"));
            }

            foreach (var items in GetAllHomeroom)
            {
                var GetMsClassDiaryOccurrencePerDay = GatClassDiaryTypeSetting.OccurrencePerDay;
                var CountTrClassDiary = GetTrClassDiary.Any(e => e.IdHomeroom == items.IdHomeroom)
                    ? GetTrClassDiary.Where(e => e.IdHomeroom == items.IdHomeroom).Count() + 1 : 1;

                if (CountTrClassDiary > GetMsClassDiaryOccurrencePerDay)
                {
                    if (GetTrClassDiary.Any(e => e.IdHomeroom == items.IdHomeroom))
                    {
                        Limit.Add(new CheckTypeFeedbackResult
                        {
                            Homeroom = GetTrClassDiary.Any(e => e.IdHomeroom == items.IdHomeroom) ? GetTrClassDiary.FirstOrDefault(e => e.IdHomeroom == items.IdHomeroom).Homeroom : "",
                            LimitPerDay = GetMsClassDiaryOccurrencePerDay,
                        });
                    }
                }
            }

            return Request.CreateApiResult2(Limit.Distinct() as object);
        }
    }
}
