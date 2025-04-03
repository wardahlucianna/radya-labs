using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.TransferStudentData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.TransferStudentData.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Student.FnStudent.TransferStudentData
{
    public class TransferStudentHandler : FunctionsHttpCrudHandler
    {
         private readonly IStudentDbContext _dbContext;
         public string ExNotExist = "{0} with {1}: {2} does not exist.";

        public TransferStudentHandler(IStudentDbContext dbContext)
        {            
            _dbContext = dbContext;
        }
       
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<TransferStudentRequest,TransferStudentValidator>();
        
            #region comment filter
            // var existReligion = await _dbContext.Entity<LtReligion>().FindAsync(body.IdReligion);            

            // if (existReligion is null)
            // {                       
            //     throw new NotFoundException(string.Format(ExNotExist, "Religion",  nameof(body.IdReligion), body.IdReligion));
            // }

            // var existBloodType = _dbContext.Entity<LtBloodType>()
            //                     .Where(x => x.Id == body.IdBloodType)
            //                     .FirstOrDefault();

            // if (existBloodType is null)
            // {                 
            //     throw new BadRequestException(string.Format(ExNotExist, "BloodType", nameof(body.IdBloodType), body.IdBloodType));
            // }
            #endregion      
          
            var existPrevSchool = await _dbContext.Entity<MsPreviousSchoolNew>().FindAsync(body.PrevSchool.IdPrevSchool);            

            if (existPrevSchool is null)
            {                       
                throw new NotFoundException(string.Format(ExNotExist, "Prev School",  nameof(body.PrevSchool.IdPrevSchool), body.PrevSchool.IdPrevSchool));
            }

            foreach (var parentData in body.Parents)
            {
                // var existReligionParent = _dbContext.Entity<LtReligion>()
                //                 .Where(x => x.Id == parentData.IdReligion)
                //                 .FirstOrDefault();

                // if (existReligionParent is null)
                // {                     
                //     throw new BadRequestException(string.Format(ExNotExist, "Religion Parent", nameof(parentData.IdReligion), parentData.IdReligion));
                // }

                var existOccupationType = _dbContext.Entity<LtOccupationType>()
                                .Where(x => x.Id == parentData.IdOccupationType)
                                .FirstOrDefault();

                if (existOccupationType is null)
                {                   
                    throw new BadRequestException(string.Format(ExNotExist, "OccupationType Parent", nameof(parentData.IdOccupationType), parentData.IdOccupationType));
                }
            }

          

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            //bisa update jika data student belum di modified by Internal Binus
            var getStudent = await _dbContext.Entity<MsStudent>()
                            .Include(x => x.StudentParents).ThenInclude(x => x.Parent)   
                            //.Where(x => x.IsActive == false || (x.IsActive == true && (x.UserUp == null || x.UserUp == "API0001")))                                                                     
                            .FirstOrDefaultAsync(x => x.Id == body.IdStudent,CancellationToken);


            if (getStudent != null)
            {
                if(getStudent.IsActive == false || (getStudent.IsActive == true && (getStudent.UserUp == null || getStudent.UserUp == "API0001")))
                {
                    getStudent.UserUp = "API0001";                  
                    getStudent.IsActive = true;
                    getStudent.IdRegistrant = body.IdRegistrant;
                    getStudent.IdBinusian = body.IdBinusian;
                    getStudent.FirstName = body.FirstName;
                    getStudent.LastName = body.LastName;
                    getStudent.IdStudentStatus = body.IdStudentStatus;
                    getStudent.Gender = body.Gender;
                    getStudent.IdReligion = body.IdReligion;
                    getStudent.IdSchool = body.IdSchool;
                    //getStudent.SiblingIdStudent = body.SiblingIdStudent;
                    getStudent.NISN = body.NISN;
                    getStudent.POB = body.POB;
                    getStudent.DOB = body.DOB;
                    getStudent.IdBirthCountry = body.IdBirthCountry;
                    getStudent.IdBirthStateProvince = body.IdBirthStateProvince;
                    getStudent.IdBirthCity = body.IdBirthCity;
                    getStudent.IdNationality = body.IdNationality;
                    getStudent.IdCountry = body.IdCountry;
                    getStudent.FamilyCardNumber = body.FamilyCardNumber;
                    getStudent.NIK = body.NIK;       
                    getStudent.KITASNumber = ((body.KITASNumber == "" || body.KITASNumber == null ) ? null : body.KITASNumber);
                    getStudent.KITASExpDate =((body.KITASExpDate.ToString() == "" || body.KITASExpDate == null ) ? null : body.KITASExpDate);
                    getStudent.NSIBNumber = ((body.NSIBNumber == "" || body.NSIBNumber == null ) ? null : body.NSIBNumber);
                    getStudent.NSIBExpDate = ((body.NSIBExpDate.ToString() == "" || body.NSIBExpDate == null ) ? null : body.NSIBExpDate);
                    getStudent.PassportNumber = ((body.PassportNumber == "" || body.PassportNumber == null ) ? null : body.PassportNumber);
                    getStudent.PassportExpDate = ((body.PassportExpDate.ToString() == "" || body.PassportExpDate == null ) ? null : body.PassportExpDate);
                    getStudent.IdReligionSubject = body.IdReligionSubject;
                    getStudent.ChildNumber = body.ChildNumber;
                    getStudent.TotalChildInFamily = body.TotalChildInFamily;
                    getStudent.IdChildStatus = body.IdChildStatus;
                    getStudent.IsHavingKJP = body.IsHavingKJP;
                    getStudent.IsSpecialTreatment = body.IsSpecialTreatment;
                    getStudent.NotesForSpecialTreatments = (body.NotesForSpecialTreatments == null? "" : body.NotesForSpecialTreatments);
                    getStudent.IdBloodType = body.IdBloodType;
                    getStudent.Height = body.Height;
                    getStudent.Weight = body.Weight;
                    getStudent.ResidenceAddress = body.ResidenceAddress;
                    getStudent.HouseNumber = body.HouseNumber;
                    getStudent.RT = body.RT;
                    getStudent.RW = body.RW;
                    getStudent.IdStayingWith = body.IdStayingWith;
                    getStudent.VillageDistrict = body.VillageDistrict;
                    getStudent.SubDistrict = body.SubDistrict;
                    getStudent.IdAddressCity = body.IdAddressCity;
                    getStudent.IdAddressStateProvince = body.IdAddressStateProvince;
                    getStudent.IdAddressCountry = body.IdAddressCountry;
                    getStudent.PostalCode = body.PostalCode;
                    getStudent.DistanceHomeToSchool = body.DistanceHomeToSchool;
                    getStudent.ResidencePhoneNumber =  (body.ResidencePhoneNumber == null? "" : body.ResidencePhoneNumber);
                    getStudent.MobilePhoneNumber1 = body.MobilePhoneNumber1;
                    getStudent.MobilePhoneNumber2 =  (body.MobilePhoneNumber2 == null? "" : body.MobilePhoneNumber2);
                    getStudent.MobilePhoneNumber3 =  (body.MobilePhoneNumber3 == null? "" : body.MobilePhoneNumber3);
                    getStudent.EmergencyContactRole = body.EmergencyContactRole;
                    getStudent.PersonalEmailAddress = body.PersonalEmailAddress;
                    getStudent.FutureDream =  (body.FutureDream == null? "" : body.FutureDream);
                    getStudent.Hobby = (body.Hobby == null? "" : body.Hobby);

                    _dbContext.Entity<MsStudent>().Update(getStudent);


                    var updateStudentPrevSchool  = _dbContext.Entity<MsStudentPrevSchoolInfo>()
                                                    .Where(x => x.IdStudent == body.IdStudent)
                                                    .FirstOrDefault();
                    
                    if(updateStudentPrevSchool != null)
                    {
                        updateStudentPrevSchool.UserUp = "API0001";          
                        updateStudentPrevSchool.IsActive = true;
                        updateStudentPrevSchool.IdPreviousSchoolNew = body.PrevSchool.IdPrevSchool;
                        updateStudentPrevSchool.Grade = (body.PrevSchool.Grade == null? "" : body.PrevSchool.Grade);
                        updateStudentPrevSchool.YearAttended = (body.PrevSchool.YearAttended == null? "" : body.PrevSchool.YearAttended);
                        updateStudentPrevSchool.YearWithdrawn = (body.PrevSchool.YearWithdrawn == null? "" : body.PrevSchool.YearWithdrawn);

                        _dbContext.Entity<MsStudentPrevSchoolInfo>().Update(updateStudentPrevSchool);
                    }

                    var updateAdmissionData = _dbContext.Entity<MsAdmissionData>()
                                                    .Where(x => x.IdStudent == body.IdStudent)
                                                    .FirstOrDefault();       

                    if(updateAdmissionData != null)
                    {
                        var CurrAYData = _dbContext.Entity<MsGrade>()
                                                .Include(a => a.MsLevel).ThenInclude(b => b.MsAcademicYear)
                                                  .Where(x => x.Code == body.AdmissionData.IdYearLevel.ToString()
                                                  && x.MsLevel.MsAcademicYear.Code == body.AdmissionData.AcademicYear
                                                  && x.MsLevel.MsAcademicYear.IdSchool == body.IdSchool)
                                                  .Select(a => new {
                                                      AcademicYear = a.MsLevel.MsAcademicYear.Id,
                                                      SchoolLevel = a.MsLevel.Id,
                                                      YearLevel = a.Id
                                                  }).FirstOrDefault();
                        
                        updateAdmissionData.UserUp = "API0001";          
                        updateAdmissionData.IsActive = true;
                        updateAdmissionData.IdSchool = body.IdSchool;
                        updateAdmissionData.IdRegistrant = body.IdRegistrant; 
                        updateAdmissionData.IdStudent = body.IdStudent;
                        updateAdmissionData.DateofEntry = ((body.AdmissionData.DateofEntry.ToString() == "" || body.AdmissionData.DateofEntry == null ) ? null : body.AdmissionData.DateofEntry);
                        updateAdmissionData.DateofFormPurchased = ((body.AdmissionData.DateofFormPurchased.ToString() == "" || body.AdmissionData.DateofFormPurchased == null ) ? null : body.AdmissionData.DateofFormPurchased);
                        updateAdmissionData.DateofApplReceived = ((body.AdmissionData.DateofApplReceived.ToString() == "" || body.AdmissionData.DateofApplReceived == null ) ? null : body.AdmissionData.DateofApplReceived);
                        updateAdmissionData.DateofReregistration = ((body.AdmissionData.DateofReregistration.ToString() == "" || body.AdmissionData.DateofReregistration == null ) ? null : body.AdmissionData.DateofReregistration);
                        updateAdmissionData.JoinToSchoolDate = body.AdmissionData.JoinToSchoolDate;
                        updateAdmissionData.AdmissionYear = (CurrAYData != null ? CurrAYData.AcademicYear : body.AdmissionData.AdmissionYear);
                        updateAdmissionData.AcademicYear = (CurrAYData != null? CurrAYData.AcademicYear : body.AdmissionData.AdmissionYear);
                        updateAdmissionData.IdAcademicSemester = body.AdmissionData.IdAcademicSemester;
                        updateAdmissionData.TotalScore = body.AdmissionData.TotalScore;
                        updateAdmissionData.Grade = body.AdmissionData.Grade;
                        updateAdmissionData.IdSchoolSubject = body.AdmissionData.IdSchoolSubject;
                        updateAdmissionData.IdSchoolTPKSStatus = body.AdmissionData.IdSchoolTPKSStatus;
                        updateAdmissionData.TPKSNotes = (body.AdmissionData.TPKSNotes == null? "" : body.AdmissionData.TPKSNotes);
                        updateAdmissionData.IdSchoolLevel = (CurrAYData != null ? CurrAYData.SchoolLevel : body.AdmissionData.IdSchoolLevel.ToString());//body.AdmissionData.IdSchoolLevel;
                        updateAdmissionData.IdYearLevel = (CurrAYData != null ? CurrAYData.YearLevel : body.AdmissionData.IdYearLevel.ToString());//body.AdmissionData.;
                        updateAdmissionData.IsEnrolledForFirstTime = body.AdmissionData.IsEnrolledForFirstTime;

                        _dbContext.Entity<MsAdmissionData>().Update(updateAdmissionData);
                    }  

                    
                    //var deleteStudentChargingAdmission = _dbContext.Entity<TrStudentChargingAdmission>()
                    //                                .Where(x => x.IdStudent == body.IdStudent)
                    //                                .ToList();
                    //if(deleteStudentChargingAdmission != null)
                    //{
                    //      _dbContext.Entity<TrStudentChargingAdmission>().RemoveRange(deleteStudentChargingAdmission);                               
                    //}  

                    foreach(var rptChargingAdmission in body.ChargingAdmission)
                    {
                        var CurrAYChargingAdmission = _dbContext.Entity<MsGrade>()
                                          .Include(a => a.MsLevel).ThenInclude(b => b.MsAcademicYear)
                                            .Where(x => x.Code == rptChargingAdmission.IdYearLevel //body.AdmissionData.IdYearLevel.ToString()
                                            && x.MsLevel.MsAcademicYear.Code == rptChargingAdmission.AcademicYear //body.AdmissionData.AcademicYear)
                                            && x.MsLevel.MsAcademicYear.IdSchool == body.IdSchool)
                                            .Select(a => new {
                                                AcademicYear = a.MsLevel.MsAcademicYear.Id,
                                                SchoolLevel = a.MsLevel.Id,
                                                YearLevel = a.Id
                                            }).FirstOrDefault();

                        //var newStudentChargingAdmission = new TrStudentChargingAdmission{
                        //UserIn = "API0001", 
                        //Id = Guid.NewGuid().ToString(),
                        //IdSchool = body.IdSchool,
                        //IdStudent = body.IdStudent,
                        //FormNumber = rptChargingAdmission.FormNumber,
                        //AdmissionYear = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.AcademicYear : rptChargingAdmission.AdmissionYear),                          
                        //AcademicYear = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.AcademicYear : rptChargingAdmission.AdmissionYear),
                        //IdAcademicSemester = rptChargingAdmission.IdAcademicSemester,
                        //IdSchoolLevel = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.SchoolLevel : rptChargingAdmission.IdSchoolLevel),//rptChargingAdmission.IdSchoolLevel,
                        //IdYearLevel = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.YearLevel : rptChargingAdmission.IdYearLevel),// rptChargingAdmission.IdYearLevel,
                        //ComponentClass = rptChargingAdmission.ComponentClass,
                        //FeeGroupName = rptChargingAdmission.FeeGroupName,
                        //ChargingAmount = rptChargingAdmission.ChargingAmount,
                        //DueDate = rptChargingAdmission.DueDate,
                        //ChargingStatus = rptChargingAdmission.ChargingStatus
                        //};

                        //_dbContext.Entity<TrStudentChargingAdmission>().Add(newStudentChargingAdmission);    

                    }

                    var getParentSibling = await _dbContext.Entity<MsStudent>()
                            .Include(x => x.StudentParents).ThenInclude(x => x.Parent)                                                                                                    
                            .FirstOrDefaultAsync(x => x.Id == body.SiblingIdStudent,CancellationToken);  



                    foreach(var rptParent in body.Parents)
                    {
                        var getParent = getStudent.StudentParents.Where(a => a.Parent.IdParentRole == rptParent.IdParentRole).FirstOrDefault();
                        //Edit Fikri 23 Dec 2021 - Add Validasi when ParentSibling is null
                        //var ExistParentSibling = getParentSibling.StudentParents.Where(a => a.Parent.IdParentRole == rptParent.IdParentRole).FirstOrDefault();   
                        var ExistParentSibling = new MsStudentParent();

                        if (getParentSibling == null)
                        {
                            ExistParentSibling = null;
                        }
                        else
                        {
                            ExistParentSibling = getParentSibling.StudentParents.Where(a => a.Parent.IdParentRole == rptParent.IdParentRole).FirstOrDefault();
                        }
                        if (getParent != null || ExistParentSibling != null)
                        {
                            var updateParent = (getParent != null ? getParent.Parent : ExistParentSibling.Parent);
                            if(updateParent.UserUp == null ||updateParent.UserUp == "API0001")
                            {                           
                                updateParent.UserUp = "API0001";
                                updateParent.IsActive = true;
                                updateParent.FirstName = rptParent.FirstName;
                                updateParent.LastName = rptParent.LastName;
                                updateParent.Gender = rptParent.Gender;
                                updateParent.POB = rptParent.POB;
                                updateParent.DOB = rptParent.DOB;
                                updateParent.AliveStatus = rptParent.AliveStatus;
                                updateParent.IdReligion = rptParent.IdReligion;
                                updateParent.IdLastEducationLevel = rptParent.IdLastEducationLevel;
                                updateParent.IdNationality = rptParent.IdNationality;
                                updateParent.IdCountry = rptParent.IdCountry;
                                updateParent.FamilyCardNumber = rptParent.FamilyCardNumber;
                                updateParent.NIK = rptParent.NIK;
                                updateParent.KITASNumber = ((rptParent.KITASNumber == "" || rptParent.KITASNumber == null ) ? null : rptParent.KITASNumber);
                                updateParent.KITASExpDate =((rptParent.KITASExpDate.ToString() == "" || rptParent.KITASExpDate == null ) ? null : rptParent.KITASExpDate);
                                updateParent.PassportNumber = ((rptParent.PassportNumber == "" || rptParent.PassportNumber == null ) ? null : rptParent.PassportNumber);
                                updateParent.PassportExpDate = ((rptParent.PassportExpDate.ToString() == "" || rptParent.PassportExpDate == null ) ? null : rptParent.PassportExpDate);
                                updateParent.BinusianStatus = rptParent.BinusianStatus;
                                updateParent.IdBinusian = rptParent.IdBinusian;
                                updateParent.ResidenceAddress = rptParent.ResidenceAddress;
                                updateParent.HouseNumber = rptParent.HouseNumber;
                                updateParent.RT = rptParent.RT;
                                updateParent.RW = rptParent.RW;
                                updateParent.VillageDistrict = rptParent.VillageDistrict;
                                updateParent.SubDistrict = rptParent.SubDistrict;
                                updateParent.IdAddressCity = rptParent.IdAddressCity;
                                updateParent.IdAddressStateProvince = rptParent.IdAddressStateProvince;
                                updateParent.IdAddressCountry = rptParent.IdAddressCountry;
                                updateParent.PostalCode = rptParent.PostalCode;                           
                                updateParent.ResidencePhoneNumber =  (rptParent.ResidencePhoneNumber == null? "" : rptParent.ResidencePhoneNumber);
                                updateParent.MobilePhoneNumber1 = rptParent.MobilePhoneNumber1;
                                updateParent.MobilePhoneNumber2 =  (rptParent.MobilePhoneNumber2 == null? "" : rptParent.MobilePhoneNumber2);
                                updateParent.MobilePhoneNumber3 =  (rptParent.MobilePhoneNumber3 == null? "" : rptParent.MobilePhoneNumber3);
                                updateParent.PersonalEmailAddress = rptParent.PersonalEmailAddress;
                                updateParent.WorkEmailAddress = (rptParent.WorkEmailAddress == null? "" : rptParent.WorkEmailAddress);
                                updateParent.IdOccupationType = rptParent.IdOccupationType;
                                updateParent.OccupationPosition = rptParent.OccupationPosition;
                                updateParent.CompanyName = (rptParent.CompanyName == null? "" : rptParent.CompanyName);
                                updateParent.IdParentSalaryGroup = rptParent.IdParentSalaryGroup;
                            
                                _dbContext.Entity<MsParent>().Update(updateParent);
                            }  
                            if(getParent == null && ExistParentSibling != null)
                            {
                                //menambahkan parent yang exist di sibling tp tidak di idstudent tsb
                                 var newStudentParent = new MsStudentParent{
                                 Id = Guid.NewGuid().ToString(),
                                 UserIn = "API0001",                        
                                 IdStudent = body.IdStudent,
                                 IdParent = ExistParentSibling.Parent.Id

                                };
                            _dbContext.Entity<MsStudentParent>().Add(newStudentParent);
                            }                       
                        }
                        else
                        {
                            var generateIdParent = Guid.NewGuid().ToString();
                            var newParent = new MsParent
                            {
                                Id = generateIdParent,
                                UserIn = "API0001",                            
                                FirstName = rptParent.FirstName,
                                LastName = rptParent.LastName,
                                Gender = rptParent.Gender, 
                                POB = rptParent.POB,
                                DOB = rptParent.DOB,
                                IdParentRole = rptParent.IdParentRole,      
                                AliveStatus = rptParent.AliveStatus,
                                IdReligion = rptParent.IdReligion,
                                IdLastEducationLevel = rptParent.IdLastEducationLevel,
                                IdNationality = rptParent.IdNationality,
                                IdCountry = rptParent.IdCountry,
                                FamilyCardNumber = rptParent.FamilyCardNumber,
                                NIK = rptParent.NIK,
                                PassportNumber = rptParent.PassportNumber,
                                PassportExpDate = ((rptParent.PassportExpDate.ToString() == "" || rptParent.PassportExpDate == null ) ? null : rptParent.PassportExpDate),
                                KITASNumber = rptParent.KITASNumber,
                                KITASExpDate = ((rptParent.KITASExpDate.ToString() == "" || rptParent.KITASExpDate == null ) ? null : rptParent.KITASExpDate),
                                BinusianStatus = rptParent.BinusianStatus,
                                IdBinusian = rptParent.IdBinusian,
                                ParentNameForCertificate = "",  
                                ResidenceAddress = rptParent.ResidenceAddress,
                                HouseNumber = rptParent.HouseNumber  ,
                                RT = rptParent.RT,
                                RW = rptParent.RW,   
                                VillageDistrict = rptParent.VillageDistrict,
                                SubDistrict = rptParent.SubDistrict,
                                IdAddressCity = rptParent.IdAddressCity,
                                IdAddressStateProvince = rptParent.IdAddressStateProvince,
                                IdAddressCountry = rptParent.IdAddressCountry,
                                PostalCode = rptParent.PostalCode,
                                ResidencePhoneNumber = (rptParent.ResidencePhoneNumber == null? "" : rptParent.ResidencePhoneNumber),
                                MobilePhoneNumber1 = rptParent.MobilePhoneNumber1,
                                MobilePhoneNumber2 =  (rptParent.MobilePhoneNumber2 == null? "" : rptParent.MobilePhoneNumber2),
                                MobilePhoneNumber3 =  (rptParent.MobilePhoneNumber3 == null? "" : rptParent.MobilePhoneNumber3),
                                PersonalEmailAddress = rptParent.PersonalEmailAddress,
                                WorkEmailAddress = (rptParent.WorkEmailAddress == null? "" : rptParent.WorkEmailAddress),
                                IdOccupationType = rptParent.IdOccupationType,
                                OccupationPosition = rptParent.OccupationPosition,
                                CompanyName = (rptParent.CompanyName == null? "" : rptParent.CompanyName),
                                IdParentSalaryGroup = rptParent.IdParentSalaryGroup                            
                            };

                             _dbContext.Entity<MsParent>().Add(newParent);    

                             var newStudentParent = new MsStudentParent{
                                 Id = Guid.NewGuid().ToString(),
                                 UserIn = "API0001",                          
                                 IdStudent = body.IdStudent,
                                 IdParent = generateIdParent

                             };
                            _dbContext.Entity<MsStudentParent>().Add(newStudentParent);                             
                        }
                       
                    }

                }
                else{
                    throw new BadRequestException("Not Authorized to update this student.");  
                }
                  
                
            }
            else
            {
                //insert student
                var newStudent = new MsStudent
                {
                    Id = body.IdStudent,
                    UserIn = "API0001",
                    IdRegistrant = body.IdRegistrant,
                    IdBinusian = body.IdBinusian,
                    FirstName = body.FirstName,
                    LastName = body.LastName,
                    IdStudentStatus = body.IdStudentStatus,
                    Gender = body.Gender,
                    IdReligion = body.IdReligion,
                    IdSchool = body.IdSchool,
                    NISN = body.NISN,
                    POB = body.POB,
                    DOB = body.DOB,
                    IdBirthCountry = body.IdBirthCountry,
                    IdBirthStateProvince = body.IdBirthStateProvince,
                    IdBirthCity = body.IdBirthCity,
                    IdNationality = body.IdNationality,
                    IdCountry = body.IdCountry,
                    FamilyCardNumber = body.FamilyCardNumber,
                    NIK = body.NIK,
                    KITASNumber = ((body.KITASNumber == "" || body.KITASNumber == null) ? null : body.KITASNumber),
                    KITASExpDate = ((body.KITASExpDate.ToString() == "" || body.KITASExpDate == null) ? null : body.KITASExpDate),
                    NSIBNumber = ((body.NSIBNumber == "" || body.NSIBNumber == null) ? null : body.NSIBNumber),
                    NSIBExpDate = ((body.NSIBExpDate.ToString() == "" || body.NSIBExpDate == null) ? null : body.NSIBExpDate),
                    PassportNumber = ((body.PassportNumber == "" || body.PassportNumber == null) ? null : body.PassportNumber),
                    PassportExpDate = ((body.PassportExpDate.ToString() == "" || body.PassportExpDate == null) ? null : body.PassportExpDate),
                    IdReligionSubject = body.IdReligionSubject,
                    ChildNumber = body.ChildNumber,
                    TotalChildInFamily = body.TotalChildInFamily,
                    IdChildStatus = body.IdChildStatus,
                    IsHavingKJP = body.IsHavingKJP,
                    IsSpecialTreatment = body.IsSpecialTreatment,
                    NotesForSpecialTreatments = (body.NotesForSpecialTreatments == null ? "" : body.NotesForSpecialTreatments),
                    IdBloodType = body.IdBloodType,
                    Height = body.Height,
                    Weight = body.Weight,
                    ResidenceAddress = body.ResidenceAddress,
                    HouseNumber = body.HouseNumber,
                    RT = body.RT,
                    RW = body.RW,
                    IdStayingWith = body.IdStayingWith,
                    VillageDistrict = body.VillageDistrict,
                    SubDistrict = body.SubDistrict,
                    IdAddressCity = body.IdAddressCity,
                    IdAddressStateProvince = body.IdAddressStateProvince,
                    IdAddressCountry = body.IdAddressCountry,
                    PostalCode = body.PostalCode,
                    DistanceHomeToSchool = body.DistanceHomeToSchool,
                    ResidencePhoneNumber = (body.ResidencePhoneNumber == null ? "" : body.ResidencePhoneNumber),
                    MobilePhoneNumber1 = body.MobilePhoneNumber1,
                    MobilePhoneNumber2 = (body.MobilePhoneNumber2 == null ? "" : body.MobilePhoneNumber2),
                    MobilePhoneNumber3 = (body.MobilePhoneNumber3 == null ? "" : body.MobilePhoneNumber3),
                    EmergencyContactRole = body.EmergencyContactRole,
                    PersonalEmailAddress = body.PersonalEmailAddress,
                    FutureDream = (body.FutureDream == null ? "" : body.FutureDream),
                    Hobby = (body.Hobby == null ? "" : body.Hobby)
                };
                _dbContext.Entity<MsStudent>().Add(newStudent);


                var newStudentPrevSchool = new MsStudentPrevSchoolInfo
                {
                    UserIn = "API0001",
                    IdStudent = body.IdStudent,
                    IdRegistrant = body.IdRegistrant,
                    IdPreviousSchoolNew = body.PrevSchool.IdPrevSchool,
                    Grade = (body.PrevSchool.Grade == null ? "" : body.PrevSchool.Grade),
                    YearAttended = (body.PrevSchool.YearAttended == null ? "" : body.PrevSchool.YearAttended),
                    YearWithdrawn = (body.PrevSchool.YearWithdrawn == null ? "" : body.PrevSchool.YearWithdrawn)
                };

                _dbContext.Entity<MsStudentPrevSchoolInfo>().Add(newStudentPrevSchool);
                              
                var CurrAYData = _dbContext.Entity<MsGrade>()
                                              .Include(a => a.MsLevel).ThenInclude(b => b.MsAcademicYear)
                                                .Where(x => x.Code == body.AdmissionData.IdYearLevel.ToString()
                                                && x.MsLevel.MsAcademicYear.Code == body.AdmissionData.AcademicYear
                                                && x.MsLevel.MsAcademicYear.IdSchool == body.IdSchool)
                                                .Select(a => new {
                                                    AcademicYear = a.MsLevel.MsAcademicYear.Id,
                                                    SchoolLevel = a.MsLevel.Id,
                                                    YearLevel = a.Id
                                                }).FirstOrDefault();

                var newStudentAdmissionData = new MsAdmissionData
                {
                    UserIn = "API0001",
                    IdSchool = body.IdSchool,
                    IdRegistrant = body.IdRegistrant,
                    IdStudent = body.IdStudent,
                    DateofEntry = ((body.AdmissionData.DateofEntry.ToString() == "" || body.AdmissionData.DateofEntry == null) ? null : body.AdmissionData.DateofEntry),
                    DateofFormPurchased = ((body.AdmissionData.DateofFormPurchased.ToString() == "" || body.AdmissionData.DateofFormPurchased == null) ? null : body.AdmissionData.DateofFormPurchased),
                    DateofApplReceived = ((body.AdmissionData.DateofApplReceived.ToString() == "" || body.AdmissionData.DateofApplReceived == null) ? null : body.AdmissionData.DateofApplReceived),
                    DateofReregistration = ((body.AdmissionData.DateofReregistration.ToString() == "" || body.AdmissionData.DateofReregistration == null) ? null : body.AdmissionData.DateofReregistration),
                    JoinToSchoolDate = body.AdmissionData.JoinToSchoolDate,
                    AdmissionYear = (CurrAYData != null ? CurrAYData.AcademicYear : body.AdmissionData.AdmissionYear),
                    AcademicYear = (CurrAYData != null ? CurrAYData.AcademicYear : body.AdmissionData.AdmissionYear),
                    IdAcademicSemester = body.AdmissionData.IdAcademicSemester,
                    TotalScore = body.AdmissionData.TotalScore,
                    Grade = body.AdmissionData.Grade,
                    IdSchoolSubject = body.AdmissionData.IdSchoolSubject,
                    IdSchoolTPKSStatus = body.AdmissionData.IdSchoolTPKSStatus,
                    TPKSNotes = (body.AdmissionData.TPKSNotes == null ? "" : body.AdmissionData.TPKSNotes),
                    IdSchoolLevel = (CurrAYData != null ? CurrAYData.SchoolLevel : body.AdmissionData.IdSchoolLevel.ToString()), //body.AdmissionData.IdSchoolLevel,
                    IdYearLevel = (CurrAYData != null ? CurrAYData.YearLevel : body.AdmissionData.IdYearLevel.ToString()),
                    IsEnrolledForFirstTime = body.AdmissionData.IsEnrolledForFirstTime
                };

                _dbContext.Entity<MsAdmissionData>().Add(newStudentAdmissionData);

                foreach (var rptChargingAdmission in body.ChargingAdmission)
                {
                    var CurrAYChargingAdmission = _dbContext.Entity<MsGrade>()
                                            .Include(a => a.MsLevel).ThenInclude(b => b.MsAcademicYear)
                                              .Where(x => x.Code == rptChargingAdmission.IdYearLevel //body.AdmissionData.IdYearLevel.ToString()
                                              && x.MsLevel.MsAcademicYear.Code == rptChargingAdmission.AcademicYear
                                              && x.MsLevel.MsAcademicYear.IdSchool == body.IdSchool)//body.AdmissionData.AcademicYear)
                                              .Select(a => new {
                                                  AcademicYear = a.MsLevel.MsAcademicYear.Id,
                                                  SchoolLevel = a.MsLevel.Id,
                                                  YearLevel = a.Id
                                              }).FirstOrDefault();

                    //var newStudentChargingAdmission = new TrStudentChargingAdmission
                    //{
                    //    UserIn = "API0001",
                    //    Id = Guid.NewGuid().ToString(),
                    //    IdSchool = body.IdSchool,
                    //    IdStudent = body.IdStudent,
                    //    FormNumber = rptChargingAdmission.FormNumber,
                    //    AdmissionYear = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.AcademicYear : rptChargingAdmission.AdmissionYear),// rptChargingAdmission.AdmissionYear,
                    //    AcademicYear = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.AcademicYear : rptChargingAdmission.AdmissionYear),
                    //    IdAcademicSemester = rptChargingAdmission.IdAcademicSemester,
                    //    IdSchoolLevel = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.SchoolLevel : rptChargingAdmission.IdSchoolLevel),//rptChargingAdmission.IdSchoolLevel,
                    //    IdYearLevel = (CurrAYChargingAdmission != null ? CurrAYChargingAdmission.YearLevel : rptChargingAdmission.IdYearLevel),//rptChargingAdmission.IdYearLevel,
                    //    ComponentClass = rptChargingAdmission.ComponentClass,
                    //    FeeGroupName = rptChargingAdmission.FeeGroupName,
                    //    ChargingAmount = rptChargingAdmission.ChargingAmount,
                    //    DueDate = rptChargingAdmission.DueDate,
                    //    ChargingStatus = rptChargingAdmission.ChargingStatus
                    //};


                    //_dbContext.Entity<TrStudentChargingAdmission>().Add(newStudentChargingAdmission);

                }

                var getSiblingStudent = _dbContext.Entity<MsSiblingGroup>()
                                       .Where(x => x.IdStudent == body.SiblingIdStudent)
                                       .FirstOrDefault();
                //.FirstOrDefaultAsync(x => x.Id == body.SiblingIdStudent,CancellationToken);

                if (getSiblingStudent == null)
                {
                    var newSiblingGroup = new MsSiblingGroup
                    {
                        UserIn = "API0001",
                        Id = Guid.NewGuid().ToString(),
                        IdStudent = body.IdStudent
                    };
                    _dbContext.Entity<MsSiblingGroup>().Add(newSiblingGroup);
                }
                else
                {
                    var getSiblingGroup = _dbContext.Entity<MsSiblingGroup>()
                                        .Where(x => x.Id == getSiblingStudent.Id && x.IdStudent == body.IdStudent)
                                        .FirstOrDefault();
                    if (getSiblingGroup == null)
                    {
                        var newSiblingGroup = new MsSiblingGroup
                        {
                            UserIn = "API0001",
                            Id = getSiblingStudent.Id,
                            IdStudent = body.IdStudent
                        };
                        _dbContext.Entity<MsSiblingGroup>().Add(newSiblingGroup);
                    }

                }

                await NewMethod(body, getSiblingStudent);

            }


            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

             return Request.CreateApiResult2();

            async Task NewMethod(TransferStudentRequest body, MsSiblingGroup getSiblingStudent)
            {
                if (getSiblingStudent != null)
                {
                    // studentid punya sibling dengan parent yang sama
                    var getParent = await _dbContext.Entity<MsStudent>()
                            .Include(x => x.StudentParents).ThenInclude(x => x.Parent)
                            .FirstOrDefaultAsync(x => x.Id == body.SiblingIdStudent, CancellationToken);

                    foreach (var rptParent in body.Parents)
                    {
                        if (getParent.StudentParents.Where(a => a.Parent.IdParentRole == rptParent.IdParentRole).ToList().Count == 0)
                        {
                            var generateIdParent = Guid.NewGuid().ToString();
                            var newParent = new MsParent
                            {
                                Id = generateIdParent,
                                UserIn = "API0001",
                                FirstName = rptParent.FirstName,
                                LastName = rptParent.LastName,
                                Gender = rptParent.Gender,
                                POB = rptParent.POB,
                                DOB = rptParent.DOB,
                                IdParentRole = rptParent.IdParentRole,
                                AliveStatus = rptParent.AliveStatus,
                                IdReligion = rptParent.IdReligion,
                                IdLastEducationLevel = rptParent.IdLastEducationLevel,
                                IdNationality = rptParent.IdNationality,
                                IdCountry = rptParent.IdCountry,
                                FamilyCardNumber = rptParent.FamilyCardNumber,
                                NIK = rptParent.NIK,
                                PassportNumber = rptParent.PassportNumber,
                                PassportExpDate = ((rptParent.PassportExpDate.ToString() == "" || rptParent.PassportExpDate == null) ? null : rptParent.PassportExpDate),
                                KITASNumber = rptParent.KITASNumber,
                                KITASExpDate = ((rptParent.KITASExpDate.ToString() == "" || rptParent.KITASExpDate == null) ? null : rptParent.KITASExpDate),
                                BinusianStatus = rptParent.BinusianStatus,
                                IdBinusian = rptParent.IdBinusian,
                                ParentNameForCertificate = "",
                                ResidenceAddress = rptParent.ResidenceAddress,
                                HouseNumber = rptParent.HouseNumber,
                                RT = rptParent.RT,
                                RW = rptParent.RW,
                                VillageDistrict = rptParent.VillageDistrict,
                                SubDistrict = rptParent.SubDistrict,
                                IdAddressCity = rptParent.IdAddressCity,
                                IdAddressStateProvince = rptParent.IdAddressStateProvince,
                                IdAddressCountry = rptParent.IdAddressCountry,
                                PostalCode = rptParent.PostalCode,
                                ResidencePhoneNumber = (rptParent.ResidencePhoneNumber == null ? "" : rptParent.ResidencePhoneNumber),
                                MobilePhoneNumber1 = rptParent.MobilePhoneNumber1,
                                MobilePhoneNumber2 = (rptParent.MobilePhoneNumber2 == null ? "" : rptParent.MobilePhoneNumber2),
                                MobilePhoneNumber3 = (rptParent.MobilePhoneNumber3 == null ? "" : rptParent.MobilePhoneNumber3),
                                PersonalEmailAddress = rptParent.PersonalEmailAddress,
                                WorkEmailAddress = (rptParent.WorkEmailAddress == null ? "" : rptParent.WorkEmailAddress),
                                IdOccupationType = rptParent.IdOccupationType,
                                OccupationPosition = rptParent.OccupationPosition,
                                CompanyName = (rptParent.CompanyName == null ? "" : rptParent.CompanyName),
                                IdParentSalaryGroup = rptParent.IdParentSalaryGroup
                            };

                            _dbContext.Entity<MsParent>().Add(newParent);

                            var newStudentParent = new MsStudentParent
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserIn = "API0001",
                                IdStudent = body.IdStudent,
                                IdParent = generateIdParent
                            };

                            _dbContext.Entity<MsStudentParent>().Add(newStudentParent);

                        }
                        else if (getParent.StudentParents.Where(a => a.Parent.IdParentRole == rptParent.IdParentRole && (a.Parent.UserUp == null || a.Parent.UserUp == "API0001")).ToList().Count > 0)
                        {
                            //parent exists dan data belum diubah oleh internal binus
                            var updateParent = getParent.StudentParents.Where(a => a.Parent.IdParentRole == rptParent.IdParentRole && (a.Parent.UserUp == null || a.Parent.UserUp == "API0001")).First().Parent;

                            updateParent.UserUp = "API0001";
                            updateParent.IsActive = true;
                            updateParent.FirstName = rptParent.FirstName;
                            updateParent.LastName = rptParent.LastName;
                            updateParent.Gender = rptParent.Gender;
                            updateParent.POB = rptParent.POB;
                            updateParent.DOB = rptParent.DOB;
                            updateParent.AliveStatus = rptParent.AliveStatus;
                            updateParent.IdReligion = rptParent.IdReligion;
                            updateParent.IdLastEducationLevel = rptParent.IdLastEducationLevel;
                            updateParent.IdNationality = rptParent.IdNationality;
                            updateParent.IdCountry = rptParent.IdCountry;
                            updateParent.FamilyCardNumber = rptParent.FamilyCardNumber;
                            updateParent.NIK = rptParent.NIK;
                            updateParent.KITASNumber = ((rptParent.KITASNumber == "" || rptParent.KITASNumber == null) ? null : rptParent.KITASNumber);
                            updateParent.KITASExpDate = ((rptParent.KITASExpDate.ToString() == "" || rptParent.KITASExpDate == null) ? null : rptParent.KITASExpDate);
                            updateParent.PassportNumber = ((rptParent.PassportNumber == "" || rptParent.PassportNumber == null) ? null : rptParent.PassportNumber);
                            updateParent.PassportExpDate = ((rptParent.PassportExpDate.ToString() == "" || rptParent.PassportExpDate == null) ? null : rptParent.PassportExpDate);
                            updateParent.BinusianStatus = rptParent.BinusianStatus;
                            updateParent.IdBinusian = rptParent.IdBinusian;
                            updateParent.ResidenceAddress = rptParent.ResidenceAddress;
                            updateParent.HouseNumber = rptParent.HouseNumber;
                            updateParent.RT = rptParent.RT;
                            updateParent.RW = rptParent.RW;
                            updateParent.VillageDistrict = rptParent.VillageDistrict;
                            updateParent.SubDistrict = rptParent.SubDistrict;
                            updateParent.IdAddressCity = rptParent.IdAddressCity;
                            updateParent.IdAddressStateProvince = rptParent.IdAddressStateProvince;
                            updateParent.IdAddressCountry = rptParent.IdAddressCountry;
                            updateParent.PostalCode = rptParent.PostalCode;
                            updateParent.ResidencePhoneNumber = (rptParent.ResidencePhoneNumber == null ? "" : rptParent.ResidencePhoneNumber);
                            updateParent.MobilePhoneNumber1 = rptParent.MobilePhoneNumber1;
                            updateParent.MobilePhoneNumber2 = (rptParent.MobilePhoneNumber2 == null ? "" : rptParent.MobilePhoneNumber2);
                            updateParent.MobilePhoneNumber3 = (rptParent.MobilePhoneNumber3 == null ? "" : rptParent.MobilePhoneNumber3);
                            updateParent.PersonalEmailAddress = rptParent.PersonalEmailAddress;
                            updateParent.WorkEmailAddress = (rptParent.WorkEmailAddress == null ? "" : rptParent.WorkEmailAddress);
                            updateParent.IdOccupationType = rptParent.IdOccupationType;
                            updateParent.OccupationPosition = rptParent.OccupationPosition;
                            updateParent.CompanyName = (rptParent.CompanyName == null ? "" : rptParent.CompanyName);
                            updateParent.IdParentSalaryGroup = rptParent.IdParentSalaryGroup;

                            _dbContext.Entity<MsParent>().Update(updateParent);
                        }
                    }

                    foreach (var rptParent in getParent.StudentParents)
                    {
                        var newStudentParent = new MsStudentParent
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserIn = "API0001",
                            IdStudent = body.IdStudent,
                            IdParent = rptParent.IdParent

                        };
                        _dbContext.Entity<MsStudentParent>().Add(newStudentParent);
                    }

                }
                else
                {
                    //student baru masuk dan tidak punya sibling
                    foreach (var rptParent in body.Parents)
                    {

                        var generateIdParent = Guid.NewGuid().ToString();
                        var newParent = new MsParent
                        {
                            Id = generateIdParent,
                            UserIn = "API0001",
                            FirstName = rptParent.FirstName,
                            LastName = rptParent.LastName,
                            Gender = rptParent.Gender,
                            POB = rptParent.POB,
                            DOB = rptParent.DOB,
                            IdParentRole = rptParent.IdParentRole,
                            AliveStatus = rptParent.AliveStatus,
                            IdReligion = rptParent.IdReligion,
                            IdLastEducationLevel = rptParent.IdLastEducationLevel,
                            IdNationality = rptParent.IdNationality,
                            IdCountry = rptParent.IdCountry,
                            FamilyCardNumber = rptParent.FamilyCardNumber,
                            NIK = rptParent.NIK,
                            PassportNumber = rptParent.PassportNumber,
                            PassportExpDate = ((rptParent.PassportExpDate.ToString() == "" || rptParent.PassportExpDate == null) ? null : rptParent.PassportExpDate),
                            KITASNumber = rptParent.KITASNumber,
                            KITASExpDate = ((rptParent.KITASExpDate.ToString() == "" || rptParent.KITASExpDate == null) ? null : rptParent.KITASExpDate),
                            BinusianStatus = rptParent.BinusianStatus,
                            IdBinusian = rptParent.IdBinusian,
                            ParentNameForCertificate = "",
                            ResidenceAddress = rptParent.ResidenceAddress,
                            HouseNumber = rptParent.HouseNumber,
                            RT = rptParent.RT,
                            RW = rptParent.RW,
                            VillageDistrict = rptParent.VillageDistrict,
                            SubDistrict = rptParent.SubDistrict,
                            IdAddressCity = rptParent.IdAddressCity,
                            IdAddressStateProvince = rptParent.IdAddressStateProvince,
                            IdAddressCountry = rptParent.IdAddressCountry,
                            PostalCode = rptParent.PostalCode,
                            ResidencePhoneNumber = (rptParent.ResidencePhoneNumber == null ? "" : rptParent.ResidencePhoneNumber),
                            MobilePhoneNumber1 = rptParent.MobilePhoneNumber1,
                            MobilePhoneNumber2 = (rptParent.MobilePhoneNumber2 == null ? "" : rptParent.MobilePhoneNumber2),
                            MobilePhoneNumber3 = (rptParent.MobilePhoneNumber3 == null ? "" : rptParent.MobilePhoneNumber3),
                            PersonalEmailAddress = rptParent.PersonalEmailAddress,
                            WorkEmailAddress = (rptParent.WorkEmailAddress == null ? "" : rptParent.WorkEmailAddress),
                            IdOccupationType = rptParent.IdOccupationType,
                            OccupationPosition = rptParent.OccupationPosition,
                            CompanyName = (rptParent.CompanyName == null ? "" : rptParent.CompanyName),
                            IdParentSalaryGroup = rptParent.IdParentSalaryGroup
                        };


                        _dbContext.Entity<MsParent>().Add(newParent);

                        var newStudentParent = new MsStudentParent
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserIn = "API0001",
                            IdStudent = body.IdStudent,
                            IdParent = generateIdParent.ToString()

                        };
                        _dbContext.Entity<MsStudentParent>().Add(newStudentParent);



                    }
                }
            }
        }

       
    }

    
}
