using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Common.Model
{
    public class ItemValueVm : IItemValueVm
    {
        public ItemValueVm() {}
        
        public ItemValueVm(string id) : this(id, null) {}

        public ItemValueVm(string id, string description)
        {
            Id = id;
            Description = description;
        }

        public string Id { get; set; }
        public string Description { get; set; }
    }
}
