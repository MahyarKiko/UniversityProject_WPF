using Bibliothek.Entities;
using Bibliothek.Utility;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Bibliothek
{
    public partial class Login : Window
    {
        // Datenbankkontext für die Interaktion mit der Bibliothek-Datenbank
        Bibliothek_Content db;
        // Hilfsobjekt für Dateioperationen
        FileUtil fileUtil;
        // Benutzerobjekt für die Speicherung der Benutzerdaten
        User user;

        public Login()
        {
            InitializeComponent();

            
            db = new Bibliothek_Content();
            fileUtil = new FileUtil();

            // Lesen der Benutzerdaten aus einer JSON Datei
            string userData = fileUtil.ReadStringFromJson();
            if (userData != string.Empty && userData != null)
            {
                // Deserialisieren der Benutzerdaten
                string json = userData/*.DecryptSHA()*/;
                user = JsonConvert.DeserializeObject<User>(json);
                // Setzen des Benutzernamens und des Passworts in die entsprechenden Felder
                txt_Username.Text = user.Email;
                txt_Password.Password = user.Password.DecryptSHA();
            }
            else
            {
                
                user = new User();
            }

            // Hinzufügen von Event Handlern für das Laden des Fensters und des Passwortfeld
            Loaded += Login_Loaded;
            txt_Password.Loaded += Txt_Password_Loaded;
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            // Setzt den Fokus auf das Passwortfeld wenn das Fenster geladen ist
            txt_Password.Focus();
        }

        private void Txt_Password_Loaded(object sender, RoutedEventArgs e)
        {
            // Setzt den Fokus auf den Login Button, nachdem das Passwortfeld geladen ist
            btn_Login.Focus();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Freigeben der Datenbankressourcen, wenn das Fenster geschlossen wird
            db.Dispose();
        }

        private async void RunLogin(object sender, RoutedEventArgs e)
        {
            // Überprüfen der Anmeldeinformationen des Benutzers
            var user = await db.User.FirstOrDefaultAsync(t => t.Email == txt_Username.Text && t.Password == txt_Password.Password.EncryptSHA());

            if (user != null)
            {
                // Überprüfen ob der Benutzer aktiv ist
                if (!user.IsActive)
                {
                    MessageBox.Show("Einloggen nicht erlaubt. Bitte kontaktieren Sie den zuständigen Administrator!!");
                    txt_Username.Text = "";
                    txt_Password.Password = "";
                    return;
                }

                // Speichern der Benutzerdaten in einer JSON Datei
                string json = JsonConvert.SerializeObject(user);
                fileUtil.SaveStringToJson(json);
                
                Menu menue = new Menu();
                menue.Show();
                this.Close();
            }
            else
            {
                
                MessageBox.Show("Benutzername oder Passwort ist falsch");
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
        
            MessageBoxResult result = MessageBox.Show("Möchten Sie die Seite schließen?", "Bestätigung", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // Löschen der Benutzerdaten und Beenden der Anwendung
                fileUtil.DeleteString();
                Application.Current.Shutdown();
            }
        }

        private void RunRegister(object sender, RoutedEventArgs e)
        {
            // Öffnen des Registrierungsfensters und Schließen des Anmeldefensters
            Register register = new Register();
            register.Show();
            this.Close();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Überprüfen, ob die Enter Taste gedrückt wurde, um den Login auszuführen
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RunLogin(sender, e);
            }
        }
    }
}

