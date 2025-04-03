using System;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class ValidateGeneratedClassIdHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ValidateGeneratedClassIdHandler(
            ISchedulingDbContext dbContext
        )
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<ValidateGeneratedClassIdRequest>(nameof(ValidateGeneratedClassIdRequest.IdAcadyear),
                                                                                nameof(ValidateGeneratedClassIdRequest.IdGrade),
                                                                                nameof(ValidateGeneratedClassIdRequest.Semester),
                                                                                nameof(ValidateGeneratedClassIdRequest.IdSubject),
                                                                                nameof(ValidateGeneratedClassIdRequest.ClassIdFormat),
                                                                                nameof(ValidateGeneratedClassIdRequest.ClassIdToValidate));

            var result = new ValidateGeneratedClassIdResult();
            if (param.ClassIdToValidate.Contains("|"))
            {
                try
                {
                    var generated = await GenerateClassId(param);
                    result.ClassId = generated.ClassId;
                    result.AutoIncreamentValue = generated.AutoIncreamentValue;
                    result.IsValidated = true;
                }
                catch
                {
                    result.IsValidated = false;
                    result.ClassId = param.ClassIdToValidate;
                }
            }
            else
            {
                result.IsValidated = false;
                result.ClassId = param.ClassIdToValidate;
            }

            return Request.CreateApiResult2(result as object);
        }

        private async Task<GeneratedClassIdResult> GenerateClassId(ValidateGeneratedClassIdRequest request)
        {
            var alphabets = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "0", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            var @char = request.ClassIdToValidate.Substring(request.ClassIdToValidate.IndexOf('|') - 1, 1);
            var initial = await _dbContext.Entity<MsLesson>()
                                .CountAsync(x => x.IdAcademicYear == request.IdAcadyear
                                                 && x.IdGrade == request.IdGrade
                                                 && x.Semester == request.Semester
                                                 && x.IdSubject == request.IdSubject
                                                 && x.ClassIdFormat == request.ClassIdFormat) + (request.BookedClassIds != null ? request.BookedClassIds.Count : 0);
            request.ClassIdToValidate = request.ClassIdToValidate.Replace($"{@char}|", "|");


            if (int.TryParse(@char, out var number))
                return await GetClassIdRecursion(request, number + initial, false);
            else
                return await GetClassIdRecursion(request, Array.IndexOf(alphabets, @char) + 1 + initial, true);
        }

        private async Task<GeneratedClassIdResult> GetClassIdRecursion(ValidateGeneratedClassIdRequest request, int number, bool isCharacter)
        {
            var classIdToValidate = request.ClassIdToValidate.Replace("|", isCharacter ? GetCharacterFromIndex(number) : number.ToString());
            if (await _dbContext.Entity<MsLesson>()
                                .AnyAsync(x => x.IdAcademicYear == request.IdAcadyear
                                          && x.IdGrade == request.IdGrade
                                          && x.Semester == request.Semester
                                          && x.IdSubject == request.IdSubject
                                          && x.ClassIdGenerated == classIdToValidate) || (request.BookedClassIds != null && request.BookedClassIds.Contains(classIdToValidate)))
                return await GetClassIdRecursion(request, number + 1, isCharacter);
            else
                return new GeneratedClassIdResult
                {
                    ClassId = classIdToValidate,
                    AutoIncreamentValue = isCharacter ? GetCharacterFromIndex(number) : number.ToString()
                };
        }

        private string GetCharacterFromIndex(int columnNumber)
        {
            var dividend = columnNumber;
            var columnName = string.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}
