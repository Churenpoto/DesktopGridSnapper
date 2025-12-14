using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DesktopGridSnapper
{
    public class GridOverlay : Form
    {
        private readonly Screen targetScreen;
        private int cellWidth;
        private int cellHeight;
        private int iconOffsetX;
        private int iconOffsetY;
        private Color gridColor;

        public Screen TargetScreen => targetScreen;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadyToShow { get; set; } = false;
        public void UpdateGrid(
            int newCellWidth,
            int newCellHeight,
            int newOffsetX,
            int newOffsetY,
            Color newGridColor)
        {
            cellWidth = newCellWidth;
            cellHeight = newCellHeight;
            iconOffsetX = newOffsetX;
            iconOffsetY = newOffsetY;
            gridColor = newGridColor;

            Invalidate();   // Å© Ç±Ç±Ç™èdóv
        }

        public struct GridCell
        {
            public int Col;
            public int Row;
        }

        public void SetGridOpacity(int alpha)
        {
            alpha = Math.Clamp(alpha, 0, 255);

            gridColor = Color.FromArgb(
                alpha,
                gridColor.R,
                gridColor.G,
                gridColor.B
            );
        }

        public void SetGridColor(Color color)
        {
            gridColor = color;
        }

        public GridOverlay(
            Screen screen,
            int cellWidthPx,
            int cellHeightPx,
            int offsetX,
            int offsetY,
            Color gridColor)
        {
            targetScreen = screen;

            cellWidth = Math.Max(16, cellWidthPx);
            cellHeight = Math.Max(16, cellHeightPx);

            iconOffsetX = offsetX;
            iconOffsetY = offsetY;

            this.gridColor = gridColor;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = false;
            DoubleBuffered = true;

            Bounds = targetScreen.Bounds;

            BackColor = Color.Black;
            TransparencyKey = Color.Black;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (ReadyToShow)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            int ex = (int)GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(
                this.Handle,
                GWL_EXSTYLE,
                (IntPtr)(ex | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW)
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Rectangle wa = targetScreen.WorkingArea;
            using var pen = new Pen(gridColor, 1);

            // vertical lines
            for (int x = wa.Left + cellWidth; x < wa.Right; x += cellWidth)
                e.Graphics.DrawLine(pen, x, wa.Top, x, wa.Bottom);

            // horizontal lines
            for (int y = wa.Top + cellHeight; y < wa.Bottom; y += cellHeight)
                e.Graphics.DrawLine(pen, wa.Left, y, wa.Right, y);
        }

        public GridCell GetCellFromPoint(Point p)
        {
            Rectangle wa = targetScreen.WorkingArea;

            int col = (p.X - wa.Left) / cellWidth;
            int row = (p.Y - wa.Top) / cellHeight;

            if (col < 0) col = 0;
            if (row < 0) row = 0;

            return new GridCell { Col = col, Row = row };
        }

        public Point GetPointFromCell(GridCell cell)
        {
            Rectangle wa = targetScreen.WorkingArea;

            int maxCol = Math.Max(0, (wa.Width / cellWidth) - 1);
            int maxRow = Math.Max(0, (wa.Height / cellHeight) - 1);

            int col = Math.Clamp(cell.Col, 0, maxCol);
            int row = Math.Clamp(cell.Row, 0, maxRow);

            int x = wa.Left + col * cellWidth + iconOffsetX;
            int y = wa.Top + row * cellHeight + iconOffsetY;

            return new Point(x, y);
        }

        public Point GetSnappedPoint(Point p) => GetPointFromCell(GetCellFromPoint(p));

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
