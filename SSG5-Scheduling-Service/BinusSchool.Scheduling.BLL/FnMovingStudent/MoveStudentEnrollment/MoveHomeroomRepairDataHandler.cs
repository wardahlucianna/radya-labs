using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class MoveHomeroomRepairDataHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public MoveHomeroomRepairDataHandler(ISchedulingDbContext schoolDbContext, IMachineDateTime datetime)
        {
            _dbContext = schoolDbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<MoveHomeroomRepairDataRequest>();

            var listHTrMoveStudentEnroll = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                        .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom)
                        .Where(e=>e.HomeroomStudent.Homeroom.AcademicYear.Id==param.IdAcademicYear)
                        .Where(e=>e.HomeroomStudent.IdStudent== "1570001086")
                        .ToListAsync(CancellationToken);

            var listHomeroom = await _dbContext.Entity<MsHomeroom>()
                        .Include(e=>e.Grade)
                        .Include(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                        .Where(e => e.AcademicYear.Id == param.IdAcademicYear)
                        .ToListAsync(CancellationToken);

            var listIdStudent = listHTrMoveStudentEnroll.Select(e => e.HomeroomStudent.IdStudent).Distinct().ToList();

            #region update HTrMoveStudentHomeroom
            foreach (var itemIdStudent in listIdStudent) 
            {
                var listHTrMoveStudentEnrollbyStudent = listHTrMoveStudentEnroll
                                                            .Where(e => e.HomeroomStudent.IdStudent == itemIdStudent)
                                                            .ToList();

                var listHTrMoveStudentEnrollBySmt1 = listHTrMoveStudentEnrollbyStudent
                                                        .Where(e => e.HomeroomStudent.Semester == 1 )
                                                        //.OrderBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.StartDate).ThenBy(e => e.DateIn)
                                                        .ToList();

                var listHTrMoveStudentEnrollBySmt2 = listHTrMoveStudentEnrollbyStudent
                                                        .Where(e => e.HomeroomStudent.Semester == 2)
                                                        //.OrderBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.StartDate).ThenBy(e => e.DateIn)
                                                        .ToList();

                foreach (var itemHTrMoveStudentEnrollBySmt in listHTrMoveStudentEnrollBySmt1)
                {
                    var homeroomSmt1Old = listHomeroom.Where(e => e.Id == itemHTrMoveStudentEnrollBySmt.IdHomeroomOld).FirstOrDefault();
                    var homeroomSmt1New = listHomeroom.Where(e => e.Id == itemHTrMoveStudentEnrollBySmt.IdHomeroomNew).FirstOrDefault();

                    var homeroomSmt2Old = listHomeroom.Where(e=> e.Semester==2
                                                                    && e.Grade.Code== homeroomSmt1Old.Grade.Code
                                                                    && e.GradePathwayClassroom.Classroom.Code== homeroomSmt1Old.GradePathwayClassroom.Classroom.Code
                                                            ).FirstOrDefault();

                    var homeroomSmt2New = listHomeroom.Where(e => e.Semester == 2
                                                                    && e.Grade.Code == homeroomSmt1New.Grade.Code
                                                                    && e.GradePathwayClassroom.Classroom.Code == homeroomSmt1New.GradePathwayClassroom.Classroom.Code
                                                            ).FirstOrDefault();

                    var getHTrMoveStudentEnrollBySmt2 = listHTrMoveStudentEnrollBySmt2.Where(e=>e.StartDate== itemHTrMoveStudentEnrollBySmt.StartDate
                                                                        && e.IsShowHistory == itemHTrMoveStudentEnrollBySmt.IsShowHistory
                                                                        && e.Note == itemHTrMoveStudentEnrollBySmt.Note
                                                                        )
                                                                .FirstOrDefault();

                    getHTrMoveStudentEnrollBySmt2.IdHomeroomOld = homeroomSmt2Old.Id;
                    getHTrMoveStudentEnrollBySmt2.IdHomeroomNew = homeroomSmt2New.Id;
                    getHTrMoveStudentEnrollBySmt2.DateUp = getHTrMoveStudentEnrollBySmt2.DateUp==null?null: getHTrMoveStudentEnrollBySmt2.DateUp;
                    getHTrMoveStudentEnrollBySmt2.UserUp = getHTrMoveStudentEnrollBySmt2.UserUp==null?null: getHTrMoveStudentEnrollBySmt2.UserUp;

                    _dbContext.Entity<HTrMoveStudentHomeroom>().Update(getHTrMoveStudentEnrollBySmt2);
                    await _dbContext.SaveChangesAsync();
                }
            }
            #endregion

            #region homeroom student
            var listHTrMoveStudentEnrollNew = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                       .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                       .Where(e => e.HomeroomStudent.Homeroom.AcademicYear.Id == param.IdAcademicYear)
                       .Where(e => listIdStudent.Contains(e.HomeroomStudent.IdStudent))
                       .ToListAsync(CancellationToken);

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                        .Where(e => e.Homeroom.AcademicYear.Id == param.IdAcademicYear
                                        && listIdStudent.Contains(e.IdStudent))
                        .ToListAsync(CancellationToken);

            foreach (var item in listHomeroomStudent)
            {
                var lastIdHomeroom = listHTrMoveStudentEnrollNew
                                        .Where(e => e.IdHomeroomStudent == item.Id
                                                && e.HomeroomStudent.Semester == item.Semester
                                                && e.StartDate.Date <= _datetime.ServerTime.Date)
                                        .OrderBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.StartDate).ThenBy(e => e.DateIn)
                                        .LastOrDefault();

                item.IdHomeroom = lastIdHomeroom.IdHomeroomNew;
                _dbContext.Entity<MsHomeroomStudent>().Update(item);
                await _dbContext.SaveChangesAsync();
            }
            #endregion

            var listHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                   .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom)
                                                   .Include(e => e.LessonNew)
                                                   .Where(e => e.HomeroomStudent.Homeroom.AcademicYear.Id == param.IdAcademicYear)
                                                   .Where(e => listIdStudent.Contains(e.HomeroomStudent.IdStudent))
                                                   .ToListAsync(CancellationToken);

            foreach (var item in listHomeroomStudentEnrollment)
            {
                if(item.HomeroomStudent.Homeroom.Semester!= item.LessonNew.Semester)
                {
                    item.IsActive = false;
                    _dbContext.Entity<TrHomeroomStudentEnrollment>().Update(item);
                    await _dbContext.SaveChangesAsync();
                }


            }


            return Request.CreateApiResult2();
        }
    }
}
