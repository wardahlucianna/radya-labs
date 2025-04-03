using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetStudentProgrammeStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _datetime;

        public GetStudentProgrammeStudentHandler(ISchedulingDbContext schoolDbContext, IMachineDateTime Datetime)
        {
            _dbContext = schoolDbContext;
            _datetime = Datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentProgrammeStudentRequest>();
            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => true);
            string[] _columns = { "studentId", "studentName", "homeroom"};

            var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                    .Where(e => e.Grade.Level.AcademicYear.IdSchool == param.schoolId
                                        && (_datetime.ServerTime.Date >= e.StartDate.Date && _datetime.ServerTime.Date <= e.EndDate.Date))
                                    .Select(e=>e.Grade.Level.IdAcademicYear)
                                    .FirstOrDefaultAsync(CancellationToken);

            var studentProgrames = await _dbContext.Entity<TrStudentProgramme>()
                                    .Where(x => x.IdSchool == param.schoolId)
                                    .Select(x => x.IdStudent)
                                    .ToListAsync();

            var listStudent = await _dbContext.Entity<MsHomeroomStudent>()
                        .Include(e => e.Student)
                        .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                        .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                        .Where(x => x.Homeroom.Grade.Level.AcademicYear.IdSchool == param.schoolId
                                && (x.Homeroom.Grade.Code.Contains("11") || x.Homeroom.Grade.Code.Contains("12"))
                                && x.Homeroom.Grade.Level.IdAcademicYear == idAcademicYear
                                && !studentProgrames.Contains(x.IdStudent)
                                )
                        .GroupBy(e => new
                        {
                            e.IdStudent,
                            e.Student.LastName,
                            e.Student.FirstName,
                            gardeCode = e.Homeroom.Grade.Code,
                            classroomCode = e.Homeroom.GradePathwayClassroom.Classroom.Code
                        })
                        .Select(e => new GetStudentProgrammeStudentResult
                        {
                            idStudent = e.Key.IdStudent,
                            studentName = NameUtil.GenerateFullName(e.Key.FirstName, e.Key.LastName),
                            homeroom = e.Key.gardeCode + e.Key.classroomCode,
                        }).ToListAsync(CancellationToken);

            
            var query = listStudent.Distinct();

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(e => e.studentName.ToLower().Contains(param.Search.ToLower()));

            switch (param.OrderBy)
            {
                case "studentId":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.idStudent)
                        : query.OrderBy(x => x.idStudent);
                    break;

                case "studentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.studentName)
                        : query.OrderBy(x => x.studentName);
                    break;

                case "homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.studentName)
                        : query.OrderBy(x => x.homeroom);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();

                items = result.ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
