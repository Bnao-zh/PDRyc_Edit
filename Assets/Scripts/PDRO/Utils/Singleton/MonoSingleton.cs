using UnityEngine;

namespace PDRO.Utils.Singleton
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {

        public static T Instance { get; private set; }

        public static string SceneToDo { get; private set; } = "Scenes/114514Scene（看到我就是你忘了设SceneToDo）";
        public static void SetSceneToDo(string sceneName) => SceneToDo = sceneName;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = GetComponent<T>();
            }
            else Destroy(this);
            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}