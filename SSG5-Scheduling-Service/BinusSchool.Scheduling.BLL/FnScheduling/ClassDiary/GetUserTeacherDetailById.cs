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
using BinusSchool.Data.Model.Scheduling.FnSchedule.Schedule;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.ClassDiary.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetUserTeacherDetailById : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetUserTeacherDetailById(ISchedulingDbContext schoolDbContext, IMachineDateTime dateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUserTeacherDetailByIdRequest>();

            var semester = await _dbContext.Entity<MsPeriod>()
                        .Include(e => e.Grade).ThenInclude(e => e.Level)
                        .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                        .Select(e => e.Semester).Distinct().SingleOrDefaultAsync(CancellationToken);

            var GetHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                        .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                        .Where(e => e.IdBinusian == param.UserId && e.Homeroom.Grade.Level.IdAcademicYear==param.IdAcademicYear && e.Homeroom.Semester == semester)
                        .Select(e => new 
                        {
                            AcademicYear = new ItemValueVm
                            {
                                Id = e.Homeroom.Grade.Level.IdAcademicYear,
                                Description = e.Homeroom.Grade.Level.Description
                            },
                            Level = new ItemValueVm
                            {
                                Id = e.Homeroom.Grade.Level.Id,
                                Description = e.Homeroom.Grade.Level.Description
                            },
                            Grade = new ItemValueVm
                            {
                                Id = e.Homeroom.Grade.Id,
                                Description = e.Homeroom.Grade.Description
                            },
                            Semester = e.Homeroom.Semester,
                            Homeroom = new ItemValueVm
                            {
                                Id = e.Homeroom.Id,
                                Description = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                            },
                        }).ToListAsync(CancellationToken);
                ;

            var result = new GetUserTeacherDetailByIdResult
            {
                AcademicYear = GetHomeroomTeacher.Select(e=>e.AcademicYear).FirstOrDefault(),
                Level = GetHomeroomTeacher.Select(e => e.Level).FirstOrDefault(),
                Grade = GetHomeroomTeacher.Select(e => e.Grade).FirstOrDefault(),
                Semester = GetHomeroomTeacher.Select(e => e.Semester).FirstOrDefault(),
                Homeroom = GetHomeroomTeacher.Select(e=>e.Homeroom).ToList(),
            };

                return Request.CreateApiResult2(result as object);
        }


    }
}
