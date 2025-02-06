using UnityEngine;
using Object = UnityEngine.Object;

namespace PDRO.Utils.Pool
{
    public abstract class ObjectPoolBase<T> where T : MonoBehaviour
    {
        protected T PoolObject;
        protected Transform Parent;
        public int Length = 0;

        protected ObjectPoolBase(T @object, int poolLength, Transform parent)
        {
            Parent = parent;
        }

        protected T CreateObject()
        {
            Length++;
            var obj = Object.Instantiate(PoolObject, Parent);
            obj.gameObject.SetActive(false);
            return obj;
        }

        protected virtual T GetObject() => null;

        public virtual void ReturnObject(T obj)
        {
        }
    }
}
