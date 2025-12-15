using System.Windows.Forms;
using System.Drawing;

namespace DesktopGridSnapper
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private ComboBox comboScreens;
        private NumericUpDown numericCellW;
        private NumericUpDown numericCellH;
        private NumericUpDown numericOffsetX;
        private NumericUpDown numericOffsetY;

        private Button applyButton;
        private Button buttonGridColor;

        private Panel panelGridColor;

        private Label labelScreen;
        private Label labelCell;
        private Label labelOffset;

        private System.Windows.Forms.Timer timer1;
        private ColorDialog colorDialog1;

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private NumericUpDown numericGridAlpha;
        private Label labelGridAlpha;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            comboScreens = new ComboBox();
            numericCellW = new NumericUpDown();
            numericCellH = new NumericUpDown();
            numericOffsetX = new NumericUpDown();
            numericOffsetY = new NumericUpDown();
            applyButton = new Button();
            buttonGridColor = new Button();
            panelGridColor = new Panel();
            labelScreen = new Label();
            labelCell = new Label();
            labelOffset = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            colorDialog1 = new ColorDialog();
            labelGridAlpha = new Label();
            numericGridAlpha = new NumericUpDown();
            trayMenu = new ContextMenuStrip(components);
            toggleButton = new Button();
            ((System.ComponentModel.ISupportInitialize)numericCellW).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericCellH).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericOffsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericOffsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericGridAlpha).BeginInit();
            SuspendLayout();
            // 
            // comboScreens
            // 
            comboScreens.DropDownStyle = ComboBoxStyle.DropDownList;
            comboScreens.Location = new Point(90, 12);
            comboScreens.Name = "comboScreens";
            comboScreens.Size = new Size(230, 28);
            comboScreens.TabIndex = 4;
            comboScreens.SelectedIndexChanged += comboScreens_SelectedIndexChanged;
            // 
            // numericCellW
            // 
            numericCellW.Location = new Point(90, 52);
            numericCellW.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            numericCellW.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
            numericCellW.Name = "numericCellW";
            numericCellW.Size = new Size(90, 27);
            numericCellW.TabIndex = 6;
            numericCellW.Value = new decimal(new int[] { 120, 0, 0, 0 });
            // 
            // numericCellH
            // 
            numericCellH.Location = new Point(190, 52);
            numericCellH.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            numericCellH.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
            numericCellH.Name = "numericCellH";
            numericCellH.Size = new Size(90, 27);
            numericCellH.TabIndex = 7;
            numericCellH.Value = new decimal(new int[] { 96, 0, 0, 0 });
            // 
            // numericOffsetX
            // 
            numericOffsetX.Location = new Point(90, 92);
            numericOffsetX.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numericOffsetX.Minimum = new decimal(new int[] { 200, 0, 0, int.MinValue });
            numericOffsetX.Name = "numericOffsetX";
            numericOffsetX.Size = new Size(90, 27);
            numericOffsetX.TabIndex = 9;
            numericOffsetX.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // numericOffsetY
            // 
            numericOffsetY.Location = new Point(190, 92);
            numericOffsetY.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numericOffsetY.Minimum = new decimal(new int[] { 200, 0, 0, int.MinValue });
            numericOffsetY.Name = "numericOffsetY";
            numericOffsetY.Size = new Size(90, 27);
            numericOffsetY.TabIndex = 10;
            numericOffsetY.Value = new decimal(new int[] { 12, 0, 0, 0 });
            // 
            // applyButton
            // 
            applyButton.Location = new Point(12, 200);
            applyButton.Name = "applyButton";
            applyButton.Size = new Size(154, 34);
            applyButton.TabIndex = 13;
            applyButton.Text = "Apply";
            applyButton.Click += applyButton_Click;
            // 
            // buttonGridColor
            // 
            buttonGridColor.Location = new Point(12, 132);
            buttonGridColor.Name = "buttonGridColor";
            buttonGridColor.Size = new Size(120, 28);
            buttonGridColor.TabIndex = 11;
            buttonGridColor.Text = "Grid Color...";
            buttonGridColor.Click += buttonGridColor_Click;
            // 
            // panelGridColor
            // 
            panelGridColor.BorderStyle = BorderStyle.FixedSingle;
            panelGridColor.Location = new Point(140, 132);
            panelGridColor.Name = "panelGridColor";
            panelGridColor.Size = new Size(42, 28);
            panelGridColor.TabIndex = 12;
            // 
            // labelScreen
            // 
            labelScreen.AutoSize = true;
            labelScreen.Location = new Point(12, 16);
            labelScreen.Name = "labelScreen";
            labelScreen.Size = new Size(53, 20);
            labelScreen.TabIndex = 3;
            labelScreen.Text = "Screen";
            // 
            // labelCell
            // 
            labelCell.AutoSize = true;
            labelCell.Location = new Point(12, 56);
            labelCell.Name = "labelCell";
            labelCell.Size = new Size(64, 20);
            labelCell.TabIndex = 5;
            labelCell.Text = "Cell (px)";
            // 
            // labelOffset
            // 
            labelOffset.AutoSize = true;
            labelOffset.Location = new Point(12, 96);
            labelOffset.Name = "labelOffset";
            labelOffset.Size = new Size(79, 20);
            labelOffset.TabIndex = 8;
            labelOffset.Text = "Offset (x,y)";
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 80;
            // 
            // labelGridAlpha
            // 
            labelGridAlpha.AutoSize = true;
            labelGridAlpha.Location = new Point(12, 167);
            labelGridAlpha.Name = "labelGridAlpha";
            labelGridAlpha.Size = new Size(92, 20);
            labelGridAlpha.TabIndex = 1;
            labelGridAlpha.Text = "Grid Opacity";
            // 
            // numericGridAlpha
            // 
            numericGridAlpha.Location = new Point(119, 167);
            numericGridAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericGridAlpha.Name = "numericGridAlpha";
            numericGridAlpha.Size = new Size(80, 27);
            numericGridAlpha.TabIndex = 2;
            numericGridAlpha.Value = new decimal(new int[] { 120, 0, 0, 0 });
            numericGridAlpha.ValueChanged += numericGridAlpha_ValueChanged;
            // 
            // trayMenu
            // 
            trayMenu.ImageScalingSize = new Size(20, 20);
            trayMenu.Name = "trayMenu";
            trayMenu.Size = new Size(61, 4);
            // 
            // toggleButton
            // 
            toggleButton.Location = new Point(186, 200);
            toggleButton.Name = "toggleButton";
            toggleButton.Size = new Size(134, 34);
            toggleButton.TabIndex = 14;
            toggleButton.Text = "Toggle Grid";
            toggleButton.UseVisualStyleBackColor = true;
            toggleButton.Click += toggleButton_Click;
            // 
            // MainForm
            // 
            ClientSize = new Size(340, 240);
            Controls.Add(toggleButton);
            Controls.Add(labelGridAlpha);
            Controls.Add(numericGridAlpha);
            Controls.Add(labelScreen);
            Controls.Add(comboScreens);
            Controls.Add(labelCell);
            Controls.Add(numericCellW);
            Controls.Add(numericCellH);
            Controls.Add(labelOffset);
            Controls.Add(numericOffsetX);
            Controls.Add(numericOffsetY);
            Controls.Add(buttonGridColor);
            Controls.Add(panelGridColor);
            Controls.Add(applyButton);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Desktop Grid Snapper";
            ((System.ComponentModel.ISupportInitialize)numericCellW).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericCellH).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericOffsetX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericOffsetY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericGridAlpha).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private Button toggleButton;
    }
}
