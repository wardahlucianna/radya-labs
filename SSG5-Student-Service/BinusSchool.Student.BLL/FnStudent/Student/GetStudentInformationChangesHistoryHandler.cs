using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    public class GetStudentInformationChangesHistoryHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentInformationChangesHistoryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetStudentInformationChangesHistoryRequest, GetStudentInformationChangesHistoryValidator>();

            var columns = new[] { "fieldName", "oldValue", "newValue", "actionDate", "proposedBy", "notes", "approvalStatus" };

            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Regex.Replace(x.FieldName, '([A-Z][a-z])', ' $1', System.Text.RegularExpressions.RegexOptions.Compiled).Trim()" },
                { columns[1], "OldFieldValue" },
                { columns[2], "CurrentFieldValue" },
                { columns[3], "ApprovalDate.HasValue ? ApprovalDate.Value : RequestedDate.Value" },
                { columns[4], "RequestedBy" },
                { columns[5], "Notes" },
                { columns[6], "StudentDataApprovalStatus.ApprovalStatusName" }
            };

            var predicate = PredicateBuilder.True<TrStudentInfoUpdate>();
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.FieldName, param.SearchPattern())
                    || EF.Functions.Like(x.OldFieldValue, param.SearchPattern())
                    || EF.Functions.Like(x.CurrentFieldValue, param.SearchPattern())
                    || EF.Functions.Like(x.RequestedBy, param.SearchPattern())
                    || EF.Functions.Like(x.Notes, param.SearchPattern())
                    );

            var getIdParent = await _dbContext.Entity<MsStudentParent>()
                                .Where(x => x.IdStudent == param.IdStudent)
                                .Select(x => x.IdParent)
                                .ToListAsync(CancellationToken);

            var getParentRoleList = await _dbContext.Entity<LtParentRole>()
                                        .ToListAsync(CancellationToken);

            var query = _dbContext.Entity<TrStudentInfoUpdate>()
                            .Include(x => x.StudentDataApprovalStatus)
                            .Where(x => (x.IdUser == param.IdStudent ||
                                        getIdParent.Any(y => y == x.IdUser))
                                         &&
                                        ((param.IdStudentInfoUpdateList != null) ? param.IdStudentInfoUpdateList.Any(y => y == x.Id) : true)
                                        )
                            .Where(predicate)
                            .OrderByDescending(x => x.RequestedDate);

            var items = await query
                            .OrderByDynamic(param, aliasColumns)
                            .SetPagination(param)
                            .Select(x => new GetStudentInformationChangesHistoryResult
                            {
                                Constraint1Value = x.Constraint1Value,
                                TableName = x.TableName,
                                DatabaseFieldName = x.FieldName,
                                // split space
                                FieldName = Regex.Replace(x.FieldName, "([A-Z][a-z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim(),
                                OldValue = x.OldFieldValue,
                                NewValue = x.CurrentFieldValue,
                                ActionDate = x.ApprovalDate.HasValue ? x.ApprovalDate : x.RequestedDate,
                                ProposedBy = x.RequestedBy,
                                Notes = x.Notes,
                                ApprovalStatus = new ItemValueVm
                                {
                                    Id = x.IdApprovalStatus.ToString(),
                                    Description = x.StudentDataApprovalStatus.ApprovalStatusName
                                },
                                IsParentUpdate = x.IsParentUpdate
                            })
                            .ToListAsync(CancellationToken);

            #region modify field description for Lt
            string[] fieldCountryList = new string[] { "IdBirthCountry", "IdCountry", "IdAddressCountry" };
            var itemsFieldCountryList = items.Where(x => fieldCountryList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltCountryListRaw = await _dbContext.Entity<LtCountry>()
                                    .Where(x => itemsFieldCountryList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsFieldCountryList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.CountryName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldProvinceList = new string[] { "IdBirthStateProvince", "IdAddressStateProvince" };
            var itemsFieldProvinceList = items.Where(x => fieldProvinceList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltProvinceListRaw = await _dbContext.Entity<LtProvince>()
                                    .Where(x => itemsFieldProvinceList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsFieldProvinceList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.ProvinceName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldCityList = new string[] { "IdBirthCity", "IdAddressCity" };
            var itemsFieldCityList = items.Where(x => fieldCityList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltCityListRaw = await _dbContext.Entity<LtCity>()
                                    .Where(x => itemsFieldCityList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsFieldCityList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.CityName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldNationalityList = new string[] { "IdNationality" };
            var itemsFieldNationalityList = items.Where(x => fieldNationalityList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltNationalityListRaw = await _dbContext.Entity<LtNationality>()
                                    .Where(x => itemsFieldNationalityList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsFieldNationalityList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.NationalityName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldReligionList = new string[] { "IdReligion" };
            var itemsFieldReligionList = items.Where(x => fieldReligionList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltReligionListRaw = await _dbContext.Entity<LtReligion>()
                                    .Where(x => itemsFieldReligionList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsFieldReligionList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.ReligionName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldReligionSubjectList = new string[] { "IdReligionSubject" };
            var itemsFieldReligionSubjectList = items.Where(x => fieldReligionSubjectList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltReligionSubjectListRaw = await _dbContext.Entity<LtReligionSubject>()
                                    .Where(x => itemsFieldReligionSubjectList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsFieldReligionSubjectList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.ReligionSubjectName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldChildStatusList = new string[] { "IdChildStatus" };
            var itemsChildStatusList = items.Where(x => fieldChildStatusList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltChildStatusListRaw = await _dbContext.Entity<LtChildStatus>()
                                    .Where(x => itemsChildStatusList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsChildStatusList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.ChildStatusName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldParentRoleList = new string[] { "EmergencyContactRole" };
            var itemsParentRoleList = items.Where(x => fieldParentRoleList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltParentRoleListRaw = await _dbContext.Entity<LtParentRole>()
                                    .Where(x => itemsParentRoleList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsParentRoleList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.ParentRoleNameEng
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldStayingWithList = new string[] { "IdStayingWith" };
            var itemsStayingWithList = items.Where(x => fieldStayingWithList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltStayingWithListRaw = await _dbContext.Entity<LtStayingWith>()
                                    .Where(x => itemsStayingWithList.Select(y => y.OldValue).Any(y => y == x.IdStayingWith.ToString()) ||
                                                itemsStayingWithList.Select(y => y.NewValue).Any(y => y == x.IdStayingWith.ToString()))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.IdStayingWith.ToString(),
                                        Description = x.StayingWithName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldPreviousSchoolOldList = new string[] { "IdPreviousSchoolOld" };
            var itemsPreviousSchoolOldList = items.Where(x => fieldPreviousSchoolOldList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltPreviousSchoolOldListRaw = await _dbContext.Entity<MsPreviousSchoolOld>()
                                    .Where(x => itemsPreviousSchoolOldList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsPreviousSchoolOldList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.SchoolName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldPreviousSchoolNewList = new string[] { "IdPreviousSchoolNew" };
            var itemsPreviousSchoolNewList = items.Where(x => fieldPreviousSchoolNewList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltPreviousSchoolNewListRaw = await _dbContext.Entity<MsPreviousSchoolNew>()
                                    .Where(x => itemsPreviousSchoolNewList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsPreviousSchoolNewList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.SchoolName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldBankList = new string[] { "IdBank" };
            var itemsBankList = items.Where(x => fieldBankList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltBankListRaw = await _dbContext.Entity<MsBank>()
                                    .Where(x => itemsBankList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsBankList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.BankName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldParentRelationshipList = new string[] { "IdParentRelationship" };
            var itemsParentRelationshipList = items.Where(x => fieldParentRelationshipList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltParentRelationshipListRaw = await _dbContext.Entity<LtParentRelationship>()
                                    .Where(x => itemsParentRelationshipList.Select(y => y.OldValue).Any(y => y == x.IdParentRelationship.ToString()) ||
                                                itemsParentRelationshipList.Select(y => y.NewValue).Any(y => y == x.IdParentRelationship.ToString()))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.IdParentRelationship.ToString(),
                                        Description = x.RelationshipNameEng
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldAliveStatusList = new string[] { "AliveStatus" };
            var itemsAliveStatusList = items.Where(x => fieldAliveStatusList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltAliveStatusListRaw = await _dbContext.Entity<LtAliveStatus>()
                                    .Where(x => itemsAliveStatusList.Select(y => y.OldValue).Any(y => y == x.AliveStatus.ToString()) ||
                                                itemsAliveStatusList.Select(y => y.NewValue).Any(y => y == x.AliveStatus.ToString()))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.AliveStatus.ToString(),
                                        Description = x.AliveStatusName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldLastEducationLevelList = new string[] { "IdLastEducationLevel" };
            var itemsLastEducationLevelList = items.Where(x => fieldLastEducationLevelList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltLastEducationLevelListRaw = await _dbContext.Entity<LtLastEducationLevel>()
                                    .Where(x => itemsLastEducationLevelList.Select(y => y.OldValue).Any(y => y == x.IdLastEducationLevel.ToString()) ||
                                                itemsLastEducationLevelList.Select(y => y.NewValue).Any(y => y == x.IdLastEducationLevel.ToString()))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.IdLastEducationLevel.ToString(),
                                        Description = x.LastEducationLevelName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldBinusianStatusList = new string[] { "BinusianStatus" };
            var itemsBinusianStatusList = items.Where(x => fieldBinusianStatusList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltBinusianStatusListRaw = await _dbContext.Entity<LtBinusianStatus>()
                                    .Where(x => itemsBinusianStatusList.Select(y => y.OldValue).Any(y => y == x.BinusianStatus.ToString()) ||
                                                itemsBinusianStatusList.Select(y => y.NewValue).Any(y => y == x.BinusianStatus.ToString()))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.BinusianStatus.ToString(),
                                        Description = x.BinusianStatusName
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldOccupationTypeList = new string[] { "IdOccupationType" };
            var itemsOccupationTypeList = items.Where(x => fieldOccupationTypeList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltOccupationTypeListRaw = await _dbContext.Entity<LtOccupationType>()
                                    .Where(x => itemsOccupationTypeList.Select(y => y.OldValue).Any(y => y == x.Id) ||
                                                itemsOccupationTypeList.Select(y => y.NewValue).Any(y => y == x.Id))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.Id,
                                        Description = x.OccupationTypeNameEng
                                    })
                                    .ToListAsync(CancellationToken);

            string[] fieldParentSalaryGroupList = new string[] { "IdParentSalaryGroup" };
            var itemsParentSalaryGroupList = items.Where(x => fieldParentSalaryGroupList.Any(y => y == x.DatabaseFieldName)).ToList();
            var ltParentSalaryGroupListRaw = await _dbContext.Entity<LtParentSalaryGroup>()
                                    .Where(x => itemsParentSalaryGroupList.Select(y => y.OldValue).Any(y => y == x.IdParentSalaryGroup.ToString()) ||
                                                itemsParentSalaryGroupList.Select(y => y.NewValue).Any(y => y == x.IdParentSalaryGroup.ToString()))
                                    .Select(x => new ItemValueVm
                                    {
                                        Id = x.IdParentSalaryGroup.ToString(),
                                        Description = x.ParentSalaryGroupName
                                    })
                                    .ToListAsync(CancellationToken);
            #endregion

            string[] fieldIntBooleanList = new string[] { "IsHavingKJP", "IsHomeSchooling" };

            foreach (var item in items)
            {
                var tempLt = new List<ItemValueVm>();

                if (fieldCountryList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltCountryListRaw;
                if (fieldProvinceList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltProvinceListRaw;
                if (fieldCityList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltCityListRaw;
                if (fieldNationalityList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltNationalityListRaw;
                if (fieldReligionList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltReligionListRaw;
                if (fieldReligionSubjectList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltReligionSubjectListRaw;
                if (fieldChildStatusList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltChildStatusListRaw;
                if (fieldParentRoleList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltParentRoleListRaw;
                if (fieldStayingWithList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltStayingWithListRaw;
                if (fieldPreviousSchoolOldList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltPreviousSchoolOldListRaw;
                if (fieldPreviousSchoolNewList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltPreviousSchoolNewListRaw;
                if (fieldBankList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltBankListRaw;
                if (fieldParentRelationshipList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltParentRelationshipListRaw;
                if (fieldAliveStatusList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltAliveStatusListRaw;
                if (fieldLastEducationLevelList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltLastEducationLevelListRaw;
                if (fieldBinusianStatusList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltBinusianStatusListRaw;
                if (fieldOccupationTypeList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltOccupationTypeListRaw;
                if (fieldParentSalaryGroupList.Any(x => x == item.DatabaseFieldName))
                    tempLt = ltParentSalaryGroupListRaw;

                if (tempLt.Count > 0)
                {
                    item.OldValue = tempLt.Where(x => x.Id == item.OldValue).Select(x => x.Description).FirstOrDefault();
                    item.NewValue = tempLt.Where(x => x.Id == item.NewValue).Select(x => x.Description).FirstOrDefault();
                }

                // int bool
                if (fieldIntBooleanList.Any(x => x == item.DatabaseFieldName))
                {
                    item.OldValue = item.OldValue == "0" ? "False" : "True";
                    item.NewValue = item.NewValue == "0" ? "False" : "True";
                }

                // date field
                DateTime oldValueDateTime = new DateTime();
                DateTime newValueDateTime = new DateTime();
                bool isDateTimeOldValue = string.IsNullOrWhiteSpace(item.OldValue) ? false : DateTime.TryParse(item.OldValue, out oldValueDateTime);
                bool isDateTimeNewValue = string.IsNullOrWhiteSpace(item.NewValue) ? false : DateTime.TryParse(item.NewValue, out newValueDateTime);
                item.OldValue = isDateTimeOldValue ? oldValueDateTime.ToString("dd MMM yyyy") : item.OldValue;
                item.NewValue = isDateTimeNewValue ? newValueDateTime.ToString("dd MMM yyyy") : item.NewValue;

                // delete "Id" from fieldname
                var hasIdInString = item.FieldName.Length < 3 ? false : item.FieldName.Substring(0, 3) == "Id ";
                if (hasIdInString)
                    item.FieldName = item.FieldName.Replace("Id ", "");

                // Parent data
                if(item.TableName.ToUpper() == "MSPARENT")
                {
                    var parentRole = getParentRoleList.Where(x => x.Id == item.Constraint1Value).FirstOrDefault();

                    if(parentRole != null)
                    {
                        item.FieldName = parentRole.ParentRoleNameEng + " - " + item.FieldName;
                    }
                }
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
