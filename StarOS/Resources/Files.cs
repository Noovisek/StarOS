using System;
using IL2CPU.API.Attribs;

namespace StarOS.Resources
{
    public static class Files
    {
        [ManifestResourceStream(ResourceName = "StarOS.Resources.Wallpapers.wallpaper.bmp")]
        public static byte[] StarOSBackgroundRaw; // lub inny typ, który będzie trzymał zasób

        [ManifestResourceStream(ResourceName = "StarOS.Resources.Cursors.cursor.bmp")]
        public static byte[] StarOSCursorRaw; // lub inny typ, który będzie trzymał zasób

        [ManifestResourceStream(ResourceName = "StarOS.Resources.Icons.settings.bmp")]
        public static byte[] SettingsRaw; // lub inny typ, który będzie trzymał zasób

        [ManifestResourceStream(ResourceName = "StarOS.Resources.Icons.notepad.bmp")]
        public static byte[] NotepadRaw; // lub inny typ, który będzie trzymał zasób

        [ManifestResourceStream(ResourceName = "StarOS.Resources.Icons.startlogo.bmp")]
        public static byte[] StartRaw;

        [ManifestResourceStream(ResourceName = "StarOS.Resources.Icons.terminal.bmp")]
        public static byte[] TerminalRaw;

        [ManifestResourceStream(ResourceName = "StarOS.Resources.Icons.3dviever.bmp")]
        public static byte[] Viewer3DRaw;



    }
}
