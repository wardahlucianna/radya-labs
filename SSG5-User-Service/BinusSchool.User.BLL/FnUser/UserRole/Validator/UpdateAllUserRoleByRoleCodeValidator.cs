using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using FluentValidation;

namespace BinusSchool.User.FnUser.UserRole.Validator
{
  
    public class UpdateAllUserRoleByRoleCodeValidator : AbstractValidator<UpdateAllUserRoleByRoleCodeRequest>
    {
        public UpdateAllUserRoleByRoleCodeValidator()
        {           
            RuleFor(x => x.RoleCode).NotEmpty();
            RuleFor(x => x.UserList)
             .NotEmpty()
             .ForEach(data => data.ChildRules(data =>
             {
                 data.RuleFor(x => x.IdUser).NotEmpty();                
             }));
        }
    }
}
