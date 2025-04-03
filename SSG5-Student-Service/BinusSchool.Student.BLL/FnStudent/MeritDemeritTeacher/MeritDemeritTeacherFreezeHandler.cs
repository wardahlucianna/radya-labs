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
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemerit.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemerit
{
    public class MeritDemeritTeacherFreezeHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public MeritDemeritTeacherFreezeHandler(IStudentDbContext schoolDbContext, IMachineDateTime DateTime)
        {
            _dbContext = schoolDbContext;
            _dateTime = DateTime;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetFreezeRequest>();
            string[] _columns = { "AcademicYear", "Semester", "Level", "Grade", "Homeroom", "IdStudent", "NameStudent", "Freeze" };

            var query = (from HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>()
                         join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                         join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                         join GradePathway in _dbContext.Entity<MsGradePathway>() on GradePathwayClassroom.IdGradePathway equals GradePathway.Id
                         join Grade in _dbContext.Entity<MsGrade>() on GradePathway.IdGrade equals Grade.Id
                         join Level in _dbContext.Entity<MsLevel>() on Grade.IdLevel equals Level.Id
                         join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Level.IdAcademicYear equals AcademicYear.Id
                         join Student in _dbContext.Entity<MsStudent>() on HomeroomStudent.IdStudent equals Student.Id
                         join StudentStatus in _dbContext.Entity<TrStudentStatus>() on Student.Id equals StudentStatus.IdStudent
                         //join User in _dbContext.Entity<MsUser>() on Student.Id equals User.Id
                         join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                         join Freeze in _dbContext.Entity<MsStudentFreezeMeritDemerit>() on HomeroomStudent.Id equals Freeze.IdHomeroomStudent into JoinedFreeze
                         from Freeze in JoinedFreeze.DefaultIfEmpty()
                         where Level.IdAcademicYear == param.IdAcademiYear
                          && StudentStatus.IdAcademicYear == param.IdAcademiYear
                          && StudentStatus.ActiveStatus == true
                          && StudentStatus.EndDate == null 
                         select new
                         {
                             IdHomeroomStudent = HomeroomStudent.Id,
                             AcademicYear = AcademicYear.Description,
                             IdAcademicYear = AcademicYear.Id,
                             Semester = Homeroom.Semester.ToString(),
                             Level = Level.Description,
                             IdLevel = Level.Id,
                             Grade = Grade.Description,
                             IdGrade = Grade.Id,
                             IdHomerrom = Homeroom.Id,
                             Homeroom = (Grade.Code) + (Classroom.Code),
                             HomeroomCode = Grade.Code,
                             CodeClassroom = Classroom.Code,
                             CodeGrade = Grade.Code,
                             IdStudent = Student.Id,
                             NameStudent = (Student.FirstName==null?"":Student.FirstName) + (Student.MiddleName == null ? "" : " "+Student.MiddleName) + (Student.LastName==null?"":" "+Student.LastName),
                             IsFreeze = Freeze == null ? false : Freeze.IsFreeze,
                         }).OrderBy(e=>e.Grade).ThenBy(e=>e.CodeClassroom).Distinct();

            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                query = query.Where(x => x.Semester == param.Semester.ToString());
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomerrom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.IsFreeze.ToString()))
                query = query.Where(x => x.IsFreeze == param.IsFreeze);
            if (!string.IsNullOrEmpty(param.Search))
                query = query.Where(x => x.IdStudent.Contains(param.Search) || (x.NameStudent).Contains(param.Search));

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "Semester":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Semester)
                        : query.OrderBy(x => x.Semester);
                    break;
                case "Level":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Level)
                        : query.OrderBy(x => x.Level);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Homeroom":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Homeroom)
                        : query.OrderBy(x => x.Homeroom);
                    break;
                case "IdStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdStudent)
                        : query.OrderBy(x => x.IdStudent);
                    break;
                case "NameStudent":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.NameStudent)
                        : query.OrderBy(x => x.NameStudent);
                    break;
                default:
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;

            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetFreezeResult
                {
                    IdHomeroomStudent =  x.IdHomeroomStudent,
                    AcademicYear = x.AcademicYear,
                    IdAcademicYear = x.IdAcademicYear,
                    Semester = x.Semester,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    IdStudent = x.IdStudent,
                    NameStudent = x.NameStudent,
                    IsFreeze = x.IsFreeze,
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);
                items = result.Select(x => new GetFreezeResult
                {
                    IdHomeroomStudent = x.IdHomeroomStudent,
                    AcademicYear = x.AcademicYear,
                    IdAcademicYear = x.IdAcademicYear,
                    Semester = x.Semester,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    IdStudent = x.IdStudent,
                    NameStudent = x.NameStudent,
                    IsFreeze = x.IsFreeze,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdHomeroomStudent).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<UpdateFreezeRequest, UpdateFreezeRequestValidator>();
            var ScoreContinuation = _dbContext.Entity<MsStudentFreezeMeritDemerit>().SingleOrDefault(e => e.IdHomeroomStudent == body.IdHomeroomStudent);

            if (ScoreContinuation == null)
            {
                var newStudentFreezeMeritDemerit = new MsStudentFreezeMeritDemerit
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHomeroomStudent = body.IdHomeroomStudent,
                    IsFreeze = body.Isfreeze,
                };
                _dbContext.Entity<MsStudentFreezeMeritDemerit>().Add(newStudentFreezeMeritDemerit);
            }
            else
            {
                ScoreContinuation.IsFreeze = body.Isfreeze;
                _dbContext.Entity<MsStudentFreezeMeritDemerit>().Update(ScoreContinuation);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            
            return Request.CreateApiResult2();
        }
    }
}
