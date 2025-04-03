using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction.Status;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MasterServiceAsAction.Status
{
    public class GetListServiceAsActionStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetListServiceAsActionStatusHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListServiceAsActionStatusRequest>(nameof(GetListServiceAsActionStatusRequest.IdSchool));

            var result = await _dbContext.Entity<MsServiceAsActionStatus>()
                .Select(x => new GetListServiceAsActionStatusResult
                {
                    Id = x.Id,
                    Description = x.StatusDesc,
                    IsDetail = x.IsDetail == null ? false : x.IsDetail,
                    IsOverall = x.IsOverall == null ? false : x.IsOverall
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }
    }
}
