using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.LatenessSetting.Validator;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Attendance.FnAttendance.LatenessSetting;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.LatenessSetting
{
    public class GetLatenessSettingDetailHandler : FunctionsHttpCrudHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetLatenessSettingDetailHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string idLevel)
        {

            var item = await _dbContext.Entity<MsLevel>()
                        .Include(e => e.AcademicYear)
                        .Where(e => e.Id == idLevel)
                        .Select(e => new GetLatenessSettingDetailResult
                        {
                            AcademicYear = new ItemValueVm
                            {
                                Id = e.AcademicYear.Id,
                                Description = e.AcademicYear.Description
                            },
                            Level = new ItemValueVm
                            {
                                Id = e.Id,
                                Description = e.Description
                            },
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            var getLatenessSetting = await _dbContext.Entity<MsLatenessSetting>()
                        .Include(e=>e.Level).ThenInclude(e => e.AcademicYear)
                        .Where(e => e.IdLevel == idLevel)
                        .Select(e => new 
                        {
                            Period = e.Period,
                            TotalLate = e.TotalLate,
                            TotalUnexcusedAbsend = e.TotalUnexcusedAbsend,
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            if (getLatenessSetting != null)
            {
                item.Period = getLatenessSetting.Period;
                item.TotalLate = getLatenessSetting.TotalLate;
                item.TotalUnexcusedAbsend = getLatenessSetting.TotalUnexcusedAbsend;
            }

            return Request.CreateApiResult2(item as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddAndUpdateLatenessSettingRequest, AddAndUpdateLatenessSettingValidator>();

            var GetLatenessSetting = await _dbContext.Entity<MsLatenessSetting>()
                                    .Where(e => e.IdLevel == body.IdLevel)
                                    .FirstOrDefaultAsync(CancellationToken);

            if (GetLatenessSetting == null)
            {
                MsLatenessSetting newLatenessSeeting = new MsLatenessSetting
                {
                    Id = Guid.NewGuid().ToString(),
                    IdLevel = body.IdLevel,
                    Period = body.Period,
                    TotalLate = body.TotalLate,
                    TotalUnexcusedAbsend = body.TotalUnexcusedAbsend,
                };
                _dbContext.Entity<MsLatenessSetting>().Add(newLatenessSeeting);
            }
            else
            {
                GetLatenessSetting.Period = body.Period;
                GetLatenessSetting.TotalLate = body.TotalLate;
                GetLatenessSetting.TotalUnexcusedAbsend = body.TotalUnexcusedAbsend;
                _dbContext.Entity<MsLatenessSetting>().Update(GetLatenessSetting);
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
