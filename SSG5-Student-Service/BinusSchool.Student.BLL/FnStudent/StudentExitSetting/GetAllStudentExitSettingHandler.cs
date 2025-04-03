using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using static Azure.Core.HttpHeader;

namespace BinusSchool.Student.FnStudent.StudentExitSetting
{
    public class GetAllStudentExitSettingHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = new[] { "AcademicYear","Semester", "IdStudent", "StudentName", "Level", "Grade", "Homeroom"};

        private readonly IStudentDbContext _studentDbContext;
        private readonly IMachineDateTime _dateTime;

        public GetAllStudentExitSettingHandler(IStudentDbContext studentDbContext, IMachineDateTime dateTime)
        {
            _studentDbContext = studentDbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAllStudentExitSettingRequest, GetAllStudentExitSettingValidator>();
            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x =>
                 x.Homeroom.Grade.MsLevel.IdAcademicYear == param.AcademicYear);

            if (param.Semester.HasValue)
            {
                predicate = predicate.And(x => x.Semester == param.Semester);
            }
            else
            {
                // get Active Semester
                var getActiveSemester = _studentDbContext.Entity<MsPeriod>()
                                            .Include(x => x.Grade)
                                            .Include(x => x.Grade.MsLevel)
                                            .Include(x => x.Grade.MsLevel.MsAcademicYear)
                                            .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                            .Where(x => x.Grade.MsLevel.IdAcademicYear == param.AcademicYear)
                                            .OrderByDescending(x => x.StartDate)
                                            .Select(x => x.Semester)
                                            .FirstOrDefault();
                if (getActiveSemester == 0)
                    throw new BadRequestException("Active semester not found!");

                predicate = predicate.And(x => x.Semester == getActiveSemester);
            }

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Homeroom.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);
            if (param.IsExit != null)
                predicate = predicate.And(x => x.StudentExitSetting.IsExit == param.IsExit);

            var query = _studentDbContext.Entity<MsHomeroomStudent>()
                            .Include(x => x.Homeroom)
                                    .ThenInclude(x => x.Grade)
                                        .ThenInclude(x => x.MsLevel)
                                            .ThenInclude(x => x.MsAcademicYear)
                            .Include(x => x.Student)
                            .Include(x => x.StudentExitSetting)
                            .Include(x => x.Homeroom)
                                .ThenInclude(x => x.MsGradePathwayClassroom)
                                    .ThenInclude(x => x.Classroom)
                            .Where(predicate)
                            .Select(e => new
                            {
                                AcademicYear = e.Homeroom.Grade.MsLevel.MsAcademicYear.Code,
                                IdAcademicYear = e.Homeroom.Grade.MsLevel.MsAcademicYear.Id,
                                Semester  = e.Homeroom.Semester,
                                Level = e.Homeroom.Grade.MsLevel.Code,
                                Grade = e.Homeroom.Grade.Code,
                                Homeroom = e.Homeroom.Grade.Code + e.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                                IdStudent = e.IdStudent,
                                Student = (string.IsNullOrWhiteSpace(e.Student.FirstName) ? "" : e.Student.FirstName.Trim() + " ")
                                            + (string.IsNullOrWhiteSpace(e.Student.MiddleName) ? "" : e.Student.MiddleName.Trim() + " ")
                                            + (string.IsNullOrWhiteSpace(e.Student.LastName) ? "" : e.Student.LastName.Trim() + " "),
                                IdHomeroomStudent = e.Id
                            })
                            ;

            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.IdStudent.Contains(param.Search) ||x.Student.ToLower().Contains(param.Search));

            query = param.OrderBy switch
            {
                "AcademicYear" => param.OrderType == Common.Model.Enums.OrderType.Asc
                                ? query.OrderBy(x => x.AcademicYear)
                                : query.OrderByDescending(x => x.AcademicYear),
                "Semester" => param.OrderType == Common.Model.Enums.OrderType.Asc
                                ? query.OrderBy(x => x.Semester)
                                : query.OrderByDescending(x => x.Semester),
                "Level" => param.OrderType == Common.Model.Enums.OrderType.Asc
                                ? query.OrderBy(x => x.Level)
                                : query.OrderByDescending(x => x.Level),
                "Grade" => param.OrderType == Common.Model.Enums.OrderType.Asc
                                ? query.OrderBy(x => x.Level).ThenBy(x => x.Grade)
                                : query.OrderByDescending(x => x.Level).ThenBy(x => x.Grade),
                "Homeroom" => param.OrderType == Common.Model.Enums.OrderType.Asc
                                ? query.OrderBy(x => x.Level).ThenBy(x=>x.Grade).ThenBy(x=>x.Homeroom)
                                : query.OrderByDescending(x => x.Level).ThenBy(x => x.Grade).ThenBy(x => x.Homeroom),
                "IdStudent" => param.OrderType == Common.Model.Enums.OrderType.Asc
                                ? query.OrderBy(x => x.IdStudent)
                                : query.OrderByDescending(x => x.IdStudent),
                "StudentName" => param.OrderType == Common.Model.Enums.OrderType.Asc
                                ? query.OrderBy(x => x.Student)
                                : query.OrderByDescending(x => x.Student),
            };

            int count = 0;
            if (param.Return == Common.Model.Enums.CollectionType.Lov)
            {
                IReadOnlyList<IItemValueVm> items;
                items = await query.Select(x => new ItemValueVm
                {
                    Id = x.IdHomeroomStudent,
                    Description = x.Student
                }).ToListAsync();

                count = param.CanCountWithoutFetchDb(items.Count) ? items.Count : await query.Select(x => x.IdHomeroomStudent).CountAsync();

                return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            }
            else
            {
                var GetData = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                var result = GetData
                    .Select(x => new GetAllStudentExitSettingResult
                    {
                        Id = x.IdHomeroomStudent,
                        IdAcademicYear = x.IdAcademicYear,
                        AcademicYear = x.AcademicYear,
                        IdStudent = x.IdStudent,
                        StudentName = x.Student,
                        Semester = x.Semester,
                        LevelCode = x.Level,
                        GradeCode = x.Grade,
                        HomeroomName = x.Homeroom,
                    }).Distinct().ToList();

                var idHomeroomStudents = result.Select(x => x.Id).ToList();
                var studentExits = await _studentDbContext.Entity<MsStudentExitSetting>().Where(x => idHomeroomStudents.Contains(x.IdHomeroomStudent)).AsNoTracking().ToListAsync();

                foreach (var item in result)
                {
                    item.IsExit = studentExits.Any(x => x.IdHomeroomStudent == item.Id) ? studentExits.FirstOrDefault(x => x.IdHomeroomStudent == item.Id).IsExit : false;
                }

                count = param.CanCountWithoutFetchDb(result.Count) ? result.Count : await query.Select(x => x.IdHomeroomStudent).CountAsync();

                return Request.CreateApiResult2(result as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
            }

        }
    }
}
