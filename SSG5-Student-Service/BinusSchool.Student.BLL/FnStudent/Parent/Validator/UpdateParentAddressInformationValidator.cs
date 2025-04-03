using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using FluentValidation;

namespace BinusSchool.Student.FnStudent.Parent.Validator
{
    public class UpdateParentAddressInformationValidator : AbstractValidator<UpdateParentAddressInformationRequest>
    {
        public UpdateParentAddressInformationValidator()
        {
            /*RuleFor(x => x.ResidenceAddress).NotEmpty();
            RuleFor(x => x.HouseNumber).NotEmpty();
            RuleFor(x => x.RT).NotEmpty();
            RuleFor(x => x.RW).NotEmpty();
            RuleFor(x => x.VillageDistrict).NotEmpty();
            RuleFor(x => x.SubDistrict).NotEmpty();
            RuleFor(x => x.IdAddressCity).NotEmpty();
            RuleFor(x => x.IdAddressStateProvince).NotEmpty();
            RuleFor(x => x.IdAddressCountry).NotEmpty();
            RuleFor(x => x.PostalCode).NotEmpty();
            */
        }
    }
}
