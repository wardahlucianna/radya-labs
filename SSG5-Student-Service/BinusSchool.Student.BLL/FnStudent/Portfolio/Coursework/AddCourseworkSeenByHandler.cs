using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Portfolio.Coursework.Validator;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.User;
using System.Transactions;
using System.Data;
using BinusSchool.Common.Constants;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework
{
    public class AddCourseworkSeenByHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public AddCourseworkSeenByHandler(IStudentDbContext studentDbContext)
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddCourseworkSeenByRequest, AddCourseworkSeenByValidator>();

            var idStudentData = await _dbContext.Entity<TrCourseworkAnecdotalStudent>()
                .Where(x => x.Id == body.IdCourseworkAnecdotalStudent)
                .Select(x=> x.IdStudent)
                .FirstOrDefaultAsync(CancellationToken);

            if (idStudentData == null)
                return Request.CreateApiResult2();

            if (body.IdUser.Contains("P"))
            {
                var idStudentCompare = string.Concat(body.IdUser.Where(char.IsDigit));
                var sibligGroup = await _dbContext.Entity<MsSiblingGroup>()
                    .Where(x => x.IdStudent == idStudentCompare).Select(x => x.Id).FirstOrDefaultAsync(CancellationToken);

                var siblingStudent = await _dbContext.Entity<MsSiblingGroup>().Where(x => x.Id == sibligGroup).Select(x => x.IdStudent).ToListAsync(CancellationToken);
                if (sibligGroup != null)
                {
                    if (!siblingStudent.Any(idStudent => idStudent == idStudentData))
                    {
                        return Request.CreateApiResult2();
                    }
                }
                else
                {
                    var dataStudentParent = await _dbContext.Entity<MsStudentParent>()
                                            .Where(x => x.IdStudent == idStudentCompare)
                                            .Select(x => new
                                            {
                                                idParent = x.IdParent
                                            }).FirstOrDefaultAsync(CancellationToken);

                    var listChildrens = await _dbContext.Entity<MsStudentParent>()
                                .Include(x => x.Student).ThenInclude(e => e.StudentGrades).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel).ThenInclude(e => e.MsAcademicYear)
                                .Where(x => x.IdParent == dataStudentParent.idParent)
                                .Select(x => x.Student.Id).ToListAsync(CancellationToken);

                    if (!listChildrens.Any(idStudent => idStudent == idStudentData))
                    {
                        return Request.CreateApiResult2();
                    }
                }
            }

            var checkData = await _dbContext.Entity<TrCourseworkAnecdotalStudentSeen>()
                .Where(x => x.IdCourseworkAnecdotalStudent == body.IdCourseworkAnecdotalStudent && x.IdUserSeen == body.IdUser)
                .FirstOrDefaultAsync(CancellationToken);

            if(checkData == null)
            {
                var CourseworkAnecdotalSeenBy = new TrCourseworkAnecdotalStudentSeen
                {
                    Id = Guid.NewGuid().ToString(),
                    IdCourseworkAnecdotalStudent = body.IdCourseworkAnecdotalStudent,
                    IdUserSeen = body.IdUser
                };

                _dbContext.Entity<TrCourseworkAnecdotalStudentSeen>().Add(CourseworkAnecdotalSeenBy);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
