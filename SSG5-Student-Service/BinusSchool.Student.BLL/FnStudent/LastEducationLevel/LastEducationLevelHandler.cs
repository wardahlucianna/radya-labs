using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.LastEducationLevel
{
    public class LastEducationLevelHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;       
        public LastEducationLevelHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var query = _dbContext.Entity<LtLastEducationLevel>();
            IReadOnlyList<IItemValueVm> items;
            items = await query
                    .Select(x => new ItemValueVm { 
                        Id = x.IdLastEducationLevel.ToString(), 
                        Description = x.LastEducationLevelName
                    })
                    .ToListAsync(CancellationToken);
                  
            return Request.CreateApiResult2(items);
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new System.NotImplementedException();
        }
    }
}
