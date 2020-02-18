using System;

namespace UnityEngine.GameFoundation.Promise
{
    /// <summary>
    /// Handle to read data of a promise.
    /// </summary>
    public struct Deferred
    {
        /// <summary>
        /// The token is stored because promises can be recycled and change their token;
        /// but a deferred should track only the promise with the token it had at creation.
        /// </summary>
        long m_Token;

        Promise m_Promise;

        /// <summary>
        /// A flag to determine if the handled promise has been fulfilled or rejected.
        /// </summary>
        public bool isDone => m_Promise.IsDone(m_Token);

        /// <summary>
        /// A flag to determine if the handled promise has been fulfilled.
        /// </summary>
        public bool isFulfilled => m_Promise.IsFulfilled(m_Token);

        /// <summary>
        /// The exception that prevented the handled promise to be fulfilled if it has been rejected.
        /// </summary>
        public Exception error => m_Promise.GetError(m_Token);

        internal Deferred(Promise promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.token;
        }

        /// <summary>
        /// Get the yield instruction related to the handled promise.
        /// </summary>
        /// <returns>
        /// Returns a yield instruction that will yield as long the handled promise isn't settled.
        /// </returns>
        public CustomYieldInstruction Wait() => m_Promise.Wait(m_Token);

        /// <summary>
        /// Reset the handled promise in order to recycle it.
        /// </summary>
        public void Release() => m_Promise.Release(m_Token);
    }

    /// <inheritdoc cref="Deferred" />
    /// <typeparam name="TResult">Type of the result of the async operation.</typeparam>
    public struct Deferred<TResult>
    {
        /// <inheritdoc cref="Deferred.m_Token" />
        long m_Token;

        Promise<TResult> m_Promise;

        /// <inheritdoc cref="Deferred.isDone" />
        public bool isDone => m_Promise.IsDone(m_Token);

        /// <inheritdoc cref="Deferred.isFulfilled" />
        public bool isFulfilled => m_Promise.IsFulfilled(m_Token);

        /// <summary>
        /// The result of the async operation if the handled promise could be fulfilled.
        /// </summary>
        public TResult result => m_Promise.GetResult(m_Token);

        /// <inheritdoc cref="Deferred.error" />
        public Exception error => m_Promise.GetError(m_Token);

        internal Deferred(Promise<TResult> promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.token;
        }

        /// <inheritdoc cref="Deferred.Wait" />
        public CustomYieldInstruction Wait() => m_Promise.Wait(m_Token);

        /// <inheritdoc cref="Deferred.Release" />
        public void Release() => m_Promise.Release(m_Token);
    }
}
