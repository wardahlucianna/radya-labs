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
    public class GetMeritDemeritDisciplineMappingCheckPointHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetMeritDemeritDisciplineMappingCheckPointHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMeritDemeritComponentSettingRequest>();
            var predicate = PredicateBuilder.Create<MsMeritDemeritComponentSetting>(x => x.Grade.Level.AcademicYear.IdSchool == param.Idschool && x.IdGrade == param.IdGrade);

            GetMeritDemeritDisciplineMappingCheckPointResult items = new GetMeritDemeritDisciplineMappingCheckPointResult
            {
                IsShowPoint = _dbContext.Entity<MsMeritDemeritComponentSetting>()
                 .SingleOrDefault(predicate)==null?true: _dbContext.Entity<MsMeritDemeritComponentSetting>()
                 .SingleOrDefault(predicate).IsUsePointSystem,
                IsShowDemerit = _dbContext.Entity<MsMeritDemeritComponentSetting>()
                 .SingleOrDefault(predicate) == null ? true : _dbContext.Entity<MsMeritDemeritComponentSetting>()
                 .SingleOrDefault(predicate).IsUseDemeritSystem,
            };

            return Request.CreateApiResult2(items as object);
        }
    }
}
