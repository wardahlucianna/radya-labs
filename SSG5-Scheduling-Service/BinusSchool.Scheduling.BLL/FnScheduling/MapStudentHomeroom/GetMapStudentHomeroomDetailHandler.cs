using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class GetMapStudentHomeroomDetailHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetMapStudentHomeroomDetailRequest.IdGrade)
        });

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetMapStudentHomeroomDetailHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("id", out var id))
                throw new ArgumentNullException(nameof(id));

            var param = Request.ValidateParams<GetMapStudentHomeroomDetailRequest>(_requiredParams.Value);
            var hrStudents = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom).ThenInclude(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                .Include(x => x.Homeroom).ThenInclude(x => x.Grade)
                .Where(x => x.IdHomeroom == (string)id)
                .ToListAsync(CancellationToken);

            var checkStudentStatus = await _dbContext.Entity<TrStudentStatus>().Select(x => new { x.IdStudent, x.StartDate, x.EndDate, x.IdStudentStatus, x.CurrentStatus, x.ActiveStatus })
                .Where(x => (x.StartDate == _dateTime.ServerTime.Date || x.EndDate == _dateTime.ServerTime.Date
                    || (x.StartDate < _dateTime.ServerTime.Date
                        ? x.EndDate != null ? (x.EndDate > _dateTime.ServerTime.Date && x.EndDate < _dateTime.ServerTime.Date) || x.EndDate > _dateTime.ServerTime.Date : x.StartDate <= _dateTime.ServerTime.Date
                        : x.EndDate != null ? ((_dateTime.ServerTime.Date > x.StartDate && _dateTime.ServerTime.Date < x.EndDate) || _dateTime.ServerTime.Date > x.EndDate) : x.StartDate <= _dateTime.ServerTime.Date)) && x.CurrentStatus == "A" && x.ActiveStatus == false)
                .ToListAsync();

            if (checkStudentStatus != null)
            {
                hrStudents = hrStudents.Where(x => !checkStudentStatus.Select(z => z.IdStudent).ToList().Contains(x.IdStudent)).ToList();
            }

            var result = Enumerable.Empty<GetMapStudentHomeroomDetailResult>();
            if (hrStudents.Count != 0)
            {
                var idStudents = hrStudents.Select(x => x.IdStudent).ToList();
                var studentGradesCurrent = await _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.Student).ThenInclude(x => x.Religion)
                    .Include(x => x.StudentGradePathways).ThenInclude(x => x.Pathway)
                    .Include(x => x.StudentGradePathways).ThenInclude(x => x.PathwayNextAcademicYear)
                    .Include(x => x.Grade)
                    .Where(x => x.IdGrade == param.IdGrade && idStudents.Contains(x.IdStudent))
                    .IgnoreQueryFilters()
                    .ToListAsync(CancellationToken);

                var academic = await _dbContext.Entity<MsGrade>()
                    .Include(e => e.Level).ThenInclude(e => e.AcademicYear)
                    .Where(x => x.Id == param.IdGrade)
                    .Select(e => new
                    {
                        year = e.Level.AcademicYear.Code,
                        idSchool = e.Level.AcademicYear.IdSchool,
                        gradeCode = e.Code,
                    })
                    .FirstOrDefaultAsync(CancellationToken);

                if (academic == null)
                    throw new BadRequestException("Grade is nopt found");

                var NextYear = Convert.ToInt32(academic.year) + 1;

                var studentGradesNext = await _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.StudentGradePathways).ThenInclude(x => x.Pathway)
                    .Where(x => idStudents.Contains(x.IdStudent)
                                && x.StudentGradePathways.Any()
                                && x.Grade.Level.AcademicYear.Code == NextYear.ToString()
                                && x.Grade.Level.AcademicYear.IdSchool == academic.idSchool
                                )
                    .Select(e => new
                    {
                        e.Id,
                        e.IdStudent,
                        Pathway = e.StudentGradePathways.Where(e => e.Pathway != null).Select(e => e.Pathway).FirstOrDefault()
                    })
                    .ToListAsync(CancellationToken);


                List<GetMapStudentHomeroomDetailResult> listMapStudentHomeroomDetail = new List<GetMapStudentHomeroomDetailResult>();
                foreach (var item in hrStudents)
                {
                    var studentGradesCurrentByIdStudent = studentGradesCurrent.Where(e => e.IdStudent == item.IdStudent).FirstOrDefault();

                    if (studentGradesCurrentByIdStudent == null)
                        continue;

                    var pathway = new ItemValueVm();
                    var lastPathway = new ItemValueVm();
                    if (studentGradesCurrentByIdStudent.StudentGradePathways.Any())
                    {
                        pathway = studentGradesCurrentByIdStudent.StudentGradePathways.FirstOrDefault(x => x.IsActive).IdPathwayNextAcademicYear == null
                                        ? studentGradesCurrentByIdStudent.StudentGradePathways
                                            .Where(x => x.IsActive)
                                            .Select(x => new ItemValueVm(x.IdPathway, x.Pathway.Description))
                                            .FirstOrDefault()
                                        : studentGradesCurrentByIdStudent.StudentGradePathways
                                            .Where(x => x.IsActive)
                                            .Select(x => new ItemValueVm(x.IdPathwayNextAcademicYear, x.PathwayNextAcademicYear.Description))
                                            .FirstOrDefault();
                        lastPathway = studentGradesCurrentByIdStudent.StudentGradePathways
                           .Where(x => x.IsActive)
                           .Select(x => new ItemValueVm(x.IdPathway, x.Pathway.Description))
                           .FirstOrDefault();
                    }
                    else
                    {
                        pathway = default;
                        lastPathway = default;
                    }

                    var MapStudentHomeroomDetail = new GetMapStudentHomeroomDetailResult
                    {
                        Id = item.Id,
                        IdStudent = item.IdStudent,
                        StudentName = NameUtil.GenerateFullName(item.Student.FirstName, item.Student.MiddleName, item.Student.LastName),
                        Code = studentGradesCurrentByIdStudent.Grade.Description,
                        Description = NameUtil.GenerateFullNameWithId(item.IdStudent,
                            item.Student.FirstName,
                            item.Student.MiddleName,
                            item.Student.LastName),
                        Gender = item.Student.Gender,
                        Religion = item.Student.Religion.ReligionName,
                        IsActive = true,
                        LastHomeroom = string.Format("{0}{1} - {2} {3}",
                            item.Homeroom.Grade.Code,
                            item.Homeroom.GradePathwayClassroom.Classroom.Code,
                            item.Homeroom.Grade.Code,
                            item.Homeroom.GradePathwayClassroom.Classroom.Description),
                        Pathway = pathway,
                        LastPathway = lastPathway
                    };

                    listMapStudentHomeroomDetail.Add(MapStudentHomeroomDetail);

                }

                //ordering
                result = listMapStudentHomeroomDetail.OrderBy(x => x.IdStudent).Distinct();

                switch (param.OrderBy)
                {
                    case "StudentName":
                        result = param.OrderType == OrderType.Desc
                            ? result.OrderByDescending(x => x.StudentName)
                            : result.OrderBy(x => x.StudentName);
                        break;
                    case "IdStudent":
                        result = param.OrderType == OrderType.Desc
                            ? result.OrderByDescending(x => x.IdStudent)
                            : result.OrderBy(x => x.IdStudent);
                        break;
                };
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
