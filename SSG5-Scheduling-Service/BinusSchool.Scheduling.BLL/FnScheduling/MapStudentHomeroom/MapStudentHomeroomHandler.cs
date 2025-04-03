using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.MapStudentHomeroom.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class MapStudentHomeroomHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public MapStudentHomeroomHandler(ISchedulingDbContext dbContext, IMessage messageService, IEventSchool eventSchool)
        {
            _dbContext = dbContext;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var items = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(e=>e.Homeroom).ThenInclude(e=>e.AcademicYear)
                .Include(x => x.HomeroomStudentEnrollments)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(items.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var item in items)
            {
                // don't set inactive when row have to-many relation
                if (item.HomeroomStudentEnrollments.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(item.Id, string.Format(Localizer["ExAlreadyUse"], string.Empty));
                }
                else
                {
                    item.IsActive = false;
                    _dbContext.Entity<MsHomeroomStudent>().Update(item);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            var idSchool = items.Select(e => e.Homeroom.AcademicYear.IdSchool).FirstOrDefault();
            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = idSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = idSchool
            });

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMapStudentHomeroomRequest, AddMapStudentHomeroomValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var homeroom = await _dbContext.Entity<MsHomeroom>()
                .Include(x => x.HomeroomStudents)
                .Include(x => x.AcademicYear)
                .FirstOrDefaultAsync(x => x.Id == body.IdHomeroom, CancellationToken);
            if (homeroom is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Homeroom"], "Id", body.IdHomeroom));

            var studentAlreadyMappeds = await _dbContext.Entity<MsHomeroomStudent>()
                .Where(x
                    => x.Homeroom.IdGrade == homeroom.IdGrade
                    && body.Students.Select(y => y.IdStudent).Contains(x.IdStudent)
                    && x.IdHomeroom == homeroom.Id)
                .Select(x => x.IdStudent)
                .ToListAsync(CancellationToken);

            if (studentAlreadyMappeds.Count != 0 && body.FromCopySemester == false)
                throw new BadRequestException(string.Format(Localizer["ScheduleStudentAlreadyMap"], string.Join(", ", studentAlreadyMappeds)));

            var stdAlreadyHaveHomerooms = await _dbContext.Entity<MsHomeroomStudent>()
                .Where(x => x.Homeroom.IdGrade == homeroom.IdGrade
                            && body.Students.Select(y => y.IdStudent).Contains(x.IdStudent)
                            && x.Semester == homeroom.Semester)
                .Select(x => x.IdStudent)
                .ToListAsync(CancellationToken);

            if (stdAlreadyHaveHomerooms.Count != 0 && body.FromCopySemester == false)
                throw new BadRequestException(string.Format(Localizer["ScheduleStudentAlreadyMap"], string.Join(", ", stdAlreadyHaveHomerooms)));

            foreach (var student in body.Students)
            {
                if (body.FromCopySemester == false)
                {
                    var existStudent = homeroom.HomeroomStudents.FirstOrDefault(x => x.IdStudent == student.IdStudent && x.IdHomeroom == body.IdHomeroom);
                    if (existStudent is null)
                    {
                        var newHrStudent = new MsHomeroomStudent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroom = homeroom.Id,
                            IdStudent = student.IdStudent,
                            Gender = student.Gender,
                            Religion = student.Religion,
                            Semester = homeroom.Semester
                        };
                        _dbContext.Entity<MsHomeroomStudent>().Add(newHrStudent);
                    }
                    else
                    {
                        existStudent.Gender = student.Gender;
                        existStudent.Religion = student.Religion;
                        _dbContext.Entity<MsHomeroomStudent>().Update(existStudent);
                    }
                }
                else
                {
                    if (!stdAlreadyHaveHomerooms.Contains(student.IdStudent))
                    {
                        var newHrStudent = new MsHomeroomStudent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHomeroom = homeroom.Id,
                            IdStudent = student.IdStudent,
                            Gender = student.Gender,
                            Religion = student.Religion,
                            Semester = homeroom.Semester
                        };
                        _dbContext.Entity<MsHomeroomStudent>().Add(newHrStudent);
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = homeroom.AcademicYear.IdSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = homeroom.AcademicYear.IdSchool
            });
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new System.NotImplementedException();
        }
    }
}
