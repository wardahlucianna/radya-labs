using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.StudentDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetGcAccessHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;
        public GetGcAccessHandler(IStudentDbContext studentDbContext, IRolePosition serviceRolePosition)
        {
            _dbContext = studentDbContext;
            _serviceRolePosition = serviceRolePosition;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGcAccessRequest>();

            var listIdGradeByCounsellor = await _dbContext.Entity<MsCounselorGrade>()
                                    .Where(e => e.Counselor.IdUser == param.IdUser
                                                && e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                    .Select(e => e.IdGrade)
                                    .ToListAsync(CancellationToken);

            var items = new GetGcAccessResult
            {
                CanAccess = listIdGradeByCounsellor.Any(),
            };


            return Request.CreateApiResult2(items as object);
        }
    }
}
