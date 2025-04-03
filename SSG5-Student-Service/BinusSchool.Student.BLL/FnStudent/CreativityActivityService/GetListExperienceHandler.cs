using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetListExperienceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListExperienceHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListExperienceRequest>();

            var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                   join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                   join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                   where a.Id == param.IdUser
                                    
                                  select new LtRole
                                  {
                                      IdRoleGroup = rg.IdRoleGroup
                                  }).FirstOrDefaultAsync(CancellationToken);

            if(CheckRole == null)
                throw new BadRequestException($"User in this role not found");

            var predicate = PredicateBuilder.Create<TrExperience>(x => x.HomeroomStudent.IdStudent==param.IdStudent && param.IdAcademicYear.Contains(x.IdAcademicYear));

            if(CheckRole.IdRoleGroup == "PRT")
            {
                predicate = predicate.And(x => x.Status== ExperienceStatus.Approved || x.Status == ExperienceStatus.Completed);
            }
            else if (CheckRole.IdRoleGroup != "PRT" && CheckRole.IdRoleGroup != "STD")
            {
                var queryExperience = _dbContext.Entity<TrExperience>()
                                 .Include(e=>e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel)
                                 .Where(e => param.IdAcademicYear.Contains(e.IdAcademicYear));

                var getAyByCAS = await _dbContext.Entity<TrCasAdvisorStudent>()
                              .Include(e => e.HomeroomStudent)
                              .Include(e => e.CasAdvisor)
                              .Where(e => param.IdAcademicYear.Contains(e.CasAdvisor.IdAcademicYear) && e.HomeroomStudent.IdStudent == param.IdStudent)
                              .ToListAsync(CancellationToken);

                if (param.ViewAs.ToLower() == "supervisor" || param.ViewAs.ToLower() == "advisor")
                {
                    getAyByCAS = getAyByCAS.Where(x => x.CasAdvisor.IdUserCAS == param.IdUser).ToList();
                }

                var getIdAyByCas = getAyByCAS.Select(x => x.CasAdvisor.IdAcademicYear).ToList();

                //var getIdAyByCas = await _dbContext.Entity<TrCasAdvisorStudent>()
                //              .Include(e => e.HomeroomStudent)
                //              .Include(e => e.CasAdvisor)
                //              .Where(e => e.CasAdvisor.IdUserCAS == param.IdUser && param.IdAcademicYear.Contains(e.CasAdvisor.IdAcademicYear) && e.HomeroomStudent.IdStudent==param.IdStudent)
                //              .Select(e => e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear)
                //              .ToListAsync(CancellationToken);

                //var listIdExperience = await queryExperience
                //                        .Where(e => getIdAyByCas.Contains(e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear) && e.HomeroomStudent.IdStudent==param.IdStudent)
                //                        .Select(e=>e.Id)
                //                        .Distinct().ToListAsync(CancellationToken);

                var listExperience = await queryExperience
                                        .Where(e => getIdAyByCas.Contains(e.HomeroomStudent.Homeroom.Grade.MsLevel.IdAcademicYear) && e.HomeroomStudent.IdStudent == param.IdStudent)
                                        .Distinct().ToListAsync(CancellationToken);

                //var listIdExperienceByExperience = await queryExperience
                //           .Where(e => e.IdUserSupervisor == param.IdUser && e.HomeroomStudent.IdStudent == param.IdStudent)
                //           .Select(e => e.Id)
                //           .Distinct().ToListAsync(CancellationToken);

                var listExperienceByExperience = await queryExperience
                           .Where(e => e.HomeroomStudent.IdStudent == param.IdStudent).ToListAsync(CancellationToken);

                if (!string.IsNullOrEmpty(param.ViewAs) && param.ViewAs.ToLower() == "supervisor")
                {
                    listExperience = listExperience.Where(x => x.IdUserSupervisor == param.IdUser && (x.Status == ExperienceStatus.Approved || x.Status == ExperienceStatus.Completed)).ToList();
                    listExperienceByExperience = listExperienceByExperience.Where(x => x.IdUserSupervisor == param.IdUser && (x.Status == ExperienceStatus.Approved || x.Status == ExperienceStatus.Completed)).ToList();
                }
                //else if (!string.IsNullOrEmpty(param.ViewAs) && param.ViewAs.ToLower() == "advisor")
                //{
                //    listExperience = listExperience.Where(x => x.IdUserSupervisor == param.IdUser).ToList();
                //    listExperienceByExperience = listExperienceByExperience.Where(x => x.IdUserSupervisor == param.IdUser).ToList();
                //}


                var listIdExperience = listExperience.Select(x => x.Id).ToList();

                var listIdExperienceByExperience = listExperienceByExperience.Select(x => x.Id).ToList();

                listIdExperience.AddRange(listIdExperienceByExperience.Distinct());

                predicate = predicate.And(x => listIdExperience.Contains(x.Id));
            }

            string[] _columns = { "AcademicYear", "ExperienceName", "StartDate", "EndDate", "Location", "Status"};

            var query = _dbContext.Entity<TrExperience>()
               .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel).ThenInclude(e=>e.MsAcademicYear)
               .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Student)
              .Where(predicate)
              .Select(x => new
              {
                  Id = x.Id,
                  AcademicYear = x.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                  ExperienceName = x.ExperienceName,
                  StartDate = x.StartDate.Date,
                  EndDate = x.EndDate.Date,
                  Location = x.ExperienceLocation,
                  Status = x.Status,
                  CanEdit = CheckRole.IdRoleGroup == "STD" ? (x.Status == ExperienceStatus.NeedRevision || x.Status == ExperienceStatus.ToBeDetermined) : CheckRole.IdRoleGroup != "PRT" && x.Status != ExperienceStatus.Completed && param.ViewAs != "supervisor" ? true : false,
                  CanDelete = (x.Status == ExperienceStatus.NeedRevision || x.Status == ExperienceStatus.ToBeDetermined) && (CheckRole.IdRoleGroup != "PRT"),
                  FirstName = x.HomeroomStudent.Student.FirstName,
                  MiddleName = x.HomeroomStudent.Student.MiddleName,
                  LastName = x.HomeroomStudent.Student.LastName,
              });

            //ordering
            switch (param.OrderBy)
            {
                case "AcademicYear":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.AcademicYear)
                        : query.OrderBy(x => x.AcademicYear);
                    break;
                case "ExperienceName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.ExperienceName)
                        : query.OrderBy(x => x.ExperienceName);
                    break;
                case "StartDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.StartDate)
                        : query.OrderBy(x => x.StartDate);
                    break;
                case "EndDate":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.EndDate)
                        : query.OrderBy(x => x.EndDate);
                    break;
                case "Location":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Location)
                        : query.OrderBy(x => x.Location);
                    break;
                case "Status":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Status)
                        : query.OrderBy(x => x.Status);
                    break;
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .ToListAsync(CancellationToken);


                items = result.Select(x => new GetListExperienceResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    ExperienceName = x.ExperienceName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Location = x.Location.GetDescription(),
                    Status = x.Status.GetDescription(),
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    StudentName = NameUtil.GenerateFullNameWithId(x.FirstName,x.MiddleName,x.LastName)
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetListExperienceResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    ExperienceName = x.ExperienceName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Location = x.Location.GetDescription(),
                    Status = x.Status.GetDescription(),
                    CanEdit = x.CanEdit,
                    CanDelete = x.CanDelete,
                    StudentName = NameUtil.GenerateFullNameWithId(x.FirstName, x.MiddleName, x.LastName)
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
