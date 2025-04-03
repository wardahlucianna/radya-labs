using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.ConsentForm;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.ConsentForm
{
    public class GetPeriodEntryHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetPeriodEntryHandler(IDocumentDbContext dbContext,
              IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPeriodEntryRequest>(nameof(GetPeriodEntryRequest.IdAcademicYear), nameof(GetPeriodEntryRequest.Semester), nameof(GetPeriodEntryRequest.IdParent));

            var ReturnResult = new GetPeriodEntryResult();

            #region
            /*
            var GetGroupActive = await _dbContext.Entity<MsBLPWeekPeriod>()
                                            //.Where(a => a.IdSurveyCategory == "")
                                            .Where(a => a.StartDate <= _dateTime.ServerTime)
                                            .OrderByDescending(a => a.StartDate)
                                            .Select(a =>  a.IdBLPGroup )                                           
                                            .FirstOrDefaultAsync(CancellationToken);

            var GetSiblingStudent = await _dbContext.Entity<MsSiblingGroup>()
                                            .Where(a => a.IdStudent == param.IdStudent)
                                            .Join(_dbContext.Entity<MsSiblingGroup>(),
                                                sibling => sibling.Id,
                                                siblinggroup => siblinggroup.Id,
                                                (sibling, siblinggroup) => siblinggroup.IdStudent)
                                        .Select(x => x)
                                        .ToListAsync();     

            var BLPStudent = await _dbContext.Entity<TrBLPGroupStudent>()
                                    .Where(a => GetSiblingStudent.Contains(a.IdStudent)
                                    && a.IdBLPGroup == GetGroupActive
                                    && a.IdAcademicYear == param.IdAcademicYear
                                    && a.Semester == param.Semester)
                                    .Select(a => new { 
                                    a.IdStudent,
                                    a.BLPStatus.BLPStatusName
                                    })
                                    .ToListAsync(CancellationToken);
            */
            #endregion
                      

            var GetStudentGrade = await _dbContext.Entity<MsHomeroomStudent>()
                                                 .Include(x => x.Student)
                                                     .ThenInclude(y => y.StudentParents)
                                                 .Include(x => x.Homeroom)
                                .Where(a => a.Student.StudentParents.Any(b => b.IdParent == param.IdParent)
                                             && a.Homeroom.IdAcademicYear == param.IdAcademicYear
                                             && a.Semester == param.Semester)
                               .Select(a => new
                               {
                                   a.IdStudent,
                                   a.Homeroom.IdGrade
                               })
                               .ToListAsync();

            int getDayToday = (int)_dateTime.ServerTime.DayOfWeek + 1;              
            var GetSurveyActive = await _dbContext.Entity<MsSurveyPeriod>()
                                        .Include(x => x.Grade).ThenInclude(y => y.Level)
                                        .Include(x => x.ConsentCustomSchedules).ThenInclude(y => y.ToDay)
                                        .Include(x => x.ConsentCustomSchedules).ThenInclude(y => y.FromDay)
                                       .Where(a => 
                                       a.IdSurveyCategory == "1"
                                       && a.StartDate <= _dateTime.ServerTime
                                       && a.EndDate >= _dateTime.ServerTime
                                       && a.Semester == param.Semester
                                       //&& GetStudentGrade.Select(b => b.IdGrade).Contains(a.IdGrade)
                                       && GetStudentGrade.Select(b => b.IdGrade).Contains(a.IdGrade)
                                       )
                                       .Select(a => new GetPeriodEntryResult(){
                                           IdSurveyPeriod  = a.Id,
                                           IdGrade = a.IdGrade,
                                           GradeName = a.Grade.Description,
                                           StartDate = a.StartDate,
                                           EndDate = a.EndDate,
                                           HasRegularSchedule = a.CustomSchedule,                                       
                                           SurveyPeriodRegular = (a.CustomSchedule ? a.ConsentCustomSchedules.Select(b => new SurveyPeriodRegularVm()
                                                                    { 
                                                                        StartDate = (new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Day, b.StartTime.Hours, b.StartTime.Minutes, 0)),
                                                                        EndDate = (new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Day, b.EndTime.Hours, b.EndTime.Minutes, 0)),
                                                                        RegularScheduleOpenStatus = ((new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.StartDay) - getDayToday).Day, b.StartTime.Hours, b.StartTime.Minutes, 0) <= _dateTime.ServerTime) && ( _dateTime.ServerTime <=  new DateTime(_dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Year, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Month, _dateTime.ServerTime.AddDays(Convert.ToInt32(b.EndDay) - getDayToday).Day, b.EndTime.Hours, b.EndTime.Minutes, 0)) ? true : false),
                                                                                                                                         
                                                                    }).FirstOrDefault()
                                                                    : null )
                                       })                                       
                                       .ToListAsync(CancellationToken); 


            return Request.CreateApiResult2(GetSurveyActive as object);

          
        }
    }
}
