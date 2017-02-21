using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Model
{
    public class AlignmentJob
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string AlignmentID { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "You have to name your first sequence, if not real sequence you can just give it a fake name")]
        public string FirstSequenceName { get; set; }

        [Required(ErrorMessage = "Hashing the Sequence is required!")]
        public string FirstSequenceHash { get; set; }

        [Required(ErrorMessage = "You have to name your Second sequence, if not real sequence you can just give it a fake name")]
        public string SecondSequenceName { get; set; }

        [Required(ErrorMessage = "Hashing the Sequence is required!")]
        public string SecondSequenceHash { get; set; }

        [Column(TypeName = "VARBINARY(MAX)")]
        public byte[] ByteText { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        public DateTime ExpirationDate { get; set; } = DateTime.Now.AddDays(7); // Indicate how many days it will be stored in the database

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
