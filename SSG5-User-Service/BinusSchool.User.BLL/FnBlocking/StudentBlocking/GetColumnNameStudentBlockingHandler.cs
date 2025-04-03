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
using static BinusSchool.Data.Model.User.FnBlocking.StudentBlocking.GetColumnNameStudentBlockingResult;

namespace BinusSchool.User.FnBlocking.StudentBlocking
{
    public class GetColumnNameStudentBlockingHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetColumnNameStudentBlockingHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetColumnNameStudentBlockingRequest>();

            var dataBlocking1 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category != "FEATURE" && x.BlockingCategory.IdSchool==param.IdSchool)
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Order)
                .ToListAsync(CancellationToken);
            var dataBlocking2 = await _dbContext.Entity<MsBlockingCategoryType>()
                .Include(x => x.BlockingCategory).ThenInclude(x => x.UserBlockings)
                .Include(x => x.BlockingCategory).ThenInclude(x => x.StudentBlockings)
                .Include(x => x.BlockingType)
                .Where(x => x.BlockingType.Category == "FEATURE" && x.BlockingCategory.IdSchool == param.IdSchool)
                .OrderBy(x => x.BlockingCategory.Name).ThenBy(x => x.BlockingType.Name )
                .ToListAsync(CancellationToken);

            var data = dataBlocking1.Concat(dataBlocking2).OrderBy(x => x.BlockingCategory.Name).Select(y => new GetColumnsNameQueryResult
            {
                BlockingCategory = y.BlockingCategory.Name,
                BlockingType = y.BlockingType.Name,
            }).ToList();

            var columnBlokcingCategory = data.GetRange(0, data.Count)
                              .Select(x => new
                              {
                                  ColumnName = $"{x.BlockingCategory}"
                              })
                              .ToList();

            var columnBlockingType = data.GetRange(0, data.Count)
                  .Select(x => new
                  {
                      ColumnName = $"{x.BlockingType}"
                  })
                  .ToList();

            List<string> _columns = new List<string>();

            _columns.Add("StudentName");
            _columns.Add("StudentId");
            _columns.Add("Class/HomeRoom");
            _columns.AddRange(columnBlokcingCategory.Select(c => c.ColumnName));
            List<string> _columns2 = new List<string>();
            _columns2.Add("StudentName");
            _columns2.Add("StudentId");
            _columns2.Add("Class/HomeRoom");
            _columns2.AddRange(columnBlockingType.Select(c => c.ColumnName));
            var columnResult = new GetColumnNameStudentBlockingResult
            {
                Columns1 = _columns,
                Columns2 = _columns2
            };

            return Request.CreateApiResult2(columnResult as object);
        }
    }
}
