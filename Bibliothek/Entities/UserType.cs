using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class UserType
    {
        public UserType()
        {
            TypeToMenuAccess = new HashSet<TypeToMenuAccess>();
            User = new HashSet<User>();
        }

        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        [InverseProperty("Type")]
        public virtual ICollection<TypeToMenuAccess> TypeToMenuAccess { get; set; }
        [InverseProperty("UserType")]
        public virtual ICollection<User> User { get; set; }
    }
}
