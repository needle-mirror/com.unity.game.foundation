using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.GameFoundation.Promise
{
    /// <summary>
    /// A <see cref="Promise"/> pool manager.
    /// </summary>
    public class PromiseGenerator
    {
        /// <summary>
        /// Simple component only existing for the routines to be updated.
        /// </summary>
        class PromiseUpdater : MonoBehaviour
        {
            void Awake()
            {
                //This needs to be called in the awake instead of being called by InitUpdater() to be editor unit tests compliant.
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// A reference of the routine updater.
        /// </summary>
        static PromiseUpdater m_Updater;

        /// <summary>
        /// Initializes the updater of the routines.
        /// </summary>
        static void InitUpdater()
        {
            var go = new GameObject
            {
                hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector
            };
            m_Updater = go.AddComponent<PromiseUpdater>();
        }

        /// <summary>
        /// Clears the list before releasing it in the pool.
        /// </summary>
        /// <param name="list"></param>
        static void Release<TPooled>(List<TPooled> list) => list.Clear();

        /// <summary>
        /// Stores the already created promises for reuse.
        /// </summary>
        Dictionary<Type, Pool<Promise>> m_PromisePools = new Dictionary<Type, Pool<Promise>>();

        /// <summary>
        /// A list of reusable list of deferred, used for the Wait method.
        /// </summary>
        Pool<List<Deferred>> m_DeferredListPool =
            new Pool<List<Deferred>>(() => new List<Deferred>(), Release);

        /// <summary>
        /// A list of reusable list of exceptions, used for the rejection of the
        /// Wait deferred.
        /// </summary>
        Pool<List<Exception>> m_ErrorListPool =
            new Pool<List<Exception>>(() => new List<Exception>(), Release);

        /// <summary>
        /// Helper for token creation.
        /// Initialized with 1 because 0 means None token.
        /// </summary>
        long m_TokenGenerator = 1;

        /// <summary>
        /// Creates a new promise generator.
        /// </summary>
        public PromiseGenerator()
        {
            if (m_Updater == null)
            {
                InitUpdater();
            }
        }

        /// <summary>
        /// Take a promise out of the pool, or create one if it is empty,
        /// and set the two given handlers to handle this promise.
        /// </summary>
        /// <param name="deferred">A handler to read data from the taken promise.</param>
        /// <param name="completer">A handler to write data to the taken promise.</param>
        public void GetPromiseHandles(out Deferred deferred, out Completer completer)
        {
            var promiseType = typeof(Promise);

            if (!m_PromisePools.TryGetValue(promiseType, out var pool))
            {
                pool = new Pool<Promise>(() => new Promise(this), null);

                m_PromisePools.Add(promiseType, pool);
            }

            var promise = pool.Get();

            promise.token = m_TokenGenerator++;

            deferred = new Deferred(promise);
            completer = new Completer(promise);
        }

        /// <summary>
        /// Extract a promise from the pool, or create one if it is empty,
        /// and set the two given handlers to handle this promise.
        /// </summary>
        /// <param name="deferred">A handler to read data from the taken promise.</param>
        /// <param name="completer">A handler to write data to the taken promise.</param>
        /// <typeparam name="TResult">Type of the result of the promise.</typeparam>
        public void GetPromiseHandles<TResult>(out Deferred<TResult> deferred, out Completer<TResult> completer)
        {
            var promiseType = typeof(Promise<TResult>);

            if (!m_PromisePools.TryGetValue(promiseType, out var pool))
            {
                pool = new Pool<Promise>(() => new Promise<TResult>(this), null);
                m_PromisePools.Add(promiseType, pool);
            }

            var promise = pool.Get() as Promise<TResult>;

            promise.token = m_TokenGenerator++;

            deferred = new Deferred<TResult>(promise);
            completer = new Completer<TResult>(promise);
        }

        /// <summary>
        /// Waits for a list of deferred to be done.
        /// </summary>
        /// <param name="deferreds">The list of deferred to wait for.</param>
        /// <param name="tolerant">It true, the returned deferred is fulfilled
        /// even if some deferred from the list are rejected.
        /// Otherwise, one rejection will also reject the returned deferred.
        /// In this case, the errors from all the rejected deferred are
        /// associated in an <see cref="AggregateException"/>.</param>
        /// <returns>A single deferred, done when the given list of
        /// deferred are done.</returns>
        public Deferred Wait
            (IEnumerable<Deferred> deferreds, bool tolerant = true)
        {
            var list = m_DeferredListPool.Get();
            list.AddRange(deferreds);

            GetPromiseHandles(out var deferred, out var completer);

            var routine = WaitRoutine(list, tolerant, completer);
            if (!deferred.isDone)
            {
                m_Updater.StartCoroutine(routine);
            }

            return deferred;
        }

        /// <summary>
        /// This routine waits for all the given deferred to be done, and
        /// resolves or reject a single completer.
        /// </summary>
        /// <param name="deferreds">The list of deferred to check</param>
        /// <param name="tolerant">If true, rejects the given completer with the
        /// list of errors.</param>
        /// <param name="completer">The completer which completes when all the
        /// deferred are done.</param>
        /// <returns></returns>
        IEnumerator WaitRoutine
            (List<Deferred> deferreds, bool tolerant, Completer completer)
        {
            List<Exception> errors = null;

            for (var i = 0; i < deferreds.Count; i++)
            {
                var deferred = deferreds[i];

                while (!deferred.isDone)
                {
                    yield return null;
                }

                if (!deferred.isFulfilled && !tolerant)
                {
                    if (errors == null)
                    {
                        errors = m_ErrorListPool.Get();
                    }

                    errors.Add(deferred.error);
                }

                deferred.Release();
            }

            if (errors != null)
            {
                var error = new AggregateException(errors);
                completer.Reject(error);
                m_ErrorListPool.Release(errors);
            }
            else
            {
                completer.Resolve();
            }

            m_DeferredListPool.Release(deferreds);
        }

        /// <summary>
        /// Releases the given promise for later reuse.
        /// </summary>
        /// <param name="promise">The promise to release.</param>
        internal void Release(Promise promise)
        {
            var promiseType = promise.GetType();

            m_PromisePools.TryGetValue(promiseType, out var pool);

            pool.Release(promise);
        }
    }
}
