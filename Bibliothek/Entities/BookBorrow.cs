using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class BookBorrow
    {
        [Key]
        public int ID { get; set; }
        public int BookID { get; set; }
        [Column(TypeName = "date")]
        public DateTime CommitDate { get; set; }
        [Column(TypeName = "date")]
        public DateTime ReturnDate { get; set; }
        public int UserID { get; set; }
        public bool IsAccept { get; set; }
        public bool IsBack { get; set; }

        [ForeignKey(nameof(BookID))]
        [InverseProperty("BookBorrow")]
        public virtual Book Book { get; set; }
        [ForeignKey(nameof(UserID))]
        [InverseProperty("BookBorrow")]
        public virtual User User { get; set; }
    }
}
