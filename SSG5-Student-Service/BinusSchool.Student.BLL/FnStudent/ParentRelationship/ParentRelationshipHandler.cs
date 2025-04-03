using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.ParentRelationship;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model;

namespace BinusSchool.Student.FnStudent.ParentRelationship
{
    public class ParentRelationshipHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public ParentRelationshipHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CollectionRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var predicate = PredicateBuilder.Create<LtParentRelationship>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdParentRelationship.ToString(), $"%{param.Search}%")
                    || EF.Functions.Like(x.RelationshipNameEng, $"%{param.Search}%"));

            var query = _dbContext.Entity<LtParentRelationship>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.IdParentRelationship.ToString(),
                        Description = x.RelationshipNameEng
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new ItemValueVm
                    {
                        Id = x.IdParentRelationship.ToString(),
                        Description = x.RelationshipNameEng
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdParentRelationship).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
