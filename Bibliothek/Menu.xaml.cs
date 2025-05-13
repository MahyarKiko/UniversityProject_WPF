using Bibliothek.Entities;
using Bibliothek.Model;
using Bibliothek.Utility;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;

namespace Bibliothek
{
    /// <summary>
    /// Interaktionslogik für Menue.xaml
    /// </summary>
    public partial class Menu : Window
    {
        // Datenbankkontext für den Zugriff auf die Bibliothek-Datenbank
        Bibliothek_Content db;
        // Hilfsobjekt für Dateioperationen
        FileUtil fileUtil;
        // Benutzerobjekt, das die aktuellen Benutzerdaten enthält
        User user;
        // Liste der Menüzugriffe des aktuellen Benutzers
        List<MenuAccess> currentMenu;

        public Menu()
        {
            InitializeComponent();
            db = new Bibliothek_Content(); 
            fileUtil = new FileUtil(); // Initialisieren des FileUtil Objekts

            // Benutzerinformationen aus einer Datei laden
            string userData = fileUtil.ReadStringFromJson();
            if (userData != string.Empty && userData != null)
            {
                string json = userData; // JSON Daten entschlüsseln, falls erforderlich
                user = JsonConvert.DeserializeObject<User>(json); // Deserialisieren des Benutzerobjekts
            }

            // Anzeigen des vollständigen Namens des Benutzers
            lbl_FullName.Content = user.FirstName + " " + user.LastName;
            currentMenu = new List<MenuAccess>(); // Initialisieren der Liste der Menüzugriffe
            getMenuAccess(); 
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            db.Dispose(); // Freigeben der Datenbankressourcen
            fileUtil.DeleteString(); // Löschen der Benutzerdatei
        }

        public async Task getMenuAccess()
        {
            // Abrufen der Menüzugriffe für den aktuellen Benutzer
            currentMenu = await (from menu in db.Menu
                                 join u in db.User on user.ID equals u.ID 
                                 join ur in db.UserPermission on new { MenuID = menu.ID, UserID = u.ID, IsAccept = true } equals new { ur.MenuID, ur.UserID, ur.IsAccept } into urGroup
                                 from ur in urGroup.DefaultIfEmpty()
                                 join ma in db.TypeToMenuAccess on new { MenüID = menu.ID, TypeID = u.UserTypeID, IsActive = true } equals new { MenüID = ma.MenuID, TypeID = ma.TypeID, ma.IsActive } into maGroup
                                 from ma in maGroup.DefaultIfEmpty()
                                 where (ur.ID != null || ma.ID != null) 
                                 select new MenuAccess()
                                 {
                                     MenuName = menu.MenuName,
                                     MenuURL = menu.ManuURL
                                 }).ToListAsync();

            List<System.Windows.Controls.Button> buttons = new List<System.Windows.Controls.Button>(); 

            // Erstellen von Schaltflächen für jedes Menüelement
            foreach (MenuAccess item in currentMenu)
            {
                System.Windows.Controls.Button btn = new System.Windows.Controls.Button();
                btn.Name = item.MenuURL; 
                btn.Content = item.MenuName; 
                btn.Width = win.Width / 3 - 48;
                btn.Height = 100;
                btn.Margin = new Thickness(15);
                btn.Style = (Style)this.FindResource("ButtonStyle");
                btn.HorizontalAlignment = System.Windows.HorizontalAlignment.Left; // Horizontale Ausrichtung der Schaltfläche
                btn.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                btn.Click += new RoutedEventHandler(this.onLeftClick); // Ereignishandler für den Klick hinzufügen
                buttons.Add(btn); // Schaltfläche zur Liste hinzufügen
            }

            // Hinzufügen der Schaltflächen zu einem StackPanel, um sie in Zeilen anzuordnen
            for (int i = 0; i < buttons.Count; i += 3)
            {
                StackPanel sp = new StackPanel();
                sp.Orientation = System.Windows.Controls.Orientation.Horizontal; 
                for (int j = i; j < i + 3 && j < buttons.Count; j++)
                {
                    sp.Children.Add(buttons[j]); // Schaltflächen zum StackPanel hinzufügen
                }
                sp_Root.Children.Add(sp); 
            }
        }

        public void onLeftClick(object sender, EventArgs e)
        {
            Button btn = sender as Button; // Sender als Schaltfläche casten
            OpenForm(btn.Name.Trim()); 
        }

        private void OpenForm(string formName)
        {
            
            string namespaceName = "Bibliothek";

            // Vollständigen Formularnamen erstellen
            string fullFormName = $"{namespaceName}.{formName}";

           
            Type formType = Type.GetType(fullFormName);
            if (formType == null)
            {
                System.Windows.MessageBox.Show("Das angeforderte Formular existiert nicht oder ist nicht korrekt benannt"); // Fehlermeldung anzeigen, wenn das Formular nicht gefunden wird
                return;
            }

            // Instanz des Formulars erstellen und anzeigen
            Window formInstance = (Window)Activator.CreateInstance(formType);
            if (formInstance != null)
            {
                formInstance.Show(); 
            }

            
            if (formName == "Login")
            {
                this.Close();
            }
        }
    }
}



