using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;

namespace BinusSchool.Scheduling.FnExtracurricular.PrivilegeUserElective
{
    public class GetPrivilegeShowButtonUserElectiveHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = {
            nameof(GetPrivilegeShowButtonUserElectiveRequest.IdUser),
            nameof(GetPrivilegeShowButtonUserElectiveRequest.IdSchool)
        };

        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetPrivilegeShowButtonUserElectiveHandler(
            ISchedulingDbContext DbContext,
            IMachineDateTime dateTime
            )
        {
            _dbContext = DbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPrivilegeShowButtonUserElectiveRequest>(_requiredParams);

            var result = await GetPrivilegeShowButtonUserElective(new GetPrivilegeShowButtonUserElectiveRequest
            {
                IdUser = param.IdUser,
                IdSchool = param.IdSchool,
                IdAcademicYear = param.IdAcademicYear
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<GetPrivilegeShowButtonUserElectiveResult> GetPrivilegeShowButtonUserElective(GetPrivilegeShowButtonUserElectiveRequest param)
        {
            GetActiveAcademicYearElectiveResult GetActiveAcademicYear = new GetActiveAcademicYearElectiveResult();

            #region Get Academic Year
            var GetAcademicYear = _dbContext.Entity<MsPeriod>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                    .ToList();

            if (string.IsNullOrEmpty(param.IdAcademicYear))
            {
                GetActiveAcademicYear = GetAcademicYear
                        .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                        .OrderByDescending(x => x.StartDate)
                        .Select(x => new GetActiveAcademicYearElectiveResult
                        {
                            AcademicYear = new ItemValueVm
                            {
                                Id = x.Grade.Level.AcademicYear.Id,
                                Description = x.Grade.Level.AcademicYear.Description
                            },
                        })
                        .FirstOrDefault();
            }
            else
            {
                GetActiveAcademicYear = GetAcademicYear
                        .Where(x => x.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                        .Select(x => new GetActiveAcademicYearElectiveResult
                        {
                            AcademicYear = new ItemValueVm
                            {
                                Id = x.Grade.Level.AcademicYear.Id,
                                Description = x.Grade.Level.AcademicYear.Description
                            },
                        })
                        .FirstOrDefault();
            }

            if (GetActiveAcademicYear == null)
                throw new BadRequestException($"Academic Year not exists");
            #endregion

            var dataSpvCoach = await _dbContext.Entity<MsExtracurricularSpvCoach>()
                    .Where(x => x.IdBinusian == param.IdUser)
                    .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == param.IdSchool)
                    .Where(x => x.Extracurricular.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.Id).Contains(param.IdAcademicYear))
                    .Select(x => new {
                        IdElective = x.IdExtracurricular,
                        IdBinusian = x.IdBinusian
                    }).Distinct()
                    .ToListAsync(CancellationToken);

            var showButton = new GetPrivilegeShowButtonUserElectiveResult();

            if (dataSpvCoach.Count() > 0)
            {
                showButton.ShowButton = false;
            }
            else
            {
                showButton.ShowButton = true;
            }

            return showButton;
        }
    }
}
