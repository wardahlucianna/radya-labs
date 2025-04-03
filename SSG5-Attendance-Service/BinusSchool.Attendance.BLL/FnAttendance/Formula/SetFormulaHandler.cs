using System;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Formula.Validator;
using BinusSchool.Attendance.FnAttendance.Utils;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Formula;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Formula
{
    public class SetFormulaHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public SetFormulaHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetFormulaRequest, SetFormulaValidator>();

            if (!await _dbContext.Entity<MsLevel>()
                                 .AnyAsync(x => x.Id == body.IdLevel))
                throw new NotFoundException("Level is not found");

            var attendances = await _dbContext.Entity<MsAttendance>()
                                              .ToListAsync();

            if (!body.AttendanceRate.Validate(attendances))
                throw new BadRequestException("Attendance Rate Formula format is invalid");

            if (!body.PresenceInClass.Validate(attendances))
                throw new BadRequestException("Presence in Class Formula format is invalid");

            var data = await _dbContext.Entity<MsFormula>()
                                       .FirstOrDefaultAsync(x => x.IdLevel == body.IdLevel);
            if (data is null)
                _dbContext.Entity<MsFormula>().Add(new MsFormula
                {
                    Id = Guid.NewGuid().ToString(),
                    IdLevel = body.IdLevel,
                    AttendanceRate = body.AttendanceRate,
                    PresenceInClass = body.PresenceInClass,
                    UserIn = AuthInfo.UserId
                });
            else
            {
                data.AttendanceRate = body.AttendanceRate;
                data.PresenceInClass = body.PresenceInClass;
                data.UserUp = AuthInfo.UserId;
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
