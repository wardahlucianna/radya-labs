using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching;

namespace BinusSchool.Student.FnStudent.MasterSearching
{
    public class GetFieldDataListHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetFieldDataListHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetFieldDataListRequest>
                (nameof(GetFieldDataListRequest.IdDataFieldRole));

            var query = await _dbContext.Entity<MsProfileDataField>()
                .Include(x => x.ProfileDataFieldGroup)
                .Where(x => (request.IdDataFieldRole == "STF" ? x.IdProfileDataFieldGroup == "5" : x.IdProfileDataFieldGroup != "5"))
                .ToListAsync(CancellationToken);

            var items = query.Select(x => new GetFieldDataListResult
                {
                    IdProfileDataFieldGroup = x.IdProfileDataFieldGroup,
                    GroupName = x.ProfileDataFieldGroup.Description,
                    ListField = new List<GetProfileDataField>
                {
                    new GetProfileDataField
                    {
                        IdProfileDataField = x.Id,
                        ProfileDataFieldName = x.FieldDataProfile,
                        AliasName = x.AliasName,
                        OrderNumber = x.OrderNumber
                    }
                }
                })
                .GroupBy(x => new { x.IdProfileDataFieldGroup, x.GroupName })
                .Select(group => new GetFieldDataListResult
                {
                    IdProfileDataFieldGroup = group.Key.IdProfileDataFieldGroup,
                    GroupName = group.Key.GroupName,
                    ListField = group.SelectMany(x => x.ListField)
                                     .OrderBy(x => x.OrderNumber)
                                     .ToList()
                })
                .OrderBy(x => x.IdProfileDataFieldGroup)
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
