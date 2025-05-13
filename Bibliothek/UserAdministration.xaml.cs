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
    /// Interaction logic for UserAdministration.xaml
    /// </summary>
    public partial class UserAdministration : Window
    {
        
        List<UserModel> users;
        
        UserModel selectedUser;
       
        bool? selectedViewType = null;
       
        List<bool> comboValue;
        
        bool convertStatusTo;

        
        public UserAdministration()
        {
            InitializeComponent();
           
            users = new List<UserModel>();
            selectedUser = new UserModel();
           
            comboValue = new List<bool>() { true, false };

           
            getUser();

          
            rb_All.IsChecked = true;
        }

        //  Aktualisieren des Benutzerstatus
        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            // Bestaetigungsnachricht anzeigen
            MessageBoxResult result = MessageBox.Show("Möchten Sie diese Aktion fortsetzen?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // Wenn der Benutzer die Anfrage bestaetigt, wird der Status aktualisiert
            if (result == MessageBoxResult.Yes)
            {
                UpdateUserStatus();
            }
        }

        //  Abrufen der Benutzer aus der Datenbank
        private async Task getUser()
        {
            
            Bibliothek_Content db = new Bibliothek_Content();
            
            users = await (from u in db.User
                           join t in db.UserType on u.UserTypeID equals t.ID
                           select new UserModel()
                           {
                               ID = u.ID,
                               FirstName = u.FirstName ,
                               LastName = u.LastName,
                               Email = u.Email,
                               CellPhone = u.CellPhone,
                               Address = u.Address,
                               Country = u.Country,
                               PostalCode = (int)u.PostalCode,
                               City= u.City,
                               UserType = t.Type,
                               IsActive = u.IsActive
                           }
                           ).ToListAsync();

            // Überprüfen, ob Benutzer vorhanden sind
            if (users.Count > 0)
            {
               
                UsersDataGrid.ItemsSource = users;
            }
            else
            {
                
                MessageBox.Show("Es gibt keinen Benutzer");
            }
        }

        
        private async Task UpdateUserStatus()
        {
            // Datenbank Kontext erstellen
            Bibliothek_Content db = new Bibliothek_Content();

            // Benutzer aus der Datenbank abrufen  der aktualisiert werden soll
            var updateUser = await db.User.FirstOrDefaultAsync(t => selectedUser.ID == t.ID);
            if (updateUser != null)
            {
                
                updateUser.IsActive = (bool)!selectedUser.IsActive;
                await db.SaveChangesAsync();
               
                SearchTextBox.Text = "";
                getUser();
            }
        }

        // Methode, die aufgerufen wird, wenn sich der Suchtext ändert
        private void onSearchChange(object sender, TextChangedEventArgs e)
        {
            
            ApplySearchFilter();
        }

       
        private void onViewTypeChange(object sender, RoutedEventArgs e)
        {
            // Ansichtstyp basierend auf dem ausgewählten Radio Button setzen
            if (rb_All.IsChecked.Value) selectedViewType = null;
            if (rb_Active.IsChecked.Value) selectedViewType = true;
            if (rb_Deactive.IsChecked.Value) selectedViewType = false;

           
            ApplySearchFilter();
        }

        
        private void ApplySearchFilter()
        {
            // Suchtext in Kleinbuchstaben konvertieren
            string searchText = SearchTextBox.Text.ToLower();
            List<UserModel> filteredBooks = new List<UserModel>();

            // Filter basierend auf dem ausgewählten Ansichtstyp anwenden
            if (selectedViewType != null)
            {
                filteredBooks = users.Where(b => (b.FirstName .ToLower().Contains(searchText) ||
                                        b.LastName.ToLower().Contains(searchText) ||
                                        b.UserType.ToLower().Contains(searchText) ||
                                        b.Email.ToLower().Contains(searchText)) &&
                                        b.IsActive == selectedViewType
                                        ).ToList();
            }
            else
            {
                filteredBooks = users.Where(b => b.FirstName.ToLower().Contains(searchText) ||
                                     b.LastName.ToLower().Contains(searchText) ||
                                     b.UserType.ToLower().Contains(searchText) ||
                                     b.Email.ToLower().Contains(searchText)
                                     ).ToList();
            }

         
            UsersDataGrid.ItemsSource = filteredBooks;
        }

       
        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Überprüfen, ob ein Benutzer ausgewählt ist
            if (UsersDataGrid.SelectedItem != null)
            {
              
                selectedUser = (UserModel)UsersDataGrid.SelectedItem;
               
                btn_UpdateStatus.IsEnabled = true;
               
                string toActiveUser = "Aktivieren des Benutzerstatus", toDeactiveUser = "Änderung des Status zu Deaktiv";
                if (selectedUser.IsActive)
                {
                    btn_UpdateStatus.Content = toDeactiveUser;
                }
                else
                {
                    btn_UpdateStatus.Content = toActiveUser;
                }
            }
            else
            {
                // Wenn kein Benutzer ausgewählt ist, den ausgewählten Benutzer auf null setzen
                selectedUser = null;
                // Schaltfläche zur Statusaktualisierung deaktivieren
                btn_UpdateStatus.IsEnabled = false;
            }
        }
    }
}
