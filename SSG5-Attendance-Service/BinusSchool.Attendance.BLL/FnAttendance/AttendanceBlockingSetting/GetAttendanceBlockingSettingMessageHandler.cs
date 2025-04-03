using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class GetAttendanceBlockingSettingMessageHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetAttendanceBlockingSettingMessageHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceBlockingSettingMessageRequest>(nameof(GetAttendanceBlockingSettingMessageRequest.IdAcadyear));

            var GetMappingAttendanceLevel = await _dbContext.Entity<MsLevel>()
                        .Include(x => x.MappingAttendances).ThenInclude(x => x.AttendanceMappingAttendances)
                        .Where(x => x.IdAcademicYear == param.IdAcadyear.ToString())
                        .Select(x => new GetBlockingMapAttendanceAYResult
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                        })
                        .OrderBy(x => x.Description)
                        .ToListAsync(CancellationToken);

            var GetBLockingAttendanceMappingExisting = await _dbContext.Entity<MsBlockingTypeAtdSetting>()
                   .Include(x => x.BlockingType)
                   .Include(x => x.AcademicYear)
                   .Include(x => x.Level)
                   .Include(x => x.AtdMappingAtd)
                   .Where(x => x.IdAcademicYear == param.IdAcadyear.ToString())
                   .Select(x => new GetAttendanceBlockingSettingDetailResult
                   {
                       Id = x.Level.Id,
                       Code = x.Level.Code,
                       Description = x.Level.Description,
                   })
                   .OrderBy(x => x.Description)
                   .ToListAsync(CancellationToken);

            var listExcludeLevel = new List<CodeWithIdVm>();

            foreach (var item in GetMappingAttendanceLevel)
            {
                if (GetBLockingAttendanceMappingExisting.Any(x=> x.Id == item.Id) == false)
                {
                    listExcludeLevel.Add(new CodeWithIdVm
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Description = item.Description
                    });
                }
            }

            var level = string.Empty;
            if (listExcludeLevel != null)
            {
                foreach (var item in listExcludeLevel)
                {
                    level = string.IsNullOrEmpty(level)
                        ? item.Code
                        : !object.ReferenceEquals(item, listExcludeLevel.Last()) ? level + $", {item.Code}" : level + $" and {item.Code}";
                }
            }

            //var result = new GetAttendanceBlockingSettingMessageResult
            //{
            //    DissableBlockingAtd = !string.IsNullOrEmpty(level),
            //    Message = string.IsNullOrEmpty(level) ? string.Empty : $"Attendance Name for Attendance BLocking in {level} hasn't been set up for this Academic Year. You won't be able to block student's attendance for student in those level before this set up is done."
            //};

            var result = new GetAttendanceBlockingSettingMessageResult
            {
                DissableBlockingAtd = false,
                Message = string.Empty
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
