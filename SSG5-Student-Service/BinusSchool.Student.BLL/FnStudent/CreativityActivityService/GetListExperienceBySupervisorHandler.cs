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
    public class GetListExperienceBySupervisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListExperienceBySupervisorHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListExperienceBySupervisorRequest>();

            var predicate = PredicateBuilder.Create<TrExperience>(x => x.IdUserSupervisor == param.IdUser && x.HomeroomStudent.IdStudent==param.IdStudent && param.IdAcademicYear.Contains(x.IdAcademicYear) && (x.Status== ExperienceStatus.Approved|| x.Status == ExperienceStatus.Completed));
            string[] _columns = { "AcademicYear", "ExperienceName", "StartDate", "EndDate", "Location", "Status"};

            var query = _dbContext.Entity<TrExperience>()
               .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel).ThenInclude(e=>e.MsAcademicYear)
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


                items = result.Select(x => new GetListExperienceBySupervisorResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    ExperienceName = x.ExperienceName,
                    StartDate = x.StartDate.ToString("dd MMM yyyy"),
                    EndDate = x.EndDate.ToString("dd MMM yyyy"),
                    Location = x.Location.GetDescription(),
                    Status = x.Status.GetDescription(),
                }).ToList();
            }
            else
            {
                var result = await query
                    .SetPagination(param)
                    .ToListAsync(CancellationToken);

                items = result.Select(x => new GetListExperienceBySupervisorResult
                {
                    Id = x.Id,
                    AcademicYear = x.AcademicYear,
                    ExperienceName = x.ExperienceName,
                    StartDate = x.StartDate.ToString("dd MMM yyyy"),
                    EndDate = x.EndDate.ToString("dd MMM yyyy"),
                    Location = x.Location.GetDescription(),
                    Status = x.Status.GetDescription(),
                }).ToList();
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }
    }
}
