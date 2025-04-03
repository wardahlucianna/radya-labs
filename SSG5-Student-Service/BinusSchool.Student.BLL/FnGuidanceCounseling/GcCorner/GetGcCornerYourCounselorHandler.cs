using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.GcCorner;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.GcCorner
{
    public class GetGcCornerYourCounselorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetGcCornerYourCounselorHandler(IStudentDbContext studentDbContext, IMachineDateTime dateTime)
        {
            _dbContext = studentDbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetGcCornerYourCounselorRequest>();

            var dataCouncelor = new GetGcCornerYourCounselorResult();

            var GetSemesterAy = await _dbContext.Entity<MsPeriod>()
                                .Include(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                                .Where(e => e.StartDate <= _dateTime.ServerTime.Date && e.EndDate >= _dateTime.ServerTime.Date && e.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear)
                                .Select(e => new { Semester = e.Semester, IdAcademicYear = e.Grade.MsLevel.IdAcademicYear })
                                .Distinct().SingleOrDefaultAsync(CancellationToken);

            var dataStudent = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(e => e.Student)
                    .Include(e => e.Homeroom)
                        .ThenInclude(e => e.MsGradePathwayClassroom)
                            .ThenInclude(e => e.GradePathway)
                                .ThenInclude(e => e.Grade)
                                    .ThenInclude(e => e.MsLevel)
                                        .ThenInclude(e => e.MsAcademicYear)
                    .Include(e => e.Homeroom)
                         .ThenInclude(e => e.MsGradePathwayClassroom)
                            .ThenInclude(e => e.Classroom)
                     .Where(e => e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.MsAcademicYear.Id == param.IdAcademicYear && e.Student.Id == param.IdStudent && e.Homeroom.Semester == GetSemesterAy.Semester)
                     .Select(e => new
                     {
                         Id = e.Id,
                         IdGrade = e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Id,
                     }).SingleOrDefaultAsync(CancellationToken);

            if(dataStudent != null)
            {
                dataCouncelor = await _dbContext.Entity<MsCounselorGrade>()
                       .Include(p => p.Counselor).ThenInclude(p => p.AcademicYear)
                       .Where(p => p.Grade.Id == dataStudent.IdGrade)
                       .Select(x => new GetGcCornerYourCounselorResult
                       {
                           Id = x.Id,
                           Name = x.Counselor.User.DisplayName,
                           AcademicYear = new CodeWithIdVm
                           {
                               Id = x.Counselor.AcademicYear.Id,
                               Code = x.Counselor.AcademicYear.Code,
                               Description = x.Counselor.AcademicYear.Description
                           },
                           OfficeLocation = x.Counselor.OfficerLocation,
                           ExtensionNumber = x.Counselor.ExtensionNumber,
                           Email = x.Counselor.User.Email == null ? "-" : x.Counselor.User.Email,
                           OtherInformation = x.Counselor.OtherInformation,
                           Photo = x.Counselor.CounselorPhoto.Select(e => new PhotoGcCournerCounsellor
                           {
                               Id = e.Id,
                               Url = e.Url,
                               OriginalFilename = e.OriginalName,
                               FileName = e.FileName,
                               FileSize = e.FileSize,
                               FileType = e.FileType,
                           }).ToList()
                       }).FirstOrDefaultAsync(CancellationToken);
            }

            

            if (dataCouncelor.Id == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Your counselor"], "Id", param.IdStudent));


            return Request.CreateApiResult2(dataCouncelor as object);
        }
    }
}
