using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class Menu
    {
        public Menu()
        {
            TypeToMenuAccess = new HashSet<TypeToMenuAccess>();
            UserPermission = new HashSet<UserPermission>();
        }

        [Key]
        public int ID { get; set; }
        [Required]
        [StringLength(100)]
        public string MenuName { get; set; }
        [Required]
        [StringLength(150)]
        public string ManuURL { get; set; }

        [InverseProperty("Menu")]
        public virtual ICollection<TypeToMenuAccess> TypeToMenuAccess { get; set; }
        [InverseProperty("Menu")]
        public virtual ICollection<UserPermission> UserPermission { get; set; }
    }
}
