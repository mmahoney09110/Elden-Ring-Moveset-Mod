using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace EldenEncouragement
{
    class RegistryHelper
    {
        static string SaveFilePath = "";
        static readonly string DefaultSteamPath = @"C:\\Program Files (x86)\\Steam\\steamapps\\common\\ELDEN RING\\Game\\";
        static readonly string RegistryKeyPath = "HKEY_CURRENT_USER\\Software\\EldenRingOverlay";
        static readonly string RegistryValueName = "SaveFilePath";

        [STAThread]

        public static String LoadSaveFilePath()
        {
            // Get the overlay's directory (e.g. "C:\Games\ELDEN RING\Game\SoulWeapon\")
            string overlayDir = AppDomain.CurrentDomain.BaseDirectory;

            // Trim trailing directory separators so GetParent returns the correct parent
            overlayDir = overlayDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Get the parent directory (should be "C:\Games\ELDEN RING\Game")
            string parentDir = Directory.GetParent(overlayDir)?.FullName;

            // Construct the full path to save.txt
            string ParentDirectoryPath = Path.Combine(parentDir ?? "", "eldenring.exe");

            // Check registry for previously saved path
            string savedPath = Registry.GetValue(RegistryKeyPath, RegistryValueName, null) as string;
            if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
            {
                return savedPath;
            }

            // Check default Steam path
            if (Directory.Exists(DefaultSteamPath))
            {
                string saveFilePath = Path.Combine(DefaultSteamPath, "save.txt");
                return saveFilePath;
            }

            // Check parent directory path
            if (File.Exists(ParentDirectoryPath))
            {
                ParentDirectoryPath = Path.Combine(parentDir ?? "", "save.txt");
                return ParentDirectoryPath;
            }

            // Prompt user for Elden Ring executable
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Locate Elden Ring Executable",
                Filter = "Elden Ring Executable (eldenring.exe)|eldenring.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Extract the directory (not the full exe path)
                string gameDirectory = System.IO.Path.GetDirectoryName(openFileDialog.FileName);

                string saveFilePath = Path.Combine(gameDirectory, "save.txt");

                Registry.SetValue(RegistryKeyPath, RegistryValueName, saveFilePath);
                return saveFilePath; // Now it correctly finds save.txt inside the game directory
            }
            else
            {
                MessageBox.Show("Something went wrong when searching for the elden ring directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
                return "Not Found";
            }
        }
    }
}
