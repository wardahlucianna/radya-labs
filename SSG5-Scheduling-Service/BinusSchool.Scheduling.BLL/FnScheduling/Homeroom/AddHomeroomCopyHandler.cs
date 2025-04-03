using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.Homeroom.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Spreadsheet;
using SendGrid.Helpers.Mail;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class AddHomeroomCopyHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public AddHomeroomCopyHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddHomeroomCopyRequest, AddHomeroomCopyValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var ListHomeroom = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.HomeroomPathways)
                .Include(x => x.HomeroomTeachers)
                .Include(x => x.Grade)
                .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Include(x => x.HomeroomStudents)
                .Where(e => body.IdHomeroom.Contains(e.Id))
                .ToListAsync(CancellationToken);

            foreach (var itemHomeroom in ListHomeroom)
            {
                var existHomeroom = await _dbContext.Entity<MsHomeroom>()
                .FirstOrDefaultAsync(x
                    => x.IdAcademicYear == body.IdAcadyearCopyTo
                    && x.Semester == body.SemesterCopyTo
                    && x.IdGrade == itemHomeroom.IdGrade
                    && x.IdGradePathwayClassRoom == itemHomeroom.IdGradePathwayClassRoom
                    && x.IdGradePathway == itemHomeroom.IdGradePathway, CancellationToken);

                if (itemHomeroom.HomeroomStudents.Count == 0)
                {
                    throw new BadRequestException($"Homeroom (mapping student) with : {itemHomeroom.Grade.Code}{itemHomeroom.GradePathwayClassroom.Classroom.Code} has not been mapped so it cannot copy to next semester.");
                }

                var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
                .Where(x => (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                    || (x.StartDate < _dateTime.ServerTime.Date
                        ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                        : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

                if (existHomeroom != null)
                {
                    //throw new BadRequestException($"Classroom with : {itemHomeroom.Grade.Code}{itemHomeroom.GradePathwayClassroom.Classroom.Code} with semester: {body.SemesterCopyTo} is already exists.");
                    var homeroomStudentExist = await _dbContext.Entity<MsHomeroomStudent>()
                                               .Where(x => x.IdHomeroom == existHomeroom.Id)
                                               .Select(x=> x.IdStudent)
                                               .ToListAsync(CancellationToken);

                    var listStudents = itemHomeroom.HomeroomStudents.Where(x => !homeroomStudentExist.Contains(x.IdStudent)).ToList();
                    if (checkStudentStatus.Any())
                        listStudents = listStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();

                    var stdAlreadyHaveHomerooms = await _dbContext.Entity<MsHomeroomStudent>()
                    .Where(x => x.Homeroom.IdGrade == itemHomeroom.IdGrade
                                && listStudents.Select(y=> y.IdStudent).Contains(x.IdStudent)
                                && x.Semester == body.SemesterCopyTo)
                    .Select(x => x.IdStudent)
                    .ToListAsync(CancellationToken);

                    listStudents = listStudents.Where(x => !stdAlreadyHaveHomerooms.Contains(x.IdStudent)).ToList();

                    foreach (var student in listStudents)
                    {
                        var newHrStudent = new MsHomeroomStudent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroom = existHomeroom.Id,
                            IdStudent = student.IdStudent,
                            Gender = student.Gender,
                            Religion = student.Religion,
                            Semester = existHomeroom.Semester
                        };
                        _dbContext.Entity<MsHomeroomStudent>().Add(newHrStudent);
                    }
                }
                else
                {
                    var homeroom = new MsHomeroom
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = body.IdAcadyearCopyTo,
                        Semester = body.SemesterCopyTo,
                        IdGrade = itemHomeroom.IdGrade,
                        IdGradePathwayClassRoom = itemHomeroom.IdGradePathwayClassRoom,
                        IdVenue = itemHomeroom.IdVenue,
                        IdGradePathway = itemHomeroom.IdGradePathway
                    };
                    _dbContext.Entity<MsHomeroom>().Add(homeroom);

                    if (itemHomeroom.HomeroomPathways?.Any() ?? false)
                    {
                        foreach (var itemHomeroomPathways in itemHomeroom.HomeroomPathways)
                        {
                            var hrPathway = new MsHomeroomPathway
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdHomeroom = homeroom.Id,
                                IdGradePathwayDetail = itemHomeroomPathways.IdGradePathwayDetail
                            };
                            _dbContext.Entity<MsHomeroomPathway>().Add(hrPathway);
                        }
                    }

                    foreach (var itemHomeroomTeacher in itemHomeroom.HomeroomTeachers)
                    {
                        _dbContext.Entity<MsHomeroomTeacher>().Add(new MsHomeroomTeacher
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroom = homeroom.Id,
                            IdBinusian = itemHomeroomTeacher.IdBinusian,
                            IdTeacherPosition = itemHomeroomTeacher.IdTeacherPosition,
                            IsAttendance = itemHomeroomTeacher.IsAttendance,
                            IsScore = itemHomeroomTeacher.IsScore,
                            IsShowInReportCard = itemHomeroomTeacher.IsShowInReportCard,
                            DateIn = DateTimeUtil.ServerTime
                        });
                    }

                    var listStudents = itemHomeroom.HomeroomStudents.ToList();
                    if (checkStudentStatus.Any())
                        listStudents = itemHomeroom.HomeroomStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();

                    var stdAlreadyHaveHomerooms = await _dbContext.Entity<MsHomeroomStudent>()
                        .Where(x => x.Homeroom.IdGrade == itemHomeroom.IdGrade
                                    && listStudents.Select(y => y.IdStudent).Contains(x.IdStudent)
                                    && x.Semester == body.SemesterCopyTo)
                        .Select(x => x.IdStudent)
                        .ToListAsync(CancellationToken);

                    listStudents = listStudents.Where(x => !stdAlreadyHaveHomerooms.Contains(x.IdStudent)).ToList();

                    foreach (var student in listStudents)
                    {
                        var newHrStudent = new MsHomeroomStudent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroom = homeroom.Id,
                            IdStudent = student.IdStudent,
                            Gender = student.Gender,
                            Religion = student.Religion,
                            Semester = homeroom.Semester
                        };
                        _dbContext.Entity<MsHomeroomStudent>().Add(newHrStudent);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
