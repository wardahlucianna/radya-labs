using BinusSchool.Data.Model.User.FnUser.HierarchyMapping;
using FluentValidation;

namespace BinusSchool.User.FnUser.HierarchyMapping.Validator
{
    public class UpdateHierarchyMappingValidator : AbstractValidator<UpdateHierarchyMappingRequest>
    {
        public UpdateHierarchyMappingValidator()
        {
            Include(new AddHierarchyMappingValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
