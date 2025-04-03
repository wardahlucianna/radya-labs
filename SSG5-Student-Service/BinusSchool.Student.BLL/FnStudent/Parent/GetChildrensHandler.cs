using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Persistence.StudentDb.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.StudentDb.Entities.User;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BinusSchool.Student.FnStudent.Parent
{
    public class GetChildrensHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly ILogger<GetChildrensHandler> _logger;
        public GetChildrensHandler(IStudentDbContext dbContext, ILogger<GetChildrensHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetChildRequest>(nameof(GetChildRequest.IdParent));

            //implement new code get Children 

            var generatedIdStudent = string.Concat(AuthInfo.UserName.Where(char.IsDigit));
            _logger.LogInformation("Logged Auth Info" + AuthInfo.UserName);
            var dataUser = await _dbContext.Entity<MsUser>().Where(x => x.Id == param.IdParent).Select(x =>new { x.Username , x.DisplayName}).FirstOrDefaultAsync(CancellationToken);
            var idStudent = string.Concat(dataUser.Username.Where(char.IsDigit));
            var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                    .Where(x => x.IdStudent == idStudent)
                                    .Select(x => new
                                    {
                                        idParent = x.IdParent
                                    }).FirstOrDefaultAsync(CancellationToken);

            var parent = await _dbContext.Entity<MsParent>().Where(x => x.Id == dataStudentParent.idParent).Select(x => new GetChildResult
            {
                Id = param.IdParent,
                //Name = x.FirstName,
                Name = dataUser.DisplayName,
                Order = 1,
                Role = RoleConstant.Parent
            }).ToListAsync();

            var sibligGroup = await _dbContext.Entity<MsSiblingGroup>()
                .Where(x => x.IdStudent == idStudent).Select(x => x.Id).FirstOrDefaultAsync(CancellationToken);
            var childrens = new List<GetChildResult>();
            if (sibligGroup != null)
            {
                var siblingStudent = await _dbContext.Entity<MsSiblingGroup>().Where(x => x.Id == sibligGroup).Select(x => x.IdStudent).ToListAsync(CancellationToken);
                childrens = await _dbContext.Entity<MsStudentGrade>()
                           .Where(a => siblingStudent.Any(y => y == a.IdStudent)
                           && a.Grade.MsLevel.IdAcademicYear == (string.IsNullOrEmpty(param.IdAcademicYear) ? a.Grade.MsLevel.IdAcademicYear : param.IdAcademicYear))
                           .OrderByDescending(a => a.Grade.MsLevel.MsAcademicYear.Code).ThenByDescending(a => a.Grade.OrderNumber)
                           .Select(a => new
                           {
                               Id = a.IdStudent,
                               Name = a.Student.FirstName,
                               Order = 2,
                               Role = RoleConstant.Student,
                               IdSchool = a.Student.IdSchool,
                               IdLevel = a.Grade.IdLevel,
                               LevelCode = a.Grade.MsLevel.Code,
                               LevelDesc = a.Grade.MsLevel.Description
                           })
                           .GroupBy(x => new { x.Id, x.Name, x.Order, x.Role, x.IdSchool })
                           .Select(x => new GetChildResult
                           {
                               Id = x.Key.Id,
                               Name = x.Key.Name,
                               Order = x.Key.Order,
                               Role = x.Key.Role,
                               IdSchool = x.Key.IdSchool,
                               Level = x.Select(y => new CodeWithIdVm
                               {
                                   Id = y.IdLevel,
                                   Code = y.LevelCode,
                                   Description = y.LevelDesc
                               }).First()
                           })
                           .ToListAsync(CancellationToken);
                /*
                 * childrens = await _dbContext.Entity<MsStudent>()
                                .Include(e=>e.StudentGrades).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel).ThenInclude(e=>e.MsAcademicYear)
                                .Where(x => siblingStudent.Any(y => y == x.Id))
                                .Select(x => new GetChildResult
                                {
                                    Id = x.Id,
                                    Name = x.FirstName,
                                    Order = 2,
                                    Role = RoleConstant.Student,
                                    IdSchool = x.IdSchool,
                                    Level = x.StudentGrades.Count != 0
                                        ? x.StudentGrades
                                            .Where(e=> param.IdAcademicYear == null || e.Grade.MsLevel.IdAcademicYear==param.IdAcademicYear)
                                            .Select(y => new CodeWithIdVm
                                            {
                                                Id = y.Grade.IdLevel,
                                                Code = y.Grade.MsLevel.Code,
                                                Description = y.Grade.MsLevel.Description
                                            })
                                            .First()
                                        : null
                                }).ToListAsync(CancellationToken);
                */
            }
            else
            {
                childrens = await _dbContext.Entity<MsStudentGrade>()
                           .Where(x => x.Student.StudentParents.Any(a => a.IdParent == dataStudentParent.idParent)
                            && x.Grade.MsLevel.IdAcademicYear == (string.IsNullOrEmpty(param.IdAcademicYear) ? x.Grade.MsLevel.IdAcademicYear : param.IdAcademicYear))
                            .OrderByDescending(a => a.Grade.MsLevel.MsAcademicYear.Code).ThenByDescending(a => a.Grade.OrderNumber)
                            .Select(a => new
                            {
                                Id = a.IdStudent,
                                Name = a.Student.FirstName,
                                Order = 2,
                                Role = RoleConstant.Student,
                                IdSchool = a.Student.IdSchool,
                                IdLevel = a.Grade.IdLevel,
                                LevelCode = a.Grade.MsLevel.Code,
                                LevelDesc = a.Grade.MsLevel.Description
                            })
                            .GroupBy(x => new { x.Id, x.Name, x.Order, x.Role, x.IdSchool })
                            .Select(x => new GetChildResult
                            {
                                Id = x.Key.Id,
                                Name = x.Key.Name,
                                Order = x.Key.Order,
                                Role = x.Key.Role,
                                IdSchool = x.Key.IdSchool,
                                Level = x.Select(y => new CodeWithIdVm
                                {
                                    Id = y.IdLevel,
                                    Code = y.LevelCode,
                                    Description = y.LevelDesc
                                }).First()
                            })
                            .ToListAsync(CancellationToken);
                /*
                childrens = await _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Student).ThenInclude(e => e.StudentGrades).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                            .Where(x => x.IdParent == dataStudentParent.idParent)
                            .Select(x => new GetChildResult
                            {
                                Id = x.Student.Id,
                                Name = x.Student.FirstName,
                                Order = 2,
                                Role = RoleConstant.Student,
                                IdSchool = x.Student.IdSchool,
                                Level = x.Student.StudentGrades.Count != 0
                                    ? x.Student.StudentGrades
                                        .Where(e => param.IdAcademicYear == null || e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                        .Select(y => new CodeWithIdVm
                                        {
                                            Id = y.Grade.IdLevel,
                                            Code = y.Grade.MsLevel.Code,
                                            Description = y.Grade.MsLevel.Description
                                        })
                                        .First()
                                    : null
                            }).ToListAsync(CancellationToken);
                */
            }

            var data = parent.Union(childrens);
            return Request.CreateApiResult2(data as object);
        }
    }
}
