using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction.Type;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MasterServiceAsAction.Type
{
    public class GetListServiceAsActionTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListServiceAsActionTypeHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListExperienceTypeRequest>(
                    nameof(GetListExperienceTypeRequest.IdAcademicYear)
                );

            var query = await _dbContext.Entity<MsServiceAsActionType>()
                .OrderBy(x => x.TypeDesc)
                .Select(x => new
                {
                    Id = x.Id,
                    Description = x.TypeDesc
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
