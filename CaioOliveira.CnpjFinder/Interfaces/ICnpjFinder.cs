using System.Threading.Tasks;
using CaioOliveira.CnpjFinder.Models;

namespace CaioOliveira.CnpjFinder.Interfaces
{
    public interface ICnpjFinder
    {
        Task<QueryResult> Get(string Cnpj);
    }
}