using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnPeriod;
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
    public class MoveHomeroomHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public MoveHomeroomHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMoveHomeroomRequest, AddMoveHomeroomValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var checkHomerooms = await _dbContext.Entity<MsHomeroom>()
                .Include(e=>e.AcademicYear)
                .Where(x => new[] { body.IdHomeroomNew, body.IdHomeroomOld }.Contains(x.Id))
                .ToListAsync(CancellationToken);
            var srcHomeroom = checkHomerooms.First(x => x.Id == body.IdHomeroomOld);
            if (srcHomeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", body.IdHomeroomOld));
            var destHomeroom = checkHomerooms.First(x => x.Id == body.IdHomeroomNew);
            if (destHomeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", body.IdHomeroomNew));

            // also get homeroom semester 2 if semester request semester 1
            var srcHomeroom2 = default(MsHomeroom);
            var destHomeroom2 = default(MsHomeroom);
            if (destHomeroom.Semester == 1)
            {
                srcHomeroom2 = await _dbContext.Entity<MsHomeroom>()
                    .FirstOrDefaultAsync(x
                        => x.IdAcademicYear == srcHomeroom.IdAcademicYear
                        && x.Semester == 2
                        && x.IdGrade == srcHomeroom.IdGrade
                        && x.IdGradePathwayClassRoom == srcHomeroom.IdGradePathwayClassRoom
                        && x.IdGradePathway == srcHomeroom.IdGradePathway,
                        CancellationToken);

                destHomeroom2 = await _dbContext.Entity<MsHomeroom>()
                    .FirstOrDefaultAsync(x
                        => x.IdAcademicYear == destHomeroom.IdAcademicYear
                        && x.Semester == 2
                        && x.IdGrade == destHomeroom.IdGrade
                        && x.IdGradePathwayClassRoom == destHomeroom.IdGradePathwayClassRoom
                        && x.IdGradePathway == destHomeroom.IdGradePathway,
                        CancellationToken);
            }

            foreach (var (src, dst) in new[] { (srcHomeroom, destHomeroom), (srcHomeroom2, destHomeroom2) }.Where(x => x.Item1 != null))
            {
                var existHrStudents = await _dbContext.Entity<MsHomeroomStudent>()
                    .Where(x => body.IdStudents.Contains(x.IdStudent) && x.IdHomeroom == src.Id)
                    .Include(x => x.HomeroomStudentEnrollments)
                    .ToListAsync(CancellationToken);

                // update student homerooom from request
                foreach (var student in existHrStudents)
                {
                    student.IdHomeroom = dst.Id;
                    _dbContext.Entity<MsHomeroomStudent>().Update(student);
                    
                    // set inactive enrollment
                    foreach (var enrollment in student.HomeroomStudentEnrollments)
                    {
                        enrollment.IsActive = false;
                        _dbContext.Entity<MsHomeroomStudentEnrollment>().Update(enrollment);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            var idSchool = checkHomerooms.Select(e => e.AcademicYear.IdSchool).FirstOrDefault();
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

        protected override void OnFinally()
        {
            _transaction?.Dispose();
        }
    }
}
