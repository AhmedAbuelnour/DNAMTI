using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SequenceAlignment.Models
{
    public class SequenceAlignmentDbContext : IdentityDbContext
    {
        public SequenceAlignmentDbContext(DbContextOptions<SequenceAlignmentDbContext> options)
        :base(options) { }


        public DbSet<Sequence> Sequences { get; set; }
    }
}
