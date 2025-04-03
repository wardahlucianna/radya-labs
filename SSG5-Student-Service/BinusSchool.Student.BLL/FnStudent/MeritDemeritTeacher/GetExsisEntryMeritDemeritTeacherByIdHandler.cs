using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.MeritDemerit.Validator;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MeritDemeritTeacher
{
    public class GetExsisEntryMeritDemeritTeacherByIdHandler: FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetExsisEntryMeritDemeritTeacherByIdHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<GetExsisEntryMeritDemeritTeacherByIdRequest, GetExsisEntryMeritDemeritTeacherByIdValidator>();
            var GetEntryDemeritStudent = _dbContext.Entity<TrEntryDemeritStudent>()
             .Where(x => body.IdMeritDemerit.Contains(x.IdMeritDemeritMapping)).ToList();

            var GetEntryMeritStudent = _dbContext.Entity<TrEntryMeritStudent>()
             .Where(x => body.IdMeritDemerit.Contains(x.IdMeritDemeritMapping)).ToList();

            List<GetExsisEntryMeritDemeritTeacherByIdResult> Items = new List<GetExsisEntryMeritDemeritTeacherByIdResult>();

            foreach (var bodyEntryDemerit in body.IdMeritDemerit)
            {
                var ExsisEntryDemeritStudent = GetEntryDemeritStudent.Any(e => e.IdMeritDemeritMapping == bodyEntryDemerit);
                var ExsisEntryMeritStudent = GetEntryMeritStudent.Any(e => e.IdMeritDemeritMapping == bodyEntryDemerit);
                Items.Add(new GetExsisEntryMeritDemeritTeacherByIdResult
                {
                    IdMeritDemerit = bodyEntryDemerit,
                    StatusExsis = ExsisEntryDemeritStudent || ExsisEntryMeritStudent ? true : false
                });
            }
            return Request.CreateApiResult2(Items as object);
        }
    }
}
