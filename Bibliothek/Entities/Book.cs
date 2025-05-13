using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class Book
    {
        public Book()
        {
            BookBorrow = new HashSet<BookBorrow>();
            CountOfBooks = new HashSet<CountOfBooks>();
        }

        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public int AuthorID { get; set; }
        public int CategoryeID { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime ReleaseDate { get; set; }
        [Required]
        [StringLength(20)]
        public string ISBN { get; set; }
        public bool IsAvailable { get; set; }

        [ForeignKey(nameof(AuthorID))]
        [InverseProperty(nameof(Authors.Book))]
        public virtual Authors Author { get; set; }
        [ForeignKey(nameof(CategoryeID))]
        [InverseProperty(nameof(Category.Book))]
        public virtual Category Categorye { get; set; }
        [InverseProperty("Book")]
        public virtual ICollection<BookBorrow> BookBorrow { get; set; }
        [InverseProperty("Book")]
        public virtual ICollection<CountOfBooks> CountOfBooks { get; set; }
    }
}
