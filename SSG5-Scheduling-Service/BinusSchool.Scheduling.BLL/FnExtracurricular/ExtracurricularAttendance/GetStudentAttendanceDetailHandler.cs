using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetStudentAttendanceDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentAttendanceDetailHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentAttendanceDetailRequest>(
                            nameof(GetStudentAttendanceDetailRequest.IdExtracurricular),
                            nameof(GetStudentAttendanceDetailRequest.IdExtracurricularGeneratedAtt));

            var extracurricular = await _dbContext.Entity<MsExtracurricular>()
                                        .Where(x => x.Id == param.IdExtracurricular)
                                        .FirstOrDefaultAsync(CancellationToken);

            if (extracurricular == null)
            {
                throw new BadRequestException("Error: Extracurricular is not exists");
            }
            else
            {
                // Check if the extracurricular is active for attendance in report card or no
                bool isShowAttendanceRC = extracurricular.ShowAttendanceRC;

                //if (isShowAttendanceRC == false)
                //{
                //    throw new BadRequestException("Error: This extracurricular does not require attendance entry");
                //}
                //else
                //{

                //}
                var resultList = new List<GetStudentAttendanceDetailResult>();

                // get student participant
                var participantList = _dbContext.Entity<MsExtracurricularParticipant>()
                                        .Include(ep => ep.Student)
                                        .Include(ep => ep.Extracurricular)
                                        .Where(x => x.IdExtracurricular == param.IdExtracurricular && x.Status == true)
                                        .Join(_dbContext.Entity<MsHomeroomStudent>()
                                            .Include(hs => hs.Homeroom)
                                            .ThenInclude(h => h.GradePathwayClassroom)
                                            .ThenInclude(gpc => gpc.Classroom)
                                            .Include(hs => hs.Homeroom)
                                            .ThenInclude(h => h.Grade),
                                            extracurricular => new { p1 = extracurricular.Student.Id, p2 = extracurricular.IdGrade, p3 = extracurricular.Extracurricular.Semester },
                                            homeroom => new { p1 = homeroom.IdStudent, p2 = homeroom.Homeroom.IdGrade, p3 = homeroom.Homeroom.Semester },
                                            (extracurricular, homeroom) => new
                                            {
                                                student = new NameValueVm
                                                {
                                                    Id = extracurricular.Student.Id,
                                                    Name = NameUtil.GenerateFullName(extracurricular.Student.FirstName, extracurricular.Student.LastName)
                                                },
                                                homeroom = new NameValueVm
                                                {
                                                    Id = homeroom.Homeroom.Id,
                                                    Name = string.Format("{0}{1}", homeroom.Homeroom.Grade.Code, homeroom.Homeroom.GradePathwayClassroom.Classroom.Code)
                                                }
                                            })
                                        .Select(x => x)
                                        .Distinct()
                                        .ToList();

                participantList = participantList.OrderBy(x => x.student.Name).ThenBy(x => x.homeroom.Name).ToList();

                var JoinDateStudentList = await _dbContext.Entity<MsExtracurricularParticipant>()
                                            .Where(a => participantList.Select(b => b.student.Id).Contains(a.IdStudent)
                                            && a.IdExtracurricular == param.IdExtracurricular)
                                            .ToListAsync();

                foreach (var participant in participantList)
                {
                    var JoinDateStudent = JoinDateStudentList.Where(a => a.IdStudent == participant.student.Id).FirstOrDefault();

                    var sessionAttendance = _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                                                    .Include(eae => eae.ExtracurricularGeneratedAtt)
                                                    .Include(eae => eae.ExtracurricularStatusAtt)
                                                    .Where(x => x.IdStudent == participant.student.Id &&
                                                                x.ExtracurricularGeneratedAtt.Id == param.IdExtracurricularGeneratedAtt &&
                                                                x.ExtracurricularGeneratedAtt.IdExtracurricular == param.IdExtracurricular)
                                                    .Select(x => new GetStudentAttendanceDetailResult_SessionAttendance
                                                    {
                                                        ExtracurricularStatusAtt = new NameValueVm
                                                        {
                                                            Id = x.ExtracurricularStatusAtt.Id,
                                                            Name = x.ExtracurricularStatusAtt.Description
                                                        },
                                                        NeedReason = x.ExtracurricularStatusAtt.NeedReason,
                                                        Reason = x.Reason
                                                    })
                                                    .FirstOrDefault();

                    resultList.Add(new GetStudentAttendanceDetailResult
                    {
                        Student = participant.student,
                        Homeroom = participant.homeroom,
                        SessionAttendance = new GetStudentAttendanceDetailResult_SessionAttendance
                        {
                            ExtracurricularStatusAtt = new NameValueVm
                            {
                                Id = sessionAttendance?.ExtracurricularStatusAtt.Id,
                                Name = sessionAttendance?.ExtracurricularStatusAtt.Name
                            },
                            NeedReason = sessionAttendance?.NeedReason,
                            Reason = sessionAttendance?.Reason
                        },
                        JoinElectiveDate = JoinDateStudent.JoinDate
                    });
                }

                return Request.CreateApiResult2(resultList as object);
            }
            throw new NotImplementedException();
        }
    }
}
