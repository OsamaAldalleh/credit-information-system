namespace CreditScore.Models;

public class CustomerCreditScore
{
    public required string CivilId { get; set; }
    public CreditGrade Grade { get; set; }
    public int ActiveLoans { get; set; }
    public int Delinquencies { get; set; }
    public int LoansInLitigation { get; set; }
    public int RecentGuiltyVerdicts { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }
}
