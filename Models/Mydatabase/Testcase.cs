using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using  TestCaseDashboard.Models.mydatabase;


namespace TestCaseDashboard.Models.mydatabase
{
    [Table("testcase", Schema = "public")]
    public partial class Testcase
    {
        [Key]
        [Column("id")]
        [Required]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Column("screen")]
        [Required]
        public string Screen { get; set; }

        [Column("function")]
        public string? Function { get; set; }

        [Column("projectid")]
        [Required]
        public Guid Projectid { get; set; }

        public Project Project { get; set; } = default!;

        [Column("createdat")]
        public DateTime? Createdat { get; set; }

        [Column("updatedat")]
        public DateTime? Updatedat { get; set; }

        public ICollection<TestcaseTeammember> TestcaseTeammembers { get; set; } = new List<TestcaseTeammember>();
    }
}
