﻿using System;
using System.Runtime.InteropServices;

using Sharprompt.Internal;

namespace Sharprompt.Drivers;

internal sealed class DefaultConsoleDriver : IConsoleDriver
{
    static DefaultConsoleDriver()
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            throw new InvalidOperationException("Sharprompt requires an interactive environment.");
        }

        Console.TreatControlCAsInput = true;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var hConsole = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);

            if (!NativeMethods.GetConsoleMode(hConsole, out var mode))
            {
                return;
            }

            NativeMethods.SetConsoleMode(hConsole, mode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
    }

    #region IDisposable

    public void Dispose() => Reset();

    #endregion

    #region IConsoleDriver

    public void Beep() => Console.Write("\a");

    public void Reset()
    {
        Console.CursorVisible = true;
        Console.ResetColor();
    }

    public void ClearLine(int top)
    {
        SetCursorPosition(0, top);

        Console.Write("\x1b[2K");
    }

    public ConsoleKeyInfo ReadKey()
    {
        var keyInfo = Console.ReadKey(true);

        if (keyInfo.Key == ConsoleKey.C && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            CancellationCallback?.Invoke();
        }

        return keyInfo;
    }

    public void Write(string value, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }

    public void WriteLine() => Console.WriteLine();

    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

    public bool KeyAvailable => Console.KeyAvailable;

    public bool CursorVisible
    {
        get => Console.CursorVisible;
        set => Console.CursorVisible = value;
    }

    public int CursorLeft => Console.CursorLeft;

    public int CursorTop => Console.CursorTop;

    public int BufferWidth => Console.BufferWidth;

    public int BufferHeight => Console.BufferHeight;

    public Action CancellationCallback { get; set; }

    #endregion
}
