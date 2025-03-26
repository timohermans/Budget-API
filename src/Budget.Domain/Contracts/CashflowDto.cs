using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Budget.Domain.Contracts;

public class CashflowDto
{
    public required string Iban { get; set; }
    public required IEnumerable<BalanceAtDateDto> BalancesPerDate { get; set; }
}

public class BalanceAtDateDto
{
    public DateOnly Date { get; set; }
    public decimal Balance { get; set; }
}
