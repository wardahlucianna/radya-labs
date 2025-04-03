using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.MapAttendance
{
    public class GetMapAttendanceDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetMapAttendanceDetailHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("idLevel", out var idLevel))
                throw new ArgumentNullException(nameof(idLevel));

            var query = await GetMapAttendanceDetail((string)idLevel);

            return Request.CreateApiResult2(query as object);
        }

        public Task<GetMapAttendanceDetailResult> GetMapAttendanceDetail(string idLevel, CancellationToken ct = default)
        {
            return _dbContext.Entity<MsMappingAttendance>()
                .Where(x => x.IdLevel == idLevel)
                .Select(x => new GetMapAttendanceDetailResult
                {
                    Id = x.Id,
                    School = new NameValueVm(x.Level.AcademicYear.IdSchool, x.Level.AcademicYear.School.Name),
                    Acadyear = new CodeWithIdVm(x.Level.IdAcademicYear, x.Level.AcademicYear.Code, x.Level.AcademicYear.Description),
                    Level = new CodeWithIdVm(x.IdLevel, x.Level.Code, x.Level.Description),
                    Attendances = x.AttendanceMappingAttendances.Select(y => new MapAttendanceDetailItem
                    {
                        Id = y.IdAttendance,
                        IdAttendanceMapAttendance = y.Id,
                        Code = y.Attendance.Code,
                        Description = y.Attendance.Description,
                        NeedAttachment = y.Attendance.IsNeedFileAttachment,
                        AttendanceCategory = y.Attendance.AttendanceCategory,
                        AbsenceCategory = y.Attendance.AbsenceCategory,
                        ExcusedAbsenceCategory = y.Attendance.ExcusedAbsenceCategory,
                        Status = y.Attendance.Status
                    }),
                    Term = x.AbsentTerms,
                    TermSession = new MapAttendanceTermSession
                    {
                        NeedValidation = x.IsNeedValidation,
                        Validations = x.AbsentMappingAttendances.Select(y => new MapAttendanceDetailValidation
                        {
                            AbsentBy = new CodeWithIdVm(y.IdTeacherPosition, y.TeacherPosition.LtPosition.Code, y.TeacherPosition.Description),
                            Attendances = y.ListMappingAttendanceAbsents.Select(z => new MapAttendanceValidation
                            {
                                Id = z.IdAttendance,
                                Code = z.MsAttendance.Code,
                                Description = z.MsAttendance.Description,
                                NeedToValidate = z.IsNeedValidation
                            })
                        }),
                    },
                    UseWorkhabit = x.IsUseWorkhabit,
                    NeedValidation = x.IsNeedValidation,
                    IsUseDueToLateness = x.IsUseDueToLateness,
                    Workhabits = x.IsUseWorkhabit
                        ? x.MappingAttendanceWorkhabits.Select(y => new Workhabits
                        {
                            Id = y.Id,
                            IdWorkhabit = y.IdWorkhabit,
                            Code = y.Workhabit.Code,
                            Description = y.Workhabit.Description
                        })
                        : null,
                    UsingCheckboxAttendance = x.UsingCheckboxAttendance,
                    RenderAttendance = x.RenderAttendance,
                    ShowingModalReminderAttendanceEntry = x.ShowingModalReminderAttendanceEntry
                })
                .FirstOrDefaultAsync(ct != default ? ct : CancellationToken);
        }
    }
}
