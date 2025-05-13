using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class Category
    {
        public Category()
        {
            Book = new HashSet<Book>();
        }

        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(200)]
        public string Value { get; set; }

        [InverseProperty("Categorye")]
        public virtual ICollection<Book> Book { get; set; }
    }
}
