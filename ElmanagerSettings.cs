using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Elmanager
{
    [Serializable]
    public class ElmanagerSettings
    {
        private static string SettingsFile = "Elmanager" + Global.Version + ".dat";
        public GeneralSettings General = new GeneralSettings();
        public LevelEditorSettings LevelEditor = new LevelEditorSettings();
        public ReplayManagerSettings ReplayManager = new ReplayManagerSettings();
        public ReplayViewerSettings ReplayViewer = new ReplayViewerSettings();

        public static ElmanagerSettings Load()
        {
            if (File.Exists(Path.Combine(Application.StartupPath, SettingsFile)))
            {
                return GetSettings(SettingsFile);
            }
            var oldSettingFiles = Directory.GetFiles(Application.StartupPath, "Elmanager*.dat");
            try
            {
                if (oldSettingFiles.Length > 0)
                    return GetSettings(oldSettingFiles[0]);
            }
            catch (Exception)
            {
                Utils.ShowError("Could not load old settings. You need to set them again.");
            }
            return new ElmanagerSettings();
        }

        public void Save()
        {
            var appSettingsFile = new FileStream(Path.Combine(Application.StartupPath, SettingsFile), FileMode.Create);
            var binFormatter = new BinaryFormatter();
            binFormatter.Serialize(appSettingsFile, this);
            appSettingsFile.Close();
        }

        private static ElmanagerSettings GetSettings(string settingsFile)
        {
            var appSettingsFile = new FileStream(Path.Combine(Application.StartupPath, settingsFile), FileMode.Open);
            var binFormatter = new BinaryFormatter();
            var loadedSettings = (ElmanagerSettings) (binFormatter.UnsafeDeserialize(appSettingsFile, null));
            appSettingsFile.Close();
            return loadedSettings;
        }

        [Serializable]
        public class GeneralSettings
        {
            public bool CheckForUpdatesOnStartup = true;
            public string LevelDirectory = string.Empty;
            public string LgrDirectory = string.Empty;
            public string ReplayDirectory = string.Empty;
        }

        [Serializable]
        public class LevelEditorSettings
        {
            public double AutoGrassThickness = 0.2;
            public string BaseFilename = "MyLev";
            public double CaptureRadius = 0.015;
            public bool CheckTopologyDynamically;
            public bool CheckTopologyWhenSaving;
            public string DefaultTitle = "New level";
            public double DrawStep = 1.0;
            public Color HighlightColor = Color.Yellow;
            public double InitialHeight = 50.0;
            public double InitialWidth = 50.0;
            public string LastLevel;
            public int MouseClickStep = 50;
            public string NumberFormat = "0";
            public double PipeRadius = 1.0;
            public RenderingSettings RenderingSettings = new RenderingSettings();
            public Color SelectionColor = Color.Blue;
            public Size Size = new Size(800, 600);
            public bool SnapToGrid;
            public bool UseFilenameForTitle;
            public bool UseFilenameSuggestion;
            public bool UseHighlight = true;
            public FormWindowState WindowState = FormWindowState.Normal;
        }

        [Serializable]
        public class ReplayManagerSettings
        {
            public bool ConfirmDelete = true;
            public string DbFile = string.Empty;
            public bool Decimal3Shown;
            public bool DelToRecycle = true;
            public Point Location;
            public string MoveToPath = string.Empty;
            public string Nickname = "Nick";
            public bool NitroReplays;
            public string Pattern = "LNT";
            public byte[] ReplayListState;
            public bool SearchLevSubDirs = true;
            public string SearchPattern = string.Empty;
            public bool SearchRecSubDirs = true;
            public bool ShowGridInReplayList = true;
            public Size Size = new Size(800, 600);
            public bool UseDataBase;
            public bool WarnAboutOldDb = true;
            public FormWindowState WindowState = FormWindowState.Normal;
        }

        [Serializable]
        public class ReplayViewerSettings
        {
            public Color ActivePlayerColor = Color.Black;
            public bool DontSelectPlayersByDefault;
            public bool DrawOnlyPlayerFrames = true;
            public bool DrawTransparentInactive = true;
            public bool FollowDriver = true;
            public double FrameStep = 0.02;
            public bool HideStartObject = true;
            public Color InactivePlayerColor = Color.Green;
            public bool LockedCamera;
            public bool LoopPlaying;
            public int MouseClickStep = 50;
            public int MouseWheelStep = 50;
            public bool MultiSpy;
            public bool PicturesInBackGround;
            public RenderingSettings RenderingSettings = new RenderingSettings();
            public bool ShowBikeCoords = true;
            public bool ShowDriverPath;
            public Size Size = new Size(800, 600);
            public FormWindowState WindowState = FormWindowState.Normal;
        }
    }
}