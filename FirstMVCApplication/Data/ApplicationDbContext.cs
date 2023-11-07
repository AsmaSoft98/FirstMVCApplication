using FirstMVCApplication.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FirstMVCApplication.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext() : base("ApplicationDbContext")
        {
        }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           
        }
    }
}