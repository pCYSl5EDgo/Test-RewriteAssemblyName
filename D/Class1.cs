extern alias _B;
extern alias _C;

namespace A
{
    public class Class1
    {
        public int Q() => new _B::A.Class1().Q() * 2 - new _C::A.Class1().Q();
    }
}
