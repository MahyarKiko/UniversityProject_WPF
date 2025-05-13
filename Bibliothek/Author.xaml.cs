using Bibliothek.Entities;
using Bibliothek.Model;
using Bibliothek.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Bibliothek
{
    /// <summary>
    /// Interaktionslogik für das Fenster "Autor"
    /// </summary>
    public partial class Author : Window
    {
        // Liste zur speicherung von Autor Objekten
        List<AuthorModel> autrhors;
        // Ausgewählter Autor
        AuthorModel selectedAuthor;

        // Konstruktor des Fensters
        public Author()
        {
            InitializeComponent();
            autrhors = new List<AuthorModel>();
            selectedAuthor = new AuthorModel();
            getAuthors();  // Autoren aus der Datenbank laden
        }

        // Event-Handler für das Hinzufügen und Bearbeiten des Autors
        private void OnAddOrEditAuthor(object sender, RoutedEventArgs e)
        {
            try
            {
                // Eingabefelder zurücksetzen
                addorEditAuthorTextBox.Text = "";
                searchAuthorTextBox.Text = "";

                // Überprüfen, ob der Toggle-Button aktiviert ist, um zwischen Hinzufügen und Bearbeiten zu wechseln
                if (!(bool)tglBtn_addOrEditAuthor.IsChecked)
                {
                    addOrEditAuthorBtn.Content = "Hinzufügen";  // Button-Text auf "Hinzufügen" setzen
                }
                else
                {
                    addOrEditAuthorBtn.Content = "Bearbeiten";  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);  
            }
        }

        // Event-Handler, der ausgeführt wird, wenn der Benutzer den Hinzufügen- oder Bearbeiten-Button klickt
        private async void onAuthorAddOrEditClicked(object sender, RoutedEventArgs e)
        {
            // Überprüfen, ob der Toggle-Button aktiviert ist, um zwischen Hinzufügen und Bearbeiten zu unterscheiden
            if (!(bool)tglBtn_addOrEditAuthor.IsChecked)
            {
                await AddAuthor();  // Neuen Autor hinzufügen
            }
            else
            {
                await UpdateAuthor(selectedAuthor, addorEditAuthorTextBox.Text);  // Autor bearbeiten
            }
        }

        // Abrufen der Autoren aus der Datenbank
        private async Task getAuthors()
        {
            Bibliothek_Content db = new Bibliothek_Content();  // Datenbankkontext initialisieren

            // Autoren aus der Datenbank laden und in der Liste speichern
            autrhors = await db.Authors.Select(t => new AuthorModel()
            {
                ID = t.ID,
                FullName = t.Fullname
            }).ToListAsync();

            // Überprüfen, ob Autoren vorhanden sind und diese im DataGrid anzeigen
            if (autrhors.Count > 0)
            {
                AuthorsDataGrid.ItemsSource = autrhors;
            }
            else
            {
                MessageBox.Show("Es gibt keine Autoren");
            }
        }

        // Event-Handler, der ausgelöst wird, wenn die Auswahl im DataGrid geändert wird
        private void AuthorDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Überprüfen ob der Toggle-Button für Bearbeiten aktiviert ist und ein Autor ausgewählt wurde
            if ((bool)tglBtn_addOrEditAuthor.IsChecked)
            {
                if (AuthorsDataGrid.SelectedItem != null)
                {
                    // Den ausgewählten Autor speichern
                    selectedAuthor = (AuthorModel)AuthorsDataGrid.SelectedItem;
                }
                else
                {
                    selectedAuthor = null;  // Wenn nichts ausgewählt ist, den ausgewählten Autor zurücksetzen
                }
            }
        }

        //  Hinzufügen eines neuen Autors zur Datenbank
        private async Task AddAuthor()
        {
            Bibliothek_Content db = new Bibliothek_Content();  
            if (addorEditAuthorTextBox.Text != "")
            {
                // Überprüfen, ob der Autor bereits existiert
                var isExist = await db.Authors.AnyAsync(t => addorEditAuthorTextBox.Text == t.Fullname);

                if (!isExist)
                {
                    
                    MessageBoxResult result = MessageBox.Show("Möchten Sie den Namen hinzufügen?", "Bestätigung", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Autor zur Datenbank hinzufügen
                        var addAuthor = await db.Authors.AddAsync(new Authors()
                        {
                            Fullname = addorEditAuthorTextBox.Text
                        });
                        await db.SaveChangesAsync();

                        // Eingabefelder zurücksetzen und Liste der Autoren aktualisieren
                        if (addAuthor != null)
                        {
                            addorEditAuthorTextBox.Text = "";
                            searchAuthorTextBox.Text = "";
                            await getAuthors();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Dieser Autor existiert bereits");
                }
            }
            else
            {
                MessageBox.Show("Geben Sie den Namen des Autors an");
            }
        }

        //  Bearbeiten eines existierenden Autors in der Datenbank
        private async Task UpdateAuthor(AuthorModel author, string newVollName)
        {
            try
            {
                Bibliothek_Content db = new Bibliothek_Content();  

                if (addorEditAuthorTextBox.Text != "")
                {
                    if (selectedAuthor != null)
                    {
                        
                        MessageBoxResult result = MessageBox.Show("Möchten Sie den Namen bearbeiten?", "Bestätigung", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Den Autor in der Datenbank finden und aktualisieren
                            var updateAuthor = await db.Authors.FirstOrDefaultAsync(t => author.ID == t.ID);
                            if (updateAuthor != null)
                            {
                                updateAuthor.Fullname = newVollName;
                                await db.SaveChangesAsync();
                                addorEditAuthorTextBox.Text = "";
                                searchAuthorTextBox.Text = "";
                                getAuthors();  // Liste der Autoren aktualisieren
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wählen Sie den Autor aus");
                    }
                }
                else
                {
                    MessageBox.Show("Geben Sie zuerst den neuen Namen ein");
                }
            }
            catch (DbUpdateException ex)
            {
                // Interne Ausnahme abfangen und anzeigen
                var innerException = ex.InnerException?.Message;
                MessageBox.Show($"Ein Fehler ist beim Aktualisieren aufgetreten: {innerException}");
            }
        }

        //  Anwenden eines Suchfilters auf die Liste der Autoren
        private void ApplySearchFilter()
        {
            string searchText = searchAuthorTextBox.Text.ToLower();  // Suchtext in Kleinbuchstaben umwandeln

            // Liste der Autoren nach dem Suchtext filtern
            var filteredAuthors = autrhors.Where(b => b.FullName.ToLower().Contains(searchText)).ToList();
            AuthorsDataGrid.ItemsSource = filteredAuthors;  // Gefilterte Liste im DataGrid anzeigen
        }

        // Event-Handler, der ausgeführt wird, wenn sich der Text in der Suchleiste ändert
        private void onAuthorSearchChange(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();  // Suchfilter anwenden
        }
    }
}
