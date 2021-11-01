namespace EcmaCompiler.Parsers {

    public enum Kind {
        NO_KIND_DEFINED_ = -1,
        VARIABLE_,
        PARAMETER_,
        FUNCTION_,
        FIELD_,
        ARRAY_TYPE_,
        STRUCT_TYPE_,
        ALIAS_TYPE_,
        SCALAR_TYPE_,
        UNIVERSAL_
    }

    public struct TypeAttribute {
        public Object Type;
        public object Val;
        public int Pos;
        public int Size;
    }

    public static class TypeManager {
        public static Object Integer = new(-1, null, Kind.SCALAR_TYPE_);
        public static Object Char = new(-1, null, Kind.SCALAR_TYPE_);
        public static Object Bool = new(-1, null, Kind.SCALAR_TYPE_);
        public static Object String = new(-1, null, Kind.SCALAR_TYPE_);
        public static Object Universal = new(-1, null, Kind.UNIVERSAL_);

        public static bool CheckTypes(Object t1, Object t2) {
            if (t1 == t2)
                return true;
            if (t1 == Universal || t2 == Universal)
                return true;
            if (t1.Kind == Kind.UNIVERSAL_ || t2.Kind == Kind.UNIVERSAL_)
                return true;
            if (t1.Kind == t2.Kind) {

                switch (t1.Kind) {
                    case Kind.ALIAS_TYPE_:
                        return CheckTypes(t1.BaseType, t2.BaseType);
                    case Kind.ARRAY_TYPE_:
                        if (t1.ElementsNum == t2.ElementsNum)
                            return CheckTypes(t1.ElementType, t2.ElementType);
                        break;
                    case Kind.STRUCT_TYPE_:
                        var fields1 = t1.Fields;
                        var fields2 = t2.Fields;

                        if (fields1.Count != fields2.Count) return false;

                        for (int i = 0; i < fields1.Count; i++) {
                            var f1 = fields1[i];
                            var f2 = fields2[i];

                            if (!CheckTypes(f1.Type, f2.Type)) return false;
                        }
                        return true;
                }
            }
            return false;
        }

        public static void TypeError() {
            throw new TypeError("Expected a type.");
        }
    }
}
