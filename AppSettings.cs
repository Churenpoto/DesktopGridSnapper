namespace DesktopGridSnapper
{
    /// <summary>
    /// アプリ設定データ
    /// ※ Load / Save の責任は MainForm に持たせる
    /// </summary>
    public class AppSettings
    {
        public int CellWidth { get; set; }
        public int CellHeight { get; set; }

        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public int ScreenIndex { get; set; }

        // グリッド線の色（RGB）
        public int GridColorRgb { get; set; } = Color.CornflowerBlue.ToArgb();

        // ★ グリッド線の透明度（0–255）
        public int GridAlpha { get; set; } = 120;
    }

}
