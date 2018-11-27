using System.Threading.Tasks;
using MyWeb.Api.Models;

namespace MyWeb.Api.Data
{
    public interface IAuthRepository
    {
         Task<User> Register(User user , string password);

         Task<User> Login(string userName , string password);

         Task<bool> UserExists(string userName);
    }
}