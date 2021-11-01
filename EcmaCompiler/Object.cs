using EcmaCompiler.Parsers;
using System.Collections.Generic;

namespace EcmaCompiler {
    public class Object {
        public int Name { get; }
        public Object Next { get; set; }
        public Kind Kind { get; set; }

        #region Kind Dependant Fields

        // Variable, Parameter, Field
        public Object Type { get; set; }

        // Function
        public Object ReturnType { get; set; }
        public IEnumerable<Object> Params { get; set; }
        public int ParamsNum { get; set; }
        public int VarsNum { get; set; }

        // Variable, Parameter, Fields, Function
        public int IndexNum { get; set; }

        // Array
        public Object ElementType { get; set; }
        public int ElementsNum { get; set; }

        // Struct
        public List<Object> Fields { get; set; }

        // Alias
        public Object BaseType { get; set; }

        // Not function
        public int Size { get; set; }

        #endregion

        public Object() { }

        public Object(int name, Object next, Kind kind) {
            Name = name;
            Next = next;
            Kind = kind;
        }
    }
}
