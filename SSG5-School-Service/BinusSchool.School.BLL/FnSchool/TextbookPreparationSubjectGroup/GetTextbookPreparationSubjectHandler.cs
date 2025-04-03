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
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationSubjectGroup;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.TextbookPreparationSubjectGroup
{
    public class GetTextbookPreparationSubjectHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetTextbookPreparationSubjectHandler(ISchoolDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetTextbookPreparationSubjectRequest, GetTextbookPreparationSubjectValidator>();

            var predicate = PredicateBuilder.Create<MsSubject>(x => x.Grade.Level.IdAcademicYear==body.IdAcademicYear);

            if (!string.IsNullOrEmpty(body.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel== body.IdLevel);

            if (body.IdGrade.Count>0)
                predicate = predicate.And(x => body.IdGrade.Contains(x.IdGrade));

            if (!string.IsNullOrEmpty(body.Search))
                predicate = predicate.And(x => x.Description.Contains(body.Search));

            var query = _dbContext.Entity<MsSubject>()
               .Include(e => e.Grade).ThenInclude(e=>e.Level)
               .Where(predicate)
              .Select(x => new
              {
                  Id = x.Id,
                  IdLevel = x.Grade.IdLevel,
                  Level = x.Grade.Level.Code,
                  IdGrade = x.IdGrade,
                  Grade = x.Grade.Description,
                  Subject = x.Description,
              });

            IReadOnlyList<IItemValueVm> items;
            if (body.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetTextbookPreparationSubjectResult
                    {
                        Id = x.Id,
                        Level = x.Level,
                        IdLevel = x.IdLevel,
                        Grade = x.Grade,
                        IdGrade = x.IdGrade,
                        Subject = x.Subject,
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(body)
                   .Select(x => new GetTextbookPreparationSubjectResult
                   {
                       Id = x.Id,
                       Level = x.Level,
                       IdLevel = x.IdLevel,
                       Grade = x.Grade,
                       IdGrade = x.IdGrade,
                       Subject = x.Subject,
                   })
                    .ToListAsync(CancellationToken);

            var count = body.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, body.CreatePaginationProperty(count));
        }
    }
}
