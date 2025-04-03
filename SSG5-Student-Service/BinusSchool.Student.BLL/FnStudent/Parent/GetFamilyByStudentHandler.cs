using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Data.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.School.FnSubject;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Information;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.Student.FnStudent.ParentRole;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Parent
{
    public class GetFamilyByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetFamilyByStudentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentRequest>(nameof(GetStudentRequest.IdStudent));
            var studentParentsIds = await _dbContext.Entity<MsStudentParent>()
                                .Include(x => x.Parent).ThenInclude(x => x.Nationality)
                                .Where(x => x.IdStudent == param.IdStudent)
                                .Select(x => new GetParentResult {
                                    Id = x.Id,
                                    Description = "Family",
                                    nameInfo = new NameInfoVm
                                    {
                                        FirstName = x.Parent.FirstName.Trim(),
                                        //MiddleName = x.Parent.MiddleName,
                                        LastName = x.Parent.LastName.Trim()
                                    },
                                    birthInfo = new BirthInfoVm
                                    {
                                        POB = x.Parent.POB,
                                        DOB = x.Parent.DOB,
                                        IdNationality = new ItemValueVm
                                        {
                                            Id = x.Parent.Nationality.Id,
                                            Description = x.Parent.Nationality.NationalityName
                                        },
                                        IdCountry = new ItemValueVm
                                        {
                                            Id = x.Parent.Country.Id,
                                            Description = x.Parent.Country.CountryName
                                        }
                                    },
                                    addressInfo = new AddressInfoVm
                                    {
                                        ResidenceAddress = x.Parent.ResidenceAddress,
                                        HouseNumber = x.Parent.HouseNumber,
                                        RT = x.Parent.RT,
                                        RW = x.Parent.RW,
                                        VillageDistrict = x.Parent.VillageDistrict,
                                        SubDistrict = x.Parent.SubDistrict,
                                        IdAddressCity = x.Parent.IdAddressCity,
                                        IdAddressStateProvince = x.Parent.IdAddressStateProvince,
                                        IdAddressCountry = x.Parent.IdAddressCountry,
                                        PostalCode = x.Parent.PostalCode,
                                    },
                                    contactInfo = new ContactInfoVm
                                    {
                                        ResidencePhoneNumber = x.Parent.ResidencePhoneNumber,
                                        MobilePhoneNumber1 = x.Parent.MobilePhoneNumber1,
                                        MobilePhoneNumber2 = x.Parent.MobilePhoneNumber2,
                                        MobilePhoneNumber3 = x.Parent.MobilePhoneNumber3,
                                        PersonalEmailAddress = x.Parent.PersonalEmailAddress
                                    },
                                    occupationInfo = new OccupationInfoVm
                                    {
                                        CompanyNama = x.Parent.CompanyName,
                                        JobPosition = x.Parent.OccupationPosition,
                                        IdOccupationType = new ItemValueVm
                                        {
                                            Id = x.Parent.OccupationType.Id,
                                            Description = x.Parent.OccupationType.OccupationTypeNameEng
                                        },
                                        IdParentSalaryGroup = new ItemValueVm
                                        {
                                            Id = x.Parent.ParentSalaryGroup.IdParentSalaryGroup.ToString(),
                                            Description = x.Parent.ParentSalaryGroup.ParentSalaryGroupName
                                        }
                                    }
                                })
                                .ToListAsync(CancellationToken);

            var query = await _dbContext.Entity<LtParentRole>()
                    .Select(x => new GetParentRoleResult
                    {
                        Id = x.Id,
                        ParentRoleName = x.ParentRoleName,
                        ParentRoleNameEng = x.ParentRoleNameEng
                    }).ToListAsync(CancellationToken);

            var queryResult = query.Select(x => new GetParentRoleResult
                {
                    FamilyInformation = studentParentsIds.FirstOrDefault(y => y.personalInfoVm.ParentRole.Equals(x.Id))
                }).ToList();

            return Request.CreateApiResult2(queryResult as object);
        }
    
    }
}
