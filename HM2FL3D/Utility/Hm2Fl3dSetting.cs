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

        /// <summary> 土体单元的 inp 文件的绝对路径 </summary>
        [UserScopedSetting(), DefaultSettingValue(null)]
        public string ZonesInpPath
        {
            get { return this["ZonesInpPath"] as string; }
            set { this["ZonesInpPath"] = value; }
        }

        /// <summary> 结构单元的 inp 文件的绝对路径 </summary>
        [UserScopedSetting(), DefaultSettingValue(null)]
        public string StructuresInpPath
        {
            get { return this["StructuresInpPath"] as string; }
            set { this["StructuresInpPath"] = value; }
        }
    }
}