using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PDRO.Utils
{
    public static class Utility
    {
        // 传入一个AudioClip 会将AudioClip上挂载的音频文件生成频谱到一张Texture2D上
        public static Texture2D GetAudioWaveformT2D(AudioClip clip, float startTime, float endTime)
        {
            int resolution = 80;    // 这个值可以控制频谱的密度吧，我也不知道
            int width = 225;       // 这个是最后生成的Texture2D图片的宽度
            int height = 175;       // 这个是最后生成的Texture2D图片的高度

            resolution = 44100 / resolution;

            float[] samples = new float[(Time2Samples(endTime) - Time2Samples(startTime)) * 2];

            clip.GetData(samples, Time2Samples(startTime));

            float[] waveForm = new float[(samples.Length / resolution)];

            float min = 0;
            float max = 0;

            for (int i = 0; i < waveForm.Length; i++)
            {
                waveForm[i] = 0;

                for (int j = 0; j < resolution; j++)
                {
                    waveForm[i] += Mathf.Abs(samples[(i * resolution) + j]);
                }

                if (waveForm[i] < min)
                {
                    min = waveForm[i];
                }

                if (waveForm[i] > max)
                {
                    max = waveForm[i];
                }
            }

            Color backgroundColor = Color.black;
            Color waveformColor = Color.green;
            Texture2D texture = new Texture2D(width, height);

            Color[] blank = new Color[width * height];

            for (int i = 0; i < blank.Length; ++i)
            {
                blank[i] = backgroundColor;
            }

            texture.SetPixels(blank, 0);

            float xScale = 1f * width / waveForm.Length;

            int tMid = (int)(height / 2.0f);

            var yScale = tMid / max;


            for (int i = 0; i < waveForm.Length; ++i)
            {
                int x = (int)(i * xScale);
                int yOffset = (int)(waveForm[i] * yScale);
                int startY = tMid - yOffset;
                int endY = tMid + yOffset;

                for (int y = startY; y <= endY; ++y)
                {
                    texture.SetPixel(x, y, waveformColor);
                }
            }

            texture.Apply();
            return texture;

            int Time2Samples(float time)
            {
                return Mathf.RoundToInt(time * 44100);
            }
        }
        public static bool TryAndCatch(Action tryToDo, Action<Exception> catchToDo = null)
        {
            try
            {
                tryToDo?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                try
                {
                    catchToDo?.Invoke(e);
                }
                catch (Exception exception)
                {
                    Debug.Log($"Failed To Catch Error!\n{exception.Message}");
                }

                return false;
            }
        }


        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            for (var i = 0; i < array.Length; i++)
            {
                action?.Invoke(array[i]);
            }
        }

        public static void ForEach<T>(this T[] array, Action<T, int> action)
        {
            for (var i = 0; i < array.Length; i++)
            {
                action?.Invoke(array[i], i);
            }
        }

        public static T[] FastSort<T>(this T[] array) where T : IComparable<T>
        {
            FastSort(array, 0, array.Length - 1);
            return array;
        }

        public static List<T> FastSort<T>(this List<T> array) where T : IComparable<T>
        {
            FastSort(array, 0, array.Count - 1);
            return array;
        }

        public static IList<T> FastSort<T>(this IList<T> array) where T : IComparable<T>
        {
            FastSort(array, 0, array.Count - 1);
            return array;
        }

        private static void FastSort<T>(this IList<T> array, int left, int right) where T : IComparable<T>
        {
            while (true)
            {
                if (left >= right) return;
                var leftIndex = left;
                var rightIndex = right;
                var midIndex = left;

                while (leftIndex < rightIndex)
                {
                    while (leftIndex < rightIndex && array[rightIndex].CompareTo(array[midIndex]) >= 0)
                    {
                        rightIndex--;
                    }

                    var temp = array[midIndex];
                    array[midIndex] = array[rightIndex];
                    array[rightIndex] = temp;
                    midIndex = rightIndex;

                    while (leftIndex < rightIndex && array[leftIndex].CompareTo(array[midIndex]) <= 0)
                    {
                        leftIndex++;
                    }

                    temp = array[midIndex];
                    array[midIndex] = array[leftIndex];
                    array[leftIndex] = temp;
                    midIndex = leftIndex;
                }

                FastSort(array, left, midIndex - 1);
                left = midIndex + 1;
            }
        }

        public static T[] FastSort<T>(this T[] array, Func<T, T, bool> compareFunc)
        {
            FastSort(array, 0, array.Length - 1, compareFunc);
            return array;
        }

        public static List<T> FastSort<T>(this List<T> array, Func<T, T, bool> compareFunc)
        {
            FastSort(array, 0, array.Count - 1, compareFunc);
            return array;
        }

        public static IList<T> FastSort<T>(this IList<T> array, Func<T, T, bool> compareFunc)
        {
            FastSort(array, 0, array.Count - 1, compareFunc);
            return array;
        }

        private static void FastSort<T>(this IList<T> array, int left, int right, Func<T, T, bool> compareFunc)
        {
            while (true)
            {
                if (left >= right) return;
                var leftIndex = left;
                var rightIndex = right;
                var midIndex = left;

                while (leftIndex < rightIndex)
                {
                    while (leftIndex < rightIndex && compareFunc.Invoke(array[rightIndex], array[midIndex]))
                    {
                        rightIndex--;
                    }

                    var temp = array[midIndex];
                    array[midIndex] = array[rightIndex];
                    array[rightIndex] = temp;
                    midIndex = rightIndex;

                    while (leftIndex < rightIndex && !compareFunc.Invoke(array[leftIndex], array[midIndex]))
                    {
                        leftIndex++;
                    }

                    temp = array[midIndex];
                    array[midIndex] = array[leftIndex];
                    array[leftIndex] = temp;
                    midIndex = leftIndex;
                }

                FastSort(array, left, midIndex - 1, compareFunc);
                left = midIndex + 1;
            }
        }

        public static void WaitToDo(float waitTime, Action act)
        {
            DOTween.To(() => 0, _ => { }, 0, waitTime).OnComplete(act.Invoke);
        }

        public static void TryAction<T>(Action<T> action, T obj)
        {
            try
            {
                action?.Invoke(obj);
            }
            catch (Exception e)
            {
                Debug.Log($"Failed To Action!\n{e.Message}");
            }
        }

        public static bool Inverse<TKey, TValue>(this Dictionary<TKey, TValue> origin, out Dictionary<TValue, TKey> inverse)
        {
            if (origin is not { Count: > 0 })
            {
                inverse = new Dictionary<TValue, TKey>();
                return false;
            }

            inverse = origin.ToDictionary(keyPair => keyPair.Value, keyPair => keyPair.Key);
            return true;
        }

        public static bool Inverse<TKey, TValue>(this Dictionary<TKey, TValue> origin, out (TValue, TKey)[] inverse)
        {
            if (origin is not { Count: > 0 })
            {
                inverse = Array.Empty<(TValue, TKey)>();
                return false;
            }

            inverse = origin.Select(x => (x.Value, x.Key)).ToArray();
            return true;
        }

        public static bool Inverse<TKey, TValue>(this (TKey, TValue)[] origin, out Dictionary<TValue, TKey> inverse)
        {
            if (origin is not { Length: > 0 })
            {
                inverse = new Dictionary<TValue, TKey>();
                return false;
            }

            inverse = origin.ToDictionary(keyPair => keyPair.Item2, keyPair => keyPair.Item1);
            return true;
        }

        public static bool Inverse<TKey, TValue>(this (TKey, TValue)[] origin, out (TValue, TKey)[] inverse)
        {
            if (origin is not { Length: > 0 })
            {
                inverse = Array.Empty<(TValue, TKey)>();
                return false;
            }

            inverse = origin.Select(x => (x.Item2, x.Item1)).ToArray();
            return true;
        }

        public static TKey GetKey<TKey, TValue>(this Dictionary<TKey, TValue> origin, TValue value) where TKey : Object where TValue : Object
        {
            foreach (var keyPair in origin)
            {
                if (keyPair.Value == value)
                    return keyPair.Key;
            }

            foreach (var keyPair in origin)
            {
                return keyPair.Key;
            }

            return null;
        }
    }

    public static class DoTweenHelper
    {
        public static Tween DoColor(this Graphic graphic, Color color, float duration)
        {
            return DOTween.To(() => graphic.color, x => graphic.color = x, color, duration);
        }

        public static Tween DoFade(this Graphic graphic, float alpha, float duration)
        {
            var color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
            return graphic.DoColor(color, duration);
        }

        public static Tween DoFade(this CanvasGroup group, float alpha, float duration)
        {
            return DOTween.To(() => group.alpha, x => group.alpha = x, alpha, duration);
        }

        public static Tween DoVolume(this AudioSource source, float volume, float duration)
        {
            return DOTween.To(() => source.volume, x => source.volume = x, volume, duration);
        }

        public static Tween DoFill(this Image image, float fill, float duration)
        {
            return DOTween.To(() => image.fillAmount, x => image.fillAmount = x, fill, duration);
        }

        public static Tween DoSize(this Image image, Vector2 size, float duration)
        {
            return DOTween.To(() => image.rectTransform.sizeDelta, x => image.rectTransform.sizeDelta = x, size, duration);
        }
    }

    public static class UnityHelper
    {
        public static Color SetAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Vector2 SetX(this Vector2 vector, float x)
        {
            return new(x, vector.y);
        }

        public static Vector2 SetY(this Vector2 vector, float y)
        {
            return new(vector.x, y);
        }

        public static Vector3 SetX(this Vector3 vector, float x)
        {
            return new(x, vector.y, vector.z);
        }

        public static Vector3 SetY(this Vector3 vector, float y)
        {
            return new(vector.x, y, vector.z);
        }

        public static Vector3 SetZ(this Vector3 vector, float z)
        {
            return new(vector.x, vector.y, z);
        }

        public static Task ToTask(this AsyncOperation operation)
        {
            var taskSource = new TaskCompletionSource<bool>();
            operation.completed += _ => taskSource.TrySetResult(operation.isDone);
            return taskSource.Task;
        }

        public static bool IsChinese(this char c)
        {
            return c >= 0x4E00 && c <= 0x9FA5;
        }

        public static bool HasChinese(this string str)
        {
            var ch = str.ToCharArray();
            return ch.Any(IsChinese);
        }

        public static bool AllChinese(this string str)
        {
            var ch = str.ToCharArray();
            return ch.All(IsChinese);
        }

        public static bool IsInvalid(this string str)
        {
            return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
        }

        public static bool IsValid(this string str) => !IsInvalid(str);

        public static bool Contains(this string str, params string[] subs)
        {
            for (var i = 0; i < subs.Length; i++)
            {
                if (!str.Contains(subs[i]))
                    return false;
            }
            return true;
        }

        public static void KillCoroutine(this MonoBehaviour mono, Coroutine coroutine)
        {
            if (coroutine is not null)
                mono.StopCoroutine(coroutine);
        }
    }
}
