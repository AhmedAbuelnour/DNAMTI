using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models
{
    public class AlignmentJob
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string AlignmentID { get; set; } = Guid.NewGuid().ToString();

        [MaxLength(50),Required(ErrorMessage = "You have to name your first sequence")]
        public string FirstSequenceName { get; set; }

        [Required]
        public string FirstSequenceHash { get; set; }

        [MaxLength(50),Required(ErrorMessage = "You have to name your second sequence")]
        public string SecondSequenceName { get; set; }

        [Required]
        public string SecondSequenceHash { get; set; }

        [Column(TypeName = "VARBINARY(MAX)")]
        public byte[] ByteText { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.Now.ToLocalTime();

        public DateTime ExpirationDate { get; set; } = DateTime.Now.ToLocalTime().AddDays(7); // Indicate how many days it will be stored in the database


        public bool IsAlignmentCompleted { get; set; }

        public string UserFK { get; set; }

        public int Gap { get; set; } = -8;

        public int GapOpenPenalty { get; set; } = -2;

        public int GapExtensionPenalty { get; set; } = -2;

        public string ScoringMatrix { get; set; }

        public string Algorithm { get; set; }

        [ForeignKey(nameof(UserFK))]
        public IdentityUser UserNavigation { get; set; }
    }
}
