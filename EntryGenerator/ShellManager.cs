using System;
using System.Drawing;
using System.Runtime.InteropServices;
using EntryGenerator.Enums;
using EntryGenerator.Structs;

namespace EntryGenerator
{
    public class ShellManager
    {
        public static Icon GetIcon(string path, ItemType type, IconSize iconSize, ItemState state)
        {
            uint attributes = (uint)(type == ItemType.Folder ? FileAttribute.Directory : FileAttribute.File);
            uint flags = (uint)(ShellAttribute.Icon | ShellAttribute.UseFileAttributes);

            if (type == ItemType.Folder && state == ItemState.Open)
            {
                flags |= (uint)ShellAttribute.OpenIcon;
            }
            if (iconSize == IconSize.Small)
            {
                flags |= (uint)ShellAttribute.SmallIcon;
            }
            else
            {
                flags |= (uint)ShellAttribute.LargeIcon;
            }

            ShellFileInfo fileInfo = new ShellFileInfo();
            uint size = (uint)Marshal.SizeOf(fileInfo);
            IntPtr result = Interop.SHGetFileInfo(path, attributes, out fileInfo, size, flags);

            if (result == IntPtr.Zero)
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            try
            {
                return (Icon)Icon.FromHandle(fileInfo.hIcon).Clone();
            }
            finally
            {
                Interop.DestroyIcon(fileInfo.hIcon);
            }
        }
    }
}
