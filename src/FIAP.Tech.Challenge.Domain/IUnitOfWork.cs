using System.Threading;
using System.Threading.Tasks;

namespace FIAP.Tech.Challenge.Domain;

public interface IUnitOfWork
{
    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}
