using Coravel.Invocable;
using Gamebot.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gamebot.Invocables;

public class RehydrateMatches : IInvocable, ICancellableInvocable
{
    private GameContext _dbContext;
    private API Api;

    public CancellationToken CancellationToken { get; set; }

    public RehydrateMatches(GameContext dbContext, API api)
    {
        _dbContext = dbContext;
        Api = api;
    }

    public async Task Invoke()
    {
        var matches = _dbContext.ActiveMatches.ToList();
        foreach (var match in matches)
        {
            var updatedMatch = await Api.FetchMatchInformation(match.Match.MatchLink);
            var dbMatch = await _dbContext.ActiveMatches.FirstOrDefaultAsync(
                m => m.Match.MatchLink == match.Match.MatchLink,
                cancellationToken: CancellationToken
            );
            dbMatch.Match = updatedMatch;
            await _dbContext.SaveChangesAsync(CancellationToken);
        }
    }
}
