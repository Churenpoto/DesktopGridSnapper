namespace DesktopGridSnapper
{
    /// <summary>
    /// アプリ設定データ
    /// ※ Load / Save の責任は MainForm に持たせる
    /// </summary>
    public class AppSettings
    {
        public int CellWidth { get; set; } = 64;
        public int CellHeight { get; set; } = 52;

        public int OffsetX { get; set; } = 14;
        public int OffsetY { get; set; } = 4;

        public int ScreenIndex { get; set; } = 0;

        // グリッド線の色（RGB）
        public int GridColorRgb { get; set; } = Color.CornflowerBlue.ToArgb();

        // ★ グリッド線の透明度（0–255）
        public int GridAlpha { get; set; } = 255;
    }

}
