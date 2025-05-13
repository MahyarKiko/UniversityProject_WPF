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
    /// Interaction logic for TypeOfUsers.xaml
    /// </summary>
    public partial class TypeOfUsers : Window
    {
        // Liste zur Speicherung der Benutzertypen
        List<UserTypeModel> types;
        // Modell für den aktuell ausgewählten Benutzertyp
        UserTypeModel selectedType;

        public TypeOfUsers()
        {
            InitializeComponent();
            types = new List<UserTypeModel>(); 
            selectedType = new UserTypeModel();
            getUserType(); 
        }

      
        private void OnAddOrEditUserType(object sender, RoutedEventArgs e)
        {
            try
            {
                addorEditUserTypeTextBox.Text = ""; // Setzt den Text im Eingabefeld zurück
                searchUserTypeTextBox.Text = ""; 

                if (!(bool)tglBtn_addOrEditUserType.IsChecked)
                {
                    addOrEditUserTypeBtn.Content = "Hinzufügen"; 
                }
                else
                {
                    addOrEditUserTypeBtn.Content = "Bearbeiten";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
            }
        }

        // Hinzufügen oder Bearbeiten eines Benutzertyps
        private async void onUserTypeAddOrEditClicked(object sender, RoutedEventArgs e)
        {
            if (!(bool)tglBtn_addOrEditUserType.IsChecked)
            {
                await AddUserType(); // Ruft die Methode zum Hinzufügen eines Benutzertyps auf
            }
            else
            {
                await UpdateUserType(selectedType, addorEditUserTypeTextBox.Text); // Ruft die Methode zum Bearbeiten eines Benutzertyps auf
            }
        }

       
        private async Task getUserType()
        {
            Bibliothek_Content db = new Bibliothek_Content();

            // Lädt die Benutzertypen aus der Datenbank
            types = await db.UserType.Select(t => new UserTypeModel()
            {
                ID = t.ID,
                Type = t.Type
            }).ToListAsync();

            if (types.Count > 0)
            {
                UserTypesDataGrid.ItemsSource = types; 
            }
            else
            {
                MessageBox.Show("Keine Benutzertypen verfügbar"); 
            }
        }

        //  Behandeln der Auswahländerung im DataGrid
        private void UserTypeDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((bool)tglBtn_addOrEditUserType.IsChecked)
            {
                if (UserTypesDataGrid.SelectedItem != null)
                {
                    selectedType = (UserTypeModel)UserTypesDataGrid.SelectedItem; // Setzt den aktuell ausgewählten Benutzertyp
                }
                else
                {
                    selectedType = null; // Setzt den ausgewählten Typ auf null, wenn nichts ausgewählt ist
                }
            }
        }

        
        private async Task AddUserType()
        {
            Bibliothek_Content db = new Bibliothek_Content();
            if (addorEditUserTypeTextBox.Text != "")
            {
                // Überprüft, ob der Typ bereits existiert
                var isExist = await db.Authors.AnyAsync(t => addorEditUserTypeTextBox.Text == t.Fullname);

                if (!isExist)
                {
                    MessageBoxResult result = MessageBox.Show("Möchten Sie diesen Benutzertyp hinzufügen?", "Bestätigung", MessageBoxButton.YesNo);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        
                        var addAuthor = await db.UserType.AddAsync(new UserType()
                        {
                            Type = addorEditUserTypeTextBox.Text
                        });
                        await db.SaveChangesAsync();

                        if (addAuthor != null)
                        {
                            addorEditUserTypeTextBox.Text = ""; 
                            searchUserTypeTextBox.Text = ""; 
                            await getUserType(); 
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Dieser Type existiert schon"); 
                }
            }
            else
            {
                MessageBox.Show("Bitte tragen Sie den Namen des Benutzertyps ein"); 
            }
        }

        //  Bearbeiten eines bestehenden Benutzertyps
        private async Task UpdateUserType(UserTypeModel type, string newType)
        {
            try
            {
                Bibliothek_Content db = new Bibliothek_Content();

                if (addorEditUserTypeTextBox.Text != "")
                {
                    if (selectedType != null)
                    {
                        MessageBoxResult result = MessageBox.Show("Möchten Sie Änderungen an diesem Benutzertyp vornehmen?", "Bestätigung", MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Sucht den Benutzertyp in der Datenbank und aktualisiert ihn
                            var updateType = await db.UserType.FirstOrDefaultAsync(t => type.ID == t.ID);
                            if (updateType != null)
                            {
                                updateType.Type = newType;
                                await db.SaveChangesAsync();
                                addorEditUserTypeTextBox.Text = "";
                                searchUserTypeTextBox.Text = ""; 
                                await getUserType(); 
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Bitte wählen Sie den Benutzertyp aus"); 
                    }
                }
                else
                {
                    MessageBox.Show("Bitte geben Sie zunächst den neuen Benutzertyp ein");
                }
            }
            catch (DbUpdateException ex)
            {
                // Zeigt eine Fehlermeldung bei einem Datenbank Update Fehler an
                var innerException = ex.InnerException?.Message;
                MessageBox.Show($"An error occurred while updating the entries: {innerException}");
            }
        }

        //  Anwenden des Suchfilters auf die Liste der Benutzertypen
        private void ApplySearchFilter()
        {
            string searchText = searchUserTypeTextBox.Text.ToLower();

           
            var filteredAuthors = types.Where(b => b.Type.ToLower().Contains(searchText)).ToList();
            UserTypesDataGrid.ItemsSource = filteredAuthors; // Setzt die Datenquelle des DataGrids auf die gefilterten Ergebnisse
        }

       
        private void onUserTypeSearchChange(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter(); 
        }
    }
}