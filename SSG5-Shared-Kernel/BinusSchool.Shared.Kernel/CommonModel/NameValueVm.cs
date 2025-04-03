using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Common.Model
{
    public class NameValueVm : IUniqueId
    {
        public NameValueVm() {}
        
        public NameValueVm(string id) : this(id, null) {}

        public NameValueVm(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}