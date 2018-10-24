using System;
using System.Collections.Generic;
using System.Text;

namespace Solid.Step3.LayoutConverters
{
    public class LayoutConveterRepository
    {
        private readonly Dictionary<string, ILayoutConverter> _repository;
        public LayoutConveterRepository()
        {
            _repository =new Dictionary<string, ILayoutConverter>();
        }

        public void Register<TLayoutConverter>(string name)
            where TLayoutConverter: ILayoutConverter, new()
        {
            if (!_repository.ContainsKey(name))
                _repository.Add(name, new TLayoutConverter());
        }

        public ILayoutConverter Get(string name)
        {
            if (!_repository.ContainsKey(name))
                throw new InvalidOperationException();
            return _repository[name];
        }
    }
}
