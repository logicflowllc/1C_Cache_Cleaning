using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Input;

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

        // Apache current version name
        private ServiceController ApacheVerName;


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

            // Get Apache service status at MainForm startup
            GetApacheServiceState();

        }

        // Shutdown all 1C clients process
        private void KillAll1C ()
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
        }

        #region Apache_Control
        /////////////////////////////////////////////////////////
        // Get Apache state
        private void GetApacheServiceState ()
        {
            // Get all services
            ServiceController[] AllServices;
            AllServices = ServiceController.GetServices();
         
            foreach (ServiceController CurrentService in AllServices)
            {
                // Find Apache
                if (CurrentService.ServiceName.Contains("Apache"))
                {
                    ApacheVerName = CurrentService;

                    switch (CurrentService.Status)
                    {
                        case ServiceControllerStatus.Running :
                            SetApacheState(1);
                            break;
                        case ServiceControllerStatus.StopPending :
                            SetApacheState(2);
                            break;
                        case ServiceControllerStatus.StartPending :
                            SetApacheState(2);
                            break;
                        case ServiceControllerStatus.Stopped :
                            SetApacheState(0);
                            break;
                    }
                    break;
                }
                else {
                    // Apache not found
                    SetApacheState(3);
                }
            }
        }

        // Set Apache state 
        private void SetApacheState (short ApState)
        {
            switch (ApState)
            {
                case 0:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_Off.bmp", UriKind.Relative));
                    buttonApache.ToolTip = "Служба " + ApacheVerName.ServiceName.ToString() + " остановлена";
                    break;
                case 1:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_On.bmp", UriKind.Relative));
                    buttonApache.ToolTip = "Служба " + ApacheVerName.ServiceName.ToString() + " запущена";
                    break;
                case 2:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_Wait.bmp", UriKind.Relative));
                    buttonApache.ToolTip = "Служба " + ApacheVerName.ServiceName.ToString() + " в процессе запуска/остановки";
                    break;
                case 3:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_Wait.bmp", UriKind.Relative));
                    buttonApache.ToolTip = "Служба Apache не обнаружена";
                    break;
                default:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_Wait.bmp", UriKind.Relative));
                    break;
            }
        }

        /////////////////////////////////////////////////////////
        // Apache Start/Stop control
        private void ButtonApache_MouseDownAsync(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Check Apache service status
            if (ApacheVerName.Status == ServiceControllerStatus.Running)
            {
                // Go to service Stop
                StartApacheStopSequence();
                SetApacheState(2);
            }
            else if (ApacheVerName.Status == ServiceControllerStatus.Stopped)
            {
                // Go to service Start
                StartApacheStartSequence();
                SetApacheState(2);
            }
            else
            {
                // Default 
                MessageBox.Show(ApacheVerName.ServiceName.ToString() + " уже в процессе запуска/остановки. Повторите попытку через несколько секунд", ApacheVerName.ServiceName.ToString() + " уже запускается/останавливается ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Apache stop
        private async Task StartApacheStopSequence()
        {
            await ApacheStopSequence();
        }

        async Task ApacheStopSequence()
        {
            await Task.Run(() => StopApache());
            SetApacheState(0);
        }

        void StopApache()
        {
            ApacheVerName.Stop();
            ApacheVerName.WaitForStatus(ServiceControllerStatus.Stopped);
        }

        // Apache start
        private async Task StartApacheStartSequence()
        {
            await ApacheStartSequence();
            // Default 
            MessageBox.Show(ApacheVerName.ServiceName.ToString() + " запущен", ApacheVerName.ServiceName.ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
            SetApacheState(1);
        }

        async Task ApacheStartSequence()
        {
            await Task.Run(() => StartApache());
        }

        void StartApache()
        {
            ApacheVerName.Start();
            ApacheVerName.WaitForStatus(ServiceControllerStatus.Running);
        }

#endregion

        // Start button handler
        private void CacheCleaningButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Calling for cleaning
            CleaningCore cc = new CleaningCore();
            cc.StartCacheCleaning();
        }

        // Start button handler with killing 1C processes (Aggressive mode)
        private void CacheCleaningButtonAggressive_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string MessageBoxText = "Все процессы 1С будут принудительно завершены.\n\nПродолжить?";
            string Caption = "Очистка файлов кэша";

            MessageBoxButton MessageBoxButtons = MessageBoxButton.YesNo;
            MessageBoxImage MessageBoxIcons = MessageBoxImage.Warning;

            MessageBoxResult MessageBoxPressed = MessageBox.Show(MessageBoxText, Caption, MessageBoxButtons, MessageBoxIcons);

            switch (MessageBoxPressed)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        KillAll1C();

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
                    break;

                case MessageBoxResult.No:
                    break;
            }
        }

        // Send DB path to Cleaning Core
        private void ButtonStartCleaningTemp_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ListBoxDB.SelectedIndex != -1)
            {
                string MessageBoxText = "Все процессы 1С будут принудительно завершены.\n\nПродолжить?";
                string Caption = "Очистка временных файлов";

                MessageBoxButton MessageBoxButtons = MessageBoxButton.YesNo;
                MessageBoxImage MessageBoxIcons = MessageBoxImage.Warning;

                MessageBoxResult MessageBoxPressed = MessageBox.Show(MessageBoxText, Caption, MessageBoxButtons, MessageBoxIcons);

                switch (MessageBoxPressed)
                {
                    case MessageBoxResult.Yes:
                        string path = DBList[ListBoxDB.SelectedValue.ToString()];

                        KillAll1C();

                        CleaningCore cc = new CleaningCore();
                        cc.StartTempCleaning(DBList[ListBoxDB.SelectedValue.ToString()], ListBoxDB.SelectedItem.ToString());
                        break;

                    case MessageBoxResult.No:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите базу данных из списка", "База данных не выбрана", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Open selected DB folder
        private void ButtonTempOpenFolder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ListBoxDB.SelectedIndex != -1)
            {
                if (Directory.Exists(DBList[ListBoxDB.SelectedValue.ToString()]))
                {
                    Process.Start("explorer.exe", DBList[ListBoxDB.SelectedValue.ToString()]);
                }
                else
                {
                    MessageBox.Show("Выбранная база данных не найдена", "Путь не найден", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else
            {
                MessageBox.Show("Пожалуйста, выберите базу данных из списка", "База данных не выбрана", MessageBoxButton.OK, MessageBoxImage. Warning);
            }
        }

        #region UI_Handlers_Visuals 

        /////////////////////////////////////////////////////////
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
            LabelCacheButtonTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(183, 201, 218));
            mouseCount = 0;
        }

        /////////////////////////////////////////////////////////
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
            LabelCacheButtonAggTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 146, 146));
            mouseCount = 0;
        }

        /////////////////////////////////////////////////////////
        // Clear temp start button hover action
        private void startButtonTempAgg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                buttonStartCleaningTemp.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartTempAgg_Hover.bmp", UriKind.Relative));
                LabelTempButtonTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 37, 37));
                mouseCount += 1;
            }
        }

        // Clear temp start button leave action
        private void startButtonTempAgg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStartCleaningTemp.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartTempAgg_Normal.bmp", UriKind.Relative));
            LabelTempButtonTitle.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(199, 146, 146));
            mouseCount = 0;
        }

        /////////////////////////////////////////////////////////
        // Show selected DB path in Temp block
        private void ListBoxDB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LabelTempDBPath.Content = DBList[ListBoxDB.SelectedValue.ToString()];
        }

        /////////////////////////////////////////////////////////
        // Open folder button enter action
        private void ButtonTempOpenFolder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                ButtonTempOpenFolder.Source = new BitmapImage(new Uri(@"\Images\1CCC_OpenFolder_Hover.bmp", UriKind.Relative));
                mouseCount += 1;
            }
        }

        // Open folder button leave action
        private void ButtonTempOpenFolder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ButtonTempOpenFolder.Source = new BitmapImage(new Uri(@"\Images\1CCC_OpenFolder_Normal.bmp", UriKind.Relative));
            mouseCount = 0;
        }
        #endregion

        #region Bottom_Links
        /////////////////////////////////////////////////////////
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

        /////////////////////////////////////////////////////////
        // Bottom labels hover
        private void BottomTitles_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentLabel = (Label)sender;
            if (currentLabel != null && mouseCount == 0)
            {
                currentLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 123, 255));
            }
            mouseCount = 1;
        }

        private void BottomTitles_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentLabel = (Label)sender;
            if (currentLabel != null && mouseCount == 1)
            {
                currentLabel.Foreground = new SolidColorBrush(Color.FromRgb(103, 103, 116));
            }
            mouseCount = 0;
        }

        /////////////////////////////////////////////////////////
        // Open GitHub web site
        private void LabelGitHub_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/logicflowllc/1C_Cache_Cleaning");
        }

        // Open GitHub Releases site
        private void LabelGitHubReleases_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/logicflowllc/1C_Cache_Cleaning");
        }
        #endregion
    }
}
