using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DesktopGridSnapper
{
    public class DesktopIconManager
    {
        // ListView
        private const int LVM_FIRST = 0x1000;
        private const int LVM_GETNEXTITEM = LVM_FIRST + 12;
        private const int LVM_GETITEMPOSITION = LVM_FIRST + 16;
        private const int LVM_SETITEMPOSITION32 = LVM_FIRST + 49;

        private const int LVNI_SELECTED = 0x0002;
        private const int LVNI_FOCUSED = 0x0001;

        // keys
        private const int VK_LBUTTON = 0x01;
        private const int VK_MENU = 0x12; // Alt
        private const int VK_LEFT = 0x25;
        private const int VK_UP = 0x26;
        private const int VK_RIGHT = 0x27;
        private const int VK_DOWN = 0x28;

        private bool wasDragging = false;

        // Alt押しっぱなし対応：矢印の押下エッジで1回だけ処理する
        private bool arrowConsumed = false;

        // 直近操作したアイコン
        private int lastActiveIndex = -1;

        public void SnapIconsToGrid(GridOverlay grid)
        {
            var overlays = new Dictionary<Screen, GridOverlay>();
            if (grid != null)
                overlays[grid.TargetScreen] = grid;
            SnapIconsToGrids(overlays);
        }

        public void SnapIconsToGrids(Dictionary<Screen, GridOverlay> overlays)
        {
            if (overlays == null || overlays.Count == 0) return;

            bool altPressed = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;
            bool left = (GetAsyncKeyState(VK_LEFT) & 0x8000) != 0;
            bool right = (GetAsyncKeyState(VK_RIGHT) & 0x8000) != 0;
            bool up = (GetAsyncKeyState(VK_UP) & 0x8000) != 0;
            bool down = (GetAsyncKeyState(VK_DOWN) & 0x8000) != 0;

            bool anyArrow = left || right || up || down;
            bool mouseDragging = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;

            // Alt + Arrow : move selected icons by 1 cell
            if (altPressed && anyArrow && !arrowConsumed)
            {
                arrowConsumed = true;

                IntPtr list = FindDesktopListView();
                if (list == IntPtr.Zero) return;

                var indices = GetSelectedIndices(list);

                if (indices.Count == 0 && lastActiveIndex >= 0)
                    indices.Add(lastActiveIndex);

                if (indices.Count == 0) return;

                // capture originals
                var originals = new List<(int idx, POINT pt)>();
                foreach (int idx in indices)
                {
                    if (TryGetItemPosition(list, idx, out POINT pt))
                        originals.Add((idx, pt));
                }

                // compute targets
                var targets = new List<(int idx, Point pos)>();
                foreach (var item in originals)
                {
                    Point iconPos = new Point(item.pt.x, item.pt.y);
                    GridOverlay? grid = GetOverlayForPoint(overlays, iconPos);
                    if (grid == null) continue;

                    var cell = grid.GetCellFromPoint(iconPos);
                    if (right) cell.Col++;
                    if (left) cell.Col--;
                    if (down) cell.Row++;
                    if (up) cell.Row--;

                    Point target = grid.GetPointFromCell(cell);
                    targets.Add((item.idx, target));
                }

                // apply
                foreach (var t in targets)
                    TrySetItemPosition(list, t.idx, t.pos.X, t.pos.Y);

                lastActiveIndex = indices[0];
                return;
            }

            if (!anyArrow) arrowConsumed = false;

            // Mouse drag: snap only on release, and only for current selected icon(s).
            if (mouseDragging)
            {
                wasDragging = true;

                IntPtr list = FindDesktopListView();
                if (list != IntPtr.Zero)
                {
                    int idx = GetFirstSelectedOrFocused(list);
                    if (idx >= 0) lastActiveIndex = idx;
                }
                return;
            }

            if (!wasDragging) return;
            wasDragging = false;

            IntPtr list2 = FindDesktopListView();
            if (list2 == IntPtr.Zero) return;

            var indices2 = GetSelectedIndices(list2);
            if (indices2.Count == 0 && lastActiveIndex >= 0)
                indices2.Add(lastActiveIndex);
            if (indices2.Count == 0) return;

            // snap all selected on release
            foreach (int idx in indices2)
            {
                if (!TryGetItemPosition(list2, idx, out POINT pt2))
                    continue;

                Point iconPos = new Point(pt2.x, pt2.y);
                GridOverlay? grid = GetOverlayForPoint(overlays, iconPos);
                if (grid == null) continue;

                Point snapped = grid.GetSnappedPoint(iconPos);
                if (snapped.X == pt2.x && snapped.Y == pt2.y)
                    continue;

                TrySetItemPosition(list2, idx, snapped.X, snapped.Y);
            }
        }

        private GridOverlay? GetOverlayForPoint(Dictionary<Screen, GridOverlay> overlays, Point pt)
        {
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.Contains(pt) && overlays.TryGetValue(screen, out var overlay))
                    return overlay;
            }
            return null;
        }

        private int GetFirstSelectedOrFocused(IntPtr list)
        {
            int idx = (int)NativeMethods.SendMessage(list, LVM_GETNEXTITEM, new IntPtr(-1), new IntPtr(LVNI_SELECTED));
            if (idx >= 0) return idx;
            idx = (int)NativeMethods.SendMessage(list, LVM_GETNEXTITEM, new IntPtr(-1), new IntPtr(LVNI_FOCUSED));
            return idx;
        }

        private List<int> GetSelectedIndices(IntPtr list)
        {
            var indices = new List<int>();
            int i = -1;
            while (true)
            {
                i = (int)NativeMethods.SendMessage(list, LVM_GETNEXTITEM, (IntPtr)i, (IntPtr)LVNI_SELECTED);
                if (i < 0) break;
                indices.Add(i);
            }
            return indices;
        }

        private IntPtr FindDesktopListView()
        {
            IntPtr shellView = IntPtr.Zero;

            NativeMethods.EnumWindows((hwnd, _) =>
            {
                var sb = new StringBuilder(256);
                NativeMethods.GetClassName(hwnd, sb, sb.Capacity);
                string cls = sb.ToString();

                if (cls == "WorkerW" || cls == "Progman")
                {
                    IntPtr defView = NativeMethods.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (defView != IntPtr.Zero)
                    {
                        shellView = defView;
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);

            if (shellView == IntPtr.Zero) return IntPtr.Zero;

            IntPtr list = NativeMethods.FindWindowEx(shellView, IntPtr.Zero, "SysListView32", "FolderView");
            if (list == IntPtr.Zero)
                list = NativeMethods.FindWindowEx(shellView, IntPtr.Zero, "SysListView32", null);

            return list;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

        private bool TryGetItemPosition(IntPtr list, int index, out POINT pt)
        {
            pt = default;

            NativeMethods.GetWindowThreadProcessId(list, out uint pid);
            IntPtr hProc = NativeMethods.OpenProcess(
                NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE,
                false, pid);

            if (hProc == IntPtr.Zero) return false;

            IntPtr remote = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf<POINT>();
                remote = NativeMethods.VirtualAllocEx(hProc, IntPtr.Zero, (uint)size,
                    NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE, NativeMethods.PAGE_READWRITE);

                if (remote == IntPtr.Zero) return false;

                NativeMethods.SendMessage(list, LVM_GETITEMPOSITION, (IntPtr)index, remote);

                byte[] buf = new byte[size];
                if (!NativeMethods.ReadProcessMemory(hProc, remote, buf, size, out _))
                    return false;

                pt = BytesToStruct<POINT>(buf);
                return true;
            }
            finally
            {
                if (remote != IntPtr.Zero)
                    NativeMethods.VirtualFreeEx(hProc, remote, 0, NativeMethods.MEM_RELEASE);
                NativeMethods.CloseHandle(hProc);
            }
        }

        private bool TrySetItemPosition(IntPtr list, int index, int x, int y)
        {
            NativeMethods.GetWindowThreadProcessId(list, out uint pid);
            IntPtr hProc = NativeMethods.OpenProcess(
                NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_WRITE,
                false, pid);

            if (hProc == IntPtr.Zero) return false;

            IntPtr remote = IntPtr.Zero;
            try
            {
                POINT pt = new POINT { x = x, y = y };
                byte[] buf = StructToBytes(pt);

                remote = NativeMethods.VirtualAllocEx(hProc, IntPtr.Zero, (uint)buf.Length,
                    NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE, NativeMethods.PAGE_READWRITE);

                if (remote == IntPtr.Zero) return false;

                if (!NativeMethods.WriteProcessMemory(hProc, remote, buf, buf.Length, out _))
                    return false;

                NativeMethods.SendMessage(list, LVM_SETITEMPOSITION32, (IntPtr)index, remote);
                return true;
            }
            finally
            {
                if (remote != IntPtr.Zero)
                    NativeMethods.VirtualFreeEx(hProc, remote, 0, NativeMethods.MEM_RELEASE);
                NativeMethods.CloseHandle(hProc);
            }
        }

        private static byte[] StructToBytes<T>(T obj) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(obj, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
                return arr;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        private static T BytesToStruct<T>(byte[] arr) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(arr.Length);
            try
            {
                Marshal.Copy(arr, 0, ptr, arr.Length);
                return Marshal.PtrToStructure<T>(ptr)!;
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        internal static class NativeMethods
        {
            public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string cls, string? title);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern int GetClassName(IntPtr hWnd, StringBuilder sb, int cap);

            [DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            public const uint PROCESS_VM_OPERATION = 0x0008;
            public const uint PROCESS_VM_READ = 0x0010;
            public const uint PROCESS_VM_WRITE = 0x0020;

            public const uint MEM_COMMIT = 0x1000;
            public const uint MEM_RESERVE = 0x2000;
            public const uint MEM_RELEASE = 0x8000;
            public const uint PAGE_READWRITE = 0x04;

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool inherit, uint pid);

            [DllImport("kernel32.dll")]
            public static extern bool CloseHandle(IntPtr h);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr VirtualAllocEx(IntPtr hProc, IntPtr addr, uint size, uint type, uint protect);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool VirtualFreeEx(IntPtr hProc, IntPtr addr, uint size, uint freeType);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadProcessMemory(IntPtr hProc, IntPtr addr, byte[] buf, int size, out IntPtr read);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr hProc, IntPtr addr, byte[] buf, int size, out IntPtr written);
        }
    }
}
