using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSubject.Department;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSubject.Department.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSubject.Department
{
    public class CopyDepartmentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public CopyDepartmentHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CopyDepartmentRequest, CopyDepartmentValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var acadyear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcadyearFrom);
            if (acadyear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Acadyear"], "Id", body.IdAcadyearFrom));

            var copyDatas = await _dbContext.Entity<MsDepartment>()
                .Include(p => p.DepartmentLevels)
                .Where(p => p.IdAcademicYear == body.IdAcadyearFrom)
                .ToListAsync();

            if (copyDatas.Count == 0)
                throw new NotFoundException($"Data From Academic year {acadyear.Description} Not found");

            foreach (var item in copyDatas)
            {
                var isExist = await _dbContext.Entity<MsDepartment>()
                    .Where(x => x.IdAcademicYear == body.IdAcadyearTo && x.Code.ToLower() == item.Code.ToLower())
                    .FirstOrDefaultAsync();

                if (isExist == null)
                {
                    var department = new MsDepartment();
                    department.Id = Guid.NewGuid().ToString();
                    department.IdAcademicYear = body.IdAcadyearTo;
                    department.Code = item.Code;
                    department.Description = item.Description;
                    department.Type = (DepartmentType)item.Type;
                    department.UserIn = AuthInfo.UserId;

                    _dbContext.Entity<MsDepartment>().Add(department);

                    if (item.DepartmentLevels != null || item.DepartmentLevels.Count > 0)
                    {
                        foreach (var itemLevel in item.DepartmentLevels)
                        {
                            var levelFrom = await _dbContext.Entity<MsLevel>()
                                .Where(x => x.Id == itemLevel.IdLevel)
                                .FirstOrDefaultAsync();

                            var levelTo = await _dbContext.Entity<MsLevel>()
                                .Where(x => x.IdAcademicYear == body.IdAcadyearTo && x.Code.ToLower() == levelFrom.Code.ToLower())
                                .FirstOrDefaultAsync();

                            if (levelTo != null)
                            {
                                var level = new MsDepartmentLevel();
                                level.Id = Guid.NewGuid().ToString();
                                level.IdDepartment = department.Id;
                                level.IdLevel = levelTo.Id;
                                level.UserIn = AuthInfo.UserId;

                                _dbContext.Entity<MsDepartmentLevel>().Add(level);
                            }
                        }
                    }
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }

        protected override void OnFinally()
        {
            _transaction?.Dispose();
        }
    }
}
