using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.Homeroom.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class AddHomeroomHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public AddHomeroomHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddHomeroomRequest, AddHomeroomValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var existHomeroom = await _dbContext.Entity<MsHomeroom>()
                .FirstOrDefaultAsync(x
                    => x.IdAcademicYear == body.IdAcadyear
                    && x.Semester == body.Semester
                    && x.IdGrade == body.IdGrade
                    && x.IdGradePathwayClassRoom == body.IdClassroom
                    && x.IdGradePathway == body.IdPathway, CancellationToken);
            if (existHomeroom != null)
            {
                var queryClassroom = await _dbContext.Entity<MsGradePathwayClassroom>()
                    .Where(x => x.Id == body.IdClassroom)
                    .Select(x => new CodeVm
                    {
                        Code = x.Classroom.Code,
                        Description = x.Classroom.Description
                    })
                    .FirstOrDefaultAsync(CancellationToken);
                
                throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], Localizer["Homeroom"], Localizer["Classroom"],
                    queryClassroom != null
                        ? $"{queryClassroom.Code} ({queryClassroom.Description})"
                        : body.IdClassroom));
            }

            var semesters = new[] { body.Semester }.AsEnumerable();
            if (body.Semester == 1)
            {
                var querySemester = await _dbContext.Entity<MsPeriod>()
                    .Where(x => x.Id == body.IdGrade && x.Semester > body.Semester)
                    .Select(x => x.Semester)
                    .Distinct()
                    .ToListAsync(CancellationToken);
                
                if (querySemester.Count != 0)
                    semesters = semesters.Concat(querySemester);
            }

            // loop by semester
            foreach (var semester in semesters)
            {
                var homeroom = new MsHomeroom
                {
                    Id = Guid.NewGuid().ToString(),
                    IdAcademicYear = body.IdAcadyear,
                    Semester = semester,
                    IdGrade = body.IdGrade,
                    IdGradePathwayClassRoom = body.IdClassroom,
                    IdVenue = body.IdVenue,
                    IdGradePathway = body.IdPathway
                };
                _dbContext.Entity<MsHomeroom>().Add(homeroom);

                if (body.IdPathwayDetails?.Any() ?? false)
                {
                    foreach (var idPathwayDetail in body.IdPathwayDetails)
                    {
                        var hrPathway = new MsHomeroomPathway
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroom = homeroom.Id,
                            IdGradePathwayDetail = idPathwayDetail
                        };
                        _dbContext.Entity<MsHomeroomPathway>().Add(hrPathway);
                    }
                }

                foreach (var teacher in body.Teachers)
                {
                    _dbContext.Entity<MsHomeroomTeacher>().Add(new MsHomeroomTeacher
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdHomeroom = homeroom.Id,
                        IdBinusian = teacher.IdTeacher,
                        IdTeacherPosition = teacher.IdPosition,
                        IsAttendance = teacher.HasAttendance,
                        IsScore = teacher.HasScore,
                        IsShowInReportCard = teacher.ShowInReportCard,
                        DateIn = DateTimeUtil.ServerTime
                    });
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                .Where(x=>x.Id==body.IdAcadyear)
                .Select(e=>e.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = idSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = idSchool
            });

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }
    }
}
