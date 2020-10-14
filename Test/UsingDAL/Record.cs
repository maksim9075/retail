using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsingDAL
{
    public class Record
    {
        public int Counter { get; set; }
        public override string ToString()
        {
            return this.Counter.ToString();
        }
        public Record(int counter)
        {
            this.Counter = counter;
        }
    }
}
