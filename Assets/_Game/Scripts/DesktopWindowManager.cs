using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class DesktopWindowManager : MonoBehaviour
{
    public static DesktopWindowManager Instance { get; private set; }

    [Header("Window Dimensions")]
    public int currentWidth = 280;
    public int currentHeight = 252;

#if !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, out RECT pvParam, uint fWinIni);

    private const uint SpiGetworkarea = 0x0030;
    private const int GwlStyle = -16;
    private const uint WsPopup = 0x80000000;
    private const uint WsVisible = 0x10000000;
    private const uint WsCaption = 0x00C00000;
    private const uint WsThickframe = 0x00040000;
    private const uint WsMinimizebox = 0x00020000;
    private const uint WsMaximizebox = 0x00010000;
    private const uint WsSysmenu = 0x00080000;

    private static readonly IntPtr HwndTopmost = new IntPtr(-1);

    private const uint SwpNosize = 0x0001;
    private const uint SwpNomove = 0x0002;
    private const uint SwpFramechanged = 0x0020;
    private const uint SwpShowwindow = 0x0040;

    private IntPtr windowHandle = IntPtr.Zero;

    private struct WorkLimits
    {
        public int MinX;
        public int MaxX;
        public int MinY;
        public int MaxY;
    }

    private WorkLimits workArea;
#endif

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ResizeWindow(int newWidth, int newHeight, bool removeRibbon = false)
    {
        StartCoroutine(ResizeRoutine(newWidth, newHeight, removeRibbon));
    }

    private IEnumerator ResizeRoutine(int width, int height, bool removeRibbon)
    {
        currentWidth = width;
        currentHeight = height;

        Screen.SetResolution(width, height, FullScreenMode.Windowed);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

#if !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
        if (windowHandle == IntPtr.Zero)
        {
            windowHandle = FindCurrentProcessWindow();
        }

        if (windowHandle != IntPtr.Zero)
        {
            if (removeRibbon)
            {
                uint currentStyle = (uint)GetWindowLong(windowHandle, GwlStyle);
                uint newStyle = (currentStyle | WsPopup | WsVisible) & 
                                ~(WsCaption | WsThickframe | WsMinimizebox | WsMaximizebox | WsSysmenu);
                SetWindowLong(windowHandle, GwlStyle, unchecked((int)newStyle));

                UpdateWorkAreaLimits();
                if (SystemParametersInfo(SpiGetworkarea, 0, out RECT r, 0))
                {
                    int screenW = r.Right - r.Left;
                    int screenH = r.Bottom - r.Top;
                    int centerX = r.Left + (screenW / 2) - (width / 2);
                    int centerY = r.Top + (screenH / 2) - (height / 2);

                    SetWindowPos(windowHandle, HwndTopmost, centerX, centerY, width, height, 
                                 SwpFramechanged | SwpShowwindow);
                }
            }
            else
            {
                uint currentStyle = (uint)GetWindowLong(windowHandle, GwlStyle);
                uint newStyle = currentStyle | WsCaption | WsMinimizebox | WsSysmenu;
                newStyle &= ~(WsPopup | WsMaximizebox | WsThickframe);
                
                SetWindowLong(windowHandle, GwlStyle, unchecked((int)newStyle));

                UpdateWorkAreaLimits();
                if (SystemParametersInfo(SpiGetworkarea, 0, out RECT r, 0) && GetWindowRect(windowHandle, out RECT winRect))
                {
                    int screenW = r.Right - r.Left;
                    int screenH = r.Bottom - r.Top;
                    int winW = winRect.Right - winRect.Left;
                    int winH = winRect.Bottom - winRect.Top;
                    int centerX = r.Left + (screenW / 2) - (winW / 2);
                    int centerY = r.Top + (screenH / 2) - (winH / 2);

                    SetWindowPos(windowHandle, HwndTopmost, centerX, centerY, 0, 0, 
                                 SwpNosize | SwpFramechanged | SwpShowwindow);
                }
            }
        }
#endif
    }

    public void UpdateWorkAreaLimits()
    {
#if !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
        if (SystemParametersInfo(SpiGetworkarea, 0, out RECT r, 0))
        {
            workArea.MinX = r.Left;
            workArea.MaxX = r.Right - currentWidth;
            workArea.MinY = r.Top;
            workArea.MaxY = r.Bottom - currentHeight;
        }
#endif
    }

    public void MoveWindowClamped(int rawX, int rawY)
    {
#if !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
        if (windowHandle == IntPtr.Zero) return;

        int clampedX = Mathf.Clamp(rawX, workArea.MinX, workArea.MaxX);
        int clampedY = Mathf.Clamp(rawY, workArea.MinY, workArea.MaxY);

        SetWindowPos(windowHandle, HwndTopmost, clampedX, clampedY, 0, 0, SwpNosize | SwpShowwindow);
#endif
    }

    public bool GetCurrentWindowPosition(out int x, out int y)
    {
#if !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
        if (windowHandle != IntPtr.Zero && GetWindowRect(windowHandle, out RECT rect))
        {
            x = rect.Left;
            y = rect.Top;
            return true;
        }
#endif
        x = 0;
        y = 0;
        return false;
    }

#if !UNITY_EDITOR && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
    private IntPtr FindCurrentProcessWindow()
    {
        uint currentPid = (uint)Process.GetCurrentProcess().Id;
        IntPtr foundHwnd = IntPtr.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint pid);
            if (pid == currentPid)
            {
                int style = GetWindowLong(hWnd, GwlStyle);
                if ((style & (int)WsVisible) != 0)
                {
                    foundHwnd = hWnd;
                    return false;
                }
            }
            return true;
        }, IntPtr.Zero);

        return foundHwnd;
    }
#endif
}