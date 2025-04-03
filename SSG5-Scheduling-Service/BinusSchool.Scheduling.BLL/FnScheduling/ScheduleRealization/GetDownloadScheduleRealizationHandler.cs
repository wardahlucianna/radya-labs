using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealization;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.ScheduleRealization.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Auth.Authentications.Jwt;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealization
{

    public class GetDownloadScheduleRealizationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDownloadScheduleRealizationHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDownloadScheduleRealizationRequest>(nameof(GetDownloadScheduleRealizationRequest.IsByDate), nameof(GetDownloadScheduleRealizationRequest.Ids));

            List<GetDownloadScheduleRealizationResult> listData = new List<GetDownloadScheduleRealizationResult>();

            foreach(var dataId in param.Ids)
            {
                var predicate = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x => x.Id == dataId);
                
                var dataGenerate = _dbContext.Entity<TrGeneratedScheduleLesson>().Where(predicate).FirstOrDefault();

                var dataScheduleRealiation = _dbContext.Entity<TrScheduleRealization>().Where(x => x.IdGeneratedScheduleLesson == dataId).FirstOrDefault();

                if(dataGenerate != null)
                {
                    var dataSchedule = new GetDownloadScheduleRealizationResult()
                    {
                        StartDate = param.StartDate,
                        EndDate =  param.IsByDate ? param.StartDate : param.EndDate,
                        SessionID = dataGenerate.SessionID,
                        SessionStartTime = dataGenerate.StartTime,
                        SessionEndTime = dataGenerate.EndTime,
                        ClassID = dataGenerate.ClassID,
                        SubtituteTeacher = dataGenerate.TeacherName,
                        AbsentTeacher = dataGenerate.TeacherNameOld,
                        VenueName = dataGenerate.VenueName,
                        VenueNameOld = dataGenerate.VenueNameOld,
                        Subject = dataGenerate.SubjectName,
                        Homeroom = dataGenerate.HomeroomName,
                        NotesForSubstitution = dataScheduleRealiation != null ? dataScheduleRealiation.NotesForSubtitutions : null,
                        LogoSchool = "https://bss-webclient-prod.azurewebsites.net/assets/img/binus_logo_big.png",
                        IsByDate = param.IsByDate
                    };
                    listData.Add(dataSchedule);
                }
            }

            var result = listData.Select(x => new GetDownloadScheduleRealizationResult
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                SessionID = x.SessionID,
                SessionStartTime = x.SessionStartTime,
                SessionEndTime = x.SessionEndTime,
                ClassID = x.ClassID,
                SubtituteTeacher = x.SubtituteTeacher,
                AbsentTeacher = x.AbsentTeacher,
                VenueName = x.VenueName,
                VenueNameOld = x.VenueNameOld,
                Subject = x.Subject,
                Homeroom = x.Homeroom,
                LogoSchool = x.LogoSchool,
                IsByDate = x.IsByDate
            }).Distinct().ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
