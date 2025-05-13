using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class TypeToMenuAccess
    {
        [Key]
        public int ID { get; set; }
        public int TypeID { get; set; }
        public int MenuID { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(MenuID))]
        [InverseProperty("TypeToMenuAccess")]
        public virtual Menu Menu { get; set; }
        [ForeignKey(nameof(TypeID))]
        [InverseProperty(nameof(UserType.TypeToMenuAccess))]
        public virtual UserType Type { get; set; }
    }
}
