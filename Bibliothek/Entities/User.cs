using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class User
    {
        public User()
        {
            BookBorrow = new HashSet<BookBorrow>();
            UserPermission = new HashSet<UserPermission>();
        }

        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [StringLength(15)]
        public string CellPhone { get; set; }
        [Required]
        [StringLength(150)]
        public string Email { get; set; }
        [Required]
        public string Address { get; set; }
        public int? PostalCode { get; set; }
        [StringLength(150)]
        public string City { get; set; }
        [StringLength(150)]
        public string Country { get; set; }
        [Required]
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public int UserTypeID { get; set; }

        [ForeignKey(nameof(UserTypeID))]
        [InverseProperty("User")]
        public virtual UserType UserType { get; set; }
        [InverseProperty("User")]
        public virtual ICollection<BookBorrow> BookBorrow { get; set; }
        [InverseProperty("User")]
        public virtual ICollection<UserPermission> UserPermission { get; set; }
    }
}
