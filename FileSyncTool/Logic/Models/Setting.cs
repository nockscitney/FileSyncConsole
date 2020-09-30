namespace NickScotney.FileSync.Logic.Models
{
    public class Setting
    {
        public string SettingName { get; set; }
        public object SettingValue { get; set; }

        public Setting()
            : base()
        {
            SettingValue = null;
        }
    }
}
