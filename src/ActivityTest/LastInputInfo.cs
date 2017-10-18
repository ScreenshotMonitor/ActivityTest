using System;
using System.Runtime.InteropServices;

namespace ActivityTest
{
    public class LastInputInfo
    {
        public static TimeSpan? GetUserInactiveTime()
        {
            var info = new LASTINPUTINFO();
            info.cbSize = (uint) Marshal.SizeOf(info);
            if (GetLastInputInfo(ref info))
                return TimeSpan.FromMilliseconds(Environment.TickCount - info.dwTime);
            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    }
}