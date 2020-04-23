﻿using System;

namespace UnityEngine.Promise
{
    /// <summary>
    /// Handle to settle a promise.
    /// </summary>
    public struct Completer
    {
        /// <summary>
        /// Can be used to replace a completer for process you don't need the
        /// feedback from.
        /// </summary>
        static public Completer None => default;

        /// <summary>
        /// Implicitly converts a <see cref="Completer"/> into a
        /// <see cref="Rejectable"/> in case a method only needs to be able to
        /// reject the promise.
        /// </summary>
        /// <param name="completer">The completer to convert</param>
        static public implicit operator Rejectable (Completer completer)
            => new Rejectable(completer.m_Promise);

        /// <summary>
        /// The token is stored because <see cref="Promise"/> instances can be
        /// recycled. A completer can complete its promise only if its token
        /// matches the promise's.
        /// </summary>
        long m_Token;

        /// <summary>
        /// A reference to the promise this <see cref="Completer"/> manipulates.
        /// </summary>
        Promise m_Promise;

        /// <summary>
        /// Initializes a new instance of the <see cref="Completer"/> struct.
        /// </summary>
        /// <param name="promise">The promise this completer will
        /// manipulate.</param>
        internal Completer(Promise promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.token;
        }

        /// <summary>
        /// Fulfills the handled promise.
        /// </summary>
        public void Resolve() => m_Promise?.Resolve(m_Token);

        /// <summary>
        /// Rejects the handled promise for the given reason.
        /// </summary>
        /// <param name="reason">The exception that prevented the handled
        /// promise to be fulfilled.</param>
        public void Reject(Exception reason) => m_Promise?.Reject(m_Token, reason);

        /// <summary>
        /// Sets the progression of the promise.
        /// </summary>
        /// <param name="currentStep">The index of the current step the promise
        /// is at.</param>
        /// <param name="totalSteps">The total number of steps of the
        /// promise.</param>
        public void SetProgression(int currentStep, int totalSteps)
            => m_Promise?.SetProgression(m_Token, currentStep, totalSteps);
    }

    /// <inheritdoc cref="Completer"/>
    /// <typeparam name="TResult">Type of the result this completer can accept
    /// in its <see cref="Resolve(TResult)"/> method.</typeparam>
    public struct Completer<TResult>
    {
        /// <inheritdoc cref="Completer"/>
        static public Completer<TResult> None => default;

        /// <inheritdoc cref="Completer.None"/>
        static public implicit operator Rejectable(Completer<TResult> completer)
            => new Rejectable(completer.m_Promise);

        /// <inheritdoc cref="Completer.m_Token"/>
        long m_Token;

        /// <inheritdoc cref="Completer.m_Promise"/>
        Promise<TResult> m_Promise;

        /// <summary>
        /// Initializes a new instance of the <see cref="Completer{TResult}"/>
        /// struct.
        /// </summary>
        /// <param name="promise">The promise this completer will
        /// manipulate.</param>
        internal Completer(Promise<TResult> promise)
        {
            m_Promise = promise;
            m_Token = m_Promise.token;
        }

        /// <summary>
        /// Fulfills the handled promise with the given data.
        /// </summary>
        /// <param name="value">The promised data.</param>
        public void Resolve(TResult value) => m_Promise?.Resolve(m_Token, value);

        /// <inheritdoc cref="Completer.Reject"/>
        public void Reject(Exception reason) => m_Promise?.Reject(m_Token, reason);

        /// <inheritdoc cref="Completer.SetProgression(int, int)"/>
        public void SetProgression(int currentStep, int totalSteps)
            => m_Promise?.SetProgression(m_Token, currentStep, totalSteps);
    }
}
