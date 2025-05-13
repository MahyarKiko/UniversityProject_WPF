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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Bibliothek
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Erstellen und Konfigurieren eines Timers
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5); // Timer Intervall auf 5 Sekunden setzen
            timer.Tick += Timer_Tick; // Event Handler für das Timer Tick Ereignis hinzufügen
            timer.Start(); // Timer starten
        }

        /// <summary>
        /// Event-Handler für das Timer-Tick-Ereignis
        /// </summary>
        /// <param name="sender">Der Timer, der das Tick-Ereignis auslöst</param>
        /// <param name="e">Ereignisdaten</param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Stoppen des Timers, wenn das Tick-Ereignis ausgelöst wird
            (sender as DispatcherTimer).Stop();

            // Erstellen und Anzeigen des Login Fensters
            Login login = new Login();
            login.Show();

            
            this.Close();
        }
    }
}



