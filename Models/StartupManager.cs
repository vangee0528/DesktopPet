using Microsoft.Win32;
using System;
using System.Windows;

namespace DesktopPet
{
    public static class StartupManager
    {
        private const string RegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string AppName = "DesktopPet";

        public static bool IsStartupEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKey);
            var value = key?.GetValue(AppName);
            return value != null;
        }

        public static void SetStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
                if (key == null)
                {
                    MessageBox.Show("无法访问注册表，请以管理员身份运行程序。");
                    return;
                }

                if (enable)
                {
                    var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (string.IsNullOrEmpty(exePath))
                    {
                        MessageBox.Show("无法获取程序路径。");
                        return;
                    }
                    key.SetValue(AppName, exePath);
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"设置开机自启动失败：{ex.Message}");
            }
        }
    }
}
