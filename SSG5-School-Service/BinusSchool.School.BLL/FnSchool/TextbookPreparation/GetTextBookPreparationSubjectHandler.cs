using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.TextbookPreparation
{
    public class GetTextBookPreparationSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetTextBookPreparationSubjectHandler(ISchoolDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTextbookPreparationSubjectRequest>();

            var predicate = PredicateBuilder.Create<MsTextbookSubjectGroupDetail>(x => x.Subject.IdGrade==param.IdGrade && x.Subject.Grade.IdLevel==param.IdLevel && x.IdTextbookSubjectGroup==param.IdSubjectGroup);

            var GetSubject = await _dbContext.Entity<MsTextbookSubjectGroupDetail>()
                .Include(e => e.Subject)
                .Where(predicate)
                .Select (e=> new NameValueVm
                {
                    Id = e.IdSubject,
                    Name = e.Subject.Description
                })
               .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(GetSubject as object);
        }
    }
}
