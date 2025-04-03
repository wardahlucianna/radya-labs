using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Lesson.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class GetLessonByTeacherIDHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetLessonByTeacherIDRequest.IdTeacher) };
        
        private readonly ISchedulingDbContext _dbContext;

        public GetLessonByTeacherIDHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLessonByTeacherIDRequest>(_requiredParams);

            var predicate = PredicateBuilder.Create<MsLesson>(x => x.LessonTeachers.Any(y => y.IdUser == param.IdTeacher));

            var query = _dbContext.Entity<MsLesson>()
                .SearchByIds(param)
                .Where(predicate);

            IReadOnlyList<object> items;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                    .Select(x => new GetLessonByTeacherIDResult
                    {
                        AcademicYear = x.Grade.Level.AcademicYear.Code,
                        Grade = x.Grade.Description,
                        Semester = x.Semester,
                        ClassId = x.ClassIdGenerated,
                        Subject = x.Subject.Description,
                        VerifiedDate = x.DateIn,
                        VerifiedBy = x.UserIn
                    })
                    .ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetLessonByTeacherIDResult
                    {
                        AcademicYear = x.Grade.Level.AcademicYear.Code,
                        Grade = x.Grade.Description,
                        Semester = x.Semester,
                        ClassId = x.ClassIdGenerated,
                        Subject = x.Subject.Description,
                        VerifiedDate = x.DateIn,
                        VerifiedBy = x.UserIn
                    })
                    .ToListAsync(CancellationToken);
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
