using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetTeacherByDateAYHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetTeacherByDateAYHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private string GetSubjectTeacher(string idUser)
        {

            var subjectTeacher = _dbContext.Entity<MsLessonTeacher>()
                                    .Include(x => x.Lesson).ThenInclude(x => x.Subject)
                                    .Where(x => x.IdUser == idUser).FirstOrDefault();
            if(subjectTeacher == null)
                return null;

            return subjectTeacher.Lesson.Subject.Description;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherByDateAYRequest>(nameof(GetTeacherByDateAYRequest.IdAcademicYear),
                                                                          nameof(GetTeacherByDateAYRequest.StartDate),
                                                                          nameof(GetTeacherByDateAYRequest.EndDate));

            // var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            var predicate = PredicateBuilder.Create<MsUser>(x => x.UserRoles.Any(y => y.Role.IdRoleGroup == "TCH") && x.UserSchools.Any(y => y.IdSchool == param.IdSchool.FirstOrDefault()));

            // if(!string.IsNullOrWhiteSpace(param.IdLevel))
            //     predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);

            // if(param.IdGrade != null)
            //     predicate = predicate.And(x => param.IdGrade.Contains(x.Homeroom.IdGrade));
            
            if (!string.IsNullOrWhiteSpace(param.Search))
               predicate = predicate.And(x => EF.Functions.Like(x.DisplayName.ToUpper(), $"%{param.Search.ToUpper()}%"));

            var query = _dbContext.Entity<MsUser>()
                                 .Include(e => e.UserRoles).ThenInclude(e => e.Role)
                                 .Include(e => e.UserSchools)
                                 .Where(predicate)
                                 .Select(x => new { x.Id, x.DisplayName })
                                 .Distinct()
                                 .OrderBy(x => x.DisplayName);

            IReadOnlyList<IItemValueVm> items;
            items = await query
                .Select(x => new ItemValueVm(x.Id, $"{x.DisplayName}"))
                .Distinct()
                .ToListAsync(CancellationToken);

            items = items
                    .Select(x => new ItemValueVm(
                    x.Id,
                    x.Description
                )
            )
            .OrderBy(x => x.Description)
            .ToList();
           
            return Request.CreateApiResult2(items as object);
        }
    }
}
