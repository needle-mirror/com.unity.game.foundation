using System;

namespace UnityEngine.Promise
{
    /// <summary>
    /// Data contract for an asynchronous operation.
    /// </summary>
    class Promise : CustomYieldInstruction
    {
        /// <summary>
        /// The token used to keep a soft reference between this promise and its
        /// <see cref="Completer"/> and <see cref="Deferred"/> instances.
        /// </summary>
        internal long token { get; set; }

        /// <summary>
        /// The total number of steps the process controling the completer needs
        /// to achieve.
        /// </summary>
        internal int totalSteps;

        /// <summary>
        /// The index of the current step the process is at.
        /// </summary>
        internal int currentStep;

        /// <summary>
        /// A reference to the <see cref="PromiseGenerator"/> this
        /// <see cref="Promise"/> comes from.
        /// </summary>
        internal PromiseGenerator m_Generator;

        /// <summary>
        /// A reference to this error that has prevented this
        /// <see cref="Promise"/> to be fulfilled.
        /// </summary>
        Exception m_Error;

        /// <summary>
        /// Used by the <see cref="CustomYieldInstruction"/> to control the
        /// lifecycle of the instruction.
        /// </summary>
        protected bool m_KeepWaiting = true;

        /// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
        public override bool keepWaiting => m_KeepWaiting;

        /// <summary>
        /// Initializes a new instance of the <see cref="Promise"/> class.
        /// </summary>
        /// <param name="generator">The generator this promise comes
        /// from.</param>
        internal Promise(PromiseGenerator generator)
        {
            m_Generator = generator;
        }

        /// <inheritdoc cref="Deferred.isDone"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance.</param>
        /// <returns><c>true</c> if done, <c>false</c> otherwise.</returns>
        internal bool IsDone(long tokenValue)
        {
            ValidateToken(tokenValue);

            return !keepWaiting;
        }

        /// <inheritdoc cref="Deferred.isFulfilled"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance.</param>
        /// <returns><c>true</c> if fulfilled, <c>false</c> otherwise.</returns>
        internal bool IsFulfilled(long tokenValue)
        {
            ValidateToken(tokenValue);

            return !keepWaiting && null == m_Error;
        }

        /// <inheritdoc cref="Deferred.error"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance.</param>
        /// <returns>The error.</returns>
        internal Exception GetError(long tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Error;
        }

        /// <inheritdoc cref="Deferred.Wait"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance.</param>
        internal CustomYieldInstruction Wait(long tokenValue)
        {
            ValidateToken(tokenValue);

            return this;
        }

        /// <inheritdoc cref="Completer.SetProgression(int, int)"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Completer"/> instance.</param>
        internal void SetProgression(long tokenValue, int currentStep, int totalSteps)
        {
            ValidateToken(tokenValue);
            this.currentStep = currentStep;
            this.totalSteps = totalSteps;
        }

        /// <inheritdoc cref="Deferred.currentStep"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance.</param>
        /// <returns>The current step index.</returns>
        internal int GetCurrentStep(long tokenValue)
        {
            ValidateToken(tokenValue);
            return currentStep;
        }

        /// <inheritdoc cref="Deferred.totalSteps"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance.</param>
        /// <returns>The total steps.</returns>
        internal int GetTotalSteps(long tokenValue)
        {
            ValidateToken(tokenValue);
            return totalSteps;
        }

        /// <inheritdoc cref="Deferred.GetProgression(out int, out int)"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance.</param>
        internal void GetProgression
            (long tokenValue, out int currentStep, out int totalSteps)
        {
            ValidateToken(tokenValue);
            currentStep = this.currentStep;
            totalSteps = this.totalSteps;
        }

        /// <inheritdoc cref="Completer.Resolve\"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Completer"/> instance.</param>
        internal void Resolve(long tokenValue)
        {
            var valid = ValidateToken(tokenValue, false);

            if (!valid)
            {
                return;
            }

            m_KeepWaiting = false;
        }

        /// <inheritdoc cref="Completer.Reject(Exception)\"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Completer"/> instance.</param>
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

        /// <inheritdoc cref="Deferred.Release"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Completer"/> instance.</param>
        internal void Release(long tokenValue)
        {
            ValidateToken(tokenValue);

            ResetData();

            m_Generator.Release(this);
        }

        /// <summary>
        /// Resets the data of the promise.
        /// </summary>
        protected virtual void ResetData()
        {
            token = default;
            currentStep = 0;
            totalSteps = 0;
            m_Error = null;
            m_KeepWaiting = true;
        }

        /// <summary>
        /// Checks if the token sent by a <see cref="Deferred"/> instance or a
        /// <see cref="Completer"/> instance is valid (the same than the one
        /// stored in this promise).
        /// </summary>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred"/> instance or a <see cref="Completer"/>
        /// instance.</param>
        /// <param name="throwError">Defines if the check throws an exception
        /// in case the token is not validated.</param>
        /// <returns><c>true</c> if the token is validm, <c>false</c>
        /// otherwise.</returns>
        protected bool ValidateToken(long tokenValue, bool throwError = true)
        {
            var valid = token == tokenValue;

            if (!valid && throwError)
            {
                throw new NullReferenceException
                    ("This promise has been released and cannot be used anymore.");
            }

            return valid;
        }
    }

    /// <inheritdoc cref="Promise"/>
    /// <typeparam name="TResult">Type of the result of the async operation.</typeparam>
    class Promise<TResult> : Promise
    {
        /// <summary>
        /// The result of the promise if fulfilled.
        /// </summary>
        TResult m_Result;

        /// <inheritdoc cref="Promise(PromiseGenerator)"/>
        internal Promise(PromiseGenerator generator)
            : base(generator) { }

        /// <inheritdoc cref="Deferred{TResult}.result"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Deferred{TResult}"/> instance.</param>
        /// <returns>The result.</returns>
        internal TResult GetResult(long tokenValue)
        {
            ValidateToken(tokenValue);

            return m_Result;
        }

        /// <inheritdoc cref="Completer{TResult}.Resolve(TResult)"/>
        /// <param name="tokenValue">The token coming from a
        /// <see cref="Completer{TResult}"/> instance.</param>
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

        /// <inheritdoc cref="Promise.ResetData"/>
        protected override void ResetData()
        {
            base.ResetData();

            m_Result = default;
        }
    }
}
