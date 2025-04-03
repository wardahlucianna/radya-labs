using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class GetTypeSettingClassDiaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetTypeSettingClassDiaryHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<GetTypeSettingClassDiaryRequest, GetTypeSettingClassDiaryValidarot>();

            var GetAllLesson = await _dbContext.Entity<MsLesson>()
                                    .Where(e => body.LessoinId.Contains(e.Id)
                                    && e.IdAcademicYear == body.AcademicYearId
                                    && e.IdGrade == body.GradeId
                                    && e.IdSubject == body.SubjectId
                                    && e.Semester == body.Semester
                                    )
                                    .Select(e => e.Id)
                                    .ToListAsync(CancellationToken);

            var GetTypeSetting = await _dbContext.Entity<MsClassDiaryTypeSetting>()
                                    .Include(e => e.ClassDiaryLessonExcludes)
                                    .Where(e => (body.IsStudent ? e.AllowStudentEntryClassDiary : true) && e.IdAcademicyear == body.AcademicYearId 
                                        && !e.ClassDiaryLessonExcludes.Any(x=>body.LessoinId.Contains(x.IdLesson))
                                    )
                                    .Select(e => new ItemValueVm { Id = e.Id, Description = e.TypeName })
                                    .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(GetTypeSetting as object);
        }
    }
}
