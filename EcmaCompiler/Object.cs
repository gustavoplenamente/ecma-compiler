namespace EcmaCompiler {
    public class Object {
        public int Name { get; }
        public Object Next { get; set; }

        public Object(int name, Object next) {
            Name = name;
            Next = next;
        }
    }
}
