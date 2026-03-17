using System;
using System.Threading.Tasks;
using GymRatService.Common.Models;
using GymRatService.DAL.Core;
using Microsoft.EntityFrameworkCore;

namespace GymRatService.DAL.Repos
{
    public class UserQueryHandler: IUserQueryHandler
    {
        private readonly DBContext _context;
        public UserQueryHandler(DBContext context)
        {
            _context = context;
        }
        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Prevent duplicate registration by email
            var exists = await FindUserByEmailAsync(user.Email)!=null;
            if (exists)
            {
                throw new InvalidOperationException("A user with the provided email already exists.");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
