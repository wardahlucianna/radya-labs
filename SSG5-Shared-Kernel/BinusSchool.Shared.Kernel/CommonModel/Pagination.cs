using System;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Common.Model
{
    public abstract class Pagination : IPagination
    {
        private const int _defaultSize = 10, _maxSize = 1000;
        private int _page = 1, _size = _defaultSize;

        public virtual int Page
        {
            set => _page = value;
            get => _page < 1 ? (_page = 1) : _page;
        }

        public virtual int Size 
        {
            set => _size = value;
            get => _size <= 0 ? (_size = _defaultSize) : _size > _maxSize ? (_size = _maxSize) : _size;
        }

        public int CalculateOffset() => (Page - 1) == 0 || (Page - 1) < 0 ? 0 : (Page - 1) * Size;
    }
}