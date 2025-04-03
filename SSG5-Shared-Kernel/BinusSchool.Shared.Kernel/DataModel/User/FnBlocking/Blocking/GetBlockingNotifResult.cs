using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.Blocking
{
    public class GetBlockingNotifResult
    {
        public string StudentBlocking { get; set; }
        public string IdStudent { get; set; }
        public string BlockingTypeCategory { get; set; }
        public string BlockingCategoryName { get; set; }
        public string IdFeature { get; set; }
        public string FeatureName { get; set; }
        public string IdSubFeature { get; set; }
        public string SubFeatureName { get; set; }
        public string StudentName { get; set; }
        public List<DataParent> DataParents { get; set; }
        public string SchoolName { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
    }

    public class DataParent
    {
        public string ParentName { get; set; }
        public string EmailParent { get; set; }
    }
}
