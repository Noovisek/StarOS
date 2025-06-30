using Cosmos.System.Graphics;
using StarOS.Resources;

namespace StarOS
{
    public static class Boot
    {
        public static void OnBoot()
        {
            Gui.Wallpaper = new Bitmap(Files.StarOSBackgroundRaw);
            Gui.Cursor = new Bitmap(Files.StarOSCursorRaw);

            // Inicjalizacja ikon docka
            Gui.SettingsRaw = new Bitmap(Files.SettingsRaw);
            Gui.NotepadRaw = new Bitmap(Files.NotepadRaw);
            Gui.StartRaw = new Bitmap(Files.StartRaw);
            Gui.TerminalRaw = new Bitmap(Files.TerminalRaw);

            Gui.StartGUI();
        }
    }
}
