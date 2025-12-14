using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DesktopGridSnapper
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // ★ 多重起動チェックと先行プロセスのKILL
            try
            {
                string currentProcessName = Process.GetCurrentProcess().ProcessName;
                int currentProcessId = Process.GetCurrentProcess().Id;

                var otherInstances = Process.GetProcessesByName(currentProcessName)
                    .Where(p => p.Id != currentProcessId)
                    .ToList();

                foreach (var proc in otherInstances)
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit(2000); // 最大2秒待つ
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"既存のプロセスを終了できませんでした:\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"多重起動チェック中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm(args));
        }

    }
}
