﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs
{
    public class AddUpdateAccessRoleLoginAsRequest
    {
        public string IdSchool { get; set; }
        public string IdRole { get; set; }
        public List<string> AccessRolesList { get; set; }
    }
}
