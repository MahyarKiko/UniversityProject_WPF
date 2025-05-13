using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class BerechtigungsStufeType
    {
        public BerechtigungsStufeType()
        {
            UserType = new HashSet<UserType>();
        }

        [Key]
        [StringLength(50)]
        public string ID { get; set; }
        [Required]
        [StringLength(50)]
        public string Berechtigungsstufe { get; set; }

        [InverseProperty("BerechtigungsStufeType")]
        public virtual ICollection<UserType> UserType { get; set; }
    }
}
