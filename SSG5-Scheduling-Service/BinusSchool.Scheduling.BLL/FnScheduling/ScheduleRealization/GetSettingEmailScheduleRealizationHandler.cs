using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class GetSettingEmailScheduleRealizationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetSettingEmailScheduleRealizationHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSettingEmailScheduleRealizationRequest>(nameof(GetSettingEmailScheduleRealizationRequest.IdSchool));

            var query = _dbContext.Entity<MsSettingEmailScheduleRealization>()
                                 .Include(x => x.Role).ThenInclude(x => x.RoleGroup)
                                 .Include(x => x.TeacherPosition)
                                 .Include(x => x.Staff)
                                 .Where(x => x.IdSchool == param.IdSchool)
                                 .OrderBy(x => x.DateIn).ToList();

            List<DataCc> itemsCc = new List<DataCc>();

            foreach(var data in query)
            {
                var dataCc = new DataCc
                {
                    Id = data.Id,
                    IdSchool = data.IdSchool,
                    IsSetSpecificUser = data.IsSetSpecificUser,
                    Role = data.Role == null ? null : new CodeWithIdVm(data.Role.Id,data.Role.RoleGroup.Id, data.Role.Description),
                    Position = data.TeacherPosition == null ? null : new ItemValueVm(data.TeacherPosition.Id,data.TeacherPosition.Description),
                    User = data.Staff == null ? null : new ItemValueVm(data.Staff.IdBinusian,(!string.IsNullOrEmpty(data.Staff.FirstName)?data.Staff.FirstName:"")+ (!string.IsNullOrEmpty(data.Staff.LastName) ? " "+data.Staff.FirstName : ""))
                };

                itemsCc.Add(dataCc);
            }
                                 

            var dataSettingEmail = new GetSettingEmailScheduleRealizationResult
                {
                    To = "Substitute teacher",
                    ListDataCc = itemsCc.Select(x => new DataCc
                    {
                        Id = x.Id,
                        IdSchool = x.IdSchool,
                        IsSetSpecificUser = x.IsSetSpecificUser,
                        Role = x.Role,
                        Position = x.Position,
                        User = x.User
                    }).ToList()
                };
           
            return Request.CreateApiResult2(dataSettingEmail as object);
        }
    }
}
