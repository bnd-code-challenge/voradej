using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankAccountModel
{
    public class TransferAccount
    {
        public string? AccountSource { get; set; }

        public string? AccountDestination { get; set; }

        public Decimal? Amount { get; set; }
    }
}
