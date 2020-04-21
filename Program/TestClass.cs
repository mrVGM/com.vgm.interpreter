using System;
using System.Collections.Generic;
using System.Text;

namespace Program
{
    class TestClass
    {
        public delegate int TestEvent(string s, string a);
        public event TestEvent Asd;

        public void CallAsd() 
        {
            Asd?.Invoke("das", "da");
        }
    }
}
