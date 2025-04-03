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
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentForSalesForceHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentForSalesForceHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetStudentForSalesForceRequest, GetStudentForSalesForceValidator>();

            var query = _dbContext.Entity<MsStudent>()
                .Include(x => x.Religion)
                .Include(x => x.StudentGrades).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel).ThenInclude(x => x.MsAcademicYear)
                .Include(x => x.AdmissionData)
                .Include(x => x.StudentStatus)
                .Include(x => x.Nationality)
                .Include(x => x.StudentParents).ThenInclude(x => x.Parent).ThenInclude(x => x.ParentRole)
                .Include(x => x.StudentParents).ThenInclude(x => x.Parent).ThenInclude(x => x.Nationality)
                .Include(x => x.StudentParents).ThenInclude(x => x.Parent).ThenInclude(x => x.LtAliveStatus)
                .Include(x => x.StudentParents).ThenInclude(x => x.Parent).ThenInclude(x => x.LastEducationLevel)
                .Include(x => x.SiblingGroup)
                .Where(x => (param.IdStudent == null || !param.IdStudent.Any()) ? true : param.IdStudent.Contains(x.Id))
                .Select(x => new GetStudentForSalesForceResult
                {
                    BinusianId = x.IdBinusian.Trim(),
                    StudentId = x.Id,
                    Name = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                    Religion = x.Religion.ReligionName,
                    Gender = x.Gender.ToString(),
                    Grade = x.StudentGrades.OrderByDescending(y => y.Grade.MsLevel.MsAcademicYear.Code).Select(x => x.Grade.Description).FirstOrDefault(),
                    Level = x.StudentGrades.OrderByDescending(y => y.Grade.MsLevel.MsAcademicYear.Code).Select(x => x.Grade.MsLevel.Description).FirstOrDefault(),
                    AcademicPeriod = x.StudentGrades.OrderByDescending(y => y.Grade.MsLevel.MsAcademicYear.Code).Select(x => x.Grade.MsLevel.MsAcademicYear.Description).FirstOrDefault(),
                    SchoolLocation = x.StudentGrades.OrderByDescending(y => y.Grade.MsLevel.MsAcademicYear.Code).Select(x => x.Grade.MsLevel.MsAcademicYear.MsSchool.Description).FirstOrDefault(),
                    StudentStatus = x.StudentStatus.LongDesc,
                    JoinToSchoolDate = x.AdmissionData.JoinToSchoolDate,
                    NISN = x.NISN,
                    DateOfBirth = x.DOB,
                    PlaceOfBirth = x.POB,
                    Nationality = x.Nationality.NationalityName,
                    NationalIdentityNumber = x.NIK,
                    PhoneNumber = x.MobilePhoneNumber1,
                    MobileNumber = x.MobilePhoneNumber2,
                    AdmissionEmail = x.BinusianEmailAddress,
                    Email = x.PersonalEmailAddress,
                    Address = x.ResidenceAddress,
                    City = x.IdAddressCity,
                    Province = x.IdAddressStateProvince,
                    Country = x.IdAddressCountry,
                    PassportNumber = x.PassportNumber,
                    PassportExpireDate = x.PassportExpDate,
                    DiseaseInformation = x.NotesForSpecialTreatments,
                    SpecialTreatment = x.IsSpecialTreatment == 1,
                    InstagramId = null,
                    LineId = null,
                    Parents = x.StudentParents.Select(y => new GetStudentForSalesForceResult_ParentInformation
                    {
                        ParentType = y.Parent.ParentRole.ParentRoleNameEng,
                        BinusianId = y.Parent.Id,
                        DateOfBirth = y.Parent.DOB,
                        PlaceOfBirth = y.Parent.POB,
                        Email = y.Parent.PersonalEmailAddress,
                        PhoneNumber = y.Parent.MobilePhoneNumber1,
                        Nationality = y.Parent.Nationality.NationalityName,
                        CompanyName = y.Parent.CompanyName,
                        Position = y.Parent.OccupationPosition,
                        Status = y.Parent.LtAliveStatus.AliveStatusName,
                        Education = y.Parent.LastEducationLevel.LastEducationLevelName,
                        Address = y.Parent.ResidenceAddress
                    }).ToList(),
                    Sibings = _dbContext.Entity<MsStudent>()
                    .Include(x => x.SiblingGroup)
                    .Where(y => y.SiblingGroup.Id.Contains(x.SiblingGroup.Id) && y.Id != x.Id)
                    .Select(y => new GetStudentForSalesForceResult_SiblingInformation
                    {
                        BinusianId = y.IdBinusian.Trim(),
                        Name = NameUtil.GenerateFullName(y.FirstName, y.MiddleName, y.LastName),
                        DateOfBirth = y.DOB,
                        PlaceOfBirth = y.POB,
                        Status = y.StudentStatus.LongDesc,
                        Grade = y.StudentGrades.OrderByDescending(z => z.Grade.MsLevel.MsAcademicYear.Code).Select(y => y.Grade.Description).FirstOrDefault(),
                        SchoolLocation = y.StudentGrades.OrderByDescending(z => z.Grade.MsLevel.MsAcademicYear.Code).Select(y => y.Grade.MsLevel.MsAcademicYear.MsSchool.Description).FirstOrDefault(),
                    }).ToList()
                }).AsQueryable();

            var data = await query.SetPagination(param).ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(data.Count)
                ? data.Count
                : await query.Select(x => x).CountAsync();

            return Request.CreateApiResult2(data as object);
        }
    }
}
