using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetListStudentBySupervisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListStudentBySupervisorHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListStudentBySupervisorRequest>();

            var predicate = PredicateBuilder.Create<TrExperience>(x => x.IdUserSupervisor == param.IdUser && x.IdAcademicYear == param.IdAcademicYear && (x.Status == ExperienceStatus.Approved || x.Status == ExperienceStatus.Completed));
            string[] _columns = { "StudentName", "Grade", "OverallStatus" };

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(e => e.HomeroomStudent.Homeroom.IdGrade == param.IdGrade);

            var getStudent = await _dbContext.Entity<TrExperience>()
               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
              .Where(predicate)
              .Select(x => new
              {
                  IdStudent = x.HomeroomStudent.IdStudent,
                  FirstName = x.HomeroomStudent.Student.FirstName,
                  MiddleName = x.HomeroomStudent.Student.MiddleName,
                  LastName = x.HomeroomStudent.Student.LastName,
                  Grade = x.HomeroomStudent.Homeroom.Grade.Description,
              }).Distinct().ToListAsync(CancellationToken);

            var GetStatus = await _dbContext.Entity<TrExperienceStudent>()
              .Where(x => x.IdAcademicYear == param.IdAcademicYear && getStudent.Select(e => e.IdStudent).ToList().Contains(x.IdStudent))
              .Select(x => new
              {
                  IdStudent = x.IdStudent,
                  Status = x.StatusOverall.GetDescription(),
              }).ToListAsync(CancellationToken);

            var query = getStudent
            .Select(x => new
            {
                IdStudent = x.IdStudent,
                StudentName = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                Grade = x.Grade,
                Status = GetStatus.Where(e => e.IdStudent == x.IdStudent).SingleOrDefault() == null
                        ? ""
                        : GetStatus.Where(e => e.IdStudent == x.IdStudent).SingleOrDefault().Status,
            });

            if (param.Status != null)
                query = query.Where(e => e.Status == param.Status?.GetDescription());

            //ordering
            switch (param.OrderBy)
            {
                case "StudentName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StudentName)
                        : query.OrderBy(x => x.StudentName);
                    break;
                case "Grade":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Grade)
                        : query.OrderBy(x => x.Grade);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
            };

            IReadOnlyList<GetListStudentBySupervisorResult> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(x => new GetListStudentBySupervisorResult
                {
                    IdStudent = x.IdStudent,
                    StudentName = x.StudentName,
                    Grade = x.Grade,
                    OverallStatus = x.Status,
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetListStudentBySupervisorResult
                {
                    IdStudent = x.IdStudent,
                    StudentName = x.StudentName,
                    Grade = x.Grade,
                    OverallStatus = x.Status,
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.IdStudent).Count();


            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
