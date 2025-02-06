using UnityEngine;

namespace PDRO.Utils
{
    public static class EaseUtility
    {
        private const float HalfPI = 1.5707964f;
        private const float C1 = 1.70158f;
        private const float C2 = 2.5949095f;
        private const float C3 = 2.70158f;
        private const float C4 = 2.094395103f;
        private const float C5 = 1.396263402f;
        private const float N1 = 7.5625f;
        private const float D1 = 2.75f;
        public enum Ease
        {
            Linear = 0,
            InSine = 1, OutSine = 2, InOutSine = 3,
            InQuad = 4, OutQuad = 5, InOutQuad = 6,
            InCubic = 7, OutCubic = 8, InOutCubic = 9,
            InQuart = 10, OutQuart = 11, InOutQuart = 12,
            InQuint = 13, OutQuint = 14, InOutQuint = 15,
            InExpo = 16, OutExpo = 17, InOutExpo = 18,
            InCirc = 19, OutCirc = 20, InOutCirc = 21,
            InElastic = 22, OutElastic = 23, InOutElastic = 24,
            InBack = 25, OutBack = 26, InOutBack = 27,
            InBounce = 28, OutBounce = 29, InOutBounce = 30,
        }

        //返回0-1的归一化缓动插值，切割版
        public static float Evaluate(Ease easeType, float time, float duration, float startRange, float endRange)
        {
            //整点优化
            if (startRange <= 0f && endRange >= 1f)
            {
                return Evaluate(easeType, time, duration);
            }

            //下面是缓动截取的映射
            var range = endRange - startRange;
            var realTime = duration * startRange + time * range;
            var startEase = Evaluate(easeType, startRange, 1f);
            var endEase = Evaluate(easeType, endRange, 1f);
            var easeRange = endEase - startEase;
            return (Evaluate(easeType, realTime, duration) - startEase) / easeRange;
        }

        //返回0-1的归一化缓动插值，完整版
        public static float Evaluate(Ease easeType, float time, float duration)
        {
            var value = time / duration;
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;

            return easeType switch
            {
                Ease.Linear => value,
                Ease.InSine => 1f - Mathf.Cos(value * HalfPI),
                Ease.OutSine => Mathf.Sin(value * HalfPI),
                Ease.InOutSine => -(Mathf.Cos(value * Mathf.PI) - 1f) * 0.5f,
                Ease.InQuad => value * value,
                Ease.OutQuad => 1f - (1f - value) * (1f - value),
                Ease.InOutQuad => value < 0.5f ? 2f * value * value : 1f - Mathf.Pow(-2f * value + 2f, 2f) * 0.5f,
                Ease.InCubic => value * value * value,
                Ease.OutCubic => 1f - Mathf.Pow(1f - value, 3f),
                Ease.InOutCubic => value < 0.5f ? 4f * Mathf.Pow(value, 3f) : 1f - Mathf.Pow(-2f * value + 2f, 3f) * 0.5f,
                Ease.InQuart => value * value * value * value,
                Ease.OutQuart => 1f - Mathf.Pow(1f - value, 4f),
                Ease.InOutQuart => value < 0.5f ? 8f * Mathf.Pow(value, 4f) : 1f - Mathf.Pow(-2f * value + 2f, 4f) * 0.5f,
                Ease.InQuint => value * value * value * value * value,
                Ease.OutQuint => 1f - Mathf.Pow(1f - value, 5f),
                Ease.InOutQuint => value < 0.5f ? 16f * Mathf.Pow(value, 5f) : 1f - Mathf.Pow(-2f * value + 2f, 5f) * 0.5f,
                Ease.InExpo => value == 0f ? 0f : Mathf.Pow(2f, 10f * value - 10f),
                Ease.OutExpo => value == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * value),
                Ease.InOutExpo => value switch
                {
                    0f => 0f,
                    1f => 1f,
                    _ => value < 0.5f
                        ? Mathf.Pow(2f, 20f * value - 10f) * 0.5f
                        : (2f - Mathf.Pow(2f, -20f * value + 10f)) * 0.5f
                },
                Ease.InCirc => 1f - Mathf.Sqrt(1f - Mathf.Pow(value, 2f)),
                Ease.OutCirc => Mathf.Sqrt(1f - Mathf.Pow(value - 1f, 2f)),
                Ease.InOutCirc => value < 0.5f
                    ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * value, 2f))) * 0.5f
                    : (Mathf.Sqrt(1f - Mathf.Pow(-2f * value + 2f, 2f)) + 1f) * 0.5f,
                Ease.InBack => C3 * value * value * value - C1 * value * value,
                Ease.OutBack => 1f + C3 * Mathf.Pow(value - 1f, 3f) + C1 * Mathf.Pow(value - 1f, 2f),
                Ease.InOutBack => value < 0.5f
                    ? Mathf.Pow(2f * value, 2f) * ((C2 + 1) * 2f * value - C2) * 0.5f
                    : (Mathf.Pow(2f * value - 2f, 2f) * ((C2 + 1f) * (value * 2f - 2f) + C2) + 2f) * 0.5f,
                Ease.InElastic => value switch
                {
                    0f => 0f,
                    1f => 1f,
                    _ => -Mathf.Pow(2f, 10f * value - 10f) * Mathf.Sin((value * 10f - 10.75f) * C4)
                },
                Ease.OutElastic => value switch
                {
                    0f => 0f,
                    1f => 1f,
                    _ => Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * 10f - 0.75f) * C4) + 1f
                },
                Ease.InOutElastic => value switch
                {
                    0f => 0f,
                    1f => 1f,
                    _ => value < 0.5f
                        ? -(Mathf.Pow(2f, 20f * value - 10f) * Mathf.Sin((20f * value - 11.125f) * C5)) * 0.5f
                        : (Mathf.Pow(2f, -20f * value + 10f) * Mathf.Sin((20f * value - 11.125f) * C5)) / 2f + 1f
                },
                Ease.InBounce => 1f - EaseOutBounce(1f - value),
                Ease.OutBounce => EaseOutBounce(value),
                Ease.InOutBounce => value < 0.5f
                    ? (1f - EaseOutBounce(1f - 2f * value)) / 2f
                    : (1f + EaseOutBounce(2f * value - 1f)) / 2f,
                _ => value
            };
        }

        private static float EaseOutBounce(float value)
        {
            if (value < 1f / D1) return N1 * value * value;
            if (value < 2f / D1) return N1 * (value -= 1.5f / D1) * value + 0.75f;
            if (value < 2.5f / D1) return N1 * (value -= 2.25f / D1) * value + 0.9375f;
            else return N1 * (value -= 2.625f / D1) * value + 0.984375f;
        }
    }
}