using System.Threading.Tasks;

namespace Solid.Typetalk
{
    public interface ITypeTalkConnection
    {
        Task<TResponse> GetAsync<TRequest, TResponse>(TRequest request)
            where TRequest : TypetalkApiRequest
            where TResponse : new();
        Task Login();
    }
}
