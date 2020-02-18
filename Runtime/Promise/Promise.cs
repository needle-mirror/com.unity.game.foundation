using System;

namespace UnityEngine.GameFoundation.Promise
{
    /// <summary>
    /// Data contract for an asynchronous operation.
    /// </summary>
    class Promise : CustomYieldInstruction
    {
        internal long token { get; set; }

        internal PromiseGenerator m_Generator;

        Exception m_Error;

        protected bool m_KeepWaiting = true;

        /// <inheritdoc cref="CustomYieldInstruction.keepWaiting" />
        public override bool keepWaiting => m_KeepWaiting;

        internal Promise(PromiseGenerator generator)
        {
            m_Generator = generator;
        }

        internal bool IsDone(long tokenValue)
        {
            ValidateToken(tokenValue);

            return !keepWaiting;
        }

        internal bool IsFulfilled(long tokenValue)
        {
            ValidateToken(tokenValue);

            return !keepWaiting && null == m_Error;
        }

        internal Exception GetError(long tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Error;
        }

        internal CustomYieldInstruction Wait(long tokenValue)
        {
            ValidateToken(tokenValue);

            return this;
        }

        internal void Resolve(long tokenValue)
        {
            var valid = ValidateToken(tokenValue, false);

            if (!valid)
            {
                return;
            }

            m_KeepWaiting = false;
        }

        internal void Reject(long tokenValue, Exception reason)
        {
            var valid = ValidateToken(tokenValue, false);

            if (!valid)
            {
                return;
            }

            m_Error = reason;
            m_KeepWaiting = false;
        }

        internal void Release(long tokenValue)
        {
            ValidateToken(tokenValue);

            ResetData();

            m_Generator.Release(this);
        }

        protected virtual void ResetData()
        {
            token = default;
            m_Error = null;
            m_KeepWaiting = true;
        }

        protected bool ValidateToken(long tokenValue, bool throwError = true)
        {
            var valid = token == tokenValue;

            if (!valid && throwError)
            {
                throw new NullReferenceException("This promise has been released and isn't usable anymore.");
            }

            return valid;
        }
    }

    /// <inheritdoc cref="Promise" />
    /// <typeparam name="TResult">Type of the result of the async operation.</typeparam>
    class Promise<TResult> : Promise
    {
        TResult m_Result;

        internal Promise(PromiseGenerator generator)
            : base(generator) { }

        internal TResult GetResult(long tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Result;
        }

        internal void Resolve(long tokenValue, TResult resultValue)
        {
            var valid = ValidateToken(tokenValue, false);

            if (!valid)
            {
                return;
            }

            m_Result = resultValue;

            m_KeepWaiting = false;
        }

        protected override void ResetData()
        {
            base.ResetData();

            m_Result = default;
        }
    }
}
