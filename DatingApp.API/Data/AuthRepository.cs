using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data {
    public class AuthRepository : IAuthRepository {
        private readonly DataContext _context;

        public AuthRepository (DataContext context) {
            _context = context;
        }
        async Task<User> IAuthRepository.Login (string username, string password) {
            var user =await _context.Users.FirstOrDefaultAsync(x=>x.UserName==username);
            if(user==null){
                return null;
            }
            if(!VerifyPasswordHash(password,user.PasswordHash,user.PasswordSalt)){
                return null;
            }
            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
             using(var hmac=new System.Security.Cryptography.HMACSHA512(passwordSalt)){
               var computedHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
               for (int i = 0; i<computedHash.Length;i++)
               {
                   if(computedHash[i]!=passwordHash[i]){
                       return false;
                   }
               }

            } 
            return true;
        }

         async  Task<User> IAuthRepository.Registor (User user, string password) {
            byte[] passwordHash, PasswordSalt;
            createPasswordHash(password,out passwordHash,out PasswordSalt);
            user.PasswordHash=passwordHash;
            user.PasswordSalt=PasswordSalt;
           await _context.Users.AddAsync(user);
           await _context.SaveChangesAsync();
           return user;
        }

        private void createPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac=new System.Security.Cryptography.HMACSHA512()){
                passwordSalt=hmac.Key;
                passwordHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            
        }

        async Task<bool> IAuthRepository.UserExists (string username) {
            if(await _context.Users.AnyAsync(x=>x.UserName==username)){
                return true;
            }
            return false;

        }
    }
}