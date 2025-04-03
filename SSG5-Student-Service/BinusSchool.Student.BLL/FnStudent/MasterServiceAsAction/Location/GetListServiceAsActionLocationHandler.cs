using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MasterServiceAsAction.Location
{
    public class GetListServiceAsActionLocationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListServiceAsActionLocationHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetListServiceAsActionLocationRequest>(
                    nameof(GetListServiceAsActionLocationRequest.IdAcademicYear)
                );

            var query = await _dbContext.Entity<LtServiceAsActionLocDesc>()
                .OrderBy(x => x.SALocDes)
                .Select(x => new
                {
                    Id = x.Id,
                    Description = x.SALocDes
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            var result = query.Select(x => new ItemValueVm
            {
                Id = x.Id,
                Description = x.Description
            }); 

            return Request.CreateApiResult2(result as object);
        }
    }
}
