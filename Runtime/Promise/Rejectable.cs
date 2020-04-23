using System;

namespace UnityEngine.Promise
{
    /// <summary>
    /// The <see cref="Rejectable"/> struct is used in the situation when you
    /// need to call <see cref="Completer.Reject(Exception)"/> or
    /// <see cref="Completer{TResult}.Reject(Exception)"/>, but you don't want
    /// to deal with the exact type.
    /// </summary>
    public struct Rejectable
    {
        /// <inheritdoc cref="Completer.m_Token"/>
        long m_Token;

        /// <inheritdoc cref="Completer.m_Promise"/>
        Promise m_Promise;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rejectable"/> struct.
        /// </summary>
        /// <param name="promise"></param>
        internal Rejectable(Promise promise)
        {
            m_Promise = promise;
            if(promise != null)
            {
                m_Token = m_Promise.token;
            }
            else
            {
                m_Token = default;
            }
        }

        /// <inheritdoc cref="Completer.Reject(Exception)"/>
        public void Reject(Exception reason) => m_Promise?.Reject(m_Token, reason);
    }
}
