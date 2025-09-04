// *** PERBAIKAN: Menggunakan namespace WinUI untuk ImageSource ***
using Microsoft.UI.Xaml.Media;

namespace Stacks
{
    public class FileItem
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }

        // *** PERBAIKAN: Mengubah tipe data dari WPF ke WinUI ***
        public ImageSource? Thumbnail { get; set; }
    }
}