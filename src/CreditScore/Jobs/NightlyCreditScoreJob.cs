using CreditScore.Data;
using CreditScore.Models;
using CreditScore.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace CreditScore.Jobs;

[DisallowConcurrentExecution]
public class NightlyCreditScoreJob(
    CreditScoreDbContext creditScoreDb,
    IServiceScopeFactory scopeFactory,
    ILogger<NightlyCreditScoreJob> logger) : IJob
{
    private const int BatchSize = 100;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        var processed = 0;
        var failed = 0;

        string? lastCivilId = null;
        while (true)
        {
            var civilIds = await creditScoreDb.CreditScores
                .AsNoTracking()
                .Where(c => lastCivilId == null || string.Compare(c.CivilId, lastCivilId) > 0)
                .OrderBy(c => c.CivilId)
                .Select(c => c.CivilId)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (civilIds.Count == 0)
            {
                break;
            }

            foreach (var civilId in civilIds)
            {
                try
                {
                    using var customerScope = scopeFactory.CreateScope();
                    var creditScoresService = customerScope.ServiceProvider.GetRequiredService<CreditScoresService>();
                    await creditScoresService.RecalculateCreditScoreAsync(civilId, CreditScoreTrigger.NightlyJob);
                    processed++;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    failed++;
                    logger.LogWarning(ex, "Could not recalculate credit score for {CivilId}", civilId);
                }
            }

            lastCivilId = civilIds[^1];
        }

        logger.LogInformation("Nightly credit score job finished: {Processed} processed, {Failed} failed", processed, failed);
    }
}
