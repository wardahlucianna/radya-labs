using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetListSubjectTeacherPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetListSubjectTeacherPositionHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserSubjectDescriptionRequest>(nameof(GetUserSubjectDescriptionRequest.IdUser));
            
            if (param.Position != PositionConstant.VicePrincipal && param.Position != PositionConstant.SubjectHead && param.Position != PositionConstant.SubjectHeadAssitant)
            {
                var predicate = PredicateBuilder.True<MsLessonTeacher>();

                if (!string.IsNullOrEmpty(param.IdUser))
                    predicate = predicate.And(x => x.IdUser == param.IdUser);

                if (!string.IsNullOrEmpty(param.IdGrade))
                    predicate = predicate.And(x => x.Lesson.Subject.IdGrade == param.IdGrade);

                if (!string.IsNullOrEmpty(param.IdAcadyear))
                    predicate = predicate.And(x => x.Lesson.IdAcademicYear == param.IdAcadyear);

                if (!string.IsNullOrWhiteSpace(param.IdHomeroom))
                    predicate = predicate.And(x => x.Lesson.Subject.Grade.Homerooms.Any(y => y.Id == param.IdHomeroom));

                var query = _dbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson).ThenInclude(x => x.Subject)
                .ThenInclude(x => x.Grade)
                          .Where(predicate)
                          .Select(x => new { x.Lesson.Subject.Id, x.Lesson.Subject.Description })
                          .Distinct();
                
                query = query.OrderBy(x => x.Description);

                IReadOnlyList<IItemValueVm> items;
                items = await query
                        .Select(x => new ItemValueVm(x.Description, x.Description))
                        .Distinct()
                        .ToListAsync(CancellationToken);

                return Request.CreateApiResult2(items as object);
            }
            else
            {
                var predicate = PredicateBuilder.True<MsSubject>();

                if (!string.IsNullOrEmpty(param.IdAcadyear))
                    predicate = predicate.And(x => param.IdAcadyear.Contains(x.Grade.Level.IdAcademicYear));
                if (!string.IsNullOrEmpty(param.IdGrade))
                    predicate = predicate.And(x => param.IdGrade.Contains(x.IdGrade));

                if (param.Position == PositionConstant.SubjectHead || param.Position == PositionConstant.SubjectHeadAssitant)
                {
                    var listIdSubject = new List<string>();
                    var listTeacherAssignment = await _dbContext.Entity<TrNonTeachingLoad>()
                    .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                     .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdAcadyear
                                && x.IdUser == param.IdUser
                                && x.MsNonTeachingLoad.TeacherPosition.Position.Code == param.Position).ToListAsync(CancellationToken);
                    foreach (var item in listTeacherAssignment)
                    {
                        var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                        _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);
                        _dataNewPosition.TryGetValue("Grade", out var _GradePosition);

                        if (_GradePosition != null)
                        {
                            if (_GradePosition.Id == param.IdGrade)
                            {
                                if (_SubjectPosition != null)
                                {
                                    listIdSubject.Add(_SubjectPosition.Id);
                                }
                            }
                        }
                    }
                    if (listIdSubject.Any())
                        predicate = predicate.And(x => listIdSubject.Contains(x.Id));
                }

                var query = _dbContext.Entity<MsSubject>().Where(predicate);

                IReadOnlyList<IItemValueVm> items = default;
                items = await query
                       .Select(x => new CodeWithIdVm(x.Description, x.Description))
                       .Distinct()
                       .ToListAsync(CancellationToken);

                return Request.CreateApiResult2(items as object);
            }
        }
    }
}
