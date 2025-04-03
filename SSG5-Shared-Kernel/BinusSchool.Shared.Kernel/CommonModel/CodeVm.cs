namespace BinusSchool.Common.Model
{
    public class CodeVm<T>
    {
        public T Code { get; set; }
        public string Description { get; set; }
    }

    public class CodeVm : CodeVm<string> {}
}
