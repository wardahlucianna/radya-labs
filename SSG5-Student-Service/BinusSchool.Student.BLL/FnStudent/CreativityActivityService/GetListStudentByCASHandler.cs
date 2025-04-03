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
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetListStudentByCASHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListStudentByCASHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListStudentByCASRequest>();

            var getIdHomeroomStudent = new List<string>();

            if (param.ViewAs.ToLower() == "advisor")
            {
                var getIdHomeroomStudentByCas = await _dbContext.Entity<TrCasAdvisorStudent>()
                              .Include(e => e.HomeroomStudent)
                              .Include(e => e.CasAdvisor)
                              .Where(e => e.CasAdvisor.IdUserCAS == param.IdUser && e.CasAdvisor.IdAcademicYear == param.IdAcademicYear)
                              .Select(e => e.HomeroomStudent.Id)
                              .ToListAsync(CancellationToken);

                getIdHomeroomStudent = getIdHomeroomStudentByCas.Distinct().ToList();
            }
            else if (param.ViewAs.ToLower() == "supervisor")
            {
                var getIdHomeroomStudentByExperience = await _dbContext.Entity<TrExperience>()
                             .Where(e => e.IdUserSupervisor == param.IdUser && e.IdAcademicYear == param.IdAcademicYear)
                             .Select(e => e.HomeroomStudent.Id)
                             .ToListAsync(CancellationToken);

                getIdHomeroomStudent = getIdHomeroomStudentByExperience.Distinct().ToList();
            }
            else
            {
                var getIdHomeroomStudentByCas = await _dbContext.Entity<TrCasAdvisorStudent>()
                              .Include(e => e.HomeroomStudent)
                              .Include(e => e.CasAdvisor)
                              .Where(e => e.CasAdvisor.IdAcademicYear == param.IdAcademicYear)
                              .Select(e => e.HomeroomStudent.Id)
                              .ToListAsync(CancellationToken);

                var getIdHomeroomStudentByExperience = await _dbContext.Entity<TrExperience>()
                                 .Where(e => e.IdAcademicYear == param.IdAcademicYear)
                                 .Select(e => e.HomeroomStudent.Id)
                                 .ToListAsync(CancellationToken);

                getIdHomeroomStudent = getIdHomeroomStudentByCas.Distinct().Union(getIdHomeroomStudentByExperience).Distinct().ToList();
            }

            //getIdHomeroomStudent = getIdHomeroomStudentByCas.Distinct().ToList();//.Union(getIdHomeroomStudentByExperience).Distinct().ToList();

            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => getIdHomeroomStudent.Contains(x.Id)
                                                                            && (x.Homeroom.Grade.Code.Contains("11") || x.Homeroom.Grade.Code.Contains("12"))
                                                                            && x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);
            string[] _columns = { "StudentName", "Grade", "OverallStatus"};

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(e => e.Homeroom.IdGrade == param.IdGrade);

            var getStudent = await _dbContext.Entity<MsHomeroomStudent>()
              .Include(e => e.Student)
              .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
              .Where(predicate)
              .Select(x => new
              {
                  IdStudent = x.IdStudent,
                  FirstName = x.Student.FirstName,
                  LastName = x.Student.LastName,
                  MiddleName = x.Student.MiddleName,
                  Grade = x.Homeroom.Grade.Description,
              }).Distinct().ToListAsync(CancellationToken);

            var GetStatus = await _dbContext.Entity<TrExperienceStudent>()
              .Where(x=>x.IdAcademicYear == param.IdAcademicYear && getStudent.Select(e=>e.IdStudent).ToList().Contains(x.IdStudent))
              .Select(x => new
              {
                  IdStudent = x.IdStudent,
                  Status = x.StatusOverall,
              }).ToListAsync(CancellationToken);

            var query = getStudent
            .Select(x => new
            {
                IdStudent = x.IdStudent,
                StudentName = NameUtil.GenerateFullName(x.FirstName,x.MiddleName,x.LastName),
                Grade = x.Grade,
                Status = GetStatus.Where(e=>e.IdStudent==x.IdStudent).FirstOrDefault()==null
                        ? StatusOverallExperienceStudent.OnTrack
                        : GetStatus.Where(e => e.IdStudent == x.IdStudent).FirstOrDefault().Status,
            });


            if (!string.IsNullOrEmpty(param.StatusOverallExperienceStudent.ToString()))
                query = query.Where(e => e.Status == param.StatusOverallExperienceStudent);

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
                        ? query.OrderByDescending(x => x.Status.GetDescription())
                        : query.OrderBy(x => x.Status.GetDescription());
                    break;
            };

            IReadOnlyList<GetListStudentByCASResult> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = query
                    .ToList();


                items = result.Select(x => new GetListStudentByCASResult
                {
                    IdStudent = x.IdStudent,
                    StudentName = x.StudentName,
                    Grade = x.Grade,
                    OverallStatus = x.Status.GetDescription(),
                }).ToList();
            }
            else
            {
                var result = query
                    .SetPagination(param)
                    .ToList();

                items = result.Select(x => new GetListStudentByCASResult
                {
                    IdStudent = x.IdStudent,
                    StudentName = x.StudentName,
                    Grade = x.Grade,
                    OverallStatus = x.Status.GetDescription(),
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : query.Select(x => x.IdStudent).Count();


            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
