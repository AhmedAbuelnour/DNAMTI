using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SequenceAlignment.Models
{
    public class Sequence
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string AlignmentID { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "You have to enter the first sequence"), Column(TypeName = "VARCHAR(MAX)")]
        public string FirstSequence { get; set; }

        [Required(ErrorMessage = "You have to name your first sequence, if not real sequence you can just give it a fake name")]
        public string FirstSequenceName { get; set; }

        [Required(ErrorMessage ="Hashing the Sequence is required!")]
        public string FirstSequenceHash { get; set; }

        [Required(ErrorMessage = "You have to enter the second sequence"), Column(TypeName = "VARCHAR(MAX)")]
        public string SecondSequence { get; set; }

        [Required(ErrorMessage = "You have to name your Second sequence, if not real sequence you can just give it a fake name")]
        public string SecondSequenceName { get; set; }

        [Required(ErrorMessage = "Hashing the Sequence is required!")]
        public string SecondSequenceHash { get; set; }

        [Column(TypeName = "VARBINARY(MAX)")]
        public byte[] ByteText { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        public DateTime ExpirationDate { get; set; } = DateTime.Now.AddDays(7); // Indicate how many days it will be stored in the database

        public string UserFK { get; set; }

        [ForeignKey(nameof(UserFK))]
        public IdentityUser UserNavigation { get; set; }
    }
}
