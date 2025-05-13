using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliothek.Model
{
    internal class BookedBookModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Category { get; set; }
        public DateTime ReservedDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string IsAccept { get; set; }
        public string IsBack { get; set; }
    }
}
