using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MasterSearching.ProfileDataFieldSetting;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MasterSearching.ProfileDataFieldSetting
{
    public class GetProfileDataFieldSettingBinusianIdHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetProfileDataFieldSettingBinusianIdHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetProfileDataFieldSettingBinusianIdRequest>
                (nameof(GetProfileDataFieldSettingBinusianIdRequest.IdSchool));

            var getStaff = _dbContext.Entity<MsStaff>()
                .Where(a => a.IdSchool == request.IdSchool);

            var getUser = _dbContext.Entity<MsUser>()
                .Include(a => a.UserRoles).ThenInclude(a => a.Role);

            var joinData = from staff in getStaff
                           join user in getUser on staff.IdBinusian equals user.Id
                           select new
                           {
                               Staff = staff,
                               User = user
                           };

            var filteredData = joinData
                .Where(a => EF.Functions.Like(a.Staff.FirstName.Trim() + " " + a.Staff.LastName.Trim(), request.SearchPattern())
                         || EF.Functions.Like(a.Staff.IdBinusian, request.SearchPattern()))
                .ToList();

            IReadOnlyList<IItemValueVm> response;

            if (request.Return == CollectionType.Lov)
                response = filteredData
                    .Select(a => new ItemValueVm { Id = a.Staff.IdBinusian, Description = NameUtil.GenerateFullNameWithId(a.Staff.IdBinusian, a.Staff.FirstName, a.Staff.LastName) })
                    .ToList();
            else
                response = filteredData
                    .SetPagination(request)
                    .Select(a => new GetProfileDataFiedlSettingBinusianIdResponse
                    {
                        Id = a.Staff.IdBinusian,
                        Description = NameUtil.GenerateFullName(a.Staff.FirstName, a.Staff.LastName),
                        Role = string.Join(", ", a.User.UserRoles.Where(b => b.IsActive == true).Select(c => c.Role.Description))
                    })
                    .OrderBy(a => a.Description)
                    .ToList();

            var count = request.CanCountWithoutFetchDb(response.Count)
                ? response.Count
                : joinData.Select(a => a.Staff.IdBinusian).Count();

            return Request.CreateApiResult2(response as object, request.CreatePaginationProperty(count));
        }
    }
}
