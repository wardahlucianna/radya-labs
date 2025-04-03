using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class UpdateSESnECUserRoleHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IUserRole _userRoleApi;
        private readonly IMachineDateTime _dateTime;
        public UpdateSESnECUserRoleHandler(ISchedulingDbContext dbContext,
        IUserRole userRoleApi,
        IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _userRoleApi = userRoleApi;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateSESnECUserRoleRequest, UpdateSESnECUserRoleValidator>();

                var AYActive = await _dbContext.Entity<MsPeriod>()         
                                    .Include(x => x.Grade)
                                        .ThenInclude(y => y.Level)
                                        .ThenInclude(y => y.AcademicYear)
                                    .Where(x => x.StartDate <= _dateTime.ServerTime 
                                    && x.EndDate >= _dateTime.ServerTime
                                    && x.Grade.Level.AcademicYear.IdSchool == body.IdSchool)
                                    .FirstOrDefaultAsync(CancellationToken);

                //AYActive.Semester = 1;

                if (AYActive == null)
                    throw new BadRequestException($"AcademicYear Active not found");

                var extracurricularExtCoach = await _dbContext.Entity<MsExtracurricularExtCoachMapping>()
                                            .Include(x => x.Extracurricular)
                                                .ThenInclude(y => y.ExtracurricularGradeMappings)
                                                .ThenInclude(y => y.Grade)  
                                                .ThenInclude(y => y.Level)
                                             .Include(x => x.Extracurricular)
                                               .ThenInclude(y => y.ExtracurricularGroup)
                         .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == body.IdSchool
                                && x.Extracurricular.ExtracurricularGradeMappings.Any(a => a.Grade.Level.IdAcademicYear == AYActive.Grade.Level.IdAcademicYear)
                                && x.Extracurricular.Semester == AYActive.Semester)
                         .Select(a => new AllUserRoleUpdateVm
                         {
                             IdUser = a.IdExtracurricularExternalCoach
                         }                          
                         ).Distinct() 
                         .ToListAsync(CancellationToken);

                if(extracurricularExtCoach.Count > 0)
                {
                    var Param = new UpdateAllUserRoleByRoleCodeRequest();
                    Param.IdSchool = body.IdSchool;
                    Param.RoleCode = "EC";
                    Param.UserList = extracurricularExtCoach;

                    var syncRoleEC = await _userRoleApi.UpdateAllUserRoleByRoleCode(Param);
                }

                var extracurricularCoach = await _dbContext.Entity<MsExtracurricularSpvCoach>()
                                          .Include(x => x.Extracurricular)
                                              .ThenInclude(y => y.ExtracurricularGradeMappings)
                                              .ThenInclude(y => y.Grade)
                                              .ThenInclude(y => y.Level)    
                                          .Include(x => x.Extracurricular)
                                               .ThenInclude(y => y.ExtracurricularGroup)
                       .Where(x => x.Extracurricular.ExtracurricularGroup.IdSchool == body.IdSchool
                              && x.Extracurricular.ExtracurricularGradeMappings.Any(a => a.Grade.Level.IdAcademicYear == AYActive.Grade.Level.IdAcademicYear)
                              && x.Extracurricular.Semester == AYActive.Semester)
                       .Select(a => new AllUserRoleUpdateVm
                       {
                           IdUser = a.IdBinusian
                       }
                       ).Distinct() 
                       .ToListAsync(CancellationToken);

                var test = extracurricularCoach.Select(x => x.IdUser).ToList();


                if (extracurricularCoach.Count > 0)
                {
                    var Param = new UpdateAllUserRoleByRoleCodeRequest();
                    Param.IdSchool = body.IdSchool;
                    Param.RoleCode = "SES";
                    Param.UserList = extracurricularCoach;

                    var syncRoleSES = await _userRoleApi.UpdateAllUserRoleByRoleCode(Param);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return Request.CreateApiResult2();
        }
    }
}

