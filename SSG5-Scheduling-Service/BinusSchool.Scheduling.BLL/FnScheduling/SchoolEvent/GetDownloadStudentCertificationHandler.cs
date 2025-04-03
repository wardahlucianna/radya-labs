using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Domain.Extensions;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.User;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDownloadStudentCertificationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDownloadStudentCertificationHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        private string GetSignature1Name(MsCertificateTemplate msCertificateTemplate)
        {
            if (msCertificateTemplate.Signature1 == null) return "-";
            return msCertificateTemplate.User1.DisplayName;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDownloadStudentCertificationRequest>(nameof(GetDownloadStudentCertificationRequest.IdStudent), nameof(GetDownloadStudentCertificationRequest.IdAcadYears));

            var checkDataStudent = _dbContext.Entity<MsStudent>().Where(x => x.Id == param.IdStudent).FirstOrDefault();

            if(checkDataStudent == null)
                throw new BadRequestException($"Data Student not found");

            var addmissionAY = _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcadYears.FirstOrDefault()).FirstOrDefault();
            var currentAY = _dbContext.Entity<MsAcademicYear>().Where(x => x.Id == param.IdAcadYears.LastOrDefault()).FirstOrDefault();

            var dataPrincipal = await (from trntl in _dbContext.Entity<TrNonTeachingLoad>()
                                   join msntl in _dbContext.Entity<MsNonTeachingLoad>() on trntl.IdMsNonTeachingLoad equals msntl.Id
                                   join mstp in _dbContext.Entity<MsTeacherPosition>() on msntl.IdTeacherPosition equals mstp.Id
                                   join lp in _dbContext.Entity<LtPosition>() on mstp.IdPosition equals lp.Id
                                   join u in _dbContext.Entity<MsUser>() on trntl.IdUser equals u.Id
                                   join us in _dbContext.Entity<MsUserSchool>() on u.Id equals us.IdUser
                                   where us.IdSchool == currentAY.IdSchool && lp.Code == "VP"
                                    
                                  select new MsUser
                                  {
                                      Id = u.Id,
                                      DisplayName = u.DisplayName
                                  }).FirstOrDefaultAsync(CancellationToken);
                                  
            var predicate = PredicateBuilder.Create<TrEventActivity>(x => true);

            var dataStudent = _dbContext.Entity<MsStudent>()
                .Include(x => x.School)
            .Where(x => x.Id == param.IdStudent)
            .Select(x => new {
                x.Id,
                x.FirstName,
                x.MiddleName,
                x.LastName,
                x.Gender,
                x.School.Name,
                x.School.Description,
                x.School.Address,
                x.School.Ext,
                x.School.Telephone
            })
            .FirstOrDefault();

            var query = _dbContext.Entity<TrEventActivity>()
                .Include(x => x.Activity)
                .Include(x => x.Event)
                    .ThenInclude( x => x.AcademicYear)
                        .ThenInclude( x => x.School)
                .Include(x => x.EventActivityAwards)
                    .ThenInclude(x => x.Award)
                .Include(x => x.EventActivityAwards)
                    .ThenInclude(x => x.HomeroomStudent)
                        .ThenInclude(x => x.Student)
                .Where(predicate).Where(x => x.Event.StatusEvent == "Approved" && x.Event.StatusEventAward == "Approved" && x.EventActivityAwards.Any(y => y.HomeroomStudent.Student.Id == param.IdStudent));

            var trEvent = await query
                    .ToListAsync(CancellationToken);

            if(trEvent.Count() < 1)
            {
                var result = new GetDownloadStudentCertificationResult
                    {
                        Id = dataStudent.Id,
                        StudentName = dataStudent.FirstName + " " + dataStudent.LastName,
                        SchoolName = dataStudent.Name,
                        AdmissionAY = addmissionAY.Description,
                        CurrentAY = currentAY.Description,
                        Gender = dataStudent.Gender == Gender.Female ? "Her" : "Him",
                        VicePrincipalName = dataPrincipal != null ? dataPrincipal.DisplayName : "-",
                        SchoolDescription = dataStudent.Description,
                        SchoolAddress = dataStudent.Address,
                        SchoolTel = dataStudent.Telephone,
                        SchoolExt = dataStudent.Ext,
                        SchoolEmail = "adminbinus.edu"
                    };

                return Request.CreateApiResult2(result as object);
            }
            else
            {
                var result = trEvent
                    .Select(x => new GetDownloadStudentCertificationResult
                    {
                        Id = dataStudent.Id,
                        StudentName = dataStudent.FirstName + " " + dataStudent.LastName,
                        SchoolName = dataStudent.Name,
                        AdmissionAY = addmissionAY.Description,
                        CurrentAY = currentAY.Description,
                        Gender = dataStudent.Gender == Gender.Female ? "Her" : "Him",
                        VicePrincipalName = dataPrincipal != null ? dataPrincipal.DisplayName : "-",
                        SchoolDescription = dataStudent.Description,
                        SchoolAddress = dataStudent.Address,
                        SchoolTel = dataStudent.Telephone,
                        SchoolExt = dataStudent.Ext,
                        SchoolEmail = "adminbinus.edu",
                        DataActivityAward = x.EventActivityAwards != null ? x.EventActivityAwards.Where(y => y.HomeroomStudent.IdStudent == param.IdStudent).Select(y => new DataActivityAward{
                            SchoolActivityName = y.EventActivity.Activity.Description,
                            DateYear = y.EventActivity.Event.AcademicYear.Code,
                            StatusInvolvement = y.Award.Description
                        }).ToList() : null
                    }).FirstOrDefault();

                return Request.CreateApiResult2(result as object);
            }
        }
    }
}
