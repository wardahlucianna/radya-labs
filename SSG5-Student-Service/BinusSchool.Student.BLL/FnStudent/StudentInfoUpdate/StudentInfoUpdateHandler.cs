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
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BinusSchool.Student.FnStudent.StudentInfoUpdate
{
    public class StudentInfoUpdateHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly int _newEntryApproval;
        public StudentInfoUpdateHandler(IStudentDbContext studentDbContext, IConfiguration configuration)
        {
            _dbContext = studentDbContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentInfoUpdateRequest>(nameof(GetStudentInfoUpdateRequest.IsParentUpdate), nameof(GetStudentInfoUpdateRequest.IdApprovalStatus));
            var predicate = PredicateBuilder.Create<TrStudentInfoUpdate>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.IdUser, $"%{param.Search}%"));

            var query = _dbContext.Entity<TrStudentInfoUpdate>()
                .Where(predicate)
                .Where(x => x.IsParentUpdate == param.IsParentUpdate && x.IdApprovalStatus == _newEntryApproval)
                .OrderByDynamic(param);

            IReadOnlyList<GetStudentInfoUpdateResult> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetStudentInfoUpdateResult
                    {
                        IdUser = x.IdUser,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value,
                        IsParentUpdate = x.IsParentUpdate,
                        RequestedBy = x.RequestedBy
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetStudentInfoUpdateResult
                    {
                        IdUser = x.IdUser,
                        TableName = x.TableName,
                        FieldName = x.FieldName,
                        OldFieldValue = x.OldFieldValue,
                        CurrentFieldValue = x.CurrentFieldValue,
                        Constraint1 = x.Constraint1,
                        Constraint1Value = x.Constraint1Value,
                        Constraint2 = x.Constraint2,
                        Constraint2Value = x.Constraint2Value,
                        Constraint3 = x.Constraint3,
                        Constraint3Value = x.Constraint3Value,
                        IsParentUpdate = x.IsParentUpdate,
                        RequestedBy = x.RequestedBy
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdUser).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
