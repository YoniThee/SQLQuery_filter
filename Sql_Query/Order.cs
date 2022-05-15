using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sql_Query
{
    class Order
    {
        public string Sender;
        public string Target;
        public override string ToString()
        {
            return "Sender: " + Sender.ToString() + " Target: " + Target.ToString();
        }
    }
}
