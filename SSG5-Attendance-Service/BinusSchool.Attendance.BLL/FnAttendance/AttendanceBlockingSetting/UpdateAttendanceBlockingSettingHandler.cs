using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceBlockingSetting.Validator;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceBlockingSetting;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Attendance.FnAttendance.AttendanceBlockingSetting
{
    public class UpdateAttendanceBlockingSettingHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IAttendanceDbContext _dbContext;

        public UpdateAttendanceBlockingSettingHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var body = await Request.ValidateBody<UpdateAttendanceBlockingSettingRequest, UpdateAttendanceBlockingSettingValidator>();

            var GetBlockingTypeAttendanceSetting = await _dbContext.Entity<MsBlockingTypeAtdSetting>().Where(e => e.IdBlockingType == body.IdBlockingType && e.IdAcademicYear == body.IdAcademicYear).ToListAsync(CancellationToken);

            //remove Blocking Type Attendance Setting
            foreach (var ItemBlockingTypeAttendanceSetting in GetBlockingTypeAttendanceSetting)
            {
                var ExsisBodySubMenuId = body.MapBlockingAttendanceSettings.Any(e => e.IdLevel == ItemBlockingTypeAttendanceSetting.IdLevel && e.IdAtdMappingAtd == ItemBlockingTypeAttendanceSetting.IdAtdMappingAtd);

                if (!ExsisBodySubMenuId)
                {
                    ItemBlockingTypeAttendanceSetting.IsActive = false;
                    _dbContext.Entity<MsBlockingTypeAtdSetting>().Update(ItemBlockingTypeAttendanceSetting);
                }
            }

            //Add Blocking Type Attendance Setting
            if (body.MapBlockingAttendanceSettings != null)
            {
                foreach (var MapBlockingAttendanceSettings in body.MapBlockingAttendanceSettings)
                {
                    var ExsistdbId = GetBlockingTypeAttendanceSetting.Where(e => e.IdLevel == MapBlockingAttendanceSettings.IdLevel && e.IdAtdMappingAtd == MapBlockingAttendanceSettings.IdAtdMappingAtd).SingleOrDefault();
                    if (ExsistdbId is null)
                    {
                        var newBlockingTypeAttendanceSetting = new MsBlockingTypeAtdSetting
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdBlockingType = body.IdBlockingType,
                            IdAcademicYear = body.IdAcademicYear,
                            IdLevel = MapBlockingAttendanceSettings.IdLevel,
                            IdAtdMappingAtd = MapBlockingAttendanceSettings.IdAtdMappingAtd
                        };

                        _dbContext.Entity<MsBlockingTypeAtdSetting>().Add(newBlockingTypeAttendanceSetting);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
