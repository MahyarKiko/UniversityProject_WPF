using Bibliothek.Entities;
using System;
using System.Linq;
using System.Net.Mail;
using System.Windows;

using Bibliothek.Utility;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Bibliothek
{
    public partial class Register : Window
    {
        private int currentStep = 1; 
        Bibliothek_Content db; 
        FileUtil fileUtil; 

        
        public Register()
        {
            InitializeComponent();
            UpdateStepVisibility(); // Aktualisiert die Sichtbarkeit der Schritte
            db = new Bibliothek_Content(); 
            fileUtil = new FileUtil(); // Initialisiert die Hilfsklasse für Dateioperationen
        }

        private void txt_Address_GotFocus(object sender, RoutedEventArgs e)
        {
            // Sichtbarkeit der Adressfelder ändern und die Höhe des Fensters erhöhen
            txt_PostalCode.Visibility = Visibility.Visible;
            txt_City.Visibility = Visibility.Visible;
            txt_Country.Visibility = Visibility.Visible;
            this.Height += 150;
        }

        // Event Handler Überprüft die Formularvalidität und aktiviert oder deaktiviert den Absenden Button
        private void ValidateForm(object sender, RoutedEventArgs e)
        {
            btn_Submit.IsEnabled = IsFormValid(); // Button wird nur aktiviert, wenn das Formular gültig ist
        }

        // Überprüft, ob das Formular im aktuellen Schritt vollständig und gültig ist
        private bool IsFormValid()
        {
            switch (currentStep)
            {
                case 1:
                    
                    return !string.IsNullOrWhiteSpace(txt_email.Text) &&
                           !string.IsNullOrWhiteSpace(txt_Password.Password) &&
                           !string.IsNullOrWhiteSpace(txt_ConfirmPassword.Password);
                case 2:
                    
                    return !string.IsNullOrWhiteSpace(txt_FirstName.Text) &&
                           !string.IsNullOrWhiteSpace(txt_LastName.Text);
                case 3:
                   
                    return !string.IsNullOrWhiteSpace(txt_CellPhone.Text) &&
                           !string.IsNullOrWhiteSpace(txt_Address.Text);
                default:
                    return false;
            }
        }

        // Event Handler Wird aufgerufen, wenn der Absenden-Button geklickt wird
        private async void btn_Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFormValid())
            {
               
                MessageBox.Show("Füllen Sie bitte alle Felder vollständig und korrekt aus", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            switch (currentStep)
            {
                case 1:
                    
                    if (!Validation.IsValidEmail(txt_email.Text))
                    {
                        MessageBox.Show("Die E-Mail-Adresse ist ungültig.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (txt_Password.Password != txt_ConfirmPassword.Password)
                    {
                        MessageBox.Show("Die Passwörter stimmen nicht überein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (IsEmailRegistered(txt_email.Text))
                    {
                        MessageBox.Show("Diese E-Mail-Adresse ist bereits in unserem System registriert", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    currentStep++; // Wechselt zum nächsten Schritt
                    UpdateStepVisibility(); // Aktualisiert die Sichtbarkeit der Schritte
                    break;

                case 2:
                    
                    if (string.IsNullOrWhiteSpace(txt_FirstName.Text) || string.IsNullOrWhiteSpace(txt_LastName.Text))
                    {
                        MessageBox.Show("Name und Vorname dürfen nicht leer sein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    currentStep++; 
                    UpdateStepVisibility(); 
                    break;

                case 3:
                    
                    if (string.IsNullOrWhiteSpace(txt_CellPhone.Text) || string.IsNullOrWhiteSpace(txt_Address.Text))
                    {
                        MessageBox.Show("Telefonnummer und Adresse dürfen nicht leer sein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Hash des Passworts erstellen
                    string hashedPassword = txt_Password.Password.EncryptSHA();

                    // Erzeugt ein neues Benutzerobjekt und füllt es mit den Formularwerten
                    var user = new User()
                    {
                        Address = txt_Address.Text,
                        Email = txt_email.Text,
                        FirstName = txt_FirstName.Text,
                        LastName = txt_LastName.Text,
                        Password = hashedPassword,
                        IsActive = false,
                        CellPhone = txt_CellPhone.Text,
                        UserTypeID = 2, // Standard Benutzer Typ
                        PostalCode = Convert.ToInt32(txt_PostalCode.Text),
                        Country = txt_Country.Text,
                        City = txt_City.Text
                    };

                    // Fügt den neuen Benutzer zur Datenbank hinzu und speichert die Änderungen
                    await db.User.AddAsync(user);
                    await db.SaveChangesAsync();

                    if (user != null)
                    {
                        
                        string json = JsonConvert.SerializeObject(user);
                        string userData = json; 
                        MessageBox.Show("Anmeldung war erfolgreich, warten Sie auf Bestätigung des Admins");
                        ShowLoginPage(userData);
                    }
                    
                    break;
            }



           
        }

        // Aktualisiert die Sichtbarkeit der Layouts basierend auf dem aktuellen Schritt
        private void UpdateStepVisibility()
        {
            switch (currentStep)
            {
                case 1:
                    // Zeigt das Layout für Schritt 1 und verbirgt die anderen Layouts
                    layout_step1.Visibility = Visibility.Visible;
                    layout_step2.Visibility = Visibility.Collapsed;
                    layout_step3.Visibility = Visibility.Collapsed;
                    btn_Submit.Content = "Nächste"; 
                    btn_Back.Visibility = Visibility.Collapsed; 
                    break;
                case 2:
                    
                    layout_step1.Visibility = Visibility.Collapsed;
                    layout_step2.Visibility = Visibility.Visible;
                    layout_step3.Visibility = Visibility.Collapsed;
                    btn_Submit.Content = "Nächste"; 
                    btn_Back.Visibility = Visibility.Visible; 
                    break;
                case 3:
                   
                    layout_step1.Visibility = Visibility.Collapsed;
                    layout_step2.Visibility = Visibility.Collapsed;
                    layout_step3.Visibility = Visibility.Visible;
                    btn_Submit.Content = "Registrieren"; 
                    btn_Back.Visibility = Visibility.Visible; 
                    break;
            }

            btn_Submit.IsEnabled = IsFormValid(); 
        }


       

        // Überprüft, ob die gegebene E-Mail-Adresse bereits in der Datenbank registriert ist
        private bool IsEmailRegistered(string email)
        {
            try
            {
                using (var db = new Bibliothek_Content())
                {
                    var existingUser = db.User.FirstOrDefault(u => u.Email == email);
                    return existingUser != null; // Gibt true zurück, wenn der Benutzer bereits existiert
                }
            }
            catch (Exception ex)
            {
               
                MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Zeigt die Login Seite an und speichert die Benutzerdaten
        private void ShowLoginPage(string userData)
        {
            this.Close(); 
            fileUtil.SaveStringToJson(userData); 
            Login loginPage = new Login(); 
            loginPage.Show(); 
        }

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            currentStep--; 
            UpdateStepVisibility();
            btn_Submit.IsEnabled = IsFormValid(); 
        }

        // Event Handler Wird aufgerufen, wenn der Schließen-Button geklickt wird
        private void CloseButton_Click(object sender, EventArgs e)
        {
            ShowLoginPage("");
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
          
            return Regex.IsMatch(text, "^[0-9]+$");
        }

    }


  

}
