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

namespace BinusSchool.Scheduling.FnSchedule.ScheduleRealizationV2
{

    public class GetDownloadScheduleRealizationV2Handler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDownloadScheduleRealizationV2Handler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDownloadScheduleRealizationRequest>(nameof(GetDownloadScheduleRealizationRequest.IsByDate), nameof(GetDownloadScheduleRealizationRequest.Ids));

            List<GetDownloadScheduleRealizationResult> listData = new List<GetDownloadScheduleRealizationResult>();

            foreach(var dataId in param.Ids)
            {
                var predicate = PredicateBuilder.Create<MsScheduleLesson>(x => x.Id == dataId);
                
                var dataGenerate = _dbContext.Entity<MsScheduleLesson>().Where(predicate).FirstOrDefault();

                var dataHomeroom = _dbContext.Entity<MsHomeroom>()
                    .Include(x => x.Grade)
                    .Include(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom);

                var dataScheduleRealiationList = _dbContext.Entity<TrScheduleRealization2>().Where(x => x.ScheduleDate == dataGenerate.ScheduleDate && x.SessionID == dataGenerate.SessionID && x.ClassID == dataGenerate.ClassID).ToList();
                                
                if(dataGenerate != null && dataScheduleRealiationList.Count > 0)
                {       
                    foreach(var dataScheduleRealiation in dataScheduleRealiationList)
                    {
                        if ((dataScheduleRealiation.TeacherName != dataScheduleRealiation.TeacherNameSubtitute) || (dataScheduleRealiation.VenueName != dataScheduleRealiation.VenueNameChange))
                        {
                            var dataSchedule = new GetDownloadScheduleRealizationResult()
                            {
                                StartDate = param.StartDate,
                                EndDate = param.IsByDate ? param.StartDate : param.EndDate,
                                SessionID = dataGenerate.SessionID,
                                SessionStartTime = dataGenerate.StartTime,
                                SessionEndTime = dataGenerate.EndTime,
                                ClassID = dataGenerate.ClassID,
                                SubtituteTeacher = dataScheduleRealiation.TeacherNameSubtitute,
                                AbsentTeacher = dataScheduleRealiation.TeacherName,
                                VenueName = dataGenerate.VenueName,
                                VenueNameOld = dataScheduleRealiation.VenueNameChange,
                                Subject = dataGenerate.SubjectName,
                                //Homeroom = dataHomeroom.Where(x => x.IdGrade == dataScheduleRealiation.IdGrade && x.IdVenue == dataScheduleRealiation.IdVenue).Select(x => x.Grade.Description + x.GradePathwayClassroom.Classroom.Description).FirstOrDefault(),
                                Homeroom = dataScheduleRealiation.VenueName != dataScheduleRealiation.VenueNameChange ? dataScheduleRealiation.VenueNameChange : dataScheduleRealiation.VenueName,
                                NotesForSubstitution = dataScheduleRealiation.NotesForSubtitutions,
                                LogoSchool = "https://bss-webclient-prod.azurewebsites.net/assets/img/binus_logo_big.png",
                                IsByDate = param.IsByDate
                            };
                            listData.Add(dataSchedule);
                        }
                    }
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
                IsByDate = x.IsByDate,
                NotesForSubstitution = x.NotesForSubstitution
            }).Distinct().ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
