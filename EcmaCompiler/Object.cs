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
        public Object ReturnType { get; }
        public List<Object> Params { get; }

        // Array
        public Object ElementType { get; }
        public int Count { get; }

        // Struct
        public List<Object> Fields { get; }

        // Alias
        public Object BaseType { get; }

        #endregion

        public Object(int name, Object next, Kind kind) {
            Name = name;
            Next = next;
            Kind = kind;
        }
    }
}
