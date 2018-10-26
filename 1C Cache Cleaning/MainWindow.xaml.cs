using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace _1C_Cache_Cleaning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Counter of errors
        static int errorsCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        public object MessageBoxIcon { get; private set; }

        // Find 2 folders "1C" in Application Data in Local and Roaming
        //
        private void CacheCleaningButton_Click(object sender, RoutedEventArgs e)
        {
            string AppDataLocalGlobal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string AppDataRoamingGlobal = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string[] AppDataSubFoldersNames = {
                @"\1C\1cv8\",
                @"\1C\1cv8t\",
                @"\1C\1Cv82\",
                @"\1C\1Cv82t\",
                @"\1C\1Cv83\"
            };


            // Status of cleaning
            int statusLocal = 0;
            int statusRoaming = 0;

            // Local
            foreach (string CurrentLocalPath in AppDataSubFoldersNames)
            {
                if (Directory.Exists(AppDataLocalGlobal + CurrentLocalPath) && errorsCount < 1)
                {
                    string[] LocalSubDirs = Directory.GetDirectories(AppDataLocalGlobal + CurrentLocalPath, "*", SearchOption.TopDirectoryOnly);
                    Cleaning(LocalSubDirs, out statusLocal);
                }
            }

            // Roaming
            foreach (string CurrentLocalPath in AppDataSubFoldersNames)
            {
                if (Directory.Exists(AppDataRoamingGlobal + CurrentLocalPath) && errorsCount < 1)
                {
                    string[] LocalSubDirs = Directory.GetDirectories(AppDataRoamingGlobal + CurrentLocalPath, "*", SearchOption.TopDirectoryOnly);
                    Cleaning(LocalSubDirs, out statusLocal);
                }
            }

            // Reset errors count
            errorsCount = 0;

            if (statusLocal == 0 && statusRoaming == 0)
            {
                MessageBox.Show("Очистка кэша 1С успешно завершена", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Cleaning 
        private void Cleaning(string[] TargetPaths, out int status)
        {
            // Foreach all subfolders
            foreach (string CachePath in TargetPaths)
            {
                string ReplacedString = CachePath.Replace(@"\\", @"\");
                string[] CuttedString = ReplacedString.Split('\\');

                // If dir name length = 26
                if (CuttedString[CuttedString.Length - 1].Length == 36)
                {
                    try
                    {
                        Directory.Delete(CachePath, true);
                    }
                    catch
                    {
                        MessageBox.Show("Закройте все окна программы 1С:Предприятие и запустите очистку снова", "Не удаётся удалить кэш", MessageBoxButton.OK, MessageBoxImage.Warning);
                        // Errors count increment
                        errorsCount += 1;

                        // Returned status 1
                        status = 1;

                        // Break
                        return;
                    }
                }
            }
            status = 0;
        }

        // Open Logic Flow web site
        private void LabelLF_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("http://logicflow.ru");
        }

        // Open GitHub web site
        private void Label_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/dtinside/1C_Cache_Cleaning");
        }
    }
}
