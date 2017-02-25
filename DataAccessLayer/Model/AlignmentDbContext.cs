using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Model
{
    public class AlignmentDbContext : IdentityDbContext
    {
        public AlignmentDbContext(DbContextOptions<AlignmentDbContext> options): base(options) { }


        public DbSet<AlignmentJob> AlignmentJobs { get; set; }

        // For Windows Application, Override OnConfiguring Method
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //}
    }
}
