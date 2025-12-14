using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;

namespace DesktopGridSnapper
{
    public partial class MainForm : Form
    {
        private readonly bool silentMode = false;

        public MainForm(string[]? args = null)
        {
            InitializeComponent();

            if (args != null && Array.Exists(args, a => a.Equals("-silent", StringComparison.OrdinalIgnoreCase)))
            {
                silentMode = true;
                gridEnabled = false; // ★ silent起動時はグリッド非表示に
            }

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
            trayIcon.MouseUp += trayIcon_MouseUp;
            timer1.Tick += timer1_Tick;

            LoadScreens();
            LoadSettings();

            if (comboScreens.SelectedIndex >= 0 && comboScreens.SelectedIndex < Screen.AllScreens.Length)
                selectedScreen = Screen.AllScreens[comboScreens.SelectedIndex];
            else
                selectedScreen = Screen.PrimaryScreen;

            ApplyGrid();

            if (!silentMode && iconManager.IsAutoArrangeOrSnapToGridEnabled())
            {
                MessageBox.Show(
                    "デスクトップの「アイコンの自動整列」または「等間隔に整列」が有効になっています。\n\n" +
                    "これらが有効だと、アイコンのスナップが正しく動作しない可能性があります。\n\n" +
                    "デスクトップを右クリックして、両方のチェックを外してください。",
                    "整列設定の確認",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            if (!silentMode)
            {
                this.AcceptButton = applyButton;
            }
            else
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Visible = false;
            }

        }

        private GridOverlay? overlay;
        private readonly DesktopIconManager iconManager = new DesktopIconManager();

        private AppSettings settings = new AppSettings();

        private readonly string settingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DGSsettings.json");

        private Screen selectedScreen = Screen.PrimaryScreen;
        private Color gridColor = Color.FromArgb(unchecked((int)0x78007AFF));

        private bool gridEnabled = true;

        private Color gridBaseColor = Color.CornflowerBlue;
        private int gridAlpha = 120;

        private void trayIcon_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                trayMenu.Show(Cursor.Position);
            }
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
            Color finalGridColor = Color.FromArgb(gridAlpha, gridBaseColor);

            if (overlay == null)
            {
                System.Diagnostics.Debug.WriteLine("Create GridOverlay");
                overlay = new GridOverlay(
                    selectedScreen,
                    (int)numericCellW.Value,
                    (int)numericCellH.Value,
                    (int)numericOffsetX.Value,
                    (int)numericOffsetY.Value,
                    finalGridColor
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Update GridOverlay");
                overlay.UpdateGrid(
                    (int)numericCellW.Value,
                    (int)numericCellH.Value,
                    (int)numericOffsetX.Value,
                    (int)numericOffsetY.Value,
                    finalGridColor
                );
            }

            if (gridEnabled && !silentMode)
                overlay.Show();
            else
                overlay?.Hide();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            ApplyGrid();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (overlay != null)
                iconManager.SnapIconsToGrid(overlay);
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

            numericCellW.Value = ClampToNumeric(numericCellW, settings.CellWidth);
            numericCellH.Value = ClampToNumeric(numericCellH, settings.CellHeight);
            numericOffsetX.Value = ClampToNumeric(numericOffsetX, settings.OffsetX);
            numericOffsetY.Value = ClampToNumeric(numericOffsetY, settings.OffsetY);

            if (settings.ScreenIndex >= 0 && settings.ScreenIndex < comboScreens.Items.Count)
                comboScreens.SelectedIndex = settings.ScreenIndex;

            gridBaseColor = Color.FromArgb(settings.GridColorRgb);
            gridAlpha = settings.GridAlpha;

            panelGridColor.BackColor = Color.FromArgb(gridAlpha, gridBaseColor);
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

                Color finalColor = Color.FromArgb(gridAlpha, gridBaseColor);
                panelGridColor.BackColor = finalColor;

                if (overlay != null)
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
                if (overlay == null)
                {
                    ApplyGrid(); // overlay を生成
                }

                BeginInvoke(() =>
                {
                    if (overlay != null)
                    {
                        overlay.Show();
                    }
                });
            }
            else
            {
                overlay?.Hide();
            }
        }

        private void trayExit_Click(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            overlay?.Close();
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
            overlay?.Close();
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

            if (overlay != null)
            {
                overlay.SetGridOpacity(gridAlpha);
                overlay.Invalidate();
            }
        }
    }
}
