using Bibliothek.Entities;
using Bibliothek.Model;
using Bibliothek.Utility;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace Bibliothek
{
    public partial class Book : Window
    {
        // Listen für Bücher und reservierte Bücher
        List<BookModel> books; // Liste der verfügbaren Bücher
        List<BookedBookModel> bookedBooks; // Liste der vom Benutzer reservierten Bücher
        BookModel selectedBook; // Das aktuell ausgewählte Buch
        FileUtil fileUtil; // Hilfsklassse zum Lesen und Schreiben von Dateien
        User user, secendUser, currentUser; // Benutzerobjekte für den aktuellen Benutzer, zweiten Benutzer und den gerade bearbeiteten Benutzer

        public Book()
        {
            InitializeComponent();
            // Initialisiere die Listen
            books = new List<BookModel>();
            bookedBooks = new List<BookedBookModel>();
            fileUtil = new FileUtil();
            secendUser = new User();
            currentUser = new User();

            // Lade Benutzerdaten aus JSON Datei
            string userData = fileUtil.ReadStringFromJson();
            if (userData != string.Empty && userData != null)
            {
                string json = userData; // Die Benutzerdaten könnten hier entschlüsselt werden, falls nötig
                user = JsonConvert.DeserializeObject<User>(json); // Deserialisierung der JSON Daten zu einem User Objekt   

                if (user != null)
                {
                    if (user.UserTypeID == 1) // Wenn der Benutzer ein Admin ist
                    {
                        layout_SelectUser.Visibility = Visibility.Visible;
                        chBox_Forme.Visibility = Visibility.Visible;
                        sp_Reserved.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        layout_SelectUser.Visibility = Visibility.Collapsed;
                        chBox_Forme.Visibility = Visibility.Collapsed;
                        LoadBookedBooks(user); // Lade die vom Benutzer reservierten Bücher
                    }
                }
            }
            // Lade alle verfügbaren Bücher aus der Datenbank
            LoadBooks();
        }
// Laden der Bücher aus der Datenbank
        private async Task LoadBooks()
        {
            Bibliothek_Content db = new Bibliothek_Content(); // Datenbankverbindung erstellen
            var bookInfoQuery = (from b in db.Book
                                 join a in db.Authors on b.AuthorID equals a.ID
                                 join k in db.Category on b.CategoryeID equals k.ID
                                 join c in db.CountOfBooks on b.ID equals c.BookID
                                 where b.IsAvailable // Nur verfügbare Bücher
                                 select new
                                 {
                                     b.ID,
                                     b.Name,
                                     AutorName = a.Fullname,
                                     KategorieName = k.Value,
                                     ISBN = b.ISBN,
                                     Veröffentlichungsdatum = b.ReleaseDate,
                                     CountOfBook = c.CountOfBook
                                 }).ToList();

            // Abfrage, um die Anzahl der Ausleihen pro Buch zu erhalten
            var bookLoansQuery = (from b in db.Book
                                  where b.IsAvailable
                                  select new
                                  {
                                      b.ID,
                                      LoanCount = db.BookBorrow.Count(ba => ba.BookID == b.ID && !ba.IsBack) // Zählt die Anzahl der aktuellen Ausleihen für jedes Buch
                                  }).ToList();

            // Verknüpfen der Buchinformationen mit den Ausleihinformationen
            var result = from bi in bookInfoQuery
                         join bl in bookLoansQuery on bi.ID equals bl.ID into bookLoanInfo
                         from bli in bookLoanInfo.DefaultIfEmpty()
                         select new
                         {
                             bi.ID,
                             bi.Name,
                             bi.AutorName,
                             bi.KategorieName,
                             bi.ISBN,
                             bi.Veröffentlichungsdatum,
                             AvailableBooks = bi.CountOfBook - (bli != null ? bli.LoanCount : 0) // Berechne die Anzahl der verfügbaren Exemplare
                         };

            // Sortiere das Ergebnis nach Buchnamen
            var finalResult = result.OrderBy(r => r.Name).ToList();

            // Konvertiere das Ergebnis in eine Liste von BookModel Objekten
            books = finalResult
                .Select((book) => new BookModel
                {
                    Reihe = book.ID,
                    Title = book.Name,
                    Author = book.AutorName,
                    Category = book.KategorieName,
                    ISBN = book.ISBN,
                    ReleaseDate = book.Veröffentlichungsdatum.ToString(),
                    Available = book.AvailableBooks
                })
                .ToList();

            // Zeige die Bücher in einem DataGrid an, wenn verfügbar
            if (books.Count > 0)
            {
                BooksDataGrid.ItemsSource = books;
            }
            else
            {
                MessageBox.Show("Aktuell sind keine Bücher verfügbar");
            }
        }

        //  Laden der vom Benutzer gebuchten Bücher
        private async Task LoadBookedBooks(User _user)
        {
            lbl_BookedBook.Content = $"Reservierte Bücher für  '{_user.FirstName.ToUpper()} {_user.LastName.ToUpper()}'";
            Bibliothek_Content db = new Bibliothek_Content(); // Datenbankverbindung erstellen

            // Abfrage, um die vom Benutzer gebuchten Bücher zu laden
            bookedBooks = await (from al in db.BookBorrow
                                 join b in db.Book on al.BookID equals b.ID
                                 join a in db.Authors on b.AuthorID equals a.ID
                                 join c in db.Category on b.CategoryeID equals c.ID
                                 join u in db.User on al.UserID equals u.ID
                                 where u.ID == _user.ID
                                 select new BookedBookModel()
                                 {
                                     ID = al.ID,
                                     Title = b.Name,
                                     Author = a.Fullname,
                                     Category = c.Value,
                                     ISBN = b.ISBN,
                                     ReservedDate = al.CommitDate,
                                     ReturnDate = al.ReturnDate,
                                     IsAccept = al.IsAccept ? "Bestätigt" : "Wird überprüft",
                                     IsBack = al.IsBack ? "Rückgabe abgeschlossen" : "Nicht zurückgegeben"
                                 }).ToListAsync();

            // Zeige die gebuchten Bücher an, falls welche vorhanden sind
            if (bookedBooks.Count > 0)
            {
                sp_Reserved.Visibility = Visibility.Visible;
                BookedDataGrid.ItemsSource = bookedBooks;
            }
            else
            {
                sp_Reserved.Visibility = Visibility.Collapsed;
            }
        }

        // Ereignis Handler, der ausgelöst wird, wenn sich die Auswahl im BooksDataGrid ändert
        private void BooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem != null)
            {
                // Wenn ein Buch ausgewählt wurde, speichere es und zeige die Details an
                selectedBook = (BookModel)BooksDataGrid.SelectedItem;

                sp_Detail.Visibility = Visibility.Visible;
                sp_Reserved.Visibility = Visibility.Collapsed;
                titleTextBox.Text = selectedBook.Title;
                AuthorTextBox.Text = selectedBook.Author;
            }
            else
            {
                // Wenn kein Buch ausgewählt wurde, verstecke die Detailanzeige
                selectedBook = null;
                sp_Detail.Visibility = Visibility.Collapsed;
            }
        }

        // Ereignis Handler für den click auf den Add Button (hinzufügenbutton)
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook != null)
            {
                // Aktualisiere das ausgewählte Buch
                UpdateBook(selectedBook);
            }
            else
            {
                // Füge ein neues Buch hinzu
                AddBook();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook != null)
            {
                // Aktualisiere das ausgewählte Buch
                UpdateBook(selectedBook);
            }
            else
            {
                // Füge ein neues Buch hinzu
                AddBook();
            }
        }

        // Ereignis Handler für den click auf den Delete Button
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook != null)
            {
                // Lösche das ausgewählte Buch
                DeleteBook(selectedBook);
            }
        }

        //  Hinzufügen eines neuen Buches (aktuell nur eine Nachricht)
        private void AddBook()
        {
            // Logik zum Hinzufügen eines neuen Buches
           
            MessageBox.Show("Funktion zum Hinzufügen eines neuen Buches ist in Bearbeitung");
        }

        //  Aktualisieren eines Buches (aktuell nur eine Nachricht)
        private void UpdateBook(BookModel book)
        {
            MessageBox.Show("Buchdetails werden aktualisiert");
        }

        //  Löschen eines Buches (aktuell nur eine Nachricht)
        private void DeleteBook(BookModel book)
        {
            MessageBox.Show("Buch wird gelöscht");
        }

        // Ereignis Handler für den click auf den Buchen Button
        private void BuchenButton_Click(object sender, RoutedEventArgs e)
        {
            // Überprüft, ob ein Buch ausgewählt wurde. Wenn nicht, wird eine Warnmeldung angezeigt und die Methode beendet.
            if (selectedBook == null)
            {
                MessageBox.Show("Bitte wählen Sie ein Buch aus der Liste aus", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Wenn der Benutzer ein Administrator ist, aber weder Für mich ausgewählt hat noch einen Nutzer angegeben hat, wird eine Warnung ausgegeben.
            if (user.UserTypeID == 1 && !chBox_Forme.IsChecked.Value && SearchUserTextBox.Text == "")
            {
                MessageBox.Show("Bitte wählen Sie einen Nutzer aus der Liste aus", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Überprüft, ob das ausgewählte Buch verfügbar ist.
            if (selectedBook.Available > 0)
            {
                // Bestätigungsnachricht 
                MessageBoxResult result = MessageBox.Show($"Möchten Sie '' {selectedBook.Title.ToUpper()} '' reservieren?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes) 
                {
                    // Überprüft, ob der Benutzer ein Administrator ist.
                    if (user.UserTypeID == 1)
                    {
                        // Wenn Für mich ausgewählt ist, wird der aktuelle Benutzer für die Reservierung verwendet, andernfalls der sekundäre Benutzer.
                        currentUser = chBox_Forme.IsChecked.Value ? user : secendUser;
                    }
                    else
                    {
                        currentUser = user; // Wenn kein Administrator, wird immer der aktuelle Benutzer verwendet.
                    }

                    // Erstellt eine Verbindung zur Datenbank.
                    Bibliothek_Content db = new Bibliothek_Content();

                    // Verringert die Anzahl der verfügbaren Exemplare des Buches um 1.
                    selectedBook.Available -= 1;

                    // Fügt eine neue Buchreservierung in die Datenbank ein.
                    var buchen = db.BookBorrow.Add(new BookBorrow()
                    {
                        BookID = selectedBook.Reihe, 
                        UserID = currentUser.ID,
                        CommitDate = DateTime.Now, 
                        ReturnDate = DateTime.Now.AddMonths(3), 
                        IsAccept = false, 
                        IsBack = false 
                    });

                    
                    db.SaveChanges();

                    // Wenn die Änderungen erfolgreich gespeichert wurden, wird eine Bestätigungsmeldung angezeigt.
                    if (db != null)
                    {
                        MessageBox.Show("Buchreservierung erfolgreich abgeschlossen!");
                        LoadBooks(); // Aktualisiert die Liste der verfügbaren Bücher.
                        LoadBookedBooks(currentUser); // aktualisiert die Liste der vom Benutzer reservierten Bücher.
                    }
                }
                else // Wenn der Benutzer die Reservierung nicht bestätigt.
                {
                    sp_Detail.Visibility = Visibility.Collapsed; // Verbirgt den Detailbereich.
                    sp_Reserved.Visibility = Visibility.Visible; // Zeigt den reservierten Bereich an.
                }
            }
            else
            {
                
                MessageBox.Show("Das Buch ist derzeit nicht verfügbar");
                sp_Detail.Visibility = Visibility.Collapsed;
                sp_Reserved.Visibility = Visibility.Visible;
            }
        }


        private void onSearchChange(object sender, TextChangedEventArgs e)
        {
            // Ruft die Methode auf, die die Bücherliste basierend auf dem aktuellen Suchtext filtert.
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            // Holt den Text aus dem Suchfeld und wandelt ihn in kleinbuchstaben um, um eine casecinsensitive Suche zu ermöglichen.
            string searchText = SearchTextBox.Text.ToLower();

            // Filtert die Bücherliste, indem überprüft wird, ob der Titel, der Autor, die Kategorie oder die ISBN den Suchtext enthalten.
            var filteredBooks = books.Where(b =>
                b.Title.ToLower().Contains(searchText) ||
                b.Author.ToLower().Contains(searchText) ||
                b.Category.ToLower().Contains(searchText) ||
                b.ISBN.ToLower().Contains(searchText)
            ).ToList();

            // Setzt die gefilterte Bücherliste als Datenquelle für das DataGrid, um die Ergebnisse anzuzeigen.
            BooksDataGrid.ItemsSource = filteredBooks;
        }

        private void BuchenCancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Verbirgt den Detailbereich.
            sp_Detail.Visibility = Visibility.Collapsed;

            // Setzt die Auswahl im DataGrid zurück.
            BooksDataGrid.SelectedIndex = -1;

            /*Überprüft, ob der Benutzer ein Administrator ist und ob ein Benutzer ausgewählt wurde.
            Entsprechend wird der reservierte Bereich sichtbar oder verborgen. */
            if (user.UserTypeID == 1 && !chBox_Forme.IsChecked.Value && SearchUserTextBox.Text == "")
            {
                sp_Reserved.Visibility = Visibility.Collapsed;
            }
            else
            {
                sp_Reserved.Visibility = Visibility.Visible;
            }
        }

        private void chBox_Forme_CheckedChange(object sender, RoutedEventArgs e)
        {
            
            var ch = (CheckBox)sender;

            
            if (ch.IsChecked.Value)
            {
                layout_SelectUser.Visibility = Visibility.Collapsed;
                sp_Reserved.Visibility = Visibility.Visible;

                // Lädt die vom aktuellen Benutzer reservierten Bücher.
                LoadBookedBooks(user);
            }
            else
            {
                
                layout_SelectUser.Visibility = Visibility.Visible;
                sp_Reserved.Visibility = Visibility.Collapsed;
            }

            // Setzt das Suchfeld für den Benutzer zurück.
            SearchUserTextBox.Text = "";
            UsersDataGrid.Items.Clear(); 
            secendUser = new User();
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            // Überprüft, ob die eingegebene E-Mail-Adresse gültig ist.
            if (!Bibliothek.Utility.Validation.IsValidEmail(SearchUserTextBox.Text))
            {
                MessageBox.Show("Die Email-Adresse ist nicht gültig");
            }
            else
            {
                // Erstellt eine Verbindung zur Datenbank.
                Bibliothek_Content db = new Bibliothek_Content();

                // Sucht nach einem Benutzer in der Datenbank, der der eingegebenen E-Mail-adresse entspricht.
                var user = db.User.FirstOrDefault(t => t.Email == SearchUserTextBox.Text);

                if (user != null)
                {
                    // Wenn ein Benutzer gefunden wird, wird dieser als sekundärer Benutzer gespeichert.
                    secendUser = user;

                    
                    UsersDataGrid.Items.Clear();
                    UsersDataGrid.Items.Add(user);

                    // Lädt die vom sekundären Benutzer reservierten Bücher.
                    LoadBookedBooks(secendUser);

                    // Macht den reservierten Bereich sichtbar und verbirgt den Detailbereich.
                    sp_Reserved.Visibility = Visibility.Visible;
                    sp_Detail.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Zeigt eine Nachricht an, wenn kein Benutzer mit der eingegebenen E-Mail-Adresse gefunden wurde.
                    MessageBox.Show("Benutzer nicht gefunden");
                }
            }
        }
    }
}