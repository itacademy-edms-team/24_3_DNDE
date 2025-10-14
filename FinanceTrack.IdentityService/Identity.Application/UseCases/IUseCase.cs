using System.Threading.Tasks;

namespace Identity.Application.UseCases
{
    public interface IUseCase<R, S>
        where R : Request
        where S : Response
    {
        public Task<S> Execute(R request);
    }
}
