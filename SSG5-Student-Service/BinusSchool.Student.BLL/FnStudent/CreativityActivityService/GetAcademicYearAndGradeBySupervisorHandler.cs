using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class GetAcademicYearAndGradeBySupervisorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetAcademicYearAndGradeBySupervisorHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAcademicYearAndGradeBySupervisorRequest>();

            var GetExperience = await _dbContext.Entity<TrExperience>()
                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                        .Where(e=>e.IdUserSupervisor==param.IdUser)
                        .Select(e => new
                        {
                            IdAy = e.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Id,
                            Ay = e.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                            IdGrade = e.HomeroomStudent.Homeroom.Grade.Id,
                            Grade = e.HomeroomStudent.Homeroom.Grade.Description,
                        })
                        .Distinct().ToListAsync(CancellationToken);

            var GetCasAdvisor = await _dbContext.Entity<TrCasAdvisorStudent>()
                       .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                       .Where(e => e.CasAdvisor.IdUserCAS == param.IdUser)
                       .Select(e => new
                       {
                           IdAy = e.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Id,
                           Ay = e.HomeroomStudent.Homeroom.Grade.MsLevel.MsAcademicYear.Description,
                           IdGrade = e.HomeroomStudent.Homeroom.Grade.Id,
                           Grade = e.HomeroomStudent.Homeroom.Grade.Description,
                       })
                       .Distinct().ToListAsync(CancellationToken);

            var getCasAdvisorAndExperience = GetExperience.Union(GetCasAdvisor).ToList();

            var GetAy = getCasAdvisorAndExperience.Select(e => new
            {
                IdAy = e.IdAy,
                Ay = e.Ay
            })
            .Distinct().ToList();

            var Items = GetAy.Select(e => new GetAcademicYearAndGradeBySupervisorResult
            {
                Year = new ItemValueVm
                {
                    Id = e.IdAy,
                    Description  = e.Ay
                },
                Grade = getCasAdvisorAndExperience.Where(f=>f.IdAy==e.IdAy).Select(e => new ItemValueVm
                {
                    Id = e.IdGrade,
                    Description = e.Grade,
                }).Distinct().ToList(),
            }).ToList();


            return Request.CreateApiResult2(Items as object);
        }
    }
}
