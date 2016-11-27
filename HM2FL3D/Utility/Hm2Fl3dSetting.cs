using System.Configuration;

namespace Hm2Flac3D.Utility
{
    class Hm2Fl3dSetting : ApplicationSettingsBase
    {
        /// <summary> 工作空间文件夹的路径 </summary>
        [UserScopedSetting(), DefaultSettingValue(null)]
        public string WorkDirectory
        {
            get { return this["WorkDirectory"] as string; }
            set { this["WorkDirectory"] = value; }
        }
    }
}