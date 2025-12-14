using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;

namespace DesktopGridSnapper
{
    public partial class MainForm : Form
    {
        private Dictionary<Screen, GridOverlay> overlays = new Dictionary<Screen, GridOverlay>();
        private readonly DesktopIconManager iconManager = new DesktopIconManager();

        private AppSettings settings = new AppSettings();
        private readonly string settingsPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "DesktopGridSnapper", "settings.json");

        private Screen selectedScreen = Screen.PrimaryScreen;
        private Color gridColor = Color.FromArgb(unchecked((int)0x78007AFF));

        private bool gridEnabled = true;

        private Color gridBaseColor = Color.CornflowerBlue;
        private int gridAlpha = 120;

        public MainForm()
        {
            InitializeComponent();

            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open Settings", null, trayOpen_Click);
            trayMenu.Items.Add("Toggle Grid", null, trayToggleGrid_Click);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Exit", null, trayExit_Click);

            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "Desktop Grid Snapper",
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += trayOpen_Click;
            timer1.Tick += timer1_Tick;

            LoadScreens();
            LoadSettings();

            // 画面選択を確定
            if (comboScreens.SelectedIndex >= 0 && comboScreens.SelectedIndex < Screen.AllScreens.Length)
                selectedScreen = Screen.AllScreens[comboScreens.SelectedIndex];
            else
                selectedScreen = Screen.PrimaryScreen;

            // 起動時に自動適用（必要なければコメントアウトOK）
            ApplyGrid();
            this.AcceptButton = applyButton;
        }

        private void LoadScreens()
        {
            comboScreens.Items.Clear();
            foreach (var s in Screen.AllScreens)
            {
                string name = s.DeviceName + (s.Primary ? " (Primary)" : "");
                comboScreens.Items.Add(name);
            }
            if (comboScreens.Items.Count > 0)
                comboScreens.SelectedIndex = 0;
        }

        private void comboScreens_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = comboScreens.SelectedIndex;
            if (idx >= 0 && idx < Screen.AllScreens.Length)
                selectedScreen = Screen.AllScreens[idx];
        }

        private void ApplyGrid()
        {
            Color finalGridColor =
                Color.FromArgb(gridAlpha, gridBaseColor);

            // Apply grid settings to all screens
            foreach (var screen in Screen.AllScreens)
            {
                if (!overlays.ContainsKey(screen))
                {
                    System.Diagnostics.Debug.WriteLine($"Create GridOverlay for {screen.DeviceName}");
                    overlays[screen] = new GridOverlay(
                        screen,
                        (int)numericCellW.Value,
                        (int)numericCellH.Value,
                        (int)numericOffsetX.Value,
                        (int)numericOffsetY.Value,
                        finalGridColor
                    );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Update GridOverlay for {screen.DeviceName}");
                    overlays[screen].UpdateGrid(
                        (int)numericCellW.Value,
                        (int)numericCellH.Value,
                        (int)numericOffsetX.Value,
                        (int)numericOffsetY.Value,
                        finalGridColor
                    );
                }

                if (gridEnabled)
                    overlays[screen].Show();
                else
                    overlays[screen].Hide();
            }

            // Clean up overlays for screens that no longer exist
            var currentScreens = new HashSet<Screen>(Screen.AllScreens);
            var screensToRemove = new List<Screen>();
            foreach (var screen in overlays.Keys)
            {
                if (!currentScreens.Contains(screen))
                {
                    screensToRemove.Add(screen);
                }
            }

            foreach (var screen in screensToRemove)
            {
                overlays[screen].Close();
                overlays.Remove(screen);
            }
        }



        private void applyButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            ApplyGrid();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            iconManager.SnapIconsToGrids(overlays);
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                    if (loaded != null) settings = loaded;
                }
            }
            catch
            {
                settings = new AppSettings();
            }

            // UIへ反映（範囲外は丸める）
            numericCellW.Value = ClampToNumeric(numericCellW, settings.CellWidth);
            numericCellH.Value = ClampToNumeric(numericCellH, settings.CellHeight);
            numericOffsetX.Value = ClampToNumeric(numericOffsetX, settings.OffsetX);
            numericOffsetY.Value = ClampToNumeric(numericOffsetY, settings.OffsetY);

            if (settings.ScreenIndex >= 0 && settings.ScreenIndex < comboScreens.Items.Count)
                comboScreens.SelectedIndex = settings.ScreenIndex;

            gridBaseColor = Color.FromArgb(settings.GridColorRgb);
            gridAlpha = settings.GridAlpha;

            panelGridColor.BackColor =
                Color.FromArgb(gridAlpha, gridBaseColor);
            numericGridAlpha.Value = gridAlpha;
        }

        private decimal ClampToNumeric(NumericUpDown nud, int value)
        {
            if (value < nud.Minimum) value = (int)nud.Minimum;
            if (value > nud.Maximum) value = (int)nud.Maximum;
            return value;
        }

        private void SaveSettings()
        {
            settings.CellWidth = (int)numericCellW.Value;
            settings.CellHeight = (int)numericCellH.Value;
            settings.OffsetX = (int)numericOffsetX.Value;
            settings.OffsetY = (int)numericOffsetY.Value;
            settings.ScreenIndex = comboScreens.SelectedIndex;
            settings.GridColorRgb = gridBaseColor.ToArgb();
            settings.GridAlpha = (int)numericGridAlpha.Value;

            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath)!);
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
        }
        private void trayOpen_Click(object? sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        }

        private void buttonGridColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = gridBaseColor;

            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                gridBaseColor = colorDialog1.Color;

                Color finalColor =
                    Color.FromArgb(gridAlpha, gridBaseColor);

                panelGridColor.BackColor = finalColor;

                foreach (var overlay in overlays.Values)
                {
                    overlay.SetGridColor(finalColor);
                    overlay.Invalidate();
                }
            }
        }

        private void trayToggleGrid_Click(object? sender, EventArgs e)
        {
            gridEnabled = !gridEnabled;

            if (gridEnabled)
            {
                foreach (var overlay in overlays.Values)
                    overlay.Show();
            }
            else
            {
                foreach (var overlay in overlays.Values)
                    overlay.Hide(); // ★ Closeしない
            }

        }

        private void trayExit_Click(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            foreach (var overlay in overlays.Values)
                overlay.Close();
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            SaveSettings();
            foreach (var overlay in overlays.Values)
                overlay.Close();
            base.OnFormClosing(e);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void numericGridAlpha_ValueChanged(object sender, EventArgs e)
        {
            gridAlpha = (int)numericGridAlpha.Value;

            foreach (var overlay in overlays.Values)
            {
                overlay.SetGridOpacity(gridAlpha);
                overlay.Invalidate(); // ★ 再描画（これが無いと見た目が変わらない）
            }
        }

    }
}
