using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData
{
    public class GetLevelGradeByConcellorResult : GetItemByConcellor
    {
        public List<GetItemByConcellor> Grade { get; set; }
    }

    public class GetItemByConcellor :CodeWithIdVm
    {
        public bool IsDisabled { get; set; }
    }
}
