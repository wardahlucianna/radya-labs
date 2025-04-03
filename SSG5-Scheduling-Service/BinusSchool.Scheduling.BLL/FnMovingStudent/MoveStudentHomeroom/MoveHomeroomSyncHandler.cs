using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment.Validator;
using BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom
{
    public class MoveHomeroomSyncHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public MoveHomeroomSyncHandler(ISchedulingDbContext schoolDbContext, IMachineDateTime datetime)
        {
            _dbContext = schoolDbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<MoveHomeroomSyncRequest, MoveHomeroomSyncValidator>();

            var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                    .Include(e => e.Grade).ThenInclude(e => e.Level)
                                    .Where(e => e.Grade.Level.AcademicYear.IdSchool == body.IdSchool
                                            && (e.StartDate.Date <= body.Date))
                                    .OrderBy(e => e.StartDate)
                                    .Select(e => e.Grade.Level.IdAcademicYear)
                                    .LastOrDefaultAsync(CancellationToken);

            var listHTrMoveStudentHomeroom = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                                                .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                                                .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                                .Where(e => e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear
                                                        && body.Date >= e.StartDate.Date
                                                        && e.IsShowHistory)
                                                .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn)
                                                .ToListAsync(CancellationToken);

            var listIdHomeroomStudent = listHTrMoveStudentHomeroom.Select(e => e.IdHomeroomStudent).Distinct().ToList();
            var listIdStudent = listHTrMoveStudentHomeroom.Select(e => e.HomeroomStudent.IdStudent).Distinct().ToList();

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Where(e => e.Homeroom.IdAcademicYear == idAcademicYear
                                                && listIdStudent.Contains(e.IdStudent))
                                        .ToListAsync(CancellationToken);

            var listHomeroom = await _dbContext.Entity<MsHomeroom>()
                                        .Include(e=>e.Grade)
                                        .Include(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                        .Where(e => e.IdAcademicYear == idAcademicYear)
                                        .ToListAsync(CancellationToken);

            foreach (var IdHomeroomStudent in listIdHomeroomStudent)
            {
                var item = listHTrMoveStudentHomeroom
                            .Where(e => e.IdHomeroomStudent == IdHomeroomStudent)
                            .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn).LastOrDefault();

                if (item.IsSync == true || item == null)
                    continue;

                if (item.HomeroomStudent.Homeroom.Semester == 1)
                {
                    var homeroomNew = listHomeroom.Where(e => e.Id == item.IdHomeroomNew).FirstOrDefault();
                    var gradeCode = homeroomNew.Grade.Code;
                    var classroomCode = homeroomNew.GradePathwayClassroom.Classroom.Code;

                    var listHomeroomStudentById = listHomeroomStudent
                                                    .Where(e => e.IdStudent == item.HomeroomStudent.IdStudent)
                                                    .ToList();
                    var listHomeroomByGradeClassroom = listHomeroom.Where(e => e.Grade.Code == gradeCode && e.GradePathwayClassroom.Classroom.Code == classroomCode).ToList();

                    listHomeroomStudentById.ForEach(e => e.IdHomeroom = listHomeroomByGradeClassroom.Where(f => f.Semester == e.Semester).Select(e => e.Id).FirstOrDefault());
                    _dbContext.Entity<MsHomeroomStudent>().UpdateRange(listHomeroomStudentById);

                }
                else
                {
                    var getHomeroomStudent = listHomeroomStudent.Where(e => e.Id == IdHomeroomStudent).FirstOrDefault();
                    getHomeroomStudent.IdHomeroom = item.IdHomeroomNew;
                    _dbContext.Entity<MsHomeroomStudent>().Update(getHomeroomStudent);
                }

                item.IsSync = true;
                item.DateSync = _datetime.ServerTime;
                _dbContext.Entity<HTrMoveStudentHomeroom>().Update(item);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
