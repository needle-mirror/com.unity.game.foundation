using System;

namespace UnityEngine.GameFoundation.Promise
{
    /// <summary>
    /// Handle to settle a promise.
    /// </summary>
    public struct Completer
    {
        static public Completer None => default;

        /// <summary>
        /// The token is stored because promises can be recycled and change their token;
        /// but a completer should complete only the promise with the token it had at creation.
        /// </summary>
        long m_Token;

        Promise m_Promise;

        internal Completer(Promise promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.token;
        }

        /// <summary>
        /// Fulfill the handled promise.
        /// </summary>
        public void Resolve() => m_Promise?.Resolve(m_Token);

        /// <summary>
        /// Reject the handled promise for the given reason.
        /// </summary>
        /// <param name="reason">
        /// The exception that prevented the handled promise to be fulfilled.
        /// </param>
        public void Reject(Exception reason) => m_Promise?.Reject(m_Token, reason);
    }

    /// <inheritdoc cref="Completer" />
    /// <typeparam name="TResult">Type of the result of the async operation.</typeparam>
    public struct Completer<TResult>
    {
        /// <inheritdoc cref="Completer.m_Token" />
        long m_Token;

        Promise<TResult> m_Promise;

        internal Completer(Promise<TResult> promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.token;
        }

        /// <summary>
        /// Fulfill the handled promise with the given data.
        /// </summary>
        public void Resolve(TResult value) => m_Promise?.Resolve(m_Token, value);

        /// <inheritdoc cref="Completer.Reject" />
        public void Reject(Exception reason) => m_Promise?.Reject(m_Token, reason);
    }
}
