using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class UserTsearcho
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(150)]
        public string TableName { get; set; }
        [Required]
        [StringLength(150)]
        public string ColumnName { get; set; }
        public bool IsValidToSearch { get; set; }
    }
}
