using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.StudentHomeroomDetail.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.StudentHomeroomDetail
{
    public class GetHomeroomStudentByLevelGradeHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = new[]
        {
            nameof(GetHomeroomStudentByLevelGradeRequest.IdAcademicYear),
            nameof(GetHomeroomStudentByLevelGradeRequest.Semester)
        };

        private readonly ISchedulingDbContext _dbContext;

        public GetHomeroomStudentByLevelGradeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetHomeroomStudentByLevelGradeRequest, GetHomeroomStudentByLevelGradeValidator>();

            var predicate = PredicateBuilder.Create<MsHomeroomStudent>
                (a => a.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear
                && a.Semester == param.Semester
                && (string.IsNullOrWhiteSpace(param.IdLevel) ? true : a.Homeroom.Grade.IdLevel == param.IdLevel)
                && (string.IsNullOrWhiteSpace(param.IdGrade) ? true : a.Homeroom.IdGrade == param.IdGrade)
                && (string.IsNullOrWhiteSpace(param.IdHomeroom) ? true : a.IdHomeroom == param.IdHomeroom));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Student.FirstName, $"%{param.Search}%")
                    || EF.Functions.Like(x.Student.MiddleName, $"%{param.Search}%")
                    || EF.Functions.Like(x.Student.LastName, $"%{param.Search}%")
                    || EF.Functions.Like(x.IdStudent, $"%{param.Search}%")
                    );

            var getHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom.Grade.Level.AcademicYear)
                .Include(x => x.Student)
                .Where(predicate)
                .OrderBy(x => x.Homeroom.Grade.Level.OrderNumber)
                    .ThenBy(x => x.Homeroom.Grade.OrderNumber)
                    .ThenBy(x => x.Homeroom.GradePathwayClassroom.Classroom.Code)
                .Select(x => new GetHomeroomStudentByLevelGradeResult
                {
                    Student = new NameValueVm()
                    {
                        Id = x.IdStudent,
                        Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                    },
                    AcademicYear = new CodeWithIdVm()
                    {
                        Id = x.Homeroom.Grade.Level.IdAcademicYear,
                        Code = x.Homeroom.Grade.Level.AcademicYear.Code,
                        Description = x.Homeroom.Grade.Level.AcademicYear.Description,
                    },
                    Level = new CodeWithIdVm()
                    {
                        Id = x.Homeroom.Grade.IdLevel,
                        Code = x.Homeroom.Grade.Level.Code,
                        Description = x.Homeroom.Grade.Level.Description,
                    },
                    Grade = new CodeWithIdVm()
                    {
                        Id = x.Homeroom.IdGrade,
                        Code = x.Homeroom.Grade.Code,
                        Description = x.Homeroom.Grade.Description,
                    },
                    Homeroom = new ItemValueVm()
                    {
                        Id = x.IdHomeroom,
                        Description = x.Homeroom.Grade.Code + "" + x.Homeroom.GradePathwayClassroom.Classroom.Code
                    },
                    Semester = param.Semester
                })
                .ToListAsync(CancellationToken);

            if (getHomeroomStudent is null)
                throw new BadRequestException("Homeroom not found");

            return Request.CreateApiResult2(getHomeroomStudent as object);
        }
    }
}
