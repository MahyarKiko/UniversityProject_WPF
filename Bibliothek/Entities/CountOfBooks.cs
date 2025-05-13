using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class CountOfBooks
    {
        [Key]
        public int ID { get; set; }
        public int BookID { get; set; }
        public int CountOfBook { get; set; }

        [ForeignKey(nameof(BookID))]
        [InverseProperty("CountOfBooks")]
        public virtual Book Book { get; set; }
    }
}
