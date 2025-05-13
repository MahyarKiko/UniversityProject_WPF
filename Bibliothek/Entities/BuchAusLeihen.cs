using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class BuchAusLeihen
    {
        public int BookID { get; set; }
        [Column(TypeName = "date")]
        public DateTime DatumAusleihen { get; set; }
        [Column(TypeName = "date")]
        public DateTime DatumRückgabe { get; set; }
        public int UserID { get; set; }
        public bool IsAccept { get; set; }
        public bool IsBack { get; set; }
    }
}
