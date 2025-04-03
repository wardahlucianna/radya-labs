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
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoachAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnExtracurricular.ElectiveCoachAttendance.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoachAttendance
{
    public class AddElectiveCoachAttendanceHandler : FunctionsHttpSingleHandler
    {
      
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public AddElectiveCoachAttendanceHandler(ISchedulingDbContext dbContext,
             IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddElectiveCoachAttendanceRequest, AddElectiveCoachAttendanceValidator>();

            var userData = await _dbContext.Entity<MsExtracurricularExternalCoach>()
                                 .Where(a => a.IdExternalCoach == body.IdExternalCoach)
                                 .FirstOrDefaultAsync();                       
          
            if (userData is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["IdExternalCoach"], "Id", body.IdExternalCoach));

           
            int CurrIdDay = (int)_dateTime.ServerTime.DayOfWeek;

            if(CurrIdDay == 0)
            {
                //sunday 0 => 7
                CurrIdDay = 7;
            }

            var UserExtracurricular = await _dbContext.Entity<TrExtracurricularSessionMapping>()
           .Include(x => x.Extracurricular)
                .ThenInclude(y => y.ExtracurricularExtCoachMappings)
           .Include(x => x.Extracurricular)
                .ThenInclude(y => y.ExtracurricularGradeMappings)
                .ThenInclude(y => y.Grade)
                .ThenInclude(y => y.Level)
           .Include(x => x.ExtracurricularSession)           
           .Where(a => a.Extracurricular.ExtracurricularExtCoachMappings.Any(b => b.IdExtracurricularExternalCoach == userData.Id)//&& b.IsExtCoach)       
           && a.Extracurricular.ExtracurricularGradeMappings.Any(b => b.Grade.Level.IdAcademicYear == body.IdAcademicYear)
           && a.Extracurricular.Semester == body.Semester
           && a.ExtracurricularSession.IdDay == CurrIdDay.ToString()          
           )
           .Select(a => new
           {
               IdExtracurricular = a.IdExtracurricular,
               ExtracurricularName = a.Extracurricular.Name,
               Day = a.ExtracurricularSession.Day.Description
           })
           .ToListAsync(CancellationToken);

            var ReturnResult = new AddElectiveCoachAttendanceResult();

            if(UserExtracurricular.Count > 0)
            {
                string CurrDate = _dateTime.ServerTime.ToString("dd-MM-yyyy");
                List<string> ExtList = UserExtracurricular.Select(b => b.IdExtracurricular).ToList();

                var checkCoachAtt = _dbContext.Entity<TrExtracurricularExternalCoachAtt>()                                    
                                    .Where(a => a.IdAcademicYear == body.IdAcademicYear
                                    && a.Semester == body.Semester
                                    && a.IdExtracurricularExternalCoach == userData.Id
                                    //&& ((a.AttendanceDateTime != null ? a.AttendanceDateTime?.ToString("dd-mm-yyyy")??"" : "" ) == CurrDate)
                                    && (a.AttendanceDateTime.Day.ToString() + a.AttendanceDateTime.Month.ToString() + a.AttendanceDateTime.Year.ToString()) == (_dateTime.ServerTime.Day.ToString() + _dateTime.ServerTime.Month.ToString() + _dateTime.ServerTime.Year.ToString())
                                    && UserExtracurricular.Select(b => b.IdExtracurricular).Contains(a.IdExtracurricular) 
                                    )
                                    .Select(a => a.IdExtracurricular)
                                    .ToList();

                var InsertCoachAtt = UserExtracurricular.Where(a => !checkCoachAtt.Contains(a.IdExtracurricular))
                                                        .Select(a => new TrExtracurricularExternalCoachAtt {
                                                            Id = Guid.NewGuid().ToString(),
                                                            IdAcademicYear = body.IdAcademicYear,
                                                            Semester = body.Semester,
                                                            IdExtracurricularExternalCoach = userData.Id,
                                                            AttendanceDateTime = _dateTime.ServerTime,
                                                            IdExtracurricular = a.IdExtracurricular
                                                        }).ToList();

                if(InsertCoachAtt.Count > 0)
                {
                    ReturnResult.Date = _dateTime.ServerTime.ToString();
                    ReturnResult.Elective = InsertCoachAtt.Select(a => a.IdExtracurricular).ToList();
                }
                else
                {
                    ReturnResult.Msg = "Already attended";
                }
    

                _dbContext.Entity<TrExtracurricularExternalCoachAtt>().AddRange(InsertCoachAtt);
                await _dbContext.SaveChangesAsync(CancellationToken);

            }
            else
            {
                ReturnResult.Msg = "There is no class schedule";
            }
            

            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
