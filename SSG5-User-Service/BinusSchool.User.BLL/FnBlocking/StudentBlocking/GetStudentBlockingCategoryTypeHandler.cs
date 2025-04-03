using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class GetStudentBlockingCategoryTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetStudentBlockingCategoryTypeHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentBlockingCategoryTypeRequest>(
                            nameof(GetStudentBlockingCategoryTypeRequest.IdStudent));

            var blockingDetails = await _dbContext.Entity<MsStudentBlocking>()
                                    .Include(x => x.BlockingCategory)
                                    .Include(x => x.BlockingType)
                                    .Where(x => x.IdStudent == param.IdStudent &&
                                                x.IsBlocked == true)
                                    .Select(x => new GetStudentBlockingCategoryTypeResult_Detail
                                    {
                                        BlockingType = new ItemValueVm
                                        {
                                            Id = x.BlockingType.Id,
                                            Description = x.BlockingType.Name
                                        },
                                        BlockingCategory = new ItemValueVm
                                        {
                                            Id = x.BlockingCategory.Id,
                                            Description = x.BlockingCategory.Name
                                        }
                                    })
                                    .ToListAsync(CancellationToken);

            var result = new GetStudentBlockingCategoryTypeResult
            {
                IsBlocked = (blockingDetails != null && blockingDetails.Any()) ? true : false,
                Details = blockingDetails
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
