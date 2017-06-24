namespace SuperSight
{
    using System.IO;

    using Rage;

    internal class Settings
    {
        private const string IniFileName = Plugin.ResourcesFolder + "settings.ini";

        public InitializationFile IniFile { get; }

        public Settings()
        {
            if (!File.Exists(IniFileName))
            {
                Game.LogTrivial($"The .ini file '{IniFileName}' doesn't exist, creating default...");
                CreateDefault();
            }

            IniFile = new InitializationFile(IniFileName);
        }

        private void CreateDefault()
        {
            using (StreamWriter writer = new StreamWriter(IniFileName, false))
            {
                writer.WriteLine($"[Heli Cam Settings]");
                writer.WriteLine($"ToggleCameraKey = U");
                writer.WriteLine($"ToggleNightVisionKey = NumPad9");
                writer.WriteLine($"ToggleThermalVisionKey = NumPad7");
    }
        }
    }
}
