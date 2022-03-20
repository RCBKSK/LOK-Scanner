using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using lok_wss.database.Models;


namespace lok_wss
{
    public class examplelokContext : DbContext
    {
        public examplelokContext(DbContextOptions<lokContext> options) : base(options)
        {
        }


        public DbSet<crystalMine> crystalMine { get; set; }
        public DbSet<treasureGoblin> treasureGoblin { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<crystalMine>().ToTable("crystalMines")
               .HasKey("uguid");

            modelBuilder.Entity<treasureGoblin>().ToTable("treasureGoblins")
                .HasKey("uguid");




        }

        //unused Entity framework context for later 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"insert sql con string here");
        }

    }
}
