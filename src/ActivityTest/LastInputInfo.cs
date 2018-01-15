using System;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace ActivityTest
{
    public class LastInputInfo
    {
        public static TimeSpan? GetUserInactiveTime()
        {
            var info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (GetLastInputInfo(ref info))
                return TimeSpan.FromMilliseconds(Environment.TickCount - info.dwTime);
            return null;
        }

        public static TimeSpan? GetUserInactiveTime2()
        {
            var info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (GetLastInputInfo(ref info))
                return TimeSpan.FromMilliseconds(UnsignedTickCount() - info.dwTime);
            return null;
        }

        public static int TickCount()
        {
            return Environment.TickCount;
        }

        public static int UnsignedTickCount()
        {
            return Environment.TickCount & int.MaxValue;
        }

        public static uint? InputInfo()
        {
            var info = new LASTINPUTINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            if (GetLastInputInfo(ref info))
                return info.dwTime;
            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        public static uint? GetTickCount_32()
        {
            try
            {
                return GetTickCount();
            }
            catch
            {
                return null;
            }
        }

        public static ulong? GetTickCount_64()
        {
            try
            {
                return GetTickCount64();
            }
            catch
            {
                return null;
            }
        }


        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        [DllImport("kernel32.dll")]
        static extern ulong GetTickCount64();
    }
}