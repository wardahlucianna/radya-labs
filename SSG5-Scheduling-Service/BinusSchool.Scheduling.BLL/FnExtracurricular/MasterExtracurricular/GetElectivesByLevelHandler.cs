using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.PrivilegeUserElective;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class GetElectivesByLevelHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams = { nameof(GetElectivesByLevelRequest.IdUser), nameof(GetElectivesByLevelRequest.IdAcademicYear) };

        private readonly GetPrivilegeUserElectiveHandler _getPrivilegeUserElectiveHandler;
        private readonly ISchedulingDbContext _dbContext;

        public GetElectivesByLevelHandler(
            ISchedulingDbContext dbContext, 
            GetPrivilegeUserElectiveHandler getPrivilegeUserElectiveHandler)
        {
            _dbContext = dbContext;
            _getPrivilegeUserElectiveHandler = getPrivilegeUserElectiveHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetElectivesByLevelRequest>(_requiredParams);

            var getPrivilegeUserElective = new List<GetPrivilegeUserElectiveResult>();

            var getAySchool = _dbContext.Entity<MsLevel>()
                .Where(x => x.IdAcademicYear == param.IdAcademicYear && x.Id == (param.IdLevel != null ? param.IdLevel : x.Id))
                .Select(x => new { 
                    IdSchool = x.AcademicYear.IdSchool,
                    IdAcademicYear = x.IdAcademicYear
                })
                .FirstOrDefault();

            getPrivilegeUserElective = await _getPrivilegeUserElectiveHandler.GetAvailabilityPositionUserElective(new GetPrivilegeUserElectiveRequest
            {
                IdUser = param.IdUser,
                IdSchool = getAySchool.IdSchool,
                IdAcademicYear = getAySchool.IdAcademicYear
            });

            var listElectiveAccess = getPrivilegeUserElective.Select(x => x.IdExtracurricular).ToList();

            var predicate = PredicateBuilder.True<MsExtracurricular>();

            if (param.IdAcademicYear?.Any() ?? false)
                predicate = predicate.And(x => x.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.Id).Contains(param.IdAcademicYear));

            if (param.Semester != 0)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (param.IdLevel != null)
                predicate = predicate.And(x => x.ExtracurricularGradeMappings.Any(y => y.Grade.Level.Id == param.IdLevel));
            if (listElectiveAccess.Count() > 0)
                predicate = predicate.And(x => listElectiveAccess.Any(z => z == x.Id));

            var retVal = _dbContext.Entity<MsExtracurricular>()
                .Include(x => x.ExtracurricularGradeMappings)
                   .ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                .Where(predicate)
                .Select(x => new GetElectivesByLevelResult
                {
                    Id = x.Id,
                    ElectiveName = x.Name,
                    Grades = string.Join("; ", x.ExtracurricularGradeMappings.OrderBy(x => x.Grade.OrderNumber).ThenBy(x => x.Grade.Description).Select(x => x.Grade.Description)),
                    Semester =  x.Semester
                }); ;

            return Request.CreateApiResult2(retVal as object);
        }
    }
}
