using System;

namespace Kernel.Emv
{
    public class Transaction
    {
        public int Type { get; set; }
        public DateTime Date { get; set; }
        public int Amount { get; set; }
        public int AdditionalAmount { get; set; }
    }
}