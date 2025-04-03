using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritComponentSettingHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetMeritDemeritComponentSettingRequest.Idschool),
        };
        private static readonly string[] _columns = { "AcademicYear", "Level", "Grade", "PintSystem", "DemeritSystem"};
        private readonly ISchoolDbContext _dbContext;

        public GetMeritDemeritComponentSettingHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMeritDemeritComponentSettingRequest>(_requiredParams);
            var predicate = PredicateBuilder.Create<MsGrade>(x => x.Level.AcademicYear.IdSchool == param.Idschool);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.Level.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Id == param.IdGrade);

            IReadOnlyList<IItemValueVm> items;
            var result = await _dbContext.Entity<MsGrade>()
              .Where(predicate)
             .Select(x => new
             {
                 IdAcademicYear = x.Level.IdAcademicYear,
                 AcademicYear = x.Level.AcademicYear.Description,
                 Level = x.Level.Description,
                 Grade = x.Description,
                 IdGrade = x.Id,
                 PointSystem = _dbContext.Entity<MsMeritDemeritComponentSetting>().SingleOrDefault(e=>e.IdGrade==x.Id)==null?true: _dbContext.Entity<MsMeritDemeritComponentSetting>().SingleOrDefault(e => e.IdGrade == x.Id).IsUsePointSystem,
                 DemeritSystem = _dbContext.Entity<MsMeritDemeritComponentSetting>().SingleOrDefault(e=>e.IdGrade==x.Id)==null? true : _dbContext.Entity<MsMeritDemeritComponentSetting>().SingleOrDefault(e => e.IdGrade == x.Id).IsUseDemeritSystem,
                 IdMeritDemeritCompSetting = x.Id,
             })
             .OrderBy(x => x.IdAcademicYear).ToListAsync(CancellationToken);

            items = result.Select(x => new GetMeritDemeritComponentSettingResult
            {
                AcademicYear = x.AcademicYear,
                IdAcademicYear = x.IdAcademicYear,
                Level = x.Level,
                Grade = x.Grade,
                IdGrade = x.IdGrade,
                IsPointSystem = x.PointSystem,
                IsDemeritSystem = x.DemeritSystem,
                IdMeritDemeritCompSetting = x.IdMeritDemeritCompSetting,
            }).ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
