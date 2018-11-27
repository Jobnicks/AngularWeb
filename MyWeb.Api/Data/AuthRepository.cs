using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWeb.Api.Data;
using MyWeb.Api.Models;

namespace MyWeb.Api.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        public AuthRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<User> Login(string userName, string password)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(x=>x.Username == userName);

            if(user==null)
            return null;

            if(!VerifyPasswordHash(password,user.PasswordHash,user.PasswordSalt))
            return null;

            return user;

        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash , passwordSalt;
            CreatePasswordHash(password,out passwordHash,out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }           
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
               var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
               for(int i= 0; i <computeHash.Length;i++)
               {
                   if(computeHash[i] != passwordHash[i]) return false;
               }
            }     
            return true;      
        }

        public async Task<bool> UserExists(string userName)
        {
          if(await _context.Users.AnyAsync(x=>x.Username == userName))
          return true;

          return false;
        }
    }
}