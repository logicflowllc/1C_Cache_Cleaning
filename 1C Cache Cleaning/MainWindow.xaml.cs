using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _1C_Cache_Cleaning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Mouse hover counter
        private int mouseCount = 0;
        // List of databases and paths
        private SortedDictionary<string, string> DBList;


        public MainWindow()
        {
            InitializeComponent();

            //Get local file DB List
            DBsFinder dbsf = new DBsFinder();
            DBList = dbsf.getDBsList();

            foreach (KeyValuePair<string, string> kvp in DBList)
            {

                ListBoxDB.Items.Add(kvp.Key);
            }
        }

        // Start button handler
        private void CacheCleaningButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Calling for cleaning
            CleaningCore cc = new CleaningCore();
            cc.StartCacheCleaning();
        }

        // Start button handler with killing 1C processes
        private void CacheCleaningButtonAggressive_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Kill all processes
                foreach (Process process in Process.GetProcessesByName("1cv8"))
                {
                    process.Kill();
                    process.WaitForExit();
                }
                foreach (Process process in Process.GetProcessesByName("1cv8c"))
                {
                    process.Kill();
                    process.WaitForExit();
                }
                // Calling for cleaning
                Process[] proc1cv8 = Process.GetProcessesByName("1cv8");
                Process[] proc1cv8c = Process.GetProcessesByName("1cv8c");
                if (proc1cv8.Length == 0 && proc1cv8c.Length == 0)
                {
                    // Calling for cleaning
                    CleaningCore cc = new CleaningCore();
                    cc.StartCacheCleaning();
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        // Send DB path to Cleaning Core
        private void ButtonStartCleaningTemp_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CleaningCore cc = new CleaningCore();
            cc.StartTempCleaning(DBList[ListBoxDB.SelectedValue.ToString()]);
        }

        #region UI_Handlers
        // Open Logic Flow web site
        private void LabelLF_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://logicflow.ru");
        }

        // LogicFlow Logo hover action
        private void LogicFlow_Site_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                LogicFlow_Site.Opacity = 1;
                mouseCount += 1;
            }
        }

        // LogicFlow Logo out action
        private void LogicFlow_Site_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LogicFlow_Site.Opacity = 0.4;
            mouseCount = 0;
        }

        // Bottom labels hover
        private void BottomTitles_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentLabel = (Label)sender;
            if (currentLabel != null && mouseCount == 0)
            {
                currentLabel.Foreground = new SolidColorBrush(Color.FromRgb(87, 110, 255));
            }
            mouseCount = 1;
        }

        private void BottomTitles_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentLabel = (Label)sender;
            if (currentLabel != null && mouseCount == 1)
            {
                currentLabel.Foreground = new SolidColorBrush(Color.FromRgb(56, 56, 62));
            }
            mouseCount = 0;
        }

        // Open GitHub web site
        private void LabelGitHub_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/dtinside/1C_Cache_Cleaning");
        }

        // Open GitHub Releases site
        private void LabelGitHubReleases_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/dtinside/1C_Cache_Cleaning/releases");
        }

        // Start button hover action
        private void startButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0) {
                buttonStart.Source = new BitmapImage(new Uri(@"\Images\1CCC_Start_Hover.bmp", UriKind.Relative));
                LabelCacheButtonTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 123, 255));
                mouseCount += 1;
            }
        }

        // Start button leave action
        private void startButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStart.Source = new BitmapImage(new Uri(@"\Images\1CCC_Start_Normal.bmp", UriKind.Relative));
            LabelCacheButtonTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(97, 107, 117));
            mouseCount = 0;
        }

        // Aggressive start button hover action
        private void startButtonAgg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                buttonStartAggressive.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartAgg_Hover.bmp", UriKind.Relative));
                LabelCacheButtonAggTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 37, 37));
                mouseCount += 1;
            }
        }

        // Aggressive start button leave action
        private void startButtonAgg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStartAggressive.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartAgg_Normal.bmp", UriKind.Relative));
            LabelCacheButtonAggTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(117, 97, 97));
            mouseCount = 0;
        }

        // Aggressive start button hover action
        private void startButtonTempAgg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                buttonStartCleaningTemp.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartTempAgg_Hover.bmp", UriKind.Relative));
                LabelTempButtonTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 37, 37));
                mouseCount += 1;
            }
        }

        // Aggressive start button leave action
        private void startButtonTempAgg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStartCleaningTemp.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartTempAgg_Normal.bmp", UriKind.Relative));
            LabelTempButtonTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(117, 97, 97));
            mouseCount = 0;
        }


        #endregion
    }
}
