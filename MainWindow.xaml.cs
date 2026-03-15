using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;

using Microsoft.Win32;

using WPFTerminVkladKalk.Model;

namespace WPFTerminVkladKalk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string JsonPath = "data_kalkulacka.json";
        private List<VypocetData> historie = new List<VypocetData>();

        public MainWindow()
        {
            InitializeComponent();
            NactiJson();
        }

        private void BtnVypocitat_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtVklad.Text, out double vklad) &&
                double.TryParse(txtUrok.Text, out double urokPa) &&
                int.TryParse(txtMesice.Text, out int mesice))
            {
                DateTime start = DateTime.Now;
                DateTime konec = start.AddMonths(mesice);
                int dny = (konec - start).Days;

                // --- PŘESNÝ BANKOVNÍ VÝPOČET (ACT/360) ---
                double hrubyUrok = vklad * (urokPa / 100.0) * (dny / 360.0);

                // Zaokrouhlování daně dle zákona (dolů na celé koruny)
                double zakladDane = Math.Floor(hrubyUrok);
                double dan = Math.Floor(zakladDane * 0.15);
                double cistyUrok = hrubyUrok - dan;

                historie.Insert(0, new VypocetData
                {
                    DatumSplatnosti = konec,
                    Vklad = vklad,
                    UrokPa = urokPa,
                    PocetDni = dny,
                    CistyUrok = cistyUrok
                });

                ObnovData();
            }
        }

        private void BtnExportTxt_Click(object sender, RoutedEventArgs e) => ExportDoTxt();

        private void ExportDoTxt()
        {
            if (!historie.Any())
            {
                MessageBox.Show("Historie je prázdná, není co exportovat.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Textový soubor (*.txt)|*.txt",
                FileName = $"Vypocet_Vkladu_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (sfd.ShowDialog() == true)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("----------------------------------------------------------------------------------");
                sb.AppendLine($"HISTORIE VÝPOČTŮ TERMÍNOVANÝCH VKLADŮ (Standard ACT/360) - Generováno: {DateTime.Now:dd.MM.yyyy HH:mm}");
                sb.AppendLine("----------------------------------------------------------------------------------");
                // Přidání hlavičky sloupců pro lepší čitelnost v TXT
                sb.AppendLine(string.Format("{0,-12} | {1,-10} | {2,-8} | {3,-8} | {4,-15}",
                              "Splatnost", "Vklad (Kč)", "Sazba %", "Dní", "Čistý úrok (Kč)"));
                sb.AppendLine(new string('-', 82));

                foreach (var polozka in historie)
                {
                    sb.AppendLine(string.Format("{0,-12:dd.MM.yyyy} | {1,-10:N0} | {2,-8:N2} | {3,-8} | {4,-15:N2}",
                                  polozka.DatumSplatnosti,
                                  polozka.Vklad,
                                  polozka.UrokPa, // Tenhle sloupec nám chyběl!
                                  polozka.PocetDni,
                                  polozka.CistyUrok));
                }

                try
                {
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Export do TXT byl úspěšně dokončen.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Chyba při zápisu do souboru: {ex.Message}");
                }
            }
        }

        private void BtnSmazat_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Opravdu chcete smazat veškerou historii?", "Potvrzení", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                historie.Clear();
                if (File.Exists(JsonPath)) File.Delete(JsonPath);
                ObnovData();
            }
        }

        private void ObnovData()
        {
            dgHistorie.ItemsSource = null;
            dgHistorie.ItemsSource = historie;
            File.WriteAllText(JsonPath, JsonSerializer.Serialize(historie));
        }

        private void NactiJson()
        {
            if (File.Exists(JsonPath))
            {
                string json = File.ReadAllText(JsonPath);
                historie = JsonSerializer.Deserialize<List<VypocetData>>(json) ?? new List<VypocetData>();
                dgHistorie.ItemsSource = historie;
            }
        }
    }
}