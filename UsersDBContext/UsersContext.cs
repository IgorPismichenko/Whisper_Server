﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UsersDB;

namespace UsersDBContext
{
    public class UsersContext: DbContext
    {
        static DbContextOptions<UsersContext> _options;

        static UsersContext()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<UsersContext>();
            _options = optionsBuilder.UseSqlServer(connectionString).Options;
        }

        public UsersContext()
            : base(_options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Users> users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}