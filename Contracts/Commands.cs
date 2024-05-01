using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class Commands
    {
        public const string JOIN = "$<Join>";
        public const string LEAVE = "$<Leave>";
        public const string FULL = "$<Full>";
        public const string JOINC = "$<JoinConfirm>";
    }
}
