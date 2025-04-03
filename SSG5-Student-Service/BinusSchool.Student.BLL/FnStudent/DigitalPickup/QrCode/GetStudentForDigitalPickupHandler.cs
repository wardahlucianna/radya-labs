using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode;
using BinusSchool.Persistence.StudentDb.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.StudentDb.Entities;
using System.Collections.Generic;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Common.Exceptions;

namespace BinusSchool.Student.FnStudent.DigitalPickup.QrCode
{
    public class GetStudentForDigitalPickupHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        public GetStudentForDigitalPickupHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentForDigitalPickupRequest>(
                nameof(GetStudentForDigitalPickupRequest.IdAcademicYear), nameof(GetStudentForDigitalPickupRequest.IdParent));

            var username = await _dbContext.Entity<MsUser>()
                            .Where(x => x.Id == param.IdParent)
                            .Select(x => x.Username)
                            .FirstOrDefaultAsync(CancellationToken);

            if (username == null)
                throw new BadRequestException("User is not found");

            var mainIdStudent = string.Concat(username.Where(char.IsDigit));

            var idStudents = await _dbContext.Entity<MsSiblingGroup>()
                .Where(x => _dbContext.Entity<MsSiblingGroup>()
                    .Any(y => y.Id == x.Id && y.IdStudent == mainIdStudent))
                .Select(x => x.IdStudent)
                .ToListAsync();

            var res = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.MsLevel)
                .Include(x => x.Student)
                .Where(x => idStudents.Contains(x.IdStudent) && x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                .Join(_dbContext.Entity<MsDigitalPickupSetting>()
                    .Where(x => x.IdAcademicYear == param.IdAcademicYear),
                    student => student.Homeroom.IdGrade,
                    setting => setting.IdGrade,
                    (student, setting) => new { student, setting })
                .Select(x => new GetStudentForDigitalPickupResult
                {
                    IdStudent = x.student.IdStudent,
                    StudentName = NameUtil.GenerateFullName(x.student.Student.FirstName, x.student.Student.MiddleName, x.student.Student.LastName),
                    IdGrade = new ItemValueVm {
                        Id = x.student.Homeroom.IdGrade,
                        Description = x.student.Homeroom.Grade.Description
                    }
                })
                .Distinct().ToListAsync(CancellationToken);

            return Request.CreateApiResult2(res as object);

        }
    }
}
