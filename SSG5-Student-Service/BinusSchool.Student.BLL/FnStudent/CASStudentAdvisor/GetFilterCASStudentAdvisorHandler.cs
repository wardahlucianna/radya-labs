using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CASStudentAdvisor
{
    public class GetFilterCASStudentAdvisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetFilterCASStudentAdvisorHandler(
                IStudentDbContext DbContext
            )
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetFilterCASStudentAdvisorRequest>(
                nameof(GetFilterCASStudentAdvisorRequest.IdAcademicYear)
                );

            var listAdvisor = await _dbContext.Entity<TrCasAdvisor>()
                .Include(x => x.UserCAS)
                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                .ToListAsync(CancellationToken);

            if (!string.IsNullOrWhiteSpace(param.IdGrade))
            {
                var advisorByGrade = await _dbContext.Entity<TrCasAdvisorStudent>()
                                    .Include(x => x.CasAdvisor)
                                    .Where(x => x.HomeroomStudent.Homeroom.IdGrade == param.IdGrade)
                                    .ToListAsync();

                var FilteredAdvisor = advisorByGrade.Select(x => new
                {
                    IdUser = x.CasAdvisor.IdUserCAS,
                    Name = x.CasAdvisor.UserCAS.DisplayName
                }).Distinct().ToList();

                listAdvisor = listAdvisor.Where(x => FilteredAdvisor.Select(y => y.IdUser).ToList().Any(y => y == x.IdUserCAS)).ToList();
            }

            var result = new List<GetFilterCASStudentAdvisorResult>();

            foreach (var adv in listAdvisor)
            {
                var item = new GetFilterCASStudentAdvisorResult
                {
                    Id = adv.IdUserCAS,
                    Description = adv.UserCAS.DisplayName,
                    IdCasAdvisor = adv.Id
                };

                result.Add(item);
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
