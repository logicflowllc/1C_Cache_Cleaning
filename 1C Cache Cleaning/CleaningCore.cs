using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace _1C_Cache_Cleaning
{
    class CleaningCore
    {
        // Counter of cache size
        private double CacheSize = 0L;
        private double TempSize = 0L;
        private byte ErrorCount = 0;

        // Start of cache cleaning
        // Find all cache directories and clean it
        public string CacheCleaning(bool ConfirmClean)
        {
            CacheSize = 0;

            // Get Local and Roaming dirs paths 
            string AppDataLocalGlobal = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string AppDataRoamingGlobal = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Define list of 1C cache directories 
            string[] AppDataSubFoldersNames = {
                @"\1C\1cv8\",
                @"\1C\1cv8t\",
                @"\1C\1Cv82\",
                @"\1C\1Cv82t\",
                @"\1C\1Cv83\"
            };

            // Local
            foreach (string CurrentLocalPath in AppDataSubFoldersNames)
            {
                if (Directory.Exists(AppDataLocalGlobal + CurrentLocalPath))
                {
                    string[] LocalSubDirs = Directory.GetDirectories(AppDataLocalGlobal + CurrentLocalPath, "*", SearchOption.TopDirectoryOnly);
                    CacheDirsDeleting(LocalSubDirs, ConfirmClean);
                }
            }

            // Roaming
            foreach (string CurrentLocalPath in AppDataSubFoldersNames)
            {
                if (Directory.Exists(AppDataRoamingGlobal + CurrentLocalPath))
                {
                    string[] LocalSubDirs = Directory.GetDirectories(AppDataRoamingGlobal + CurrentLocalPath, "*", SearchOption.TopDirectoryOnly);
                    CacheDirsDeleting(LocalSubDirs, ConfirmClean);
                }
            }

            // Final message about cleaning status
            StringBuilder sb = new StringBuilder();
            sb.Append("Очистка кэша 1С завершена.\n\n");
            sb.Append("Очищено ");
            string CacheSizeConverted = ConvertSize(CacheSize);
            sb.Append(CacheSizeConverted);

            if (ConfirmClean)
            {
                MessageBox.Show(sb.ToString(), "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return CacheSizeConverted;
        }

        // Cleaning 
        // Foreach all cache directories
        private void CacheDirsDeleting(string[] TargetPaths, bool ConfirmClean)
        {
            // Foreach all subfolders
            foreach (string CachePath in TargetPaths)
            {
                string ReplacedString = CachePath.Replace(@"\\", @"\");
                string[] CuttedString = ReplacedString.Split('\\');

                // If dir name length = 36
                if (CuttedString[CuttedString.Length - 1].Length == 36)
                {
                    try
                    {
                        double TempCacheSize = 0L;

                        string[] AllCacheFiles = Directory.GetFiles(CachePath, "*.*", SearchOption.AllDirectories);

                        foreach (string FileName in AllCacheFiles)
                        {
                            File.SetAttributes(FileName, FileAttributes.Normal);
                            FileInfo Info = new FileInfo(FileName);                            
                            TempCacheSize += Info.Length;                            
                        }

                        if (ConfirmClean)
                        {
                            Directory.Delete(CachePath, true);
                        }

                        // in a case of error, current cache size will be deleted
                        CacheSize += TempCacheSize;
                    }
                    catch
                    {
                        //MessageBox.Show(ex.ToString());
                        if (ErrorCount != 0) { 
                            MessageBox.Show("Не все папки с кэшем особождены от процессов 1С.\n\nПопробуйте:\n• запустить очистку в агрессивном режиме\n• завершить все процессы 1С через диспетчер задач\n• запустить очистку после перезагрузки ПК.\n\nПроизойдёт очситка только незанятых папок", "Очистка не будет полной", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }

                        ErrorCount++;

                        // Break
                        return;
                    }
                }
            }
        }

        // Start of temp deleting
        public void StartTempCleaning (string SelectedDBPath, string SelectedDBName)
        {
            TempSize = 0;

            // List of temp files extensions
            List<string> TempExtensions = new List<string>() {"bin", "dat", "cfl", "log", "ind", "lck", "lgd", "1CL", "txt", "tmp", "lgf", "lgp", "cgr"};

            string[] SplitedDBName = SelectedDBName.Split('/');

            try
            {
                // Remove all escapes
                string DBPath = SelectedDBPath.Replace(@"\\", @"\");

                // Check if directory exist on disk
                if (Directory.Exists(DBPath))
                {
                    // Get all files 
                    string[] AllTempFiles = Directory.GetFiles(DBPath, "*.*", SearchOption.AllDirectories);

                    foreach (string FileName in AllTempFiles)
                    {
                        // Get current file extension
                        string CurrentFileExtension = FileName.Substring(FileName.Length - 3, 3);

                        // Check if temp extension list contains current file extension 
                        // and check additional file name
                        // if allright, store temp file size and delete file 
                        if (TempExtensions.Contains(CurrentFileExtension) || FileName.Contains(@"1Cv8tmp.1CD"))
                        {
                            FileInfo Info = new FileInfo(FileName);
                            TempSize += Info.Length;
                            File.Delete(FileName);
                        }
                    }

                    // Final message about cleaning status
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Очистка временных файлов базы данных\n");
                    sb.Append(SplitedDBName[SplitedDBName.Length - 1].Trim() + " завершена.\n\n");
                    sb.Append("Очищено ");
                    sb.Append(ConvertSize(TempSize));

                    MessageBox.Show(sb.ToString(), "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                } else
                {
                    MessageBox.Show("Путь к выбранной базе данных не найден", "Путь не найден", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            } catch (Exception)
            {
                MessageBox.Show("Временные файлы всё ещё заняты.\n\nЕсли выбранная база данных расположена в общей папке, опубликована на WEB-сервере " +
                    "или к ней имеют доступ другие компьютеры, находящиеся в сети, Вам необходимо самостоятельно завершить " +
                    "все сеансы 1С на других ПК и запустить очистку ещё раз.", "Временные файлы заняты другим процессом.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Convert cache size 
        private string ConvertSize(double CurrentSize)
        {
            if ((CurrentSize > (1024 * 1024 * 1024)))
            {
                double temp = Convert.ToDouble(CurrentSize) / (1024 * 1024 * 1024);
                return temp.ToString("0.00") + "Gb";
            }
            else if ((CurrentSize < (1024 * 1024 * 1024)) && (CurrentSize > (1024 * 1024)))
            {
                double temp = Convert.ToDouble(CurrentSize) / (1024 * 1024);
                return temp.ToString("0.00") + "Mb";
            }
            else if (CurrentSize < 1024 * 1024 && CurrentSize > 1024)
            {
                double temp = Convert.ToDouble(CurrentSize) / (1024);
                return temp.ToString("0.00") + "Kb";
            }
            else if (CurrentSize < 1024 && CurrentSize > 0)
            {
                return CurrentSize.ToString() + "b";
            }
            else
            {
                return "0b";
            }
        }
    }
}
