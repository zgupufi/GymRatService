using GymRatService.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace GymRatService.DAL.Core
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
