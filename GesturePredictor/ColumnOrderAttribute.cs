using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor
{
    internal class ColumnOrderAttribute
    {
        public int Value { get; set; }

        public ColumnOrderAttribute(int value)
        {
            Value = value;
        }
    }
}
