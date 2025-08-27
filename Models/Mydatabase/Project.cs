using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using  TestCaseDashboard.Models.mydatabase;


namespace TestCaseDashboard.Models.mydatabase
{
    [Table("project", Schema = "public")]
    public partial class Project
    {
        [Key]
        [Column("id")]
        [Required]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Column("projectname")]
        [Required]
        public string? Projectname { get; set; }

        [Column("source")]
        [Required]
        public ProjectSource ProjectSource { get; set; }

        [Column("createdat")]
        public DateTime? Createdat { get; set; }

        [Column("updatedat")]
        public DateTime? Updatedat { get; set; }

        public ICollection<ProjectTeammember> ProjectTeammembers { get; set; } = new List<ProjectTeammember>();

        public ICollection<Testcase> Testcases { get; set; } = new List<Testcase>();
    }
}
