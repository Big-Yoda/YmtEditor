using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YMTEditor
{
    public class Update
    {
        public VersionData versionData;
        public bool isLatest = true;
        public async void main(System.Windows.Window win)
        {
            if (!hasInternet()) return;
            var client = new HttpClient();
            var uri = new Uri("https://raw.githubusercontent.com/Big-Yoda/YmtEditor/master/version.json");
            Stream respStream = await client.GetStreamAsync(uri);
            versionData = new JsonSerializer().Deserialize<VersionData>(new JsonTextReader(new StreamReader(respStream)));
            Debug.WriteLine(float.Parse(Properties.Resources.version, CultureInfo.InvariantCulture.NumberFormat).ToString());
            isLatest = float.Parse(Properties.Resources.version.ToString(), CultureInfo.InvariantCulture.NumberFormat) == float.Parse(versionData.version.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            if (isLatest) return;
            DialogResult diaRes = MessageBox.Show($"New version found! Do you want to update now?\nChangelog v{versionData.version}:\n{versionData.message}\n\nYour Version: {Properties.Resources.version}, {Properties.Resources.stage}", $"Update, Version: {versionData.version}, {versionData.stage}", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (diaRes == DialogResult.Yes) {
                win.Close();
                Process.Start("https://discord.gg/t4CFXBn");
            }
        }
        public bool hasInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class VersionData
    {
        public string message { get; set; }
        public string version { get; set; }
        public string stage { get; set; }
        public string[] changelog { get; set; }
    }
}
