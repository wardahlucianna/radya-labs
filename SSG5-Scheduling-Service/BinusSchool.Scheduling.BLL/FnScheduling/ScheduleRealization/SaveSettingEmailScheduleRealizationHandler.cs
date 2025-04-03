using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{

    public class SaveSettingEmailScheduleRealizationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SaveSettingEmailScheduleRealizationHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SaveSettingEmailScheduleRealizationRequest, SaveSettingEmailScheduleRealizationValidator>();

            var idSchool = body.DataSettingEmailScheduleRealizations.Select(x => x.IdSchool).FirstOrDefault();

            var listSettingEmailScheduleRealization = await _dbContext.Entity<MsSettingEmailScheduleRealization>().Where(x => x.IdSchool == idSchool).ToListAsync(CancellationToken);
            listSettingEmailScheduleRealization.ForEach(x => x.IsActive = false);
            _dbContext.Entity<MsSettingEmailScheduleRealization>().UpdateRange(listSettingEmailScheduleRealization);

            foreach (var dataBody in body.DataSettingEmailScheduleRealizations)
            {
                var addSettingEmailScheduleRealization = new MsSettingEmailScheduleRealization
                {
                    Id = Guid.NewGuid().ToString(),
                    IsSetSpecificUser = dataBody.IsSetSpecificUser,
                    IdRole = dataBody.IdRole,
                    IdTeacherPosition = String.IsNullOrEmpty(dataBody.IdTeacherPosition) ? null : dataBody.IdTeacherPosition,
                    IdBinusian = String.IsNullOrEmpty(dataBody.IdUser) ? null : dataBody.IdUser,
                    IdSchool = dataBody.IdSchool
                };

                _dbContext.Entity<MsSettingEmailScheduleRealization>().AddRange(addSettingEmailScheduleRealization);

                //if(dataBody.Id == null)
                //{
                //    var addSettingEmailScheduleRealization = new MsSettingEmailScheduleRealization
                //    {
                //        Id = Guid.NewGuid().ToString(),
                //        IsSetSpecificUser = dataBody.IsSetSpecificUser,
                //        IdRole = dataBody.IdRole,
                //        IdTeacherPosition = dataBody.IdTeacherPosition,
                //        IdBinusian = dataBody.IdUser,
                //        IdSchool = dataBody.IdSchool
                //    };

                //    _dbContext.Entity<MsSettingEmailScheduleRealization>().AddRange(addSettingEmailScheduleRealization);
                //}
                //else
                //{
                //    var existSettingEmailScheduleRealization = await _dbContext.Entity<MsSettingEmailScheduleRealization>()
                //    .FirstOrDefaultAsync(x => x.Id == dataBody.Id && x.IdSchool == idSchool, CancellationToken);

                //    if(existSettingEmailScheduleRealization != null)
                //    {
                //        existSettingEmailScheduleRealization.IsSetSpecificUser = dataBody.IsSetSpecificUser;
                //        existSettingEmailScheduleRealization.IdRole = dataBody.IdRole;
                //        existSettingEmailScheduleRealization.IdTeacherPosition = dataBody.IdTeacherPosition;
                //        existSettingEmailScheduleRealization.IdBinusian = dataBody.IdUser;

                //        _dbContext.Entity<MsSettingEmailScheduleRealization>().UpdateRange(existSettingEmailScheduleRealization);
                //    }
                //}

            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
