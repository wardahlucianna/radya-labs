using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Utilities;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentInformationWithStudentStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly string _codeCrypt = "S1W55";
        public GetStudentInformationWithStudentStatusHandler(IStudentDbContext dbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentInformationWithStudentStatusRequest>(nameof(GetStudentInformationWithStudentStatusRequest.IdStudentEncrypt), nameof(GetStudentInformationWithStudentStatusRequest.IdSchool));

            var split = SplitIdDatetimeUtil.Split(EncryptStringUtil.Decrypt(HttpUtility.UrlDecode(param.IdStudentEncrypt), _codeCrypt));

            if (!split.IsValid)
            {
                throw new BadRequestException(split.ErrorMessage);
            }
            var StudentId = split.Id;

            if (StudentId.Where(char.IsDigit).ToArray().Length != 10)
            {
                throw new BadRequestException("Student ID incorrect format");
            }

            var AYActive = await _dbContext.Entity<MsPeriod>()
                            .Where(a => a.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool
                             && (_dateTime.ServerTime > a.StartDate && a.EndDate > _dateTime.ServerTime))
                            .Select(a => new
                            {
                                IdAcademicYear = a.Grade.MsLevel.IdAcademicYear,
                                IdGrade = a.IdGrade,
                                Semester = a.Semester,
                                IdPeriod = a.Id
                            })
                            .ToListAsync(CancellationToken);

            var getStudentStatus = _dbContext.Entity<TrStudentStatus>()
                            .Include(x => x.StudentStatus)
                            .Where(a => a.IdStudent == StudentId
                            && a.IdAcademicYear == AYActive.First().IdAcademicYear)
                            .OrderBy(a => a.CurrentStatus).ThenByDescending(a => a.StartDate)
                            .FirstOrDefault();

            if(getStudentStatus == null)
            {

                var checkStudentStatus = _dbContext.Entity<TrStudentStatus>()
                          .Include(x => x.StudentStatus)
                          .Where(a => a.IdStudent == StudentId)
                          .OrderBy(a => a.CurrentStatus).ThenByDescending(a => a.StartDate)
                          .FirstOrDefault();
                
                if(checkStudentStatus == null)
                {
                    throw new BadRequestException("Student status not found");
                }
                else if(checkStudentStatus.CurrentStatus == "H")
                {
                    throw new BadRequestException("Student status not active (" + checkStudentStatus.StudentStatus.ShortDesc + ")");
                }
                else
                {
                    throw new BadRequestException("Student status not found");
                }
                
            }

            var getStudentData = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                        .ThenInclude(y => y.Grade)
                        .ThenInclude(y => y.MsLevel)
                        .ThenInclude(y => y.MsAcademicYear)
                    .Include(x => x.Homeroom)
                        .ThenInclude(y => y.MsGradePathwayClassroom)
                        .ThenInclude(y => y.Classroom)
                  .Where(a => a.IdStudent == StudentId 
                  && AYActive.Select(b => b.IdGrade).Contains(a.Homeroom.IdGrade)
                  )
                  .Select(x => new GetStudentInformationWithStudentStatusResult
                  {
                      Student = new NameValueVm
                      {
                          Id = x.Id,
                          Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName)
                      },
                      Level = new ItemValueVm
                      {
                          Id = x.Homeroom.Grade.MsLevel.Id,
                          Description = x.Homeroom.Grade.MsLevel.Description
                      },
                      Grade = new ItemValueVm
                      {
                          Id = x.Homeroom.Grade.Id,
                          Description = x.Homeroom.Grade.Description
                      },
                      Homeroom = new ItemValueVm
                      {
                          Id = x.Homeroom.Id,
                          Description = x.Homeroom.Grade.Code + x.Homeroom.MsGradePathwayClassroom.Classroom.Code
                      },
                      StudentStatus = new GetStudentInformation_StudentStatus
                      { 
                          AcademicYear = new ItemValueVm
                          {
                              Id = x.Homeroom.Grade.MsLevel.IdAcademicYear,
                              Description = x.Homeroom.Grade.MsLevel.MsAcademicYear.Description
                          },
                          Id = (getStudentStatus != null ? getStudentStatus.IdStudentStatus.ToString() : ""),
                          Description = (getStudentStatus != null ? getStudentStatus.StudentStatus.LongDesc : ""),
                          statusStartDate = (getStudentStatus != null ? getStudentStatus.StartDate : new DateTime())
                      }
                  })
            .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(getStudentData as object);
        }
    }
}
