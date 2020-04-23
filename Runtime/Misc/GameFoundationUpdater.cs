using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// This object is used by <see cref="GameFoundation"/> to update and launch coroutines.
    /// </summary>
    class GameFoundationUpdater : MonoBehaviour
    {
        List<Deferred> m_UncompletedPromises = new List<Deferred>();

        void Awake()
        {
            var cachedGameObject = gameObject;
            cachedGameObject.hideFlags = HideFlags.HideAndDontSave
                | HideFlags.HideInInspector;

            if (Application.isPlaying)
            {
                DontDestroyOnLoad(cachedGameObject);
            }
        }

        void OnEnable()
        {
            StartCoroutine(PromiseCheckForRelease());
        }

        /// <summary>
        /// A coroutine to check every running promises to release them if they have been completed. 
        /// </summary>
        /// <remarks>
        /// TODO: It will be the user's responsibility to release them if the InventoryManager have an asynchronous API.
        /// </remarks>
        IEnumerator PromiseCheckForRelease()
        {
            const float kIntervalBetweenCleanup = 1.0f;
            var tick = new WaitForSecondsRealtime(kIntervalBetweenCleanup);

            while (enabled)
            {
                //Reverse loop to simplify item removal
                for (int i = m_UncompletedPromises.Count - 1; i >= 0; --i)
                {
                    var deferred = m_UncompletedPromises[i];
                    if (deferred.isDone)
                    {
                        deferred.Release();

                        m_UncompletedPromises.RemoveAt(i);
                    }
                }

                yield return tick;
            }
        }

        /// <summary>
        /// Checks the status of the deferred value.
        /// If already done, it is released, otherwise, it is queued for later check.
        /// </summary>
        public void ReleaseOrQueue(Deferred deferred)
        {
            if (deferred.isDone)
            {
                deferred.Release();
            }
            else
            {
                m_UncompletedPromises.Add(deferred);
            }
        }
    }
}
