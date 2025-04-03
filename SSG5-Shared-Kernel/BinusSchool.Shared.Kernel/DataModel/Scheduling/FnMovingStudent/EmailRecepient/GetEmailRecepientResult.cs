using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient
{
    public class GetEmailRecepientResult : CodeWithIdVm
    {
        public List<EmailRecepients> tos { get; set; }
        public List<EmailRecepients> ccs { get; set; }
    }

    public class EmailRecepients
    {
        public ItemValueVm role { get; set; }
        public ItemValueVm roleGroup { get; set; }
        public ItemValueVm teacherPosition { get; set; }
        public ItemValueVm staff { get; set; }
    }
}
