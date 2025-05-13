using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bibliothek.Utility
{
    public static class Validation
    {
        /// <summary>
        ///  Überprüft, ob die gegebene E-Mail-Adresse gültig ist
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string email)
        {
            try
            {
                //var mail = new MailAddress(email); // Versucht, eine MailAddress-Instanz zu erstellen
                //return true;

                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                Regex regex = new Regex(pattern);

                return regex.IsMatch(email);

            }
            catch
            {
                return false; // Gibt false zurück, wenn die E-Mail-Adresse ungültig ist
            }
        }
    }
}
