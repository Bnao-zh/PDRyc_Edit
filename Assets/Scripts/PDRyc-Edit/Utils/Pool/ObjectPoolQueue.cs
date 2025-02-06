using System.Collections.Generic;
using UnityEngine;

namespace PDRO.Utils.Pool
{
    /// <summary> 使用Queue队列的GameObject对象池 </summary>
    public class ObjectPoolQueue<T> : ObjectPoolBase<T> where T : MonoBehaviour
    {
        private readonly Queue<T> _pool;
        public int Count => _pool.Count;

        public ObjectPoolQueue(T @object, int poolLength, Transform parent) : base(@object, poolLength, parent)
        {
            PoolObject = @object;
            _pool = new Queue<T>();
            for (var i = 0; i < poolLength; i++)
            {
                var obj = CreateObject();
                _pool.Enqueue(obj);
            }
        }

        protected override T GetObject() => _pool.Count > 0 ? _pool.Dequeue() : CreateObject(); // 如果池子空了就重新创建物体

        public T PrepareObject() // 取出物体
        {
            T obj = GetObject();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public override void ReturnObject(T obj) // 回收物体
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
