using Common.Contracts;
using CreditScore.Models;
using CreditScore.Services;
using MassTransit;

namespace CreditScore.Consumers;

public class CustomerCreditDataChangedConsumer(CreditScoresService creditScoresService) : IConsumer<CustomerCreditDataChanged>
{
    public async Task Consume(ConsumeContext<CustomerCreditDataChanged> context)
    {
        var trigger = Enum.Parse<CreditScoreTrigger>(context.Message.Reason.ToString());
        await creditScoresService.RecalculateCreditScoreAsync(context.Message.CivilId, trigger);
    }
}
