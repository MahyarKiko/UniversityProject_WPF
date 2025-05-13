// Enthält die benötigten Namespaces und Bibliotheken
using Bibliothek.Entities;
using Bibliothek.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using static Azure.Core.HttpHeader;

namespace Bibliothek
{
    /// <summary>
    /// Interaktionslogik für die Rechte.xaml
    /// </summary>
    public partial class Permission : Window
    {
       
        List<UserType> userTypes;
        List<UserModel> users;
        UserModel selectedUser;
        int selectedTypeId = -1;

        public Permission()
        {
            InitializeComponent();
            userTypes = new List<UserType>();
            users = new List<UserModel>();
            selectedUser = new UserModel();
            FetchUserTypeComboboxItem(); 
            getUser(); 
        }

        // Lädt die Menüs basierend auf dem Benutzertyp und fügt sie dem Layout hinzu
        private async Task LoadMeneus(int userType)
        {
            Bibliothek_Content db = new Bibliothek_Content();
            layout_Menus.Children.Clear(); // Entfernt alle vorhandenen Elemente aus dem Layout
            layout_MenusBorder.Visibility = Visibility.Collapsed;
            var menuAcc = await (from m in db.Menu
                                 join t in db.TypeToMenuAccess on m.ID equals t.MenuID
                                 join u in db.UserType on t.TypeID equals u.ID
                                 where u.ID == userType
                                 select new
                                 {
                                     typeId = t.TypeID,
                                     type = u.Type,
                                     manuName = m.MenuName,
                                     menuAccess = t.IsActive,
                                 }).ToListAsync();

            if (menuAcc.Count > 0)
            {
                layout_MenusBorder.Visibility = Visibility.Visible;
                for (int i = 0; i < menuAcc.Count; i++)
                {
                    var item = new CheckBox();
                    item.FontSize = 21;
                    item.Content = menuAcc[i].manuName;
                    item.IsChecked = menuAcc[i].menuAccess;
                    item.Checked += onPermissionChange;
                    item.Unchecked += onPermissionChange;

                    // Deaktiviert die Checkbox für Administratoren
                    item.IsEnabled = menuAcc[i].type != "Administrator";
                    layout_Menus.Children.Add(item);
                }
            }
        }

       
        private async Task LoadUserMeneus(int userId)
        {
            Bibliothek_Content db = new Bibliothek_Content();
            layout_Menus.Children.Clear(); 

            // Lädt Menüs, die für den Benutzer nicht aktiv sind
            var menuAcc = await (from m in db.Menu
                                 join t in db.TypeToMenuAccess on m.ID equals t.MenuID
                                 join u in db.UserType on t.TypeID equals u.ID
                                 where u.Type == "User" && !t.IsActive
                                 select new
                                 {
                                     typeId = t.TypeID,
                                     type = u.Type,
                                     menuId = m.ID,
                                     menuName = m.MenuName,
                                     menuAccess = t.IsActive,
                                 }).ToListAsync();

            // Lädt die Berechtigungen des Benutzerss
            var userAcc = await (from m in db.Menu
                                 join r in db.UserPermission on m.ID equals r.MenuID
                                 where r.UserID == userId
                                 select new
                                 {
                                     menuId = m.ID,
                                     menuName = m.MenuName,
                                     menuAccess = r.IsAccept
                                 }).ToListAsync();

            if (menuAcc.Count > 0)
            {
                layout_UsersPermissions.Children.Clear(); // Entfernt alle vorhandenen Elemente aus dem Layout
                for (int i = 0; i < menuAcc.Count; i++)
                {
                    var item = new CheckBox();
                    item.FontSize = 21;
                    item.Content = menuAcc[i].menuName;
                    item.Checked += onUserPermissionChange;
                    item.Unchecked += onUserPermissionChange;

                    // Setzt den Status der Checkbox basierend auf den Benutzerberechtigungen
                    for (int j = 0; j < userAcc.Count; j++)
                    {
                        if (menuAcc[i].menuId == userAcc[j].menuId)
                        {
                            bool? newVal = userAcc[j].menuAccess;
                            item.IsChecked = newVal;
                        }
                    }

                    layout_UsersPermissions.Children.Add(item);
                }
            }
        }

       
        private async Task getUser()
        {
            Bibliothek_Content db = new Bibliothek_Content();
            users = await (from u in db.User
                           join t in db.UserType on u.UserTypeID equals t.ID
                           where u.IsActive && t.ID == 2
                           select new UserModel()
                           {
                               ID = u.ID,
                               FirstName = u.FirstName,
                               LastName = u.LastName,
                               Email = u.Email,
                               CellPhone = u.CellPhone,
                               Address = u.Address,
                               Country = u.Country,
                               PostalCode = (int)u.PostalCode,
                               City = u.City,
                               UserType = t.Type,
                               IsActive = u.IsActive
                           }).ToListAsync();

            // Zeigt eine Nachricht an, wenn keine Benutzer gefunden wurden
            if (users.Count > 0)
            {
                UsersDataGrid.ItemsSource = users;
            }
            else
            {
                MessageBox.Show("Keine Benutzer gefunden");
            }
        }

        // Event Handler für Textänderungen im Suchfeld
        private void onSearchChange(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        // Event Handler für Änderungen im Benutzer Typ Kombinationsfeld
        private void onViewTypeChange(object sender, RoutedEventArgs e)
        {
            ApplySearchFilter(); 
        }

        // Wendet den Suchfilter auf die Benutzerliste an
        private void ApplySearchFilter()
        {
            layout_UsersPermissions.Children.Clear(); 
            UsersDataGrid.SelectedItem = null;
            string searchText = txt_userTypeSuchen.Text.ToLower();
            List<UserModel> filteredUser = new List<UserModel>();

            // Filtert die Benutzerliste basierend auf dem Suchtext
            filteredUser = users.Where(b => (b.FirstName.ToLower().Contains(searchText) ||
                                       b.LastName.ToLower().Contains(searchText) ||
                                       b.UserType.ToLower().Contains(searchText) ||
                                       b.Email.ToLower().Contains(searchText)) &&
                                       b.IsActive == true
                                       ).ToList();

            UsersDataGrid.ItemsSource = filteredUser;
        }

        
        public async Task FetchUserTypeComboboxItem()
        {
            try
            {
                Bibliothek_Content db = new Bibliothek_Content();
                userTypes = await db.UserType.ToListAsync();

                if (userTypes.Count > 0 && userTypes != null)
                {
                    foreach (var item in userTypes)
                    {
                        comboUserType.Items.Add(item.Type); // Fügt jeden Benutzertyp dem Kombinationsfed hinzu
                    }
                }
            }
            catch (Exception e)
            {
               
                MessageBox.Show(e.Message);
            }
        }

        // Event Handler für das Klicken auf den Button (User zu Admin)
        private void OnUserToAdminClick(object sender, RoutedEventArgs e)
        {
            // Bestätigungsdialog für die Änderung
            MessageBoxResult result = MessageBox.Show("Sind Sie sicher, dass Sie den Benutzer zum Admin ändern möchten?", "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Speichert die Änderungen und fordert die Umwandlung des Benutzers in einen Administrator an
                PromptToAdminUser();
            }
        }

        //----------------------------------------------------------------------------------------------------------------

        // Speichern der Änderungen der Benutzerberechtigungen in der Datenbank
        private void SaveUserPermissionsChanges()
        {
            Bibliothek_Content db = new Bibliothek_Content();

            foreach (var child in layout_UsersPermissions.Children)
            {
                if (child is CheckBox checkbox)
                {
                    int menuId = getMenuId(checkbox.Content.ToString());

                    // Prüft, ob die Berechtigung bereits existiert
                    var existingPermission = db.UserPermission.FirstOrDefault(t => t.UserID == selectedUser.ID && t.MenuID == menuId);
                    if (existingPermission != null)
                    {
                        existingPermission.IsAccept = checkbox.IsChecked ?? false;
                    }
                    else
                    {
                        // Wenn die Berechtigung nicht existiert, wird sie hinzugefügt
                        UserPermission newPermission = new UserPermission()
                        {
                            UserID = selectedUser.ID,
                            MenuID = menuId,
                            IsAccept = checkbox.IsChecked ?? false
                        };
                        db.UserPermission.Add(newPermission);
                    }
                }
            }

            db.SaveChanges(); 
                              
            MessageBox.Show("Die Änderungen wurden erfolgreich gespeichert");
        }

        private void OnComboChange(object sender, SelectionChangedEventArgs e)
        {
            // Überprüft, ob ein gültiger Benutzer Typ ausgewählt wurde
            if (comboUserType.SelectedIndex != -1)
            {
                var selectedID = userTypes.Where(x => x.Type.Equals(comboUserType.SelectedItem.ToString())).FirstOrDefault();
                if (selectedID != null)
                {
                    selectedTypeId = selectedID.ID; 
                    LoadMeneus(selectedTypeId); 
                }
            }
        }

        //  Aktualisieren der Berechtigung für einen bestimmten Benutzertyp und ein Menü
        public void UpdateUserTypePermission(int typeID, int menuID)
        {
            Bibliothek_Content db = new Bibliothek_Content();
            var res = db.TypeToMenuAccess.FirstOrDefault(t => t.TypeID == typeID && t.MenuID == menuID);

            if (res != null)
            {
                res.IsActive = !res.IsActive; 
                db.SaveChanges(); // Speichert die Änderungen in der Datenbank
                MessageBox.Show("Die Änderungen wurden erfolgreich gespeichert");
            }
        }

        
        public int getMenuId(string menuName)
        {
            Bibliothek_Content db = new Bibliothek_Content();
            var menu = db.Menu.FirstOrDefault(t => t.MenuName == menuName);
            return menu != null ? menu.ID : -1; // Gibt die ID des Menüs zurück oder -1, wenn nicht gefunden
        }

        
        private void onPermissionChange(object sender, RoutedEventArgs e)
        {
            var item = sender as CheckBox;
            int menuId = getMenuId(item.Content.ToString()); // Holt die Menü ID basierend auf dem Namen
            UpdateUserTypePermission(selectedTypeId, menuId); // Aktualisiert die Berechtigung für den ausgewählten Benutzertyp
        }

        
        private void onUserPermissionChange(object sender, RoutedEventArgs e)
        {
            var item = sender as CheckBox;
            int menuId = getMenuId(item.Content.ToString()); // Holt die Menü ID basierend auf dem Namen

            Bibliothek_Content db = new Bibliothek_Content();
            var ex = db.UserPermission.FirstOrDefault(t => selectedUser.ID == t.UserID && menuId == t.MenuID);

            if (ex != null)
            {
                // Wenn die Berechtigung existiert und sich geändert hat, aktualisieren
                if (ex.IsAccept != item.IsChecked)
                {
                    ex.IsAccept = (bool)item.IsChecked;
                    db.SaveChanges(); 
                    MessageBox.Show("Die Berechtigung wurde erfolgreich geändert."); 
                }
            }
            else
            {
                
                var np = db.UserPermission.Add(new UserPermission
                {
                    UserID = selectedUser.ID,
                    MenuID = menuId,
                    IsAccept = (bool)item.IsChecked
                });
                db.SaveChanges(); 

                MessageBox.Show("Die Zugriffsrechte wurden erfolgreich hinzugefügt"); 
            }
        }

      
        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            btnUserToAdmin.IsEnabled = UsersDataGrid.SelectedItem != null;

            if (UsersDataGrid.SelectedItem != null)
            {
                selectedUser = (UserModel)UsersDataGrid.SelectedItem; 
                LoadUserMeneus(selectedUser.ID); 
            }
            else
            {
                selectedUser = null; 
            }
        }

        //  Umwandlung eines Benutzers in einen Administrator
        private async Task PromptToAdminUser()
        {
            Bibliothek_Content db = new Bibliothek_Content();

           
            var user = await db.User.FirstOrDefaultAsync(t => t.ID == selectedUser.ID);
            if (user != null)
            {
                user.UserTypeID = 1; 
                await db.SaveChangesAsync(); 
                await getUser(); 
                MessageBox.Show("Der Benutzer wurde erfolgreich als Administrator festgelegt"); 
            }
            else
            {
                MessageBox.Show("Der Benutzer wurde nicht gefunden oder existiert nicht."); 
            }
        }
    }
}
