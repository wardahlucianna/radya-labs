using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MasterPortfolio.Validator;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Student.FnStudent.MasterPortfolio
{
    public class GetListMasterPortfolioHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListMasterPortfolioHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMasterPortfolioRequest>();
            string[] columns = { "Name", "TypeName" };



            var query = _dbContext.Entity<MsLearnerProfile>()
                .Include(x => x.LearningGoalStudent)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    TypeName = x.Type == LearnerProfile.IbLearner ? "IB Learner Profile" : "Approaches to Learning",
                    IsUsed = x.LearningGoalStudent != null ? true : false
                })
                .AsQueryable();

            if(param.Type != null)
            {
                query = query.Where(x => x.Type == param.Type);
            }

            if (!string.IsNullOrEmpty(param.Search))
            {
                query = query.Where(x => EF.Functions.Like(x.Name.ToLower(), param.SearchPattern().ToLower()) || EF.Functions.Like(x.TypeName.ToLower(), param.SearchPattern().ToLower()));
            }

            //ordering
            switch (param.OrderBy)
            {
                case "Name":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.Name)
                        : query.OrderBy(x => x.Name);
                    break;
                case "TypeName":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.TypeName)
                        : query.OrderBy(x => x.TypeName);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;

            var result = await query.ToListAsync(CancellationToken);
            
            if (param.Return == CollectionType.Lov)
                items = result
                    .Select(x => new GetMasterPortfolioResult()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,
                        TypeName = x.Type == LearnerProfile.IbLearner ? "IB Learner Profile" : "Approaches to Learning",
                        CanEdit = x.IsUsed,
                        CanDelete = x.IsUsed,
                        Code = x.Id,
                        Description = x.Name
                    })
                    .ToList();

                
            else
                items = result
                    .SetPagination(param)
                    .Select(x => new GetMasterPortfolioResult
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,
                        TypeName = x.Type == LearnerProfile.IbLearner ? "IB Learner Profile" : "Approaches to Learning",
                        CanEdit = x.IsUsed,
                        CanDelete = x.IsUsed,
                        Code = x.Id,
                        Description = x.Name
                    })
                    .ToList();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
