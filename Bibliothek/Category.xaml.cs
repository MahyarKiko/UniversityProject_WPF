using Bibliothek.Entities;
using Bibliothek.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for Kategorie.xaml
    /// </summary>
    public partial class Category : Window
    {
        // Liste zur Speicherung der Kategorien
        List<CategoryModel> kategories;
        // Ausgewählte Kategorie für Bearbeitungen
        CategoryModel selectedKategorie;

        public Category()
        {
            InitializeComponent();

            // Initialisieren der Kategorienliste und ausgewählten Kategorie
            kategories = new List<CategoryModel>();
            selectedKategorie = new CategoryModel();
            // Kategorien aus der Datenbank abrufen
            getKategori();
        }

        
        private void OnAddOrEditKategorie(object sender, RoutedEventArgs e)
        {
            try
            {
                // Textfelder leeren
                addorEditKAtegorieTextBox.Text = "";
                searchKategorieTextBox.Text = "";

                // Überprüfen, ob der Toggle Button zum Bearbeiten oder Hinzufügen verwendet wird
                if (!(bool)tglBtn_addOrEditKategorie.IsChecked)
                {
                    // Wenn nicht bearbeitend, Button-Text auf hinzufügen setzen
                    addOrEditKategorieBtn.Content = "Hinzufügen";
                }
                else
                {
                    // Wenn bearbeitend, Button-Text auf bearbeiten 
                    addOrEditKategorieBtn.Content = "Bearbeiten";
                }
            }
            catch (Exception ex)
            {
                
                MessageBox.Show(ex.Message);
            }
        }

        // Hinzufügen oder Bearbeiten einer Kategorie bei Button Klick
        private async void onAddOrEditKategorieClicked(object sender, RoutedEventArgs e)
        {
            
            if (!(bool)tglBtn_addOrEditKategorie.IsChecked)
            {
                
                await AddKategorie();
            }
            else
            {
                
                await UpdateKategorie(selectedKategorie, addorEditKAtegorieTextBox.Text);
            }
        }

        // Abrufen der Kategorien aus der Datenbank
        private async Task getKategori()
        {
            
            Bibliothek_Content db = new Bibliothek_Content();

            // Abrufen der Kategorien aus der Datenbank
            kategories = await db.Category.Select(t => new CategoryModel()
            {
                ID = t.ID,
                Value = t.Value
            }).ToListAsync();

            // Überprüfen, ob Kategorien vorhanden sind
            if (kategories.Count > 0)
            {
               
                kategoriesDataGrid.ItemsSource = kategories;
            }
            else
            {
                MessageBox.Show("Kategorie nicht gefunden");
            }
        }

        
        private void KategorieDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if ((bool)tglBtn_addOrEditKategorie.IsChecked)
            {
                if (kategoriesDataGrid.SelectedItem != null)
                {
                    
                    selectedKategorie = (CategoryModel)kategoriesDataGrid.SelectedItem;
                }
                else
                {
                    
                    selectedKategorie = null;
                }
            }
        }

        //  Hinzufügen einer neuen Kategorie
        private async Task AddKategorie()
        {
            
            Bibliothek_Content db = new Bibliothek_Content();
            if (addorEditKAtegorieTextBox.Text != "")
            {
                // Überprüfen, ob die Kategorie bereits existiert
                var isExist = await db.Category.AnyAsync(t => addorEditKAtegorieTextBox.Text == t.Value);

                if (!isExist)
                {
                  
                    MessageBoxResult result = MessageBox.Show("Möchten Sie diese Kategorie hinzufügen?", "Bestätigung", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Kategorie zur Datenbank hinzufügen
                        var addAuthor = await db.Category.AddAsync(new Entities.Category()
                        {
                            Value = addorEditKAtegorieTextBox.Text
                        });
                        await db.SaveChangesAsync();

                        if (addAuthor != null)
                        {
                            
                            addorEditKAtegorieTextBox.Text = "";
                            searchKategorieTextBox.Text = "";
                            await getKategori();
                        }
                    }
                }
                else
                {
                    
                    MessageBox.Show("Diese Kategorie existiert bereits.");
                }
            }
            else
            {
               
                MessageBox.Show("Bitte geben Sie den Namen der Kategorie ein");
            }
        }

        //  Aktualisieren einer Kategorie
        private async Task UpdateKategorie(CategoryModel kategorie, string newValue)
        {
            try
            {
                
                Bibliothek_Content db = new Bibliothek_Content();

                if (addorEditKAtegorieTextBox.Text != "")
                {
                    if (selectedKategorie != null)
                    {
                        // Bestätigungsnachricht anzeigen
                        MessageBoxResult result = MessageBox.Show("Möchten sie die Kategorie bearbeiten?", "Bestätigung", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Kategorie in der Datenbank finden und aktualisieren
                            var updateKategorie = await db.Category.FirstOrDefaultAsync(t => kategorie.ID == t.ID);
                            if (updateKategorie != null)
                            {
                                updateKategorie.Value = newValue;
                                await db.SaveChangesAsync();
                               
                                addorEditKAtegorieTextBox.Text = "";
                                searchKategorieTextBox.Text = "";
                                getKategori();
                            }
                        }
                    }
                    else
                    {
                      
                        MessageBox.Show("Bitte wählen Sie einen Autor aus");
                    }
                }
                else
                {
                    
                    MessageBox.Show("Bitte geben Sie zuerst den neuen Namen ein");
                }
            }
            catch (DbUpdateException ex)
            {
                // Fehlermeldung anzeigen, falls ein Fehler beim Aktualisieren auftritt
                var innerException = ex.InnerException?.Message;
                MessageBox.Show($"Beim Aktualisieren der Einträge ist ein Fehler aufgetreten: {innerException}");
            }
        }

        //  Anwenden eines Suchfilters auf die Kategorien
        private void ApplySearchFilter()
        {
            string searchText = searchKategorieTextBox.Text.ToLower();

            // Filtere die Kategorien basierend auf dem Suchtext
            var filteredKategorie = kategories.Where(b => b.Value.ToLower().Contains(searchText)).ToList();
            kategoriesDataGrid.ItemsSource = filteredKategorie;
        }

       
        private void onKategorieSearchChange(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }
    }
}



