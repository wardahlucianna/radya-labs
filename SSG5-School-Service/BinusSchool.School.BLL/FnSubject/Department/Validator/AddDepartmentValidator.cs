using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnSubject.Department;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BinusSchool.School.FnSubject.Department.Validator
{
    public class AddDepartmentValidator : AbstractValidator<AddDepartmentRequest>
    {
        public AddDepartmentValidator()
        {
            RuleFor(x => x.IdAcadyear).NotEmpty();

            //RuleFor(x => x.IdAcadyear).CustomAsync(async (y, context, cancellation) =>
            //{
            //    //var check = await dbContext.Entity<MsAcademicYear>().Where(x => x.Id == y).FirstOrDefaultAsync();
            //    //if (check == null)
            //    //{
            //    //    context.AddFailure(string.Format(localizer["ExNotExist"], localizer["School"], "Id", y));
            //    //}
            //});
            //RuleFor(x => x).CustomAsync(async (y, context, cancellation) =>
            //{
            //    var check = await dbContext.Entity<MsDepartment>()
            //                       .Include(p => p.DepartmentLevels)
            //                       .ThenInclude(p => p.Level)
            //                       .Where(x => x.Description == y.DepartmentName &&
            //                                   x.DepartmentLevels.Any(p => y.IdLevel.Any(o => o == p.IdLevel)))
            //                       .FirstOrDefaultAsync();
            //    if (check != null)
            //    {
            //        context.AddFailure(string.Format(localizer["ExAlreadyExist"], "Departemnt", "Name and level", y.DepartmentName + " , " + string.Join("/", check.DepartmentLevels.Select(p => p.Level.Code).ToList())));
            //    }
            //});

            When(x => (DepartmentType)x.LevelType == DepartmentType.Level, () =>
            {
                RuleForEach(x => x.IdLevel).NotNull();
            });
        }
    }
}
