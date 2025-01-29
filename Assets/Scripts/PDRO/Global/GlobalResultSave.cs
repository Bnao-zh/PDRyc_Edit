using System.Linq;
using Newtonsoft.Json;
using PDRO.Utils;
using PDRO.Utils.Tools;
using UnityEngine;
using Record = PDRO.Gameplay.Managers.ScoreRecord;
using Dict = System.Collections.Generic.Dictionary<string, PDRO.Gameplay.Managers.ScoreRecord>;

namespace PDRO.Global
{
    public static class GlobalResultSave
    {
        public static Dict BestPerformances { get; private set; }
        public static Dict LastPerformances { get; private set; }

        private static readonly string SaveKey = StringEncryptor.Encrypt("Best_Performances");

        public static void RefreshPerformances()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                BestPerformances = JsonConvert.DeserializeObject<Dict>(StringEncryptor.Decrypt(PlayerPrefs.GetString(SaveKey)));
            }
            else BestPerformances = new Dict();

            LastPerformances = new Dict();
        }

        public static void SavePerformances()
        {
            Debug.Log(JsonConvert.SerializeObject(BestPerformances));
            PlayerPrefs.SetString(SaveKey, StringEncryptor.Encrypt(JsonConvert.SerializeObject(BestPerformances)));
        }

        public static void CommitPerformance(string id, Record record)
        {
            if (LastPerformances.ContainsKey(id))
                LastPerformances[id] = record;
            else LastPerformances.Add(id, record);
        }

        public static void PushAllPerformances()
        {
            var pairs = LastPerformances.Select(x => x).ToArray();
            pairs.ForEach(x => PushPerformance(x.Key));
        }

        public static void PushPerformance(string id)
        {
            var last = LastPerformances[id];
            if (BestPerformances.ContainsKey(id))
            {
                var best = BestPerformances[id];
                if (last.ToScore() > best.ToScore()) BestPerformances[id] = last;
            }
            else BestPerformances.Add(id, last);
        }

        public static Record FindPerformance(string id)
        {
            if (BestPerformances.ContainsKey(id))
                return BestPerformances[id];
            return new Record(1);
        }
    }
}
