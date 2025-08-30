using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestCaseDashboard.Models.mydatabase
{
    [Table("project_teammember", Schema = "public")]
    public partial class ProjectTeammember
    {
        [Key]
        [Column("id")]
        [Required]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Column("teammemberid")]
        [Required]
        public Guid Teammemberid { get; set; }

        public Teammember Teammember { get; set; }

        [Column("projectid")]
        [Required]
        public Guid Projectid { get; set; }

        public Project Project { get; set; }

         [Column("createdat")]
        public DateTime? Createdat { get; set; }

        [Column("updatedat")]
        public DateTime? Updatedat { get; set; }
    }
}