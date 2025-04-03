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
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod
{
    public class GetImmersionPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetImmersionPeriodHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetImmersionPeriodRequest>(
                            nameof(GetImmersionPeriodRequest.IdAcademicYear),
                            nameof(GetImmersionPeriodRequest.Semester));

            var columns = new[] { "academicYear", "semester", "name", "registrationStartDate", "registrationEndDate" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "AcademicYear.Id" },
                { columns[1], "Semester" },
                { columns[2], "Name" },
                { columns[3], "RegistrationStartDate" },
                { columns[4], "RegistrationEndDate" }
            };

            var predicate = PredicateBuilder.True<MsImmersionPeriod>();
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                            EF.Functions.Like(x.Name, param.SearchPattern()));

            var query = _dbContext.Entity<MsImmersionPeriod>()
                                    .Include(ip => ip.AcademicYear)
                                    .Where(x => x.IdAcademicYear == param.IdAcademicYear &&
                                                x.Semester == param.Semester
                                                //(string.IsNullOrEmpty(param.IdImmersionPeriod) ? true : x.Id == param.IdImmersionPeriod)
                                                )
                                    .Where(predicate)
                                    //.SearchByDynamic(param)
                                    .Select(x => new GetImmersionPeriodResult
                                    {
                                        Id = x.Id,
                                        AcademicYear = new CodeWithIdVm
                                        {
                                            Id = x.IdAcademicYear,
                                            Code = x.AcademicYear.Code,
                                            Description = x.AcademicYear.Description
                                        },
                                        Semester = x.Semester,
                                        Name = x.Name,
                                        RegistrationStartDate = x.RegistrationStartDate,
                                        RegistrationEndDate = x.RegistrationEndDate
                                    })
                                    .OrderBy(x => x.AcademicYear.Id)
                                    .ThenBy(x => x.Semester)
                                    .ThenBy(x => x.Name)
                                    .Distinct()
                                    .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> resultItems;
            if (param.Return == CollectionType.Lov)
                resultItems = await query
                                    .Select(x => new ItemValueVm()
                                    {
                                        Id = x.Id,
                                        Description = x.Name
                                    })
                                    .ToListAsync(CancellationToken);
            else
                resultItems = query
                                .Select(x => new GetImmersionPeriodResult
                                {
                                    Id = x.Id,
                                    AcademicYear = new CodeWithIdVm
                                    {
                                        Id = x.AcademicYear.Id,
                                        Code = x.AcademicYear.Code,
                                        Description = x.AcademicYear.Description
                                    },
                                    Semester = x.Semester,
                                    Name = x.Name,
                                    RegistrationStartDate = x.RegistrationStartDate,
                                    RegistrationEndDate = x.RegistrationEndDate
                                })
                                .SetPagination(param)
                                .ToList();

            var count = param.CanCountWithoutFetchDb(query.Count())
                ? query.Count()
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(resultItems as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
