using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Student.BLL.FnStudent.MasterServiceAsAction.Sdg
{
    public class GetListServiceAsActionSdgHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListServiceAsActionSdgHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var query = await _dbContext.Entity<LtServiceAsActionSdgs>()
                .OrderBy(x => x.SdgsDesc)
                .Select(x => new
                {
                    Id = x.IdServiceAsActionSdgs,
                    Description = x.SdgsDesc
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            if(query == null)
            {
                return Request.CreateApiResult2(null as object);
            }

            var result = query.Select(x => new ItemValueVm
            {
                Id = x.Id,
                Description = x.Description
            });

            return Request.CreateApiResult2(result as object);
        }
    }
}
