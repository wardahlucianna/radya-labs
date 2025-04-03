using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendanceDate;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendanceDate
{
    public class GetElectiveAttendanceByDateHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetElectiveAttendanceByDateHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetElectiveAttendanceByDateRequest>(                          
                           nameof(GetElectiveAttendanceByDateRequest.Date),
                           nameof(GetElectiveAttendanceByDateRequest.IdUser),
                           nameof(GetElectiveAttendanceByDateRequest.IdSchool));

            var validateUser = _dbContext.Entity<MsExtracurricularSpvCoach>()
                .Where(x => x.IdBinusian == param.IdUser)
                .ToList();

            var result = new List<GetElectiveAttendanceByDateResult>();

            if (validateUser.Count() > 0)
            {
                // Get Extracurricular by Date
                var getExtracurricularByDate = _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                    .Include(x => x.Extracurricular.ExtracurricularSpvCoach)
                                    .Include(x => x.ExtracurricularSession)
                                    .Where(x => x.Date == param.Date
                                    && x.Extracurricular.ExtracurricularSpvCoach.Any(a => a.IdBinusian == param.IdUser)
                                    && x.Extracurricular.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.IdSchool).FirstOrDefault() == param.IdSchool
                                    )
                                    .OrderBy(x => x.Extracurricular.Name)
                                    .Select(a => new GetElectiveAttendanceByDateResult()
                                    {
                                        Elective = new NameValueVm()
                                        {
                                            Id = a.Extracurricular.Id,
                                            Name = a.Extracurricular.Name
                                        },
                                        Supervisor = a.Extracurricular.ExtracurricularSpvCoach.Where(b => b.IsSpv == true)
                                                                                            .Select(b => new NameValueVm()
                                                                                            {
                                                                                                Id = b.IdBinusian,
                                                                                                Name = NameUtil.GenerateFullName(b.Staff.FirstName, b.Staff.LastName)
                                                                                            })
                                                                                            .ToList(),
                                        Time = new DateTime(a.ExtracurricularSession.StartTime.Ticks).ToString("HH:mm") + " - " + new DateTime(a.ExtracurricularSession.EndTime.Ticks).ToString("HH:mm"),
                                        IdUserUpdate = a.ExtracurricularAttendanceEntries.Select(b => b.UserUp).FirstOrDefault() == null ? a.ExtracurricularAttendanceEntries.Select(b => b.UserIn).FirstOrDefault() : a.ExtracurricularAttendanceEntries.Select(b => b.UserUp).FirstOrDefault(),
                                        UpdateTime = a.ExtracurricularAttendanceEntries.Select(b => b.DateUp).FirstOrDefault() == null ? a.ExtracurricularAttendanceEntries.Select(b => b.DateIn).FirstOrDefault() : a.ExtracurricularAttendanceEntries.Select(b => b.DateUp).FirstOrDefault(),
                                        IdElectiveGeneratedAtt = a.Id
                                    })
                                    .ToList();

                var UserUpdateList = await _dbContext.Entity<MsUser>()
                             .Where(a => getExtracurricularByDate.Select(c => c.IdUserUpdate).Contains(a.Id))
                             .ToListAsync();

                result =
                    (from a in getExtracurricularByDate
                     join b in UserUpdateList on a.IdUserUpdate equals b.Id into ab_jointable
                     from p in ab_jointable.DefaultIfEmpty()
                     select new GetElectiveAttendanceByDateResult
                     {
                         Elective = a.Elective,
                         Supervisor = a.Supervisor,
                         Time = a.Time,
                         IdUserUpdate = a.IdUserUpdate,
                         LasUpdateBy = string.IsNullOrEmpty(a.IdUserUpdate) ? "" : ((p.DisplayName ?? a.IdUserUpdate) + (a.UpdateTime == null ? "" : (", " + ((DateTime)a.UpdateTime).ToString   ( "dd-MM-yyyy HH:mm")))),
                         IdElectiveGeneratedAtt = a.IdElectiveGeneratedAtt
                     })
                    .ToList();
            }
            else
            {
                var getExtracurricularByDate = _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                    .Include(x => x.Extracurricular.ExtracurricularSpvCoach)
                                    .Include(x => x.ExtracurricularSession)
                                    .Where(x => x.Date == param.Date
                                        && x.Extracurricular.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.IdSchool).FirstOrDefault() == param.IdSchool)
                                    .OrderBy(x => x.Extracurricular.Name)
                                    .Select(a => new GetElectiveAttendanceByDateResult()
                                    {
                                        Elective = new NameValueVm()
                                        {
                                            Id = a.Extracurricular.Id,
                                            Name = a.Extracurricular.Name
                                        },
                                        Supervisor = a.Extracurricular.ExtracurricularSpvCoach
                                            .Where(b => b.IsSpv == true)
                                            .Select(b => new NameValueVm
                                            {
                                                Id = b.IdBinusian,
                                                Name = NameUtil.GenerateFullName(b.Staff.FirstName, b.Staff.LastName)
                                            })
                                            .ToList(),
                                        Time = new DateTime(a.ExtracurricularSession.StartTime.Ticks).ToString("HH:mm") + " - " + new DateTime(a.ExtracurricularSession.EndTime.Ticks).ToString("HH:mm"),
                                        IdUserUpdate = a.ExtracurricularAttendanceEntries.Select(b => b.UserUp).FirstOrDefault() == null ? a.ExtracurricularAttendanceEntries.Select(b => b.UserIn).FirstOrDefault() : a.ExtracurricularAttendanceEntries.Select(b => b.UserUp).FirstOrDefault(),
                                        UpdateTime = a.ExtracurricularAttendanceEntries.Select(b => b.DateUp).FirstOrDefault() == null ? a.ExtracurricularAttendanceEntries.Select(b => b.DateIn).FirstOrDefault() : a.ExtracurricularAttendanceEntries.Select(b => b.DateUp).FirstOrDefault(),
                                        IdElectiveGeneratedAtt = a.Id
                                    })
                                    .ToList();

                var UserUpdateList = await _dbContext.Entity<MsUser>()
                    .Where(a => getExtracurricularByDate.Select(c => c.IdUserUpdate).Contains(a.Id))
                    .ToListAsync();

                result =
                    (from a in getExtracurricularByDate
                     join b in UserUpdateList on a.IdUserUpdate equals b.Id into ab_jointable
                     from p in ab_jointable.DefaultIfEmpty()
                     select new GetElectiveAttendanceByDateResult
                     {
                         Elective = a.Elective,
                         Supervisor = a.Supervisor,
                         Time = a.Time,
                         IdUserUpdate = a.IdUserUpdate,
                         LasUpdateBy = string.IsNullOrEmpty(a.IdUserUpdate) ? "" : ((p.DisplayName ?? a.IdUserUpdate) + (a.UpdateTime == null ? "" : (", " + ((DateTime)a.UpdateTime).ToString   ( "dd-MM-yyyy HH:mm")))),
                         IdElectiveGeneratedAtt = a.IdElectiveGeneratedAtt
                     })
                    .ToList();
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
