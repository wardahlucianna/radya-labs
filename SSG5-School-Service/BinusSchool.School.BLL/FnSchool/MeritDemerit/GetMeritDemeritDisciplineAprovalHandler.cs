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
    public class GetMeritDemeritDisciplineAprovalHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = { "AcademicYear", "Level", "Approval1", "Approval2", "Approval3" };
        private readonly ISchoolDbContext _dbContext;

        public GetMeritDemeritDisciplineAprovalHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMeritDemeritDisciplineAprovalRequest>();
            var predicate = PredicateBuilder.Create<MsLevel>(x => x.AcademicYear.IdSchool == param.IdSchool);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Id == param.IdLevel);

            IReadOnlyList<IItemValueVm> items;
            var result = await _dbContext.Entity<MsLevel>()
              .Where(predicate)
             .Select(x => new
             {
                 IdAcademicYear = x.IdAcademicYear,
                 AcademicYear = x.AcademicYear.Description,
                 Level = x.Description,
                 IdLevel = x.Id,
                 Approval1 = _dbContext.Entity<MsMeritDemeritApprovalSetting>().Any(e=>e.Level.IdAcademicYear==x.IdAcademicYear && e.Level.Id==x.Id)?_dbContext.Entity<MsMeritDemeritApprovalSetting>().Where(e => e.Level.IdAcademicYear == x.IdAcademicYear && e.Level.Id == x.Id).Select(e=>new CodeWithIdVm { Id=e.PositionApproval1.Id, Code = e.PositionApproval1 .Code, Description = e.PositionApproval1.Description}).SingleOrDefault() : null,
                 Approval2 = _dbContext.Entity<MsMeritDemeritApprovalSetting>().Any(e => e.Level.IdAcademicYear == x.IdAcademicYear && e.Level.Id == x.Id) ? _dbContext.Entity<MsMeritDemeritApprovalSetting>().Where(e => e.Level.IdAcademicYear == x.IdAcademicYear && e.Level.Id == x.Id).Select(e => new CodeWithIdVm { Id = e.PositionApproval2.Id, Code = e.PositionApproval2.Code, Description = e.PositionApproval2.Description }).SingleOrDefault() : null,
                 Approval3 = _dbContext.Entity<MsMeritDemeritApprovalSetting>().Any(e => e.Level.IdAcademicYear == x.IdAcademicYear && e.Level.Id == x.Id) ? _dbContext.Entity<MsMeritDemeritApprovalSetting>().Where(e => e.Level.IdAcademicYear == x.IdAcademicYear && e.Level.Id == x.Id).Select(e => new CodeWithIdVm { Id = e.PositionApproval3.Id, Code = e.PositionApproval3.Code, Description = e.PositionApproval3.Description }).SingleOrDefault() : null,
             })
             .OrderBy(x => x.IdAcademicYear).ToListAsync(CancellationToken);

            items = result.Select(x => new GetMeritDemeritDisciplineAprovalResult
            {
                AcademicYear = x.AcademicYear,
                IdAcademicYear = x.IdAcademicYear,
                Level = x.Level,
                IdLevel = x.IdLevel,
                Approval1 = x.Approval1,
                Approval2 = x.Approval2,
                Approval3 = x.Approval3,
            }).ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
