using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetStudentParentHomeroomInformationHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetStudentParentHomeroomInformationHandler(
            IDocumentDbContext dbContext,
            IMachineDateTime dateTime
            )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentParentHomeroomInformationRequest>(
                            nameof(GetStudentParentHomeroomInformationRequest.IdSchool),
                            nameof(GetStudentParentHomeroomInformationRequest.IdStudent));

            // get Active AY
            var getActiveAYSemester = _dbContext.Entity<MsPeriod>()
                                    .Include(x => x.Grade)
                                    .Include(x => x.Grade.Level)
                                    .Include(x => x.Grade.Level.AcademicYear)
                                    .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                    .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                                    .OrderByDescending(x => x.StartDate)
                                    .Select(x => new
                                    {
                                        IdAcademicYear = x.Grade.Level.AcademicYear.Id,
                                        AcademicYearDescription = x.Grade.Level.AcademicYear.Description,
                                        Semester = x.Semester
                                    })
                                    .FirstOrDefault();

            var studentInformation = await _dbContext.Entity<MsStudent>()
                                        .Include(x => x.StudentStatus)
                                        .Include(x => x.TrStudentStatuss)
                                            .ThenInclude(x => x.StudentStatus)
                                        .Include(x => x.AdmissionData)
                                        .Include(x => x.HomeroomStudents)
                                            .ThenInclude(x => x.Homeroom)
                                            .ThenInclude(x => x.Grade)
                                            .ThenInclude(x => x.Level)
                                        .Include(x => x.HomeroomStudents)
                                            .ThenInclude(x => x.Homeroom)
                                            .ThenInclude(x => x.GradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                        .Where(x => x.Id == param.IdStudent)
                                        .Select(x => new GetStudentParentHomeroomInformationResult_StudentInformation
                                        {
                                            Student = new NameValueVm
                                            {
                                                Id = x.Id,
                                                Name = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName)
                                            },
                                            BirthDate = x.DOB,
                                            AcademicYear = new ItemValueVm
                                            {
                                                Id = getActiveAYSemester.IdAcademicYear,
                                                Description = getActiveAYSemester.AcademicYearDescription
                                            },
                                            Semester = getActiveAYSemester.Semester,
                                            Homeroom = x.HomeroomStudents
                                                        .Where(x =>
                                                            x.Homeroom.Grade.Level.IdAcademicYear == getActiveAYSemester.IdAcademicYear &&
                                                            x.Homeroom.Semester == getActiveAYSemester.Semester)
                                                        .Select(x => new ItemValueVm
                                                        {
                                                            Id = x.IdHomeroom,
                                                            Description = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                                        })
                                                        .FirstOrDefault(),
                                            StudentStatus = x.TrStudentStatuss
                                                        .Where(y => y.CurrentStatus == "A")
                                                        .OrderByDescending(y => y.StartDate)
                                                        .Any() ? 
                                                        x.TrStudentStatuss
                                                        .Where(y => y.CurrentStatus == "A")
                                                        .OrderByDescending(y => y.StartDate)
                                                        .Select(y => new GetStudentParentHomeroomInformationResult_StudentStatusInfo
                                                        {
                                                            Id = y.IdStudentStatus.ToString(),
                                                            Description = y.StudentStatus.LongDesc,
                                                            StudentStatusStartDate = y.StartDate
                                                        })
                                                        .FirstOrDefault() :
                                                        new GetStudentParentHomeroomInformationResult_StudentStatusInfo
                                                        {
                                                            Id = x.IdStudentStatus.ToString(),
                                                            Description = x.StudentStatus.LongDesc,
                                                            StudentStatusStartDate = null
                                                        },
                                            JoinDate = x.AdmissionData.JoinToSchoolDate
                                        })
                                        .FirstOrDefaultAsync(CancellationToken);

            var parentInformationList = await _dbContext.Entity<MsStudentParent>()
                                            .Include(x => x.Parent)
                                                .ThenInclude(x => x.ParentRole)
                                            .Where(x => x.IdStudent == param.IdStudent)
                                            .Select(x => new GetStudentParentHomeroomInformationResult_ParentInformation
                                            {
                                                Parent = new NameValueVm
                                                {
                                                    Id = x.Parent.Id,
                                                    Name = NameUtil.GenerateFullName(x.Parent.FirstName, x.Parent.MiddleName, x.Parent.LastName)
                                                },
                                                ParentRole = new ItemValueVm
                                                {
                                                    Id = x.Parent.ParentRole.Id,
                                                    Description = x.Parent.ParentRole.ParentRoleNameEng
                                                },
                                                Email = x.Parent.PersonalEmailAddress,
                                                MobilePhoneNumber = x.Parent.MobilePhoneNumber1,
                                                ResidencePhoneNumber = x.Parent.ResidencePhoneNumber
                                            })
                                            .OrderBy(x => x.ParentRole.Description)
                                            .ToListAsync(CancellationToken);

            var homeroomHistoryList = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(x => x.Homeroom)
                                            .ThenInclude(x => x.Grade)
                                            .ThenInclude(x => x.Level)
                                            .ThenInclude(x => x.AcademicYear)
                                        .Include(x => x.Homeroom)
                                            .ThenInclude(x => x.HomeroomTeachers)
                                            .ThenInclude(x => x.Staff)
                                        .Include(x => x.Homeroom)
                                            .ThenInclude(x => x.GradePathwayClassroom)
                                            .ThenInclude(x => x.Classroom)
                                        .Where(x => x.IdStudent == param.IdStudent)
                                        .OrderBy(x => x.Homeroom.Grade.Level.AcademicYear.Code)
                                        .ThenBy(x => x.Homeroom.Semester)
                                        .Select(x => new GetStudentParentHomeroomInformationResult_HomeroomHistory
                                        {
                                            AcademicYear = new ItemValueVm
                                            {
                                                Id = x.Homeroom.Grade.Level.AcademicYear.Id,
                                                Description = x.Homeroom.Grade.Level.AcademicYear.Description
                                            },
                                            Level = new ItemValueVm
                                            {
                                                Id = x.Homeroom.Grade.Level.Id,
                                                Description = x.Homeroom.Grade.Level.Description
                                            },
                                            Grade = new ItemValueVm
                                            {
                                                Id = x.Homeroom.Grade.Id,
                                                Description = x.Homeroom.Grade.Description
                                            },
                                            Homeroom = new ItemValueVm
                                            {
                                                Id = x.Homeroom.Id,
                                                Description = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.Classroom.Description
                                            },
                                            Semester = x.Homeroom.Semester,
                                            HomeroomTeacher = x.Homeroom.HomeroomTeachers
                                                                .Select(x => new NameValueVm
                                                                {
                                                                    Id = x.Id,
                                                                    Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                                                })
                                                                .FirstOrDefault()
                                        })
                                        .ToListAsync(CancellationToken);

            var result = new GetStudentParentHomeroomInformationResult
            {
                StudentInformation = studentInformation,
                ParentInformationList = parentInformationList,
                HomeroomHistoryList = homeroomHistoryList
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
