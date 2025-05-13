using Bibliothek.Entities;
using Bibliothek.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bibliothek
{
    /// <summary>
    /// Interaction logic for BookModify.xaml
    /// </summary>
    public partial class BookModify : Window
    {
        // Liste der Bücher, die in der Ansicht angezeigt werden
        List<BookModel> books;

        // Das aktuell ausgewählte Buch
        BookModel selectedBook;

        // IDs für den ausgewählten Autor und die Kategorie
        int selectedAuthor = -1, selectedCategory = -1;

        public BookModify()
        {
            InitializeComponent();

            
            books = new List<BookModel>();

            LoadBooks();

            // Füllt die ComboBox für Autoren und Kategorien
            FetchAuthorComboboxItem();
            FetchCategoryComboboxItem();
        }

      
        private async Task LoadBooks()
        {
            // Erstellen einer Instanz des Datenbankkontexts
            Bibliothek_Content db = new Bibliothek_Content();

            // Abfrage der Buchinformationen einschließlich der Anzahl der Bücher
            var bookInfoQuery = (from b in db.Book
                                 join a in db.Authors on b.AuthorID equals a.ID
                                 join k in db.Category on b.CategoryeID equals k.ID
                                 join c in db.CountOfBooks on b.ID equals c.BookID
                                 where b.IsAvailable
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

            // Abfrage der Anzahl der ausgeliehenenn Bücher
            var bookLoansQuery = (from b in db.Book
                                  where b.IsAvailable
                                  select new
                                  {
                                      b.ID,
                                      LoanCount = db.BookBorrow.Count(ba => ba.BookID == b.ID && ba.IsBack)
                                  }).ToList();


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
                             AvailableBooks = bi.CountOfBook - (bli != null ? bli.LoanCount : 0) // Berechnung der verfügbaren Bücher
                         };


            var finalResult = result.OrderBy(r => r.Name).ToList();

            books = finalResult
                .Select((book) => new BookModel
                {
                    Reihe = book.ID,
                    Title = book.Name,
                    Author = book.AutorName,
                    Category = book.KategorieName,
                    ISBN = book.ISBN,
                    ReleaseDate = book.Veröffentlichungsdatum.ToString(),
                    Available = book.AvailableBooks // Verfügbarkeits der Bücher setzen
                })
                .ToList();

            // Setzen der Datenquelle für das DataGrid oder Anzeige einer Nachricht, wenn keine Bücher vorhanden sind
            if (books.Count > 0)
            {
                BooksDataGrid.ItemsSource = books;
            }
            else
            {
                MessageBox.Show("Es sind keine Bücher verfügbar");
            }
        }


        private void BooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Setzen des ausgewählten Buches, wenn eine Auswahl getroffen wurde
            if (BooksDataGrid.SelectedItem != null)
            {
                selectedBook = (BookModel)BooksDataGrid.SelectedItem;
                txtTitle.Text = selectedBook.Title;
                txtISBN.Text = selectedBook.ISBN;
                txtCount.Text = selectedBook.Available.ToString();

                // Festlegen des Index des ausgewählten Autors in der ComboBox
                int selAuthorIndex = comboAuthor.Items
                       .OfType<Label>()
                       .Select((item, idx) => new { item, idx })
                       .FirstOrDefault(x => x.item.Content.ToString() == selectedBook.Author)?.idx ?? -1;

                // Festlegen des Index der ausgewählten Kategorie in der ComboBox
                int selCategoryIndex = comboCategory.Items
                       .OfType<Label>()
                       .Select((item, idx) => new { item, idx })
                       .FirstOrDefault(x => x.item.Content.ToString() == selectedBook.Category)?.idx ?? -1;

                comboAuthor.SelectedIndex = selAuthorIndex;
                comboCategory.SelectedIndex = selCategoryIndex;

                calDate.SelectedDate = DateTime.Parse(selectedBook.ReleaseDate);
            }
            else
            {
                selectedBook = null;
            }
        }

        // die Autoren in die ComboBox laden
        public async Task FetchAuthorComboboxItem()
        {
            try
            {
                Bibliothek_Content db = new Bibliothek_Content();
                var items = await db.Authors.ToListAsync();

                if (items.Count > 0 && items != null)
                {
                    foreach (var item in items)
                    {
                        Label label = new Label();
                        label.Content = item.Fullname;
                        label.Tag = item.ID;

                        comboAuthor.Items.Add(label);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //  Kategorien in die ComboBox
        public async Task FetchCategoryComboboxItem()
        {
            try
            {
                Bibliothek_Content db = new Bibliothek_Content();
                var items = await db.Category.ToListAsync();

                if (items.Count > 0 && items != null)
                {
                    foreach (var item in items)
                    {
                        Label label = new Label();
                        label.Content = item.Value;
                        label.Tag = item.ID;

                        comboCategory.Items.Add(label);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook != null)
            {
                // Aktualisiert das ausgewählte Buch
                UpdateBook(selectedBook);
            }
            else
            {
                
                AddBook(); // Fügt ein neues Buch hinzu
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBook != null)
            {
                UpdateBook(selectedBook);
            }
            else
            {
                AddBook();
            }
        }


        private async void AddBook()
        {
            try
            {
                if (string.IsNullOrEmpty(txtTitle.Text) || comboAuthor.SelectedItem == null ||
                    comboCategory.SelectedItem == null || string.IsNullOrEmpty(txtISBN.Text) ||
                    calDate.SelectedDate == null || string.IsNullOrEmpty(txtCount.Text))
                {
                    MessageBox.Show("Bitte füllen Sie alle Felder aus");
                    return;
                }

                // Neues Buch erstellen
                var newBook = new Entities.Book
                {
                    Name = txtTitle.Text,
                    AuthorID = selectedAuthor,
                    CategoryeID = selectedCategory,
                    ISBN = txtISBN.Text,
                    ReleaseDate = (DateTime)calDate.SelectedDate,
                    IsAvailable = true
                };

                using (var db = new Bibliothek_Content())
                {
                    db.Book.Add(newBook);
                    await db.SaveChangesAsync();

                    var countOfBooks = new CountOfBooks
                    {
                        BookID = newBook.ID,
                        CountOfBook = int.Parse(txtCount.Text)
                    };
                    db.CountOfBooks.Add(countOfBooks);
                    await db.SaveChangesAsync();
                }

                Cleanup();
                MessageBox.Show("Das Buch wurde erfolgreich hinzugefügt!");

                await LoadBooks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Hinzufügen des Buches: " + ex.Message);
            }
        }

        // Aktualisiert ein bestehendes Buch
        private async void UpdateBook(BookModel book)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtTitle.Text) && comboAuthor.SelectedItem != null &&
                    comboCategory.SelectedItem != null && !string.IsNullOrEmpty(txtISBN.Text) &&
                    calDate.SelectedDate != null && !string.IsNullOrEmpty(txtCount.Text))
                {
                    using (var db = new Bibliothek_Content())
                    {
                        var existingBook = db.Book.FirstOrDefault(b => b.ID == book.Reihe);
                        if (existingBook != null)
                        {
                            existingBook.Name = txtTitle.Text;
                            existingBook.AuthorID = (int)((Label)comboAuthor.SelectedItem).Tag;
                            existingBook.CategoryeID = (int)((Label)comboCategory.SelectedItem).Tag;
                            existingBook.ISBN = txtISBN.Text;
                            existingBook.ReleaseDate = (DateTime)calDate.SelectedDate;

                            var countOfBooks = db.CountOfBooks.FirstOrDefault(c => c.BookID == existingBook.ID);
                            if (countOfBooks != null)
                            {
                                countOfBooks.CountOfBook = int.Parse(txtCount.Text);
                            }

                            await db.SaveChangesAsync();
                            Cleanup();
                            MessageBox.Show("Das Buch wurde erfolgreich aktualisiert!");

                            // Aktualisierung der Bücherliste
                            await LoadBooks();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Bitte füllen Sie alle Felder aus.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler beim Aktualisieren des Buches: " + ex.Message);
            }
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Anwenden des Suchfilters wenn der Suchbutton geklickt wird
            ApplySearchFilter();
        }

        private void onSearchChange(object sender, TextChangedEventArgs e)
        {
            //  Suchfilters bei änderungen im Suchtextfeld
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            // Filtern der Bücher basierend auf dem Suchtext
            string searchText = SearchTextBox.Text.ToLower();
            var filteredBooks = books.Where(b => b.Title.ToLower().Contains(searchText) ||
                                                 b.Author.ToLower().Contains(searchText) ||
                                                 b.Category.ToLower().Contains(searchText) ||
                                                 b.ISBN.ToLower().Contains(searchText)
            ).ToList();
            // Setzen der gefilterten Bücher als Datenquelle für das DataGrid
            BooksDataGrid.ItemsSource = filteredBooks;
        }

        private void BuchenButton_Click(object sender, RoutedEventArgs e)
        {

            if (selectedBook != null)
            {
                MessageBoxResult result = MessageBox.Show("Möchten Sie mit der Buchreservierung fortfahren?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {

                }
                else
                {
                    // Logik wenn der Benutzer auf Nein klickt (nichts tun)
                }
            }
            else
            {

                MessageBox.Show("Bitte wählen Sie ein Buch aus.", "Warnung", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void onAuthorChangeItem(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox; // sender in ComboBox umwandeln
            if (combo.SelectedItem is Label selectedLabel)
            {
                string authorName = selectedLabel.Content.ToString(); // Zugriff auf den Namen des Autors
                int authorID = (int)selectedLabel.Tag;

                selectedAuthor = authorID;
            }
        }

        private void onKategorieChangeItem(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if (combo.SelectedItem is Label selectedLabel)
            {
                string kategorieName = selectedLabel.Content.ToString();
                int kategorieID = (int)selectedLabel.Tag;

                selectedCategory = kategorieID;
            }
        }

        public void Cleanup()
        {
            txtTitle.Text = string.Empty;
            txtISBN.Text = string.Empty;
            txtCount.Text = string.Empty;
            comboAuthor.SelectedIndex = -1;
            comboCategory.SelectedIndex = -1;
            calDate.SelectedDate = DateTime.Now;
        }


        private void numericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

            e.Handled = !IsTextAllowed(e.Text);
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }


        private void NumericTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private static bool IsTextAllowed(string text)
        {
            //nur zahlen
            return Regex.IsMatch(text, "^[0-9]+$");
        }
    }
}
