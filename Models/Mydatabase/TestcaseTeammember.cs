using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestCaseDashboard.Models.mydatabase
{
    [Table("testcase_teammember", Schema = "public")]
    public partial class TestcaseTeammember
    {
        [Key]
        [Column("id")]
        [Required]
        public Guid Id { get; set; }

        [Column("teammemberid")]
        [Required]
        public Guid Teammemberid { get; set; }

        public Teammember Teammember { get; set; }

        [Column("testcaseid")]
        [Required]
        public Guid Testcaseid { get; set; }

        public Testcase Testcase { get; set; }
    }
}