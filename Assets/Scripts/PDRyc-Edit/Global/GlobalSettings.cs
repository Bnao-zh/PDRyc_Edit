using Newtonsoft.Json;
using UnityEngine;

namespace PDRO.Global
{
    public class GlobalSettings
    {
        [JsonIgnore]
        public static GlobalSettings CurrentSettings
        {
            get
            {
                if (!PlayerPrefs.HasKey("Global_Settings")) return new GlobalSettings();
                var str = PlayerPrefs.GetString("Global_Settings");
                return JsonConvert.DeserializeObject<GlobalSettings>(str);
            }
            set
            {
                var str = JsonConvert.SerializeObject(value, Formatting.None);
                PlayerPrefs.SetString("Global_Settings", str);
            }
        }

        //现在在哪个分页：0 Audio，1 Gameplay，2 Others
        public int NowWhichSettings { get; set; }
        //玩家id，后续迁移到联网
        public string PlayerName { get; set; }
        //还是只有谱面延迟吧，其他奇奇怪怪的延迟确实没什么用。。。
        public float GlobalOffset { get; set; }
        //一些音频设置
        public float MainVolume { get; set; }
        public float MusicVolume { get; set; }
        public float HitSoundVolume { get; set; }
        public float UISoundVolume { get; set; }
        public int DSPBufferSize { get; set; }
        //游玩中比较重要的 看名字即可
        public bool ArcLockPoint { get; set; }
        public float GlobalNoteSpeed { get; set; }
        public bool CameraResux { get; set; }
        public bool SteamLoad { get; set; }
        public bool SteamDispose { get; set; }
        public bool HalfResolution { get; set; }

        public GlobalSettings()
        {
            NowWhichSettings = 1;
            PlayerName = "其实这个昵称是可以改的";
            GlobalOffset = 0f;
            MainVolume = 1f;
            MusicVolume = 1f;
            HitSoundVolume = 1f;
            UISoundVolume = 1f;
            GlobalNoteSpeed = 1f;
            DSPBufferSize = 0;
            CameraResux = false;
            ArcLockPoint = false;
            HalfResolution = false;
        }
    }
}
