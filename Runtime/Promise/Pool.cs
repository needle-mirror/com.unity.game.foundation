using System;
using System.Collections.Generic;

namespace UnityEngine.Promise
{
    /// <summary>
    /// A simple pooling class.
    /// </summary>
    /// <typeparam name="TPooled">The type of object to pool</typeparam>
    class Pool<TPooled> where TPooled : class
    {
        /// <summary>
        /// The pooled objects
        /// </summary>
        readonly Queue<TPooled> m_Pool = new Queue<TPooled>();

        /// <summary>
        /// A method called to create a new object.
        /// </summary>
        readonly Func<TPooled> m_Create;

        /// <summary>
        /// A method called when releasing an object.
        /// </summary>
        readonly Action<TPooled> m_Release;

        /// <summary>
        /// Creates a new pool.
        /// </summary>
        /// <param name="create">A method called to create a new object.</param>
        /// <param name="release">A method called on the object when
        /// released.</param>
        public Pool(Func<TPooled> create, Action<TPooled> release)
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }

            m_Create = create;
            m_Release = release;
        }

        /// <summary>
        /// Gets a pooled objects or creates it.
        /// </summary>
        /// <returns>A <see cref="TPooled"/> object.</returns>
        public TPooled Get()
        {
            if (m_Pool.Count > 0)
            {
                return m_Pool.Dequeue();
            }

            return m_Create();
        }

        /// <summary>
        /// Releases a poolable object.
        /// </summary>
        /// <param name="pooled">The object to release</param>
        public void Release(TPooled pooled)
        {
            m_Release?.Invoke(pooled);
            m_Pool.Enqueue(pooled);
        }
    }
}
