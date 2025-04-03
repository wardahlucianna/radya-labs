using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Common.Model
{
    public class CodeWithIdVm<T> : CodeVm<T>, IItemValueVm
    {
        public CodeWithIdVm(string id, T code, string description)
        {
            Id = id;
            Code = code;
            Description = description;
        }

        public CodeWithIdVm(string id, T code) : this(id, code, null) {}

        public CodeWithIdVm(string id) : this(id, default, null) {}

        public CodeWithIdVm() {}
        
        public string Id { get; set; }
    }

    public class CodeWithIdVm : CodeWithIdVm<string>
    {
        public CodeWithIdVm(string id, string code, string description) : base(id, code, description) {}

        public CodeWithIdVm(string id, string code) : base(id, code) {}
        
        public CodeWithIdVm(string id) : base(id) {}

        public CodeWithIdVm() {}
    }
}