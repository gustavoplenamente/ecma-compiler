using System.Collections.Generic;

namespace EcmaCompiler {
    public class Pool<T> {
        private List<T> _pool = new List<T>();

        public int Add(T value) {
            _pool.Add(value);
            return _pool.Count;
        }

        public T Get(int index) => _pool[index];
    }
}
