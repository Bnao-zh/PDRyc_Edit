using System;
using DG.Tweening;
using Newtonsoft.Json;
using PDRO.Utils;
using PDRO.Utils.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace PDRO.Gameplay.Managers
{
    public class ScoreManager : MonoSingleton<ScoreManager>
    {
        [SerializeField] private Text scoreShower, comboShower;
        [NonSerialized] public ScoreRecord CurrentScoreRecord;

        [SerializeField] private Image PMImage, FRImage;

        public void AddScoreRecord(JudgeResult result)
        {
            CurrentScoreRecord.AddScoreRecord(result);

            var combo = CurrentScoreRecord.ToCombo(false);

            scoreShower.text = $"{CurrentScoreRecord}";
            comboShower.text = combo > 1 ? $"{combo}" : "";

            ToPMFRIndicator();
        }

        private void ToPMFRIndicator()
        {
            var state = CurrentScoreRecord.ToComboState();

            if (state == ComboState.FullRecall)
            {
                PMImage.gameObject.SetActive(false);
            }
            else if (state == ComboState.TrackComplete)
            {
                PMImage.gameObject.SetActive(false);
                FRImage.gameObject.SetActive(false);
            }
        }
    }

    [Serializable]
    public class ScoreRecord
    {
        [JsonProperty("lc")] public int LastCombo { get; set; } = 0;
        [JsonProperty("mc")] public int MaxCombo { get; set; } = 0;
        [JsonProperty("PE")] public int PURE { get; set; } = 0;
        [JsonProperty("pe")] public int Pure { get; set; } = 0;
        [JsonProperty("gd")] public int Far { get; set; } = 0;
        [JsonProperty("bd")] public int Lost { get; set; } = 0;

        public bool IsPlayed { get; set; } = false;

        public int NoteCount { get; set; } = 1;
        public int JudgedNoteCount => PURE + Pure + Far + Lost;

        public ScoreRecord(int noteCount)
        {
            NoteCount = noteCount;
        }

        public void AddScoreRecord(JudgeResult result)
        {
            switch (result)
            {
                case JudgeResult.PERFECT:
                    PURE++;
                    LastCombo++;
                    break;
                case JudgeResult.Perfect:
                    Pure++;
                    LastCombo++;
                    break;
                case JudgeResult.Good:
                    Far++;
                    LastCombo++;
                    break;
                case JudgeResult.Bad:
                    Lost++;
                    LastCombo = 0;
                    break;
            }
        }

        public float ToScore()
        {
            return PURE + (PURE + Pure + Far * 0.5f) / NoteCount * 10000000f;
        }

        public int ToFixedScore() => Mathf.RoundToInt(ToScore());

        public override string ToString() => $"{ToFixedScore():00000000}";

        public int ToCombo(bool isMaxCombo)
        {
            MaxCombo = LastCombo > MaxCombo ? LastCombo : MaxCombo;
            return isMaxCombo ? MaxCombo : LastCombo;
        }

        public float ToAcc()
        {
            if (JudgedNoteCount == 0) return 0f;
            return (PURE + Pure + Far * 0.5f) / JudgedNoteCount;
        }

        public ResultLevel ToResultLevel()
        {
            var score = ToFixedScore();
            return score switch
            {
                >= 9900000 => ResultLevel.Exp,
                >= 9800000 => ResultLevel.Ex,
                >= 9500000 => ResultLevel.AA,
                >= 9200000 => ResultLevel.A,
                >= 8900000 => ResultLevel.B,
                >= 8600000 => ResultLevel.C,
                _ => ResultLevel.D
            };
        }

        public ComboState ToComboState()
        {
            //if (PURE == JudgedNoteCount) return ComboState.Theory;

            if (Far == 0 && Lost == 0) return ComboState.PureMemory;

            if (Lost == 0) return ComboState.FullRecall;

            return ComboState.TrackComplete;
        }

    }
    public enum ResultLevel
    {
        Exp = 6, Ex = 5, AA = 4, A = 3, B = 2, C = 1, D = 0
    }

    public enum ComboState
    {
        PureMemory = 3, FullRecall = 2, TrackComplete = 1, TrackLost = 0
    }
}