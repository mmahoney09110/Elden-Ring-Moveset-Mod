using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Needed for Matrix
using System.Windows.Media.Animation;
using System.Windows.Threading;
using EldenEncouragement;

namespace EldenRingOverlay
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer checkEldenRingTimer;
        private int missingCounter = 0;
        private const int MissingThreshold = 35;
        private double screenWidth;
        private double screenHeight;
        private double hpBarMaxWidth;
        private const int MaxHP = 100;
        // Add the Storyboard for fade-out animation
        private Storyboard fadeOutStoryboard;
        private Storyboard fadeInStoryboard;
        private int forceRun = 0;
        // PInvoke declarations
        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hwnd);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Dynamically computed save file path
        private readonly string saveFilePath;

        public MainWindow()
        {
            InitializeComponent();
            saveFilePath = GetSaveFilePath();
            InitializeOverlay();
            GetScreenSize();
            fadeOutStoryboard = (Storyboard)this.Resources["FadeOutStoryboard"];
            fadeInStoryboard = (Storyboard)this.Resources["FadeInStoryboard"];
            StartEldenRingLauncher();
            StartMonitoringEldenRing();
        }

        private void StartEldenRingLauncher()
        {
            // Get the overlay's directory (e.g. "C:\Games\ELDEN RING\Game\SoulWeapon\")
            string overlayDir = AppDomain.CurrentDomain.BaseDirectory;
            AppendLog($"Overlay directory: {overlayDir}");

            // Trim trailing directory separators so GetParent returns the correct parent
            overlayDir = overlayDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Get the parent directory (should be "C:\Games\ELDEN RING\Game")
            string parentDir = Directory.GetParent(overlayDir)?.FullName;
            AppendLog($"Parent directory: {parentDir}");
            if (parentDir == null)
            {
                MessageBox.Show("Failed to determine Elden Ring directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string batFile = Path.Combine(parentDir ?? "", "launchmod_eldenring.bat");
            string exeFile = Path.Combine(parentDir ?? "", "modengine2_launcher.exe");

            _ = TryLaunchGame(batFile, exeFile);
        }

        private async Task TryLaunchGame(string batFile, string exeFile)
        {
            int tryAndRun = 0;
            int hasLaunchFiles = 0;

            while (tryAndRun < 3)
            {
                if (File.Exists(batFile))
                {
                    hasLaunchFiles++;
                    AppendLog("Attempting to launch Elden Ring");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = batFile,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(batFile)
                    });
                    await Task.Delay(10000); // Increased delay for safety
                    if (IsEldenRingRunning()) return;
                }
                
                if (File.Exists(exeFile))
                {
                    //hasLaunchFiles++;
                    AppendLog("Attempting to launch Elden Ring with exe file");
                    Process.Start(new ProcessStartInfo { FileName = exeFile, UseShellExecute = true });
                    await Task.Delay(10000); // Increased delay for safety
                    if (IsEldenRingRunning()) return;
                }
                
                tryAndRun++;
            }

            if (hasLaunchFiles > 0)
            {
                MessageBox.Show("Failed to start Elden Ring after multiple attempts. Try launching it manually, then launching EldenOverlay.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("launchmod_eldenring.bat not found. Ensure launchmod_eldenring.bat or modengine2_launcher.exe is in the correct directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void StartMonitoringEldenRing()
        {
            checkEldenRingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            checkEldenRingTimer.Tick += (s, e) =>
            {
                if (IsEldenRingRunning())
                {
                    missingCounter = 0;
                }
                else
                {
                    missingCounter++;
                    if (missingCounter >= MissingThreshold)
                    {
                        SetSaveFileToDisable();
                        Application.Current.Shutdown();
                    }
                }
            };
            checkEldenRingTimer.Start();
        }

        private bool IsEldenRingRunning()
        {
            return Process.GetProcessesByName("eldenring").Length > 0;
        }

        private void SetSaveFileToDisable()
        {
            if (!string.IsNullOrEmpty(saveFilePath))
            {
                File.WriteAllText(saveFilePath, "Disable");
                AppendLog("Set save.txt to Disable");
            }
        }
    
/// <summary>
/// Dynamically computes the path to save.txt by getting the overlay directory,
/// trimming any trailing separators, then moving one level up.
/// </summary>
private string GetSaveFilePath()
        {
            String savePath = RegistryHelper.LoadSaveFilePath();
            AppendLog($"Computed save file path: {savePath}");
            // Check if file exists, if not, create it with a default value
            if (!File.Exists(savePath))
            {
                try
                {
                    File.WriteAllText(savePath, "Disable"); // Default HP value
                    AppendLog("save.txt did not exist, created with default value Disable.");
                }
                catch (Exception ex)
                {
                    AppendLog($"Error creating save.txt: {ex.Message}");
                    return null;
                }
            }

            return savePath;
        }

        /// <summary>
        /// Sets up the overlay window and starts a timer that checks fullscreen status.
        /// </summary>
        private void InitializeOverlay()
        {
            // Set up window properties for overlay appearance
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            timer.Tick += (sender, args) => CheckFullscreenStatus();
            timer.Start();
        }

        /// <summary>
        /// Checks if the Elden Ring window is in fullscreen mode (using DPI-adjusted screen size) 
        /// and shows or hides the overlay accordingly.
        /// </summary>
        private void CheckFullscreenStatus()
        {
            var hwnd = FindWindow(null, "ELDEN RING™");
            if (hwnd != IntPtr.Zero && IsWindowVisible(hwnd))
            {
                GetWindowRect(hwnd, out RECT rect);
                int windowWidth = rect.Right - rect.Left;
                int windowHeight = rect.Bottom - rect.Top;

                PresentationSource source = PresentationSource.FromVisual(this);
                double dpiFactorX = 1.0, dpiFactorY = 1.0;
                if (source != null)
                {
                    Matrix m = source.CompositionTarget.TransformToDevice;
                    dpiFactorX = m.M11;
                    dpiFactorY = m.M22;
                }
                double physicalScreenWidth = SystemParameters.PrimaryScreenWidth * dpiFactorX;
                double physicalScreenHeight = SystemParameters.PrimaryScreenHeight * dpiFactorY;

                AppendLog($"Checking fullscreen: Window size: {windowWidth} x {windowHeight}");
                AppendLog($"Physical screen size: {physicalScreenWidth} x {physicalScreenHeight}");

                // Check if the game is in fullscreen mode
                if (windowWidth == physicalScreenWidth && windowHeight == physicalScreenHeight)
                {
                    AppendLog("Elden Ring is in fullscreen mode.");
                    if (!this.IsVisible)
                    {
                        AppendLog("Showing overlay.");
                        this.Show();
                    }
                    // Position relative to the game window when fullscreen
                    PositionOverlayRelativeToGameWindow(rect);
                    
                    // Force overlay to remain topmost
                    this.Topmost = true;

                    UpdateHP();
                }
                else
                {
                    AppendLog("Elden Ring is not in fullscreen mode. Hiding overlay.");
                    if (this.IsVisible)
                        this.Hide();
                    forceRun = 0;

                }
            }
            else
            {
                AppendLog("Elden Ring window not found, hiding overlay.");
                if (this.IsVisible)
                    this.Hide();
            }
        }

        private void GetScreenSize()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            double dpiFactorX = 1.0, dpiFactorY = 1.0;
            if (source != null)
            {
                Matrix m = source.CompositionTarget.TransformToDevice;
                dpiFactorX = m.M11;
                dpiFactorY = m.M22;
            }

            screenWidth = SystemParameters.PrimaryScreenWidth * dpiFactorX;
            screenHeight = SystemParameters.PrimaryScreenHeight * dpiFactorY;

            // HP Bar width = 30% of screen width
            hpBarMaxWidth = screenWidth * 0.3;

            AppendLog($"Screen Resolution: {screenWidth} x {screenHeight}");
            AppendLog($"HP Bar Max Width: {hpBarMaxWidth}");
        }
        private void PositionOverlayRelativeToGameWindow(RECT rect)
        {
            // Set overlay size based on screen size
            this.Width = screenWidth * 0.35;  // 35% of screen width
            this.Height = screenHeight * 0.10; // 8% of screen height

            // Position the overlay relative to the game window
            this.Left = rect.Left + (rect.Right - rect.Left) * 0.023;  // 7% from the left of the game window
            this.Top = rect.Top + (rect.Bottom - rect.Top) * -0.007;    // 2% from the top of the game window

            // Set the HP Bar and dark overlay widths dynamically
            hpBarMaxWidth = this.Width * 0.94;  // 93% of the overlay width
            //HPBar.Width = hpBarMaxWidth;

            // Position the background container so it overlays the HP bar appropriately.

            Canvas.SetLeft(HPBar, this.Width * 0.029);
            Canvas.SetTop(HPBar, this.Height * 0.475);

            AppendLog("Overlay positioned relative to the game window.");
        }

        private void FadeOutStoryboard_Completed(object sender, EventArgs e)
        {
            // Unsubscribe from the event to prevent duplicate calls.
            fadeOutStoryboard.Completed -= FadeOutStoryboard_Completed;
            // Now hide the window after the fade-out animation completes.
            this.Hide();
        }

        /// <summary>
        /// Reads the HP value from save.txt and updates the HP label.
        /// </summary>
        // Flags to track the state
        private bool isFadedOut = false;
        private bool isFadedIn = true;

        // Updated HP update method
        private void UpdateHP()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    
                    string hpValue = File.ReadAllText(saveFilePath).Trim();
                    AppendLog($"Read HP value: '{hpValue}'");

                    // Check if the value is "Disable"
                    if (hpValue.Equals("Disable", StringComparison.OrdinalIgnoreCase))
                    {
                        Task.Delay(1000).Wait(); // Delay to prevent flickering
                        // Only trigger fade-out if it's not already faded out
                        if (!isFadedOut)
                        {
                            // Attach Completed event so we can hide the window after fading
                            fadeOutStoryboard.Completed += FadeOutStoryboard_Completed;
                            fadeOutStoryboard.Begin(this);
                            isFadedOut = true;
                            isFadedIn = false;
                        }
                    }
                    else
                    {
                        if (forceRun < 3)
                        {
                            fadeOutStoryboard.Completed += FadeOutStoryboard_Completed;
                            fadeOutStoryboard.Begin(this);
                            isFadedOut = true;
                            isFadedIn = false;
                            forceRun++;
                        }
                        // Trigger fade-in only if it's not already faded in
                        if (!isFadedIn)
                        {
                            // Before fading in, make sure the window is visible and its opacity is 0
                            this.Show();
                            this.Opacity = 0;
                            this.Topmost = true; // Ensure it stays on top
                            fadeInStoryboard.Begin(this);
                            isFadedIn = true;
                            isFadedOut = false;
                        }
                        else
                        {
                            //Something here to keep it on top
                            this.Topmost = false;
                            this.Topmost = true;
                        }

                        // Handle HP value updates if the content isn't "Disable"
                        if (double.TryParse(hpValue, out double hp)) // Support decimals
                        {
                            //HPLabel.Text = $"HP: {hp:F2}";
                            UpdateHPBar(hp);
                        }
                        else
                        {
                            //HPLabel.Text = "HP: Invalid value";
                            AppendLog($"Error: Invalid HP: '{hpValue}'");
                        }
                    }
                }
                else
                {
                    //HPLabel.Text = "HP: N/A";
                }
            }
            catch (Exception ex)
            {
                //HPLabel.Text = "HP: Error";
                AppendLog($"Error reading {saveFilePath}: {ex.Message}");
            }
        }

        private void UpdateHPBar(double hp)
        {
            double scale = Math.Clamp((double)hp / MaxHP, 0, 1); // Normalize HP to 0-1
            HPBar.Width = hpBarMaxWidth * scale;

            Color darkOrange = Color.FromRgb(204, 85, 0);
            Color orangeYellow = Color.FromRgb(255, 170, 0);
            Color darkYellow = Color.FromRgb(204, 170, 0);

            // Example transition value (0.0 - 1.0)
            Color resultColor = LerpColor(darkOrange, orangeYellow, darkYellow, scale);

            // Apply the color (assuming HPBar is a control that supports Background)
            HPBar.Fill = new SolidColorBrush(resultColor);
        }

        private Color LerpColor(Color start, Color mid, Color end, double t)
        {
            if (t < 0.5) // Transition from Dark Orange to Orange-Yellow
            {
                double normalizedT = t / 0.5;
                return Color.FromRgb(
                    (byte)(start.R + (mid.R - start.R) * normalizedT),
                    (byte)(start.G + (mid.G - start.G) * normalizedT),
                    (byte)(start.B + (mid.B - start.B) * normalizedT)
                );
            }
            else // Transition from Orange-Yellow to Dark Yellow
            {
                double normalizedT = (t - 0.5) / 0.5;
                return Color.FromRgb(
                    (byte)(mid.R + (end.R - mid.R) * normalizedT),
                    (byte)(mid.G + (end.G - mid.G) * normalizedT),
                    (byte)(mid.B + (end.B - mid.B) * normalizedT)
                );
            }
        }


        /// <summary>
        /// Logs messages to the TextBox for debugging.
        /// </summary>
        private void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                //if (LogTextBox != null)
                //{
                    //LogTextBox.AppendText(message + Environment.NewLine);
                    //LogTextBox.ScrollToEnd(); // Auto-scroll to the latest log
                //}
            });
        }

    }
}
