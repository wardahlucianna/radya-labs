namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class CreateHierarchyResult
    {
        public string Url { get; set; }
        public bool IsDepend { get; set; } = true;
        public bool IsMultiple { get; set; } = false;
        public bool IsAllowClear { get; set; } = true;
        public string PlaceHolder { get; set; }
        public string DefaultValue { get; set; } = null;
        public string ObjectId { get; set; } = "Id";
        public string ObjectCode { get; set; } = "description";
        public string ObjectMapping { get; set; } = string.Empty;
        public string Class { get; set; }
        public string Label { get; set; }
    }
}
