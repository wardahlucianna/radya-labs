using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment
{
    public class HomeroomStudentEnroolmentDoubelDataHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public HomeroomStudentEnroolmentDoubelDataHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<HomeroomStudentEnroolmentDoubelDataRequest>();

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                .Include(e=>e.TrHomeroomStudentEnrollments)
                                                 .Where(e=>e.HomeroomStudent.Homeroom.IdAcademicYear==param.IdAcademicYear)
                                                 .ToListAsync(CancellationToken);

            var listAscTimetableEnrollment = await _dbContext.Entity<TrAscTimetableEnrollment>()
                                                .Where(e => e.AscTimetable.IdAcademicYear == param.IdAcademicYear)
                                                .ToListAsync(CancellationToken);

            var listIdHomeroomStudentLesson = listHomeroomStudentEnrollment
                                                .GroupBy(e => new
                                                {
                                                    e.IdHomeroomStudent,
                                                    e.IdLesson
                                                })
                                                .Select(e => new
                                                {
                                                    data = e.Key,
                                                    count = e.Count()
                                                })
                                                .Where(e=>e.count>1).ToList();

            var dataDouble = listIdHomeroomStudentLesson.Select(e => e.data).ToList();

            foreach (var item in dataDouble)
            {
                var listHomeroomStudentEnrollmentByHomeroomStudentLesson = listHomeroomStudentEnrollment
                            .Where(e => e.IdLesson == item.IdLesson && e.IdHomeroomStudent == item.IdHomeroomStudent)
                            .OrderBy(e=>e.DateIn)
                            .ToList();

                if (listHomeroomStudentEnrollmentByHomeroomStudentLesson.Count > 1)
                {
                    var haveChild = listHomeroomStudentEnrollmentByHomeroomStudentLesson
                                        .Where(e=>e.TrHomeroomStudentEnrollments.Any()).ToList();

                    if (haveChild.Any())
                    {
                        var idHomeroomStudentEnrollment = haveChild.Select(e => e.Id).ToList();

                        var removeHomeroomStudentEnrollment = listHomeroomStudentEnrollmentByHomeroomStudentLesson
                                                                .Where(e => !idHomeroomStudentEnrollment.Contains(e.Id)).ToList();

                        removeHomeroomStudentEnrollment.ForEach(e=>e.IsActive = false);

                        _dbContext.Entity<MsHomeroomStudentEnrollment>().UpdateRange(removeHomeroomStudentEnrollment);

                        //var removeIdHomeroomStudentEnrollment = removeHomeroomStudentEnrollment.Select(e => e.Id).ToList();

                        //var listAscTimetableEnrollmentRemove = listAscTimetableEnrollment
                        //                                        .Where(e=> removeIdHomeroomStudentEnrollment.Contains(e.IdHomeroomStudentEnrollment))
                        //                                        .ToList();

                        //listAscTimetableEnrollmentRemove.ForEach(e => e.IsActive = false);

                        //_dbContext.Entity<TrAscTimetableEnrollment>().UpdateRange(listAscTimetableEnrollmentRemove);
                    }
                    else
                    {
                        var idHomeroomStudentEnrollment = listHomeroomStudentEnrollmentByHomeroomStudentLesson.Select(e => e.Id).LastOrDefault();

                        var removeHomeroomStudentEnrollment = listHomeroomStudentEnrollmentByHomeroomStudentLesson
                                                                .Where(e => idHomeroomStudentEnrollment!=e.Id).ToList();

                        removeHomeroomStudentEnrollment.ForEach(e => e.IsActive = false);

                        _dbContext.Entity<MsHomeroomStudentEnrollment>().UpdateRange(removeHomeroomStudentEnrollment);

                        //var removeIdHomeroomStudentEnrollment = removeHomeroomStudentEnrollment.Select(e => e.Id).ToList();

                        //var listAscTimetableEnrollmentRemove = listAscTimetableEnrollment
                        //                                        .Where(e => removeIdHomeroomStudentEnrollment.Contains(e.IdHomeroomStudentEnrollment))
                        //                                        .ToList();

                        //listAscTimetableEnrollmentRemove.ForEach(e => e.IsActive = false);

                        //_dbContext.Entity<TrAscTimetableEnrollment>().UpdateRange(listAscTimetableEnrollmentRemove);
                    }
                }
            }
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
