using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentSubject
{
    public class GetHomeroomMoveStudentSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetHomeroomMoveStudentSubjectHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomMoveStudentSubjectRequest>();

            var predicate = PredicateBuilder.Create<MsHomeroom>(x => x.IsActive);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(e => e.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(e => e.Semester == param.Semester);

            if (!string.IsNullOrEmpty(param.IdGradeOld))
                predicate = predicate.And(e => e.IdGrade == param.IdGradeOld);

            var items = await _dbContext.Entity<MsHomeroom>()
                        .Include(e => e.Grade).ThenInclude(e=>e.Level)
                        .Include(e => e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                        .Where(predicate)
                        .OrderBy(e=>e.Grade.Level.Code).ThenBy(e=>e.Grade.Code).ThenBy(e=>e.GradePathwayClassroom.Classroom.Code)
                        .Select(e => new GetHomeroomMoveStudentSubjectResult
                        {
                            IdHomeroom = e.Id,
                            Homeroom = e.Grade.Code + e.GradePathwayClassroom.Classroom.Code,
                            IdGrade = e.IdGrade
                        })
                        .ToListAsync(CancellationToken);


            return Request.CreateApiResult2(items as object);
        }
    }
}
