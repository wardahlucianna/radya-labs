using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Common.Model.Information;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Parent.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Student.FnStudent.Parent
{
    public class ParentHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbParentContext;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        public ParentHandler(IStudentDbContext dbParentContext
            , IMachineDateTime dateTime)
        {
            _dbParentContext = dbParentContext;
            _dateTime = dateTime;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }
        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var parentData = _dbParentContext.Entity<MsParent>()
                                .Where(x => x.Id == id)
                                .Select(x => new { x.IdAddressCountry, x.IdAddressCity, x.IdAddressStateProvince })
                                .FirstOrDefault();

            if (parentData is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["parent"], "Id", id));

            var addressCountry = _dbParentContext.Entity<LtCountry>()
                                .Where(x => x.Id == parentData.IdAddressCountry)
                                .Select(x => new { id = x.Id, description = x.CountryName })
                                .FirstOrDefault();

            var addressProvince = _dbParentContext.Entity<LtProvince>()
                                .Where(x => x.Id == (parentData == null ? null : parentData.IdAddressStateProvince)
                                && x.IdCountry == (addressCountry == null ? null : addressCountry.id))
                                .Select(x => new { id = x.Id, description = x.ProvinceName })
                                .FirstOrDefault();

            var addressCity = _dbParentContext.Entity<LtCity>()
                                .Where(x => x.Id == (parentData == null ? null : parentData.IdAddressCity)
                                && x.IdCountry == (addressCountry == null ? null : addressCountry.id)
                                && x.IdProvince == (addressProvince == null ? null : addressProvince.id))
                                .Select(x => new { id = x.Id, description = x.CityName })
                                .FirstOrDefault();

            var addressCountryInfo = new ItemValueVm
            {
                Id = addressCountry == null ? null : addressCountry.id,
                Description = addressCountry == null ? null : addressCountry.description,
            };

            var addressCityInfo = new ItemValueVm
            {
                Id = addressCity == null ? null : addressCity.id,
                Description = addressCity == null ? null : addressCity.description
            };

            var addressProvinceInfo = new ItemValueVm
            {
                Id = addressProvince == null ? null : addressProvince.id,
                Description = addressProvince == null ? null : addressProvince.description,
            };

            var query = await _dbParentContext.Entity<MsParent>()
                .Include(x => x.Nationality)
                .Include(x => x.Country)
                .Include(x => x.ParentRole)
                .Include(x => x.OccupationType)
                .Include(x => x.Religion)
                .Include(x => x.ParentSalaryGroup)
                .Include(x => x.LtAliveStatus)
                .Include(x => x.LtBinusianStatus)
                .Include(x => x.LastEducationLevel)
                .Include(x => x.ParentRelationship)
                .Select(x => new GetParentDetailResult
                {
                    Id = x.Id,
                    Description = x.ParentRole.ParentRoleNameEng,
                    NameInfo = new NameInfoVm
                    {
                        FirstName = x.LastName.Contains(" ") == true ? x.FirstName + " " + x.LastName.Substring(0, x.LastName.LastIndexOf(' ')).TrimEnd() : x.FirstName.Trim(),
                        //MiddleName = x.MiddleName,
                        LastName = x.LastName.Contains(" ") == true ? x.LastName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : x.LastName.Trim()
                    },
                    MedicalInfo = new MedicalInfoVm
                    {
                        Gender = x.Gender
                    },
                    PersonalInfoVm = new PersonalParentInfoVm
                    {
                        Relationship = new ItemValueVm
                        {
                            Id = x.ParentRelationship.IdParentRelationship.ToString(),
                            Description = x.ParentRelationship.RelationshipNameEng
                        },
                        ParentRole = new ItemValueVm
                        {
                            Id = x.ParentRole.Id,
                            Description = x.ParentRole.ParentRoleNameEng
                        },
                        AliveStatus = new ItemValueVm
                        {
                            Id = x.LtAliveStatus.AliveStatus.ToString(),
                            Description = x.LtAliveStatus.AliveStatusName
                        },
                        LastEducation = new ItemValueVm
                        {
                            Id = x.LastEducationLevel.IdLastEducationLevel.ToString(),
                            Description = x.LastEducationLevel.LastEducationLevelName
                        },
                        BinusianStatus = new ItemValueVm
                        {
                            Id = x.LtBinusianStatus.BinusianStatus.ToString(),
                            Description = x.LtBinusianStatus.BinusianStatusName
                        },
                        BinusianID = x.IdBinusian,
                        NameForCertificate = x.ParentNameForCertificate
                    },
                    BirthInfo = new BirthInfoDetailVm
                    {
                        POB = x.POB,
                        DOB = x.DOB,
                        IdNationality = new ItemValueVm
                        {
                            Id = x.Nationality.Id,
                            Description = x.Nationality.NationalityName
                        },
                        IdCountry = new ItemValueVm
                        {
                            Id = x.Country.Id,
                            Description = x.Country.CountryName
                        }
                    },
                    ReligionInfo = new ReligionInfoVm
                    {
                        //IdReligion = x.IdReligion
                        IdReligion = new ItemValueVm
                        {
                            Id = x.Religion.Id,
                            Description = x.Religion.ReligionName
                        }
                    },
                    AddressInfo = new AddressInfoDetailVm
                    {
                        ResidenceAddress = x.ResidenceAddress,
                        HouseNumber = x.HouseNumber,
                        RT = x.RT,
                        RW = x.RW,
                        VillageDistrict = x.VillageDistrict,
                        SubDistrict = x.SubDistrict,
                        IdAddressCity = addressCityInfo,
                        IdAddressStateProvince = addressProvinceInfo,
                        IdAddressCountry = addressCountryInfo,
                        PostalCode = x.PostalCode
                    },
                    CardInfo = new CardInfoVm
                    {
                        FamilyCardNumber = x.FamilyCardNumber,
                        NIK = x.NIK,
                        KITASNumber = x.KITASNumber,
                        KITASExpDate = x.KITASExpDate.HasValue ? x.KITASExpDate : null,
                        PassportNumber = x.PassportNumber,
                        PassportExpDate = x.PassportExpDate.HasValue ? x.PassportExpDate : null
                    },
                    ContactInfo = new ContactInfoVm
                    {
                        ResidencePhoneNumber = x.ResidencePhoneNumber,
                        MobilePhoneNumber1 = x.MobilePhoneNumber1,
                        MobilePhoneNumber2 = x.MobilePhoneNumber2,
                        MobilePhoneNumber3 = x.MobilePhoneNumber3,
                        PersonalEmailAddress = x.PersonalEmailAddress
                    },
                    OccupationInfo = new OccupationInfoVm
                    {
                        CompanyNama = x.CompanyName == null ? null : x.CompanyName,
                        JobPosition = x.OccupationPosition == null ? null : x.OccupationPosition,
                        IdOccupationType = new ItemValueVm
                        {
                            Id = x.OccupationType.Id == null ? null : x.OccupationType.Id,
                            Description = x.OccupationType.OccupationTypeNameEng == null ? null : x.OccupationType.OccupationTypeNameEng,
                        },
                        IdParentSalaryGroup = new ItemValueVm
                        {

                            Id = x.ParentSalaryGroup.IdParentSalaryGroup.ToString(),
                            Description = x.ParentSalaryGroup.ParentSalaryGroupName == null ? null : x.ParentSalaryGroup.ParentSalaryGroupName,
                        }
                    },
                    Audit = x.GetRawAuditResult2()
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);


            return Request.CreateApiResult2(query as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionRequest>(nameof(CollectionSchoolRequest.IdSchool));
            var query = _dbParentContext.Entity<MsParent>();
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .Select(x => new GetParentResult
                    {
                        Id = x.Id,
                        Description = x.ParentRole.ParentRoleNameEng,
                        personalInfoVm = new PersonalParentInfoVm
                        {
                            Relationship = new ItemValueVm
                            {
                                Id = x.ParentRelationship.IdParentRelationship.ToString(),
                                Description = x.ParentRelationship.RelationshipNameEng
                            },
                            ParentRole = new ItemValueVm
                            {
                                Id = x.ParentRole.Id,
                                Description = x.ParentRole.ParentRoleNameEng
                            },
                            AliveStatus = new ItemValueVm
                            {
                                Id = x.LtAliveStatus.AliveStatus.ToString(),
                                Description = x.LtAliveStatus.AliveStatusName
                            },
                            LastEducation = new ItemValueVm
                            {
                                Id = x.LastEducationLevel.IdLastEducationLevel.ToString(),
                                Description = x.LastEducationLevel.LastEducationLevelName
                            },
                            BinusianStatus = new ItemValueVm
                            {
                                Id = x.LtBinusianStatus.BinusianStatus.ToString(),
                                Description = x.LtBinusianStatus.BinusianStatusName
                            },
                            BinusianID = x.IdBinusian,
                            NameForCertificate = x.ParentNameForCertificate
                        },
                        nameInfo = new NameInfoVm
                        {
                            FirstName = x.LastName.Contains(" ") == true ? x.FirstName + " " + x.LastName.Substring(0, x.LastName.LastIndexOf(' ')).TrimEnd() : x.FirstName.Trim(),
                            //MiddleName = x.MiddleName,
                            LastName = x.LastName.Contains(" ") == true ? x.LastName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last() : x.LastName.Trim()
                        },
                        birthInfo = new BirthInfoVm
                        {
                            POB = x.POB,
                            DOB = x.DOB,
                            /*IdNationality = x.IdNationality,
                            IdCountry = x.IdCountry*/
                            IdNationality = new ItemValueVm
                            {
                                Id = x.Nationality.Id,
                                Description = x.Nationality.NationalityName
                            },
                            IdCountry = new ItemValueVm
                            {
                                Id = x.Country.Id,
                                Description = x.Country.CountryName
                            }
                        },
                        religionInfo = new ReligionInfoVm
                        {
                            //IdReligion = x.IdReligion
                            IdReligion = new ItemValueVm
                            {
                                Id = x.Religion.Id,
                                Description = x.Religion.ReligionName
                            }
                        },
                        addressInfo = new AddressInfoVm
                        {
                            ResidenceAddress = x.ResidenceAddress,
                            HouseNumber = x.HouseNumber,
                            RT = x.RT,
                            RW = x.RW,
                            VillageDistrict = x.VillageDistrict,
                            SubDistrict = x.SubDistrict,
                            IdAddressCity = x.IdAddressCity,
                            IdAddressStateProvince = x.IdAddressStateProvince,
                            IdAddressCountry = x.IdAddressCountry,
                            PostalCode = x.PostalCode
                        },
                        cardInfo = new CardInfoVm
                        {
                            FamilyCardNumber = x.FamilyCardNumber,
                            NIK = x.NIK,
                            KITASNumber = x.KITASNumber,
                            KITASExpDate = x.KITASExpDate.HasValue ? x.KITASExpDate : null,
                            PassportNumber = x.PassportNumber,
                            PassportExpDate = x.PassportExpDate.HasValue ? x.PassportExpDate : null
                        },
                        contactInfo = new ContactInfoVm
                        {
                            ResidencePhoneNumber = x.ResidencePhoneNumber,
                            MobilePhoneNumber1 = x.MobilePhoneNumber1,
                            MobilePhoneNumber2 = x.MobilePhoneNumber2,
                            MobilePhoneNumber3 = x.MobilePhoneNumber3,
                            PersonalEmailAddress = x.PersonalEmailAddress
                        },
                        Audit = x.GetRawAuditResult2()
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }
        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateParentRequest, UpdateParentValidator>();
                _transaction = await _dbParentContext.BeginTransactionAsync(CancellationToken);

                var getdata = await _dbParentContext.Entity<MsParent>().Where(p => p.Id == body.IdParent).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Parent ID"], "Id", body.IdParent));

                foreach (var prop in body.GetType().GetProperties())
                {
                    //var oldVal = getdata.GetType().GetProperty(prop.Name).GetValue(getdata, null);
                    var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);

                    Type myType = getdata.GetType();
                    PropertyInfo pinfo = myType.GetProperty(prop.Name);
                    if (pinfo != null && newVal != null)
                    {
                        pinfo.SetValue(getdata, newVal);
                    }
                }

                _dbParentContext.Entity<MsParent>().Update(getdata);

                await _dbParentContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
