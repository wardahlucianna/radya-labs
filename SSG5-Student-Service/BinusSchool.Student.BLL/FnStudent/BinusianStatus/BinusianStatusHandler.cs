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

namespace BinusSchool.Student.FnStudent.BinusianStatus
{
    public class BinusianStatusHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;       
        public BinusianStatusHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {           
            var query = await _dbContext.Entity<LtBinusianStatus>()
                        .Where(x => x.BinusianStatus.ToString() == id)
                        .Select(x => new ItemValueVm
                        {
                            Id = x.BinusianStatus.ToString(),
                            Description = x.BinusianStatusName
                        })
                        .ToListAsync(CancellationToken);
                        
            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {           
            var query = _dbContext.Entity<LtBinusianStatus>();
            IReadOnlyList<IItemValueVm> items;
            items = await query
                    .Select(x => new ItemValueVm { 
                        Id = x.BinusianStatus.ToString(), 
                        Description = x.BinusianStatusName
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
