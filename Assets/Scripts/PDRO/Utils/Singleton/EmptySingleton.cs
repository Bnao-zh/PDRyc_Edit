namespace PDRO.Utils.Singleton
{
    public class EmptySingleton : MonoSingleton<EmptySingleton>
    {
        protected override void OnAwake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}