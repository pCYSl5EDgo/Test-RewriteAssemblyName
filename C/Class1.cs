extern alias _A;

namespace A
{
    public class Class1
    {
        public int Q() => new _A::A.Class1().Q() * 32;
    }
}