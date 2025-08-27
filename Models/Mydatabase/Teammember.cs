using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestCaseDashboard.Models.mydatabase
{
    [Table("teammember", Schema = "public")]
    public partial class Teammember
    {
        [Key]
        [Column("id")]
        [Required]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Column("createdat")]
        public DateTime? Createdat { get; set; }

        [Column("updatedat")]
        public DateTime? Updatedat { get; set; }

        public ICollection<ProjectTeammember> ProjectTeammembers { get; set; }

        public ICollection<TestcaseTeammember> TestcaseTeammembers { get; set; }
    }
}