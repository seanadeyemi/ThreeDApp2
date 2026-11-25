using System;
using System.Collections.Generic;
using System.IO;

namespace ThreeDApp2
{
    /// <summary>User preferences, kept in a small key=value file next to the autosave.</summary>
    public class AppSettings
    {
        /// <summary>Open on the centred start view (vertical axis upright in the middle) instead of the last view.</summary>
        public bool CenterOnLaunch = true;

        /// <summary>Paint ground and sky with a horizon instead of a plain background.</summary>
        public bool GroundAndSky = true;

        /// <summary>Show the navigation gizmo in the top-right corner of the viewport.</summary>
        public bool ShowGizmo = true;

        public static AppSettings Load(string path)
        {
            AppSettings s = new AppSettings();
            try
            {
                if (!File.Exists(path)) return s;
                foreach (string raw in File.ReadAllLines(path))
                {
                    string line = raw.Trim();
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    bool value = line.Substring(eq + 1).Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (key == "CenterOnLaunch") s.CenterOnLaunch = value;
                    else if (key == "GroundAndSky") s.GroundAndSky = value;
                    else if (key == "ShowGizmo") s.ShowGizmo = value;
                }
            }
            catch (Exception)
            {
                // Unreadable settings: fall back to the defaults.
            }
            return s;
        }

        public void Save(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllLines(path, new[]
            {
                "CenterOnLaunch=" + (CenterOnLaunch ? "true" : "false"),
                "GroundAndSky=" + (GroundAndSky ? "true" : "false"),
                "ShowGizmo=" + (ShowGizmo ? "true" : "false"),
            });
        }
    }
}
