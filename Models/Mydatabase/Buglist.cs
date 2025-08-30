using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TestCaseDashboard.Models.mydatabase
{
    [Table("buglist", Schema = "public")]
    
    public partial class Buglist
    {
        [Key]
        [Column("id")]
        [Required]
        [ScaffoldColumn(false)]
        public Guid Id { get; set; }

        [Column("testcaseid")]
        [Required]
        public Guid Testcaseid { get; set; }

        public Testcase Testcase { get; set; }

        [Column("remark")]
        public string Remark { get; set; }

        [Column("image")]
        public string Image { get; set; }

        [Column("createdat")]
        public DateTime? Createdat { get; set; }

        [Column("updatedat")]
        public DateTime? Updatedat { get; set; }
    }
}