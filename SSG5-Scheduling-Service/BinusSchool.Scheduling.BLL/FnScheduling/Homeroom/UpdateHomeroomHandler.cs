using System;
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
using BinusSchool.Scheduling.FnSchedule.Homeroom.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class UpdateHomeroomHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public UpdateHomeroomHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateHomeroomRequest, UpdateHomeroomValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var homeroom = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.AcademicYear)
                .Include(x => x.HomeroomTeachers)
                .FirstOrDefaultAsync(x => x.Id == body.Id, CancellationToken);
            if (homeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", body.Id));

            // if semester 1, update semester 2 too
            var homeroom2 = default(MsHomeroom);
            if (homeroom.Semester == 1)
            {
                homeroom2 = await _dbContext.Entity<MsHomeroom>()
                    .Include(x => x.HomeroomTeachers)
                    .FirstOrDefaultAsync(x
                        => x.IdAcademicYear == homeroom.IdAcademicYear
                        && x.Semester == 2
                        && x.IdGrade == homeroom.IdGrade
                        && x.IdGradePathwayClassRoom == homeroom.IdGradePathwayClassRoom
                        && x.IdGradePathway == homeroom.IdGradePathway,
                        CancellationToken);
            }

            // var updatedHrTeachers = new List<MsHomeroomTeacher>();
            foreach (var hr in new[] { homeroom, homeroom2 })
            {
                if (hr != null)
                {
                    hr.IdVenue = body.IdVenue;
                    _dbContext.Entity<MsHomeroom>().Update(hr);

                    foreach (var existTeacher in hr.HomeroomTeachers)
                    {
                        existTeacher.IsActive = false;
                        _dbContext.Entity<MsHomeroomTeacher>().Update(existTeacher);
                    }

                    foreach (var teacher in body.Teachers)
                    {
                        _dbContext.Entity<MsHomeroomTeacher>().Add(new MsHomeroomTeacher
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroom = hr.Id,
                            IdBinusian = teacher.IdTeacher,
                            IdTeacherPosition = teacher.IdPosition,
                            IsAttendance = teacher.HasAttendance,
                            IsScore = teacher.HasScore,
                            IsShowInReportCard = teacher.ShowInReportCard,
                            DateIn = DateTimeUtil.ServerTime // to order teacher
                        });
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var idSchool = homeroom.AcademicYear.IdSchool;
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
