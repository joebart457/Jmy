using Jmy.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmy.StdLib
{
    [JmyExport]
    public class Integer
    {
        public class ValRef
        {
            public int val = 7;
        }

        public ValRef Val = new ValRef();
        public int value;

        [JmyNoExport]
        public int YouSmell() { return 0; }
        [JmyExport]
        public int Add(int a, int b) {  return a + b; }
    }
}
