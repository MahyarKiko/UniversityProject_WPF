using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class UserPermission
    {
        [Key]
        public int ID { get; set; }
        public int MenuID { get; set; }
        public int UserID { get; set; }
        public bool IsAccept { get; set; }

        [ForeignKey(nameof(MenuID))]
        [InverseProperty("UserPermission")]
        public virtual Menu Menu { get; set; }
        [ForeignKey(nameof(UserID))]
        [InverseProperty("UserPermission")]
        public virtual User User { get; set; }
    }
}
