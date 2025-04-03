using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoachAttendance
{
    public class GetElectiveCoachAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
       

        public GetElectiveCoachAttendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;        
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetElectiveCoachAttendanceRequest>(nameof(GetElectiveCoachAttendanceRequest.IdAcademicYear), nameof(GetElectiveCoachAttendanceRequest.Semester));

            var ReturnResult = new List<GetElectiveCoachAttendanceResult>();

            if (!string.IsNullOrWhiteSpace(param.IdUser))
            {
                var userData = await _dbContext.Entity<MsExtracurricularExternalCoach>()
                         .Where(a => a.Id == param.IdUser)
                         .FirstOrDefaultAsync();

                if (userData is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExtracurricularExternalCoach"], "Id", param.IdUser));
            }
             
            var AY = await _dbContext.Entity<MsAcademicYear>()
                                .Where(a => a.Id == param.IdAcademicYear).FirstOrDefaultAsync();

            if (AY is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["school"], "Id", param.IdAcademicYear));


            var predicate = PredicateBuilder.True<TrExtracurricularExternalCoachAtt>();

            if (!string.IsNullOrWhiteSpace(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear.ToString());

            if (!string.IsNullOrWhiteSpace(param.IdAcademicYear))
                predicate = predicate.And(x => x.Semester == param.Semester);

            if (!string.IsNullOrWhiteSpace(param.IdUser))
                predicate = predicate.And(x => x.IdExtracurricularExternalCoach == param.IdUser);

            var query = _dbContext.Entity<TrExtracurricularExternalCoachAtt>()
                      .Include(x => x.AcademicYear)
                      .Include(x => x.Extracurricular)
                      .ThenInclude(y => y.ExtracurricularGradeMappings)
                      .ThenInclude(y => y.Grade)
                      //.SearchByIds(param)     
                      .Where(predicate)
                      .OrderByDescending(a => a.AttendanceDateTime);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                var result = await query
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Extracurricular.Name,
                        Description = x.AttendanceDateTime.ToString("dd-MM-yyyy HH:mm")
                    })
                    .ToListAsync(CancellationToken);
                items = result;
            }
            else
            {
                var result = query
                            .SetPagination(param)
                                    .Select(a => new GetElectiveCoachAttendanceResult
                                    {
                                      Id = a.Id,
                                      IdUser = a.IdExtracurricularExternalCoach,
                                      IdExternalCoach = a.ExtracurricularExternalCoach.IdExternalCoach,
                                      UserName = a.ExtracurricularExternalCoach.Name,
                                      AttendanceDateTime = a.AttendanceDateTime.ToString("dd-MM-yyyy HH:mm"),
                                      ElectivesName = a.Extracurricular.Name  + (a.Extracurricular.ExtracurricularGradeMappings != null ? (a.Extracurricular.ExtracurricularGradeMappings.Count() > 0 ? (" - " + string.Join("; ", a.Extracurricular.ExtracurricularGradeMappings.Select(b => b.Grade.Description))) : "") : ""),
                                    })
                                    .ToList();

                items = result;
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
              ? items.Count
              : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count));
        }
    }
}
