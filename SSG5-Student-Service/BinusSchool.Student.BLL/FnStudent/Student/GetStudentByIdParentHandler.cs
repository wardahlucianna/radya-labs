using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.Student
{
    public class GetStudentByIdParentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetStudentByIdParentHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentByIdParentRequest>(
                            nameof(GetStudentByIdParentRequest.IdParent));

            bool IsExistsParamStudentStatusExcludeList = param.IdStudentStatusExcludeList == null ? false : !param.IdStudentStatusExcludeList.Any() ? false : true;

            var username = await _dbContext.Entity<MsUser>()
                            .Where(x => x.Id == param.IdParent)
                            .Select(x => x.Username)
                            .FirstOrDefaultAsync(CancellationToken);

            if (username == null)
                throw new BadRequestException("User is not found");

            var idStudent = string.Concat(username.Where(char.IsDigit));

            var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                    .Where(x => x.IdStudent == idStudent)
                                    .Select(x => new
                                    {
                                        idParent = x.IdParent
                                    }).FirstOrDefaultAsync(CancellationToken);

            var sibligGroup = await _dbContext.Entity<MsSiblingGroup>()
                                .Where(x => x.IdStudent == idStudent)
                                .Select(x => x.Id)
                                .FirstOrDefaultAsync(CancellationToken);

            List<GetStudentByIdParentResult> ReturnResult = new List<GetStudentByIdParentResult>();
            if (sibligGroup != null)
            {
                var siblingStudent = await _dbContext.Entity<MsSiblingGroup>()
                                        .Where(x => x.Id == sibligGroup)
                                        .Select(x => x.IdStudent)
                                        .ToListAsync(CancellationToken);

                ReturnResult = await _dbContext.Entity<MsStudent>()
                                .Where(x => siblingStudent.Any(y => y == x.Id) &&
                                            (!IsExistsParamStudentStatusExcludeList? true : !param.IdStudentStatusExcludeList.Any(y => y == x.IdStudentStatus))
                                        )
                                .Select(x => new GetStudentByIdParentResult
                                {
                                    IdStudent = x.Id,
                                    StudentName = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName),
                                    IdSchool = x.IdSchool
                                }).ToListAsync(CancellationToken);
            }
            else if(dataStudentParent != null)
            {
                ReturnResult = await _dbContext.Entity<MsStudentParent>()
                            .Include(x => x.Student)
                            .Where(x => x.IdParent == dataStudentParent.idParent &&
                                        (!IsExistsParamStudentStatusExcludeList ? true : !param.IdStudentStatusExcludeList.Any(y => y == x.Student.IdStudentStatus))
                                    )
                            .Select(x => new GetStudentByIdParentResult
                            {
                                IdStudent = x.Student.Id,
                                StudentName = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.MiddleName, x.Student.LastName),
                            }).ToListAsync(CancellationToken);
            }

            if (!string.IsNullOrEmpty(param.Search))
            {
                ReturnResult = ReturnResult.Where(e =>
                                                e.IdStudent.Contains(param.Search) ||
                                                e.StudentName.ToLower().Contains(param.Search.ToLower())
                                            ).ToList();
            }

            return Request.CreateApiResult2(ReturnResult as object);

        }
    }
}
