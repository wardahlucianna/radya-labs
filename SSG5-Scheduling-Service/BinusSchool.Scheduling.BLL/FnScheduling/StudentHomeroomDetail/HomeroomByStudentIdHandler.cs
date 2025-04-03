using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.School.FnSchool.GetActiveAcademicYear;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentHomeroomDetail
{
    public class HomeroomByStudentIdHandler : FunctionsHttpSingleHandler
    {

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public HomeroomByStudentIdHandler(ISchedulingDbContext dbContext, IApiService<IClassroomMap> classroomMapService, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentHomeroomDetailRequest>(nameof(GetStudentHomeroomDetailRequest.IdStudent));

            var IdSchool = await _dbContext.Entity<MsStudent>()
                            .Where(x => x.Id == param.IdStudent)
                            .Select(x => x.IdSchool)
                            .FirstOrDefaultAsync();

            var GetAcademicYear = await _dbContext.Entity<MsPeriod>()
                                    .Include(x => x.Grade)
                                        .ThenInclude(y => y.Level).ThenInclude(z => z.AcademicYear)
                                    .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == IdSchool)
                                    .OrderByDescending(x => x.StartDate)
                                    .Select(x => new 
                                    {
                                        AcademicYear = new ItemValueVm { 
                                            Id = x.Grade.Level.AcademicYear.Id,
                                            Description = x.Grade.Level.AcademicYear.Description
                                        },
                                        Semester = x.Semester,
                                    })
                                    .FirstOrDefaultAsync();

            var detailHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.Grade)
                    .ThenInclude(x => x.Level)
                    .ThenInclude(x => x.AcademicYear)
                    .ThenInclude(x => x.School)
                .Where(x => x.IdStudent == param.IdStudent && x.Semester == GetAcademicYear.Semester && x.Homeroom.IdAcademicYear == GetAcademicYear.AcademicYear.Id)
                .Select(x => new GetStudentHomeroomDetailResult {
                    StudentId = param.IdStudent,
                    SchoolId = x.Homeroom.AcademicYear.IdSchool,
                    AcadYear = new CodeWithIdVm()
                        {
                            Id = x.Homeroom.AcademicYear.Id,
                            Code = x.Homeroom.AcademicYear.Code,
                            Description = x.Homeroom.AcademicYear.Description,
                        },
                    Semester = x.Semester,
                    Level = new CodeWithIdVm()
                        {
                            Id = x.Homeroom.Grade.Level.Id,
                            Code = x.Homeroom.Grade.Level.Code,
                            Description = x.Homeroom.Grade.Level.Description,
                        },
                    Grade = new CodeWithIdVm()
                        {
                            Id = x.Homeroom.Grade.Id,
                            Code = x.Homeroom.Grade.Code,
                            Description = x.Homeroom.Grade.Description,
                        }
                })
                .FirstOrDefaultAsync();

            return Request.CreateApiResult2(detailHomeroomStudent as object);
        }
    }
}
