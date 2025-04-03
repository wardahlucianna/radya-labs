using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Employee.FnStaff.StaffEducationInformation;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.EmployeeDb.Abstractions;
using BinusSchool.Persistence.EmployeeDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Employee.FnStaff.StaffEducationInformation
{
    public class StaffEducationInformationHandler : FunctionsHttpCrudHandler
    {
        private readonly IEmployeeDbContext _dbContext;
        public StaffEducationInformationHandler(IEmployeeDbContext employeeDbContext)
        {
            _dbContext = employeeDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var param = Request.ValidateParams<CollectionRequest>();

            var query = _dbContext.Entity<TrStaffEducationInformation>()
                .Include(x => x.EducationLevel)
                .Where(x => x.IdBinusian == id)
                .OrderByDynamic(param);

            var items = await query
                .Select(x => new GetStaffEducationInformationResult
                {
                    IdBinusian = x.IdBinusian,
                    IdStaffEducation = x.IdStaffEducation,
                    IdEducationLevel = new ItemValueVm
                    {
                        Id = x.IdEducationLevel,
                        Description = x.EducationLevel.EducationLevelName
                    },
                    AttendingYear = x.AttendingYear,
                    MajorName = x.MajorName,
                    GPA = x.GPA,
                    GraduateYear = x.GraduateYear,
                    InstitutionName = x.InstitutionName
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }

        protected override Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
