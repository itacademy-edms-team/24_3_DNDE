using System.Threading.Tasks;

namespace Identity.Application.Commands
{
    public interface ICommand<R, S>
        where R : Request
        where S : Response
    {
        public Task<S> Execute(R request);
    }
}
