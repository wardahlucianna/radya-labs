using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{
    public class CheckTeacherOnScheduleRealizationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public CheckTeacherOnScheduleRealizationHandler(
            ISchedulingDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        private static readonly string[] _columns = { "Date", "Session" };

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<CheckTeacherOnScheduleRealizationRequest>(nameof(CheckTeacherOnScheduleRealizationRequest.IdAcademicYear),
                                                                                           nameof(CheckTeacherOnScheduleRealizationRequest.IdUser));
            var dataAY = _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcademicYear).FirstOrDefault();

            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization>(x => x.IdBinusianSubtitute == param.IdUser && x.IsCancel == false && (x.Status == "Subtituted" || x.Status == "Venue Change"));
            
            var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization>()
                                 .Where(predicateSubtitute).FirstOrDefault();

            if(getDataSubtituteTeacher == null)
            {
                var dataSub = new CheckTeacherOnScheduleRealizationResult
                {
                    AcademicYear = new CodeWithIdVm(dataAY.Id,dataAY.Code,dataAY.Description),
                    HaveDailyAttendance = false,
                    HaveSessionAttendance = false
                };
                
                return Request.CreateApiResult2(dataSub as object);
            }

            var dataHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>().Where(x => x.IdBinusian == getDataSubtituteTeacher.IdBinusian).FirstOrDefault();
            var dataLessonTeacher = _dbContext.Entity<MsLessonTeacher>().Where(x => x.IdUser == getDataSubtituteTeacher.IdBinusian).FirstOrDefault();

            var data = new CheckTeacherOnScheduleRealizationResult
            {
                AcademicYear = new CodeWithIdVm(dataAY.Id,dataAY.Code,dataAY.Description),
                HaveDailyAttendance = dataHomeroomTeacher == null ? false : true,
                HaveSessionAttendance = dataLessonTeacher == null ? false : true
            };
            
            return Request.CreateApiResult2(data as object);
        }
    }
}
