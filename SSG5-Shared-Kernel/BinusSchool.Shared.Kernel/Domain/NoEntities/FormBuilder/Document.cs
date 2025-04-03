namespace BinusSchool.Domain.NoEntities.FormBuilder
{
    public class Document : UniqueNoEntity
    {
        public object DocumentFormBuilder { get; set; }
        public string FormBuilderId { get; set; }
        public string ApprovalId { get; set; }
    }
}