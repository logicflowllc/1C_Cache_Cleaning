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
using System.Security.Principal;

namespace _1C_Cache_Cleaning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private int mouseCount = 0; // Mouse hover counter
        private SortedDictionary<string, string> DBList; // List of databases and paths
        private ServiceController ApacheVerName; // Apache current version name
        bool isAdmin; // Admin flag        
        string cacheSize; // CacheSize

        Color blockHeaderNormal = Color.FromArgb(0xFF, 0x3B, 0x3B, 0x42);
        Color blockHeaderHover = Color.FromArgb(0xFF, 0x58, 0x58, 0x61);
        Color blockBodyNormal = Color.FromArgb(0xFF, 0x31, 0x31, 0x36);
        Color blockBodyHover = Color.FromArgb(0xFF, 0x3B, 0x3B, 0x40);


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

            // Count cache size async
            CountCacheSize();
        }

        #region Cache_Size
        /////////////////////////////////////////////////////////
        // Cache size preloading
        private async Task CountCacheSize()
        {
            await CountCacheSizeAsync();
            if (cacheSize == "0b")
            {
                LabStdCount.Background = new SolidColorBrush(Color.FromArgb(150, 0, 123, 255));
                LabAggrCount.Background = new SolidColorBrush(Color.FromArgb(150, 0, 123, 255));
            }
            else
            {
                LabStdCount.Background = new SolidColorBrush(Color.FromArgb(150, 255, 89, 89));
                LabAggrCount.Background = new SolidColorBrush(Color.FromArgb(150, 255, 89, 89));
            }
            LabStdCount.Text = cacheSize;
            LabAggrCount.Text = cacheSize + "<";
        }

        async Task CountCacheSizeAsync()
        {
            await Task.Run(() => CountCache());
        }

        void CountCache()
        {
            CleaningCore cc = new CleaningCore();
            cacheSize = cc.CacheCleaning(false);   
        }
        #endregion

        #region Standart_Cache_Cleaning
        /////////////////////////////////////////////////////////
        // Start button handler
        private void CacheCleaningButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Calling for cleaning
            CleaningCore cc = new CleaningCore();
            cc.CacheCleaning(true);
            
            // Update counters
            CountCacheSize();
        }
        #endregion

        #region Aggressive_Cache_Cleaning
        /////////////////////////////////////////////////////////
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
                            cc.CacheCleaning(true);
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.ToString());
                    }
                    break;

                case MessageBoxResult.No:
                    break;
            }
            // Update counters
            CountCacheSize();
        }

        // Shutdown all 1C clients process
        private void KillAll1C()
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
        #endregion

        #region Monster_Cache_Cleaning
        private void CacheCleaningButtonMonster_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
        #endregion

        #region Temp_Files_Cleaning
        /////////////////////////////////////////////////////////
        // Send DB path to Cleaning Core
        private void TempCleaningButtonAggressive_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
            // Update counters
            CountCacheSize();
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
        #endregion

        #region Cache_UI_Handlers 
        /////////////////////////////////////////////////////////
        // Cache grid hover/leave
        private void CacheGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleCache.Background = new SolidColorBrush(blockHeaderHover);
            CacheGrid.Background = new SolidColorBrush(blockBodyHover);            
        }

        private void CacheGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleCache.Background = new SolidColorBrush(blockHeaderNormal);
            CacheGrid.Background = new SolidColorBrush(blockBodyNormal);            
        }

        /////////////////////////////////////////////////////////
        // Update cache button down
        private void BtnCacheUpdate_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CountCacheSize();
        }

        // Update cache button hover
        private void BtnCacheUpdate_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                BtnCacheUpdate.Source = new BitmapImage(new Uri(@"\Images\1CCC_UpdateCache_Hover.png", UriKind.Relative));
                mouseCount += 1;
            }
        }

        // Update cache button leave
        private void BtnCacheUpdate_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BtnCacheUpdate.Source = new BitmapImage(new Uri(@"\Images\1CCC_UpdateCache_Normal.png", UriKind.Relative));
            mouseCount = 0;
        }

        // Start button hover action
        private void startButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0) {
                buttonStartStd.Source = new BitmapImage(new Uri(@"\Images\1CCC_Start_Hover.png", UriKind.Relative));
                mouseCount += 1;
            }
        }

        // Start button leave action
        private void startButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStartStd.Source = new BitmapImage(new Uri(@"\Images\1CCC_Start_Normal.png", UriKind.Relative));
            mouseCount = 0;
        }

        /////////////////////////////////////////////////////////
        // Aggressive start button hover action
        private void startButtonAgg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                buttonStartAggressive.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartAgg_Hover.png", UriKind.Relative));
                mouseCount += 1;
            }
        }

        // Aggressive start button leave action
        private void startButtonAgg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStartAggressive.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartAgg_Normal.png", UriKind.Relative));
            mouseCount = 0;
        }

        /////////////////////////////////////////////////////////
        // Monster start button hover action
        private void buttonStartMonster_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                buttonStartMonster.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartMon_Hover.png", UriKind.Relative));
                mouseCount += 1;
            }
        }

        // Monster start button leave action
        private void buttonStartMonster_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStartMonster.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartMon_Normal.png", UriKind.Relative));
            mouseCount = 0;
        }
        #endregion

        #region Temp_UI_Handlers 
        /////////////////////////////////////////////////////////
        // Cache grid hover/leave
        private void TempGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleTemp.Background = new SolidColorBrush(blockHeaderHover);
            TempGrid.Background = new SolidColorBrush(blockBodyHover);
            ListBoxDB.Background = new SolidColorBrush(blockHeaderHover);
            LabelTempDBPath.Background = new SolidColorBrush(blockHeaderHover);
        }

        private void TempGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleTemp.Background = new SolidColorBrush(blockHeaderNormal);
            TempGrid.Background = new SolidColorBrush(blockBodyNormal);
            ListBoxDB.Background = new SolidColorBrush(blockHeaderNormal);
            LabelTempDBPath.Background = new SolidColorBrush(blockHeaderNormal);
        }

        /////////////////////////////////////////////////////////
        // Clear temp start button hover action
        private void startButtonTempAgg_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseCount == 0)
            {
                buttonStartCleaningTemp.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartTempAgg_Hover.png", UriKind.Relative));
                mouseCount += 1;
            }
        }

        // Clear temp start button leave action
        private void startButtonTempAgg_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            buttonStartCleaningTemp.Source = new BitmapImage(new Uri(@"\Images\1CCC_StartTempAgg_Normal.png", UriKind.Relative));

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
                ButtonTempOpenFolder.Source = new BitmapImage(new Uri(@"\Images\1CCC_OpenFolder_Hover.png", UriKind.Relative));
                mouseCount += 1;
            }
        }

        // Open folder button leave action
        private void ButtonTempOpenFolder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ButtonTempOpenFolder.Source = new BitmapImage(new Uri(@"\Images\1CCC_OpenFolder_Normal.png", UriKind.Relative));
            mouseCount = 0;
        }
        #endregion

        #region Apache_UI_Handlers
        /////////////////////////////////////////////////////////
        // Apache grid hover/leave
        private void ApacheGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleApache.Background = new SolidColorBrush(blockHeaderHover);
            ApacheGrid.Background = new SolidColorBrush(blockBodyHover);
        }

        private void ApacheGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleApache.Background = new SolidColorBrush(blockHeaderNormal);
            ApacheGrid.Background = new SolidColorBrush(blockBodyNormal);
        }
        #endregion

        #region Updates_UI_Handlers
        /////////////////////////////////////////////////////////
        // Apache grid hover/leave
        private void UpdatesGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleUpdates.Background = new SolidColorBrush(blockHeaderHover);
            UpdatesGrid.Background = new SolidColorBrush(blockBodyHover);
        }

        private void UpdatesGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LabelTitleUpdates.Background = new SolidColorBrush(blockHeaderNormal);
            UpdatesGrid.Background = new SolidColorBrush(blockBodyNormal);
        }
        #endregion

        #region Bottom_UI_Handlers 
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
        // Bottom version labels hover
        private void BottomTitles_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentLabel = (Label)sender;
            if (currentLabel != null && mouseCount == 0)
            {
                currentLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 123, 255));
            }
            mouseCount = 1;
        }

        // Bottom version labels leave
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
            Process.Start("https://github.com/logicflowllc/1C_Cache_Cleaning/releases");
        }
        #endregion

        #region Apache_Control
        /////////////////////////////////////////////////////////
        // Get Apache state
        private void GetApacheServiceState()
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
                        case ServiceControllerStatus.Running:
                            SetApacheState(1);
                            break;
                        case ServiceControllerStatus.StopPending:
                            SetApacheState(2);
                            break;
                        case ServiceControllerStatus.StartPending:
                            SetApacheState(2);
                            break;
                        case ServiceControllerStatus.Stopped:
                            SetApacheState(0);
                            break;
                    }
                    break;
                }
                else
                {
                    // Apache not found
                    SetApacheState(3);
                }
            }
        }

        // Set Apache state 
        private void SetApacheState(short ApState)
        {
            switch (ApState)
            {
                case 0:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_Off.png", UriKind.Relative));
                    buttonApache.ToolTip = "Служба " + ApacheVerName.ServiceName.ToString() + " остановлена";
                    buttonApache.IsEnabled = true;
                    break;
                case 1:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_On.png", UriKind.Relative));
                    buttonApache.ToolTip = "Служба " + ApacheVerName.ServiceName.ToString() + " запущена";
                    buttonApache.IsEnabled = true;
                    break;
                case 2:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_Wait.png", UriKind.Relative));
                    buttonApache.ToolTip = "Служба " + ApacheVerName.ServiceName.ToString() + " в процессе запуска/остановки";
                    buttonApache.IsEnabled = true;
                    break;
                case 3:
                    buttonApache.Source = new BitmapImage(new Uri(@"\Images\1CCC_Apache_Wait.png", UriKind.Relative));
                    buttonApache.ToolTip = "Служба Apache не обнаружена";
                    buttonApache.IsEnabled = false;
                    break;
            }
        }

        /////////////////////////////////////////////////////////
        // Apache Start/Stop control
        private void ButtonApache_MouseDownAsync(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                // If is administrator, the variable updates from False to True
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (isAdmin)
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
            else
            {
                string EXEFileName = new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;

                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = EXEFileName,
                    UseShellExecute = true,
                    Verb = "runas",
                };

                System.Diagnostics.Process.Start(startinfo);
                System.Environment.Exit(0);
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
    }
}
