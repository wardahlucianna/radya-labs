using System.Linq;
using BinusSchool.Data.Model.User.FnUser.HierarchyMapping;
using FluentValidation;

namespace BinusSchool.User.FnUser.HierarchyMapping.Validator
{
    public class AddHierarchyMappingValidator : AbstractValidator<AddHierarchyMappingRequest>
    {
        public AddHierarchyMappingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Hierarchies)
                .NotNull()
                .Must(x => x.Any(y => y.IdRolePositionParent == null))
                .WithMessage("At least 1 position must be a root of the hierarchy");
        }
    }
}
