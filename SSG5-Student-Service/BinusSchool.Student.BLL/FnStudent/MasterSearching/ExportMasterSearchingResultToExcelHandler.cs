using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class ExportMasterSearchingResultToExcelHandler : FunctionsHttpSingleHandler
    {

        private readonly IStudentDbContext _dbContext;
        private readonly IStudentEnrollmentDetail _studentEnrollmentService;
        public ExportMasterSearchingResultToExcelHandler(IStudentDbContext schoolDbContext,
                                                            IStudentEnrollmentDetail studentEnrollmentService)
        {
            _dbContext = schoolDbContext;
            _studentEnrollmentService = studentEnrollmentService;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            #region cara lama
            //var param = await Request.GetBody<ExportToExcelMasterSearchingDataRequest>();

            //if (param.GetAll != null && param.GetAll == false)
            //{

            //    #region Create Dynamic Where

            //    var predicate = PredicateBuilder.False<GetMasterSearchingDataResult>();

            //    #region student
            //    if (!string.IsNullOrEmpty(param.BinusianID))
            //    {
            //        predicate = predicate.Or(s => s.BinusianID.Contains(param.BinusianID));
            //    }

            //    if (!string.IsNullOrEmpty(param.StudentName))
            //    {
            //        predicate = predicate.Or(s => s.StudentName.Contains(param.StudentName));
            //    }

            //    if (!string.IsNullOrEmpty(param.ReligionName))
            //    {
            //        predicate = predicate.Or(s => s.ReligionName.Contains(param.ReligionName));
            //    }

            //    if (!string.IsNullOrEmpty(param.BinusEmailAddress))
            //    {
            //        predicate = predicate.Or(s => s.BinusEmailAddress.Contains(param.BinusEmailAddress));
            //    }
            //    #endregion

            //    #region father
            //    if (!string.IsNullOrEmpty(param.FatherName))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherName));
            //    }

            //    if (!string.IsNullOrEmpty(param.FatherMobilePhoneNumber1))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherMobilePhoneNumber1));
            //    }

            //    if (!string.IsNullOrEmpty(param.FatherResidenceAddress))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherResidenceAddress));
            //    }

            //    if (!string.IsNullOrEmpty(param.FatherEmailAddress))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherEmailAddress));
            //    }

            //    if (!string.IsNullOrEmpty(param.FatherCompanyName))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherCompanyName));
            //    }

            //    if (!string.IsNullOrEmpty(param.FatherOccupationPosition))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherOccupationPosition));
            //    }

            //    if (!string.IsNullOrEmpty(param.FatherOfficeEmail))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.FatherOfficeEmail));
            //    }
            //    #endregion

            //    #region Mother
            //    if (!string.IsNullOrEmpty(param.MotherName))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.MotherName));
            //    }

            //    if (!string.IsNullOrEmpty(param.MotherMobilePhoneNumber1))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.MotherMobilePhoneNumber1));
            //    }

            //    if (!string.IsNullOrEmpty(param.MotherResidenceAddress))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.MotherResidenceAddress));
            //    }

            //    if (!string.IsNullOrEmpty(param.MotherEmailAddress))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.MotherEmailAddress));
            //    }

            //    if (!string.IsNullOrEmpty(param.MotherCompanyName))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.MotherCompanyName));
            //    }

            //    if (!string.IsNullOrEmpty(param.MotherOccupationPosition))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.MotherOccupationPosition));
            //    }

            //    if (!string.IsNullOrEmpty(param.MotherOfficeEmail))
            //    {
            //        predicate = predicate.Or(s => s.FatherName.Contains(param.MotherOfficeEmail));
            //    }
            //    #endregion

            //    #endregion
            //    var query = _dbContext.Entity<MsStudent>()
            //                .Include(x => x.Religion)
            //                .Include(x => x.Nationality)
            //                .Include(x => x.StudentParents)
            //                .Include(x => x.StudentPrevSchoolInfo)
            //                .Select(
            //                    x => new GetMasterSearchingDataResult
            //                    {
            //                        #region Student Data
            //                        StudentStatusID = x.IdStudentStatus,
            //                        SchoolID = x.IdSchool,
            //                        BinusianID = x.Id,
            //                        StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
            //                                    + (string.IsNullOrEmpty(x.MiddleName.Trim()) ? "" : x.MiddleName + " ")
            //                                    + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
            //                        Gender = x.Gender,
            //                        ReligionID = Convert.ToInt32(x.IdReligion),
            //                        ReligionName = x.Religion.ReligionName,
            //                        DOB = Convert.ToDateTime(x.DOB),
            //                        BinusEmailAddress = x.BinusianEmailAddress,
            //                        Nationality = x.Nationality.NationalityName,
            //                        //Previous school need to create checker 
            //                        PreviousSchool = (string.IsNullOrEmpty(x.StudentPrevSchoolInfo.IdPreviousSchoolNew) ? x.StudentPrevSchoolInfo.PreviousSchoolOld.SchoolName : x.StudentPrevSchoolInfo.PreviousSchoolNew.SchoolName),
            //                        #endregion

            //                        #region Hard Code                             
            //                        SmtId = Convert.ToInt32(x.IdGender),
            //                        SchoolLevelId = x.IdChildStatus,
            //                        YearLevelId = x.AdmissionData.IdYearLevel.ToString(),
            //                        #endregion

            //                        #region Father Data
            //                        FatherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.MiddleName.Trim()) ? "" : z.Parent.MiddleName.Trim() + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
            //                        FatherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
            //                        FatherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
            //                        FatherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
            //                        FatherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.CompanyName.Trim()) ? "" : z.Parent.CompanyName)).FirstOrDefault(),
            //                        FatherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.OccupationPosition).FirstOrDefault(),
            //                        FatherOfficeEmail = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.WorkEmailAddress).FirstOrDefault(),
            //                        #endregion

            //                        #region Mother Data
            //                        MotherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.MiddleName.Trim()) ? "" : z.Parent.MiddleName.Trim() + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
            //                        MotherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
            //                        MotherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
            //                        MotherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
            //                        MotherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.CompanyName).FirstOrDefault(),
            //                        MotherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.OccupationPosition).FirstOrDefault()
            //                        #endregion

            //                    }
            //                        )
            //                        .Where(predicate)
            //                        .Where(x => x.SchoolID == param.SchoolID)
            //                        .Where(x => (param.StudentStatusID == "0" ? x.StudentStatusID == x.StudentStatusID : x.StudentStatusID == param.StudentStatusID) )
            //                        .Where(x => x.SchoolLevelId == param.SchoolLevelId)
            //                        .Where(x => (param.YearLevelId == "0" ? x.BinusianID == x.BinusianID : x.YearLevelId == param.YearLevelId))
            //                        .Where(x => x.SmtId == param.SmtId);

            //    var items = query.ToList();

            //    var generateExcelByte = GenerateExcel(items,param.FieldData);
            //    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //    {
            //        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
            //    };

            //}
            ////Get All
            //else
            //{
            //    param.GetAll = false;

            //    var query = _dbContext.Entity<MsStudent>()
            //                .Include(x => x.Religion)
            //                .Include(x => x.Nationality)
            //                .Include(x => x.StudentParents)
            //                .Include(x => x.StudentPrevSchoolInfo)
            //                .Where(x => x.IdSchool == param.SchoolID)
            //                .Where(x => (param.StudentStatusID.ToString() == "0" ? x.IdStudentStatus == x.IdStudentStatus : x.IdStudentStatus == param.StudentStatusID.ToString()))
            //                .Where(x => x.IdChildStatus == param.SchoolLevelId.ToString())
            //                .Where(x => (param.YearLevelId == "0" ? x.IdBinusian == x.IdBinusian : x.AdmissionData.IdYearLevel.ToString() == param.YearLevelId))
            //                .Where(x => x.IdGender == param.SmtId.ToString());

            //    var items = await query.Select(
            //                   x => new GetMasterSearchingDataResult
            //                   {
            //                        #region Student Data                                    
            //                       StudentStatusID = x.IdStudentStatus,
            //                       BinusianID = x.Id,
            //                       StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
            //                                   + (string.IsNullOrEmpty(x.MiddleName.Trim()) ? "" : x.MiddleName + " ")
            //                                   + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
            //                       Gender = x.Gender,
            //                       ReligionID = Convert.ToInt32(x.IdReligion),
            //                       ReligionName = x.Religion.ReligionName,
            //                       DOB = Convert.ToDateTime(x.DOB),
            //                       BinusEmailAddress = x.BinusianEmailAddress,
            //                       Nationality = x.Nationality.NationalityName,
            //                        //PreviousSchool = x.StudentPrevSchoolInfo.MasterSchoolName,
            //                        PreviousSchool = (string.IsNullOrEmpty(x.StudentPrevSchoolInfo.IdPreviousSchoolNew) ? x.StudentPrevSchoolInfo.PreviousSchoolOld.SchoolName : x.StudentPrevSchoolInfo.PreviousSchoolNew.SchoolName),
            //                        #endregion

            //                        #region Hard Code                             
            //                        SmtId = Convert.ToInt32(x.IdGender),
            //                       SchoolLevelId = x.IdChildStatus,
            //                       YearLevelId = x.AdmissionData.IdYearLevel.ToString(),
            //                        #endregion

            //                        #region Father Data
            //                        FatherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.MiddleName.Trim()) ? "" : z.Parent.MiddleName.Trim() + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
            //                       FatherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
            //                       FatherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
            //                       FatherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
            //                       FatherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.CompanyName.Trim()) ? "" : z.Parent.CompanyName)).FirstOrDefault(),
            //                       FatherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.OccupationPosition).FirstOrDefault(),
            //                       FatherOfficeEmail = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.WorkEmailAddress).FirstOrDefault(),
            //                        #endregion

            //                        #region Mother Data
            //                        MotherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.MiddleName.Trim()) ? "" : z.Parent.MiddleName.Trim() + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
            //                       MotherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
            //                       MotherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
            //                       MotherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
            //                       MotherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.CompanyName).FirstOrDefault(),
            //                       MotherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.OccupationPosition).FirstOrDefault()
            //                        #endregion

            //                    }
            //                       ).ToListAsync(CancellationToken);

            //    var generateExcelByte = GenerateExcel(items,param.FieldData);
            //    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //    {
            //        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
            //    };

            //}


            #endregion

            #region cara baru
            var param = await Request.GetBody<ExportToExcelMasterSearchingDataRequest>();

            var paramForStudentEnrollment = new GetStudentEnrollmentforStudentApprovalSummaryRequest
            {
                AcademicYearId = param.AcademicYear,
                SchoolId = param.SchoolID,
                GradeId = param.YearLevelId,
                PathwayID = param.HomeroomID
            };

            //var studentEnrollment = await _studentEnrollmentService.GetStudentEnrollmentForStudentApprovalSummary(paramForStudentEnrollment);

            var studentEnrollment = await _dbContext.Entity<MsHomeroomStudent>()
                                        .Include(x => x.Homeroom).
                                        ThenInclude(a => a.Grade).
                                        ThenInclude(b => b.MsLevel).
                                        ThenInclude(c => c.MsAcademicYear)
                                        .Include(x => x.Student)
                                        .Where(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == (string.IsNullOrEmpty(param.AcademicYear) ? x.Homeroom.Grade.MsLevel.IdAcademicYear : param.AcademicYear)
                                        && x.Homeroom.Grade.MsLevel.MsAcademicYear.IdSchool == (string.IsNullOrEmpty(param.SchoolID) ? x.Homeroom.Grade.MsLevel.MsAcademicYear.IdSchool : param.SchoolID)
                                        && x.Homeroom.IdGrade == (string.IsNullOrEmpty(param.YearLevelId) ? x.Homeroom.IdGrade : param.YearLevelId)
                                        && x.IdHomeroom == (string.IsNullOrEmpty(param.HomeroomID) ? x.IdHomeroom : param.HomeroomID)
                                        )
                                        .Select(x => new GetStudentEnrollmentforStudentApprovalSummaryResult
                                        {
                                            AcademicYearId = param.AcademicYear,
                                            GradeId = x.Homeroom.IdGrade,
                                            GradeName = x.Homeroom.Grade.Code,
                                            StudentId = x.IdStudent
                                        }).ToListAsync();

            if (param.GetAll != null && param.GetAll == false)
            {
                #region Create Dynamic Where

                var predicate = PredicateBuilder.False<GetMasterSearchingDataResult>();

                #region student
                if (!string.IsNullOrEmpty(param.BinusianID))
                {
                    predicate = predicate.Or(s => s.BinusianID.Contains(param.BinusianID));
                }

                if (!string.IsNullOrEmpty(param.StudentName))
                {
                    predicate = predicate.Or(s => s.StudentName.Contains(param.StudentName));
                }

                if (!string.IsNullOrEmpty(param.ReligionName))
                {
                    predicate = predicate.Or(s => s.ReligionName.Contains(param.ReligionName));
                }

                if (!string.IsNullOrEmpty(param.BinusEmailAddress))
                {
                    predicate = predicate.Or(s => s.BinusEmailAddress.Contains(param.BinusEmailAddress));
                }
                #endregion

                #region father
                if (!string.IsNullOrEmpty(param.FatherName))
                {
                    predicate = predicate.Or(s => s.FatherName.Contains(param.FatherName));
                }

                if (!string.IsNullOrEmpty(param.FatherMobilePhoneNumber1))
                {
                    predicate = predicate.Or(s => s.FatherMobilePhoneNumber1.Contains(param.FatherMobilePhoneNumber1));
                }

                if (!string.IsNullOrEmpty(param.FatherResidenceAddress))
                {
                    predicate = predicate.Or(s => s.FatherResidenceAddress.Contains(param.FatherResidenceAddress));
                }

                if (!string.IsNullOrEmpty(param.FatherEmailAddress))
                {
                    predicate = predicate.Or(s => s.FatherEmailAddress.Contains(param.FatherEmailAddress));
                }

                if (!string.IsNullOrEmpty(param.FatherCompanyName))
                {
                    predicate = predicate.Or(s => s.FatherCompanyName.Contains(param.FatherCompanyName));
                }

                if (!string.IsNullOrEmpty(param.FatherOccupationPosition))
                {
                    predicate = predicate.Or(s => s.FatherOccupationPosition.Contains(param.FatherOccupationPosition));
                }

                if (!string.IsNullOrEmpty(param.FatherOfficeEmail))
                {
                    predicate = predicate.Or(s => s.FatherOfficeEmail.Contains(param.FatherOfficeEmail));
                }
                #endregion

                #region Mother
                if (!string.IsNullOrEmpty(param.MotherName))
                {
                    predicate = predicate.Or(s => s.MotherName.Contains(param.MotherName));
                }

                if (!string.IsNullOrEmpty(param.MotherMobilePhoneNumber1))
                {
                    predicate = predicate.Or(s => s.MotherMobilePhoneNumber1.Contains(param.MotherMobilePhoneNumber1));
                }

                if (!string.IsNullOrEmpty(param.MotherResidenceAddress))
                {
                    predicate = predicate.Or(s => s.MotherResidenceAddress.Contains(param.MotherResidenceAddress));
                }

                if (!string.IsNullOrEmpty(param.MotherEmailAddress))
                {
                    predicate = predicate.Or(s => s.MotherEmailAddress.Contains(param.MotherEmailAddress));
                }

                if (!string.IsNullOrEmpty(param.MotherCompanyName))
                {
                    predicate = predicate.Or(s => s.MotherCompanyName.Contains(param.MotherCompanyName));
                }

                if (!string.IsNullOrEmpty(param.MotherOccupationPosition))
                {
                    predicate = predicate.Or(s => s.MotherOccupationPosition.Contains(param.MotherOccupationPosition));
                }

                if (!string.IsNullOrEmpty(param.MotherOfficeEmail))
                {
                    predicate = predicate.Or(s => s.MotherOfficeEmail.Contains(param.MotherOfficeEmail));
                }
                #endregion

                #endregion

                var studentEnrollmentResult = studentEnrollment;

                if (studentEnrollmentResult != null && studentEnrollmentResult.Count > 0)
                {

                    var StudentList = studentEnrollmentResult.Select(x => x.StudentId).ToList();

                    var query = _dbContext.Entity<MsStudent>()
                            .Include(x => x.Religion)
                            .Include(x => x.Nationality)
                            .Include(x => x.StudentParents)
                            .Include(x => x.StudentPrevSchoolInfo)
                            .Where(x => StudentList.Contains(x.Id))
                            .Select(
                                x => new GetMasterSearchingDataResult
                                {
                                    #region Student Data
                                    StudentStatusID = x.IdStudentStatus.ToString(),
                                    SchoolID = x.IdSchool,
                                    SchoolName = param.SchoolName,
                                    AcademicYear = param.AcademicYear,
                                    BinusianID = x.Id,
                                    StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
                                                + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
                                    Gender = x.Gender,
                                    ReligionID = x.IdReligion,
                                    ReligionName = x.Religion.ReligionName,
                                    DOB = Convert.ToDateTime(x.DOB).ToString("yyyy-MM-dd"),
                                    BinusEmailAddress = x.BinusianEmailAddress,
                                    Nationality = x.Nationality.NationalityName,
                                    //Previous school need to create checker 
                                    PreviousSchool = (string.IsNullOrEmpty(x.StudentPrevSchoolInfo.IdPreviousSchoolNew) ? x.StudentPrevSchoolInfo.PreviousSchoolOld.SchoolName : x.StudentPrevSchoolInfo.PreviousSchoolNew.SchoolName),
                                    #endregion

                                    #region Father Data
                                    FatherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                    FatherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                    FatherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                    FatherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                    FatherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.CompanyName.Trim()) ? "" : z.Parent.CompanyName)).FirstOrDefault(),
                                    FatherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.OccupationPosition).FirstOrDefault(),
                                    FatherOfficeEmail = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.WorkEmailAddress).FirstOrDefault(),
                                    #endregion

                                    #region Mother Data
                                    MotherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                                    MotherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                                    MotherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                                    MotherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                                    MotherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.CompanyName).FirstOrDefault(),
                                    MotherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.OccupationPosition).FirstOrDefault()
                                    #endregion

                                }
                                    )
                                    .Where(predicate)
                                    .Where(x => (param.StudentStatusID == "0" ? x.StudentStatusID == x.StudentStatusID : x.StudentStatusID == param.StudentStatusID));

                    var items = await query.OrderByDynamic(param).ToListAsync(CancellationToken);

                    var generateExcelByte = GenerateExcel(items, param.FieldData);
                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                    };
                }
                else
                {
                    var generateExcelByte = GenerateBlankExcel(param.FieldData);
                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                    };
                }

            }
            //Get All
            else
            {
                param.GetAll = false;

                var studentEnrollmentResult = studentEnrollment;

                if (studentEnrollmentResult != null && studentEnrollmentResult.Count > 0)
                {
                    var StudentList = studentEnrollmentResult.Select(x => x.StudentId).ToList();
                    var query = _dbContext.Entity<MsStudent>()
                            .Include(x => x.Religion)
                            .Include(x => x.Nationality)
                            .Include(x => x.StudentParents)
                            .Include(x => x.StudentPrevSchoolInfo)
                            .Where(x => (param.StudentStatusID == "0" ? x.IdStudentStatus.ToString() == x.IdStudentStatus.ToString() : x.IdStudentStatus.ToString() == param.StudentStatusID.ToString()))
                            .Where(x => StudentList.Contains(x.Id));

                    var items = await query.Select(
                        x => new GetMasterSearchingDataResult
                        {
                            #region Student Data                                    
                            StudentStatusID = x.IdStudentStatus.ToString(),
                            BinusianID = x.Id,
                            SchoolName = param.SchoolName,
                            AcademicYear = param.AcademicYear,
                            StudentName = (string.IsNullOrEmpty(x.FirstName.Trim()) ? "" : x.FirstName) + " "
                                        + (string.IsNullOrEmpty(x.LastName.Trim()) ? "" : x.LastName),
                            Gender = x.Gender,
                            ReligionID = x.IdReligion,
                            ReligionName = x.Religion.ReligionName,
                            DOB = Convert.ToDateTime(x.DOB).ToString("yyyy-MM-dd"),
                            BinusEmailAddress = x.BinusianEmailAddress,
                            Nationality = x.Nationality.NationalityName,
                            PreviousSchool = (string.IsNullOrEmpty(x.StudentPrevSchoolInfo.IdPreviousSchoolNew) ? x.StudentPrevSchoolInfo.PreviousSchoolOld.SchoolName : x.StudentPrevSchoolInfo.PreviousSchoolNew.SchoolName),
                            #endregion

                            #region Father Data
                            FatherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                            FatherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                            FatherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                            FatherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                            FatherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => (string.IsNullOrEmpty(z.Parent.CompanyName.Trim()) ? "" : z.Parent.CompanyName)).FirstOrDefault(),
                            FatherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.OccupationPosition).FirstOrDefault(),
                            FatherOfficeEmail = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "F").Select(z => z.Parent.WorkEmailAddress).FirstOrDefault(),
                            #endregion

                            #region Mother Data
                            MotherName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => (string.IsNullOrEmpty(z.Parent.FirstName.Trim()) ? "" : z.Parent.FirstName + " ") + (string.IsNullOrEmpty(z.Parent.LastName.Trim()) ? "" : z.Parent.LastName.Trim())).First().Trim(),
                            MotherMobilePhoneNumber1 = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.MobilePhoneNumber1).FirstOrDefault(),
                            MotherResidenceAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.ResidenceAddress).FirstOrDefault(),
                            MotherEmailAddress = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.PersonalEmailAddress).FirstOrDefault(),
                            MotherCompanyName = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.CompanyName).FirstOrDefault(),
                            MotherOccupationPosition = x.StudentParents.Where(y => y.Parent.IdParentRole.ToUpper() == "M").Select(z => z.Parent.OccupationPosition).FirstOrDefault()
                            #endregion

                        }
                            ).OrderByDynamic(param).ToListAsync(CancellationToken);

                    var count = await query.CountAsync(CancellationToken);

                    var generateExcelByte = GenerateExcel(items, param.FieldData);
                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                    };
                }
                else
                {
                    var generateExcelByte = GenerateBlankExcel(param.FieldData);
                    return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"MasterSearchingData_{DateTime.Now.Ticks}.xlsx"
                    };
                }
            }
            #endregion
        }

        public byte[] GenerateBlankExcel(string fieldData)
        {
            var result = new byte[0];
            string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Master Searching Data");

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsBold = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(0);

                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                foreach (string field in fieldDataList)
                {
                    var Judul = row.CreateCell(fieldCount);
                    Judul.SetCellValue(field);
                    row.Cells[fieldCount].CellStyle = style;
                    fieldCount++;
                }

                #endregion
                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public byte[] GenerateExcel(List<GetMasterSearchingDataResult> data, string fieldData)
        {
            var result = new byte[0];
            string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Master Searching Data");

                //Create style for header
                ICellStyle headerStyle = workbook.CreateCellStyle();

                //Set border style 
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsBold = true;
                headerStyle.SetFont(font);

                //Create style for header
                ICellStyle dataStyle = workbook.CreateCellStyle();

                //Set border style 
                dataStyle.BorderBottom = BorderStyle.Thin;
                dataStyle.BorderLeft = BorderStyle.Thin;
                dataStyle.BorderRight = BorderStyle.Thin;
                dataStyle.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont Datafont = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                Datafont.FontName = "Arial";
                Datafont.FontHeightInPoints = 12;
                dataStyle.SetFont(Datafont);

                //header 
                IRow row = excelSheet.CreateRow(0);

                #region Cara Lama
                //var Judul = row.CreateCell(0);
                //Judul.SetCellValue("StudentID");
                //row.Cells[0].CellStyle = style;

                //var Judul1 = row.CreateCell(1);
                //Judul1.SetCellValue("StudentName");
                //row.Cells[1].CellStyle = style;

                //var Judul2 = row.CreateCell(2);
                //Judul2.SetCellValue("Gender");
                //row.Cells[2].CellStyle = style;

                //var Judul3 = row.CreateCell(3);
                //Judul3.SetCellValue("ReligionName");
                //row.Cells[3].CellStyle = style;

                //var Judul4 = row.CreateCell(4);
                //Judul4.SetCellValue("Binus Email Address");
                //row.Cells[4].CellStyle = style;

                //var Judul5 = row.CreateCell(5);
                //Judul5.SetCellValue("FatherName");
                //row.Cells[5].CellStyle = style;

                //var Judul6 = row.CreateCell(6);
                //Judul6.SetCellValue("Father Mobile Phone Number 1");
                //row.Cells[6].CellStyle = style;

                //var Judul7 = row.CreateCell(7);
                //Judul7.SetCellValue("Father Residence Address");
                //row.Cells[7].CellStyle = style;

                //var Judul8 = row.CreateCell(8);
                //Judul8.SetCellValue("Father Email Address");
                //row.Cells[8].CellStyle = style;

                //var Judul9 = row.CreateCell(9);
                //Judul9.SetCellValue("Father Company Name");
                //row.Cells[9].CellStyle = style;

                //var Judul10 = row.CreateCell(10);
                //Judul10.SetCellValue("Father Occupation Position");
                //row.Cells[10].CellStyle = style;

                //var Judul11 = row.CreateCell(11);
                //Judul11.SetCellValue("Father Office Email");
                //row.Cells[11].CellStyle = style;

                //var Judul12 = row.CreateCell(12);
                //Judul12.SetCellValue("Mother Name");
                //row.Cells[12].CellStyle = style;

                //var Judul13 = row.CreateCell(13);
                //Judul13.SetCellValue("Mother Mobile Phone 1");
                //row.Cells[13].CellStyle = style;

                //var Judul14 = row.CreateCell(14);
                //Judul14.SetCellValue("Mother Residence Address");
                //row.Cells[14].CellStyle = style;

                //var Judul15 = row.CreateCell(15);
                //Judul15.SetCellValue("Mother Email Address");
                //row.Cells[15].CellStyle = style;

                //var Judul16 = row.CreateCell(16);
                //Judul16.SetCellValue("Mother Company Name");
                //row.Cells[16].CellStyle = style;

                //var Judul17 = row.CreateCell(17);
                //Judul17.SetCellValue("Mother Occupation Position");
                //row.Cells[17].CellStyle = style;

                //foreach (var item in data)
                //{
                //    row = excelSheet.CreateRow(row.RowNum + 1);

                //    row.CreateCell(0).SetCellValue(item.BinusianID);
                //    row.Cells[0].CellStyle = style;

                //    row.CreateCell(1).SetCellValue(item.FullName);
                //    row.Cells[1].CellStyle = style;

                //    row.CreateCell(2).SetCellValue(item.Gender.ToString());
                //    row.Cells[2].CellStyle = style;

                //    row.CreateCell(3).SetCellValue(item.ReligionName);
                //    row.Cells[3].CellStyle = style;

                //    row.CreateCell(4).SetCellValue(item.BinusEmailAddress);
                //    row.Cells[4].CellStyle = style;

                //    row.CreateCell(5).SetCellValue(item.FatherName);
                //    row.Cells[5].CellStyle = style;

                //    row.CreateCell(6).SetCellValue(item.FatherMobilePhoneNumber1);
                //    row.Cells[6].CellStyle = style;

                //    row.CreateCell(7).SetCellValue(item.FatherResidenceAddress);
                //    row.Cells[7].CellStyle = style;

                //    row.CreateCell(8).SetCellValue(item.FatherEmailAddress);
                //    row.Cells[8].CellStyle = style;

                //    row.CreateCell(9).SetCellValue(item.FatherCompanyName);
                //    row.Cells[9].CellStyle = style;

                //    row.CreateCell(10).SetCellValue(item.FatherOccupationPosition);
                //    row.Cells[10].CellStyle = style;

                //    row.CreateCell(11).SetCellValue(item.FatherOfficeEmail);
                //    row.Cells[11].CellStyle = style;

                //    row.CreateCell(12).SetCellValue(item.MotherName);
                //    row.Cells[12].CellStyle = style;

                //    row.CreateCell(13).SetCellValue(item.MotherMobilePhoneNumber1);
                //    row.Cells[13].CellStyle = style;

                //    row.CreateCell(14).SetCellValue(item.MotherResidenceAddress);
                //    row.Cells[14].CellStyle = style;

                //    row.CreateCell(15).SetCellValue(item.MotherEmailAddress);
                //    row.Cells[15].CellStyle = style;

                //    row.CreateCell(16).SetCellValue(item.MotherCompanyName);
                //    row.Cells[16].CellStyle = style;

                //    row.CreateCell(17).SetCellValue(item.MotherOccupationPosition);
                //    row.Cells[17].CellStyle = style;
                #endregion


                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                foreach (string field in fieldDataList)
                {
                    var Judul = row.CreateCell(fieldCount);
                    Judul.SetCellValue(field);
                    row.Cells[fieldCount].CellStyle = headerStyle;
                    fieldCount++;
                }

                int w = 0;
                foreach (var item in data)
                {
                    row = excelSheet.CreateRow(row.RowNum + 1);

                    for (int i = 0; i < fieldCount; i++)
                    {
                        if (fieldDataList[i] == "Photo")
                        {
                            try
                            {
                                byte[] file = DownloadFromBlob(item.SchoolName, item.AcademicYear, item.BinusianID);
                                row.CreateCell(i);
                                int pictureIndex = workbook.AddPicture(file, PictureType.PNG);
                                ICreationHelper helper = workbook.GetCreationHelper();
                                IDrawing drawing = excelSheet.CreateDrawingPatriarch();
                                IClientAnchor anchor = helper.CreateClientAnchor();
                                anchor.Col1 = i;
                                anchor.Row1 = row.RowNum;

                                int imgWid = 100;
                                int imgHgt = 75;
                                excelSheet.SetColumnWidth(i, imgWid * 32);
                                row.Height = (short)(imgHgt * 16);

                                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);

                                picture.Resize(1);
                            }
                            catch (Exception)
                            {
                                row.CreateCell(i);
                                row.Cells[i].CellStyle = dataStyle;
                            }
                        }

                        else
                        {
                            if (fieldDataList[i] == "Gender")
                            {
                                var value = item.Gender == 0 ? "Male" : "Female";
                                row.CreateCell(i).SetCellValue(value);
                                row.Cells[i].CellStyle = dataStyle;
                            }
                            else
                            {
                                var value = (string)item.GetType().GetProperty(fieldDataList[i]).GetValue(item, null);
                                value = string.IsNullOrEmpty(value) ? "-" : value;
                                row.CreateCell(i).SetCellValue(value);
                                row.Cells[i].CellStyle = dataStyle;
                            }
                        }

                    }
                }

                //Add Auto FIt
                for (int i = 0; i < fieldCount; i++)
                {
                    if (fieldDataList[i] != "Photo")
                    {
                        excelSheet.AutoSizeColumn(i);
                    }
                }

                #endregion
                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadFromBlob(string schoolName, string academicYear, string studentId)
        {

            CloudStorageAccount mycloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=bssstudentstorage;AccountKey=j8XfDXlD+KyoYc4Z9Puddi8+W2oGQ+MgxZ2+avOyq2vZFdXhbn091udJgUf1DZ+o8eJMD+38aPaTN9sWLUKBOg==;EndpointSuffix=core.windows.net");
            var blobClient = mycloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference("studentphoto");
            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(schoolName + "/" + academicYear + "/" + studentId + ".jpg");

            //using (var fileStream = System.IO.File.OpenWrite(studentId + ".jpg"))
            //{
            //    cloudBlockBlob.DownloadToStream(fileStream);
            //}

            cloudBlockBlob.FetchAttributes();
            long fileByteLength = cloudBlockBlob.Properties.Length;
            byte[] fileContent = new byte[fileByteLength];
            for (int i = 0; i < fileByteLength; i++)
            {
                fileContent[i] = 0x20;
            }

            cloudBlockBlob.DownloadToByteArray(fileContent, 0);

            return fileContent;
        }

    }
}
