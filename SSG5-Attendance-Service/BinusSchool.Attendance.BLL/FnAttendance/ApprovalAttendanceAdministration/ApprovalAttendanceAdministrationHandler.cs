using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.ApprovalAttendanceAdministration.validator;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Attendance.FnAttendance.ApprovalAttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.ApprovalAttendanceAdministration
{
    public class ApprovalAttendanceAdministrationHandler : FunctionsHttpCrudHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        public ApprovalAttendanceAdministrationHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsApprovalAttendanceAdministration>()
                .Include(x => x.School)
                .Include(x=>x.Role)
                .Where(x => x.IdSchool == id)
                .Select(x=>new ApprovalAttendanceAdministrationResponse
                {
                    Id = x.Id,
                    School = new CodeWithIdVm
                    {
                        Id = x.IdSchool,
                        Code = x.School.Name,
                        Description = x.School.Description
                    },
                    Role = new CodeWithIdVm
                    {
                        Id = x.IdRole,
                        Code = x.Role.Code,
                        Description = x.Role.Description
                    }
                })
                .FirstOrDefaultAsync();
                return Request.CreateApiResult2(data as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<ApprovalAttendanceAdministrationRequest, ApprovalAttendanceAdministrationValidator>();
            var data = await _dbContext.Entity<MsApprovalAttendanceAdministration>().Where(x => x.IdSchool == body.IdSchool).FirstOrDefaultAsync();
            if (data == null)
            {
                MsApprovalAttendanceAdministration msApprovalAttendanceAdministration = new MsApprovalAttendanceAdministration
                {
                    Id = Guid.NewGuid().ToString(),
                    IdRole = body.IdRole,
                    IdSchool = body.IdSchool
                };
                _dbContext.Entity<MsApprovalAttendanceAdministration>().Add(msApprovalAttendanceAdministration);
            }
            else
            {
                data.IdRole = body.IdRole;
                _dbContext.Entity<MsApprovalAttendanceAdministration>().Update(data);
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
