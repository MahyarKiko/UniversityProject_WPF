using Bibliothek.Entities;
using Bibliothek.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bibliothek
{
    /// <summary>
    /// Interaction logic for Ausleihen.xaml
    /// </summary>
    public partial class Borrow : Window
    {
        List<BookedBookModel> bookedBooks;
        BookedBookModel selectedBookedBooks;

        public Borrow()
        {
            InitializeComponent();
            bookedBooks = new List<BookedBookModel>();
            selectedBookedBooks = new BookedBookModel();
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchUserTextBox.Text))
            {
                MessageBox.Show("Bitte geben Sie den Benutzernamen ein");
                return;
            }

            if (!Bibliothek.Utility.Validation.IsValidEmail(SearchUserTextBox.Text))
            {
                MessageBox.Show("Bitte geben Sie einen gültigen Benutzernamen ein");
                return;
            }

            LoadBookedBook(SearchUserTextBox.Text);
        }

        private void LoadBookedBook(string userName)
        {
            try
            {
                using (var db = new Bibliothek_Content())
                {
                    // Abfrage, um die vom Benutzer gebuchten Bücher zu laden
                    bookedBooks = (from al in db.BookBorrow
                                   join b in db.Book on al.BookID equals b.ID
                                   join a in db.Authors on b.AuthorID equals a.ID
                                   join c in db.Category on b.CategoryeID equals c.ID
                                   join u in db.User on al.UserID equals u.ID
                                   where u.Email == userName
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
                                   }).ToList();

                    // Zeige die gebuchten Bücher an, falls welche vorhanden sind
                    if (bookedBooks.Count > 0)
                    {
                        sp_Reserved.Visibility = Visibility.Visible;
                        BookedDataGrid.ItemsSource = bookedBooks;
                    }
                    else
                    {
                        sp_Reserved.Visibility = Visibility.Collapsed;
                        MessageBox.Show("Keine Bücher für diesen Benutzer gebucht");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtoCommitBook_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBookedBooks.ID == 0)
            {
                MessageBox.Show("Bitte wählen Sie zuerst ein Buch");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Möchten Sie das Buch \"{selectedBookedBooks.Title.ToUpper()}\" ausleihen?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CommitBook();
                BookedDataGrid.SelectedIndex = -1;
            }
        }

        private void ButtonReturnBook_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBookedBooks.ID == 0)
            {
                MessageBox.Show("Bitte wählen Sie zuerst ein Buch");
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Möchten Sie das Buch \"{selectedBookedBooks.Title.ToUpper()}\" zurückgeben?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReturnBook();
                BookedDataGrid.SelectedIndex = -1;
            }
        }

        private void onSearchChange(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter(); // Ruft die vorhandene Methode auf, um die Bücherliste basierend auf der Suchanfrage zu filtern.
        }

        private void ApplySearchFilter()
        {
            string searchText = SearchTextBox.Text.ToLower();
            // Filtert die Bücherliste basierend auf dem Suchtext. Es wird geprüft, ob der Titel, der Autor, die Kategorie oder die ISSBN den Suchtext enthalten.
            var filteredBooks = bookedBooks.Where(b => b.Title.ToLower().Contains(searchText) ||
                                                     b.Author.ToLower().Contains(searchText) ||
                                                     b.Category.ToLower().Contains(searchText) ||
                                                     b.ISBN.ToLower().Contains(searchText)
            ).ToList();
            BookedDataGrid.ItemsSource = filteredBooks; // Setzt die gefilterte Bücherliste als Datenquelle für das DataGrid
        }

        private void BookedDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedBookedBooks = (BookedBookModel)BookedDataGrid.SelectedItem;
        }

        private void CommitBook()
        {
            using (var db = new Bibliothek_Content())
            {
                var commit = db.BookBorrow.FirstOrDefault(t => t.ID == selectedBookedBooks.ID);
                if (commit != null)
                {
                    commit.IsAccept = true;
                    db.SaveChanges();
                    LoadBookedBook(SearchUserTextBox.Text);
                }
                else
                {
                    MessageBox.Show("Kein Buch gefunden.");
                }
            }
        }

        private void ReturnBook()
        {
            using (var db = new Bibliothek_Content())
            {
                var commit = db.BookBorrow.FirstOrDefault(t => t.ID == selectedBookedBooks.ID);
                if (commit != null)
                {
                    commit.IsBack = true;
                    db.SaveChanges();
                    LoadBookedBook(SearchUserTextBox.Text);
                }
                else
                {
                    MessageBox.Show("Kein Buch gefunden.");
                }
            }
        }
    }
}