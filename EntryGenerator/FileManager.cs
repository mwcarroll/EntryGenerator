using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EntryGenerator.Enums;
using Size = System.Drawing.Size;

namespace EntryGenerator
{
    public static class FileManager
    {
        public static ImageSource GetImageSource(string filename)
        {
            return GetImageSource(filename, new Size(16, 16));
        }

        public static ImageSource GetImageSource(string filename, Size size)
        {
            using (Icon icon = ShellManager.GetIcon(Path.GetExtension(filename), ItemType.File, IconSize.Small, ItemState.Undefined))
            {
                return Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(size.Width, size.Height));
            }
        }
    }
}
