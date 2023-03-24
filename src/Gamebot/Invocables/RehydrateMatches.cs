using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;

namespace Gamebot.Invocables;

public class RehydrateMatches : IInvocable, ICancellableInvocable
{
    private DbContext _dbContext;

    public CancellationToken CancellationToken { get; set; }

    public RehydrateMatches(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Invoke()
    {
        // What is your invocable going to do?
    }
}
