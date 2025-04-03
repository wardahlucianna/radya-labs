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
    public class GetUserStudentDetailByIdHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetUserStudentDetailByIdHandler(ISchedulingDbContext schoolDbContext, IMachineDateTime dateTime)
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

            var result = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(e=>e.Student)
                        .Include(e => e.Homeroom).ThenInclude(e => e.AcademicYear)
                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                        .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                        .Where(e => e.IdStudent == param.UserId && e.Homeroom.AcademicYear.Id == param.IdAcademicYear && e.Homeroom.Semester == semester)
                        .Select(e => new GetUserStudentDetailByIdResult
                        {
                            StudentName = e.Student.FirstName+(e.Student.MiddleName==null?"":" "+ e.Student.MiddleName) + (e.Student.LastName == null ? "" : " " + e.Student.LastName), 
                            IdBinusan = e.Student.Id,
                            AcademicYear = new ItemValueVm
                            {
                                Id = e.Homeroom.AcademicYear.Id,
                                Description = e.Homeroom.AcademicYear.Description
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
                        }).FirstOrDefaultAsync(CancellationToken);
            ;

            return Request.CreateApiResult2(result as object);
        }
    }
}
