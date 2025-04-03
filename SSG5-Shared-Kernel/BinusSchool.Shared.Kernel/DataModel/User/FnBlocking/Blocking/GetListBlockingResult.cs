using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.Blocking
{
    public class GetListBlockingResult : ItemValueVm
    {
        public string IdFeature { get; set; }
        public string ActionFeature { get; set; }
        public string ControllerFeature { get; set; }
        public string IdSubFeature { get; set; }
        public string ActionSubFeature { get; set; }
        public string ControllerSubFeature { get; set; }
        public string BLockingMessage { get; set; }
    }
}
