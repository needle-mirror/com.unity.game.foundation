using System;
using System.Collections;
using UnityEngine.GameFoundation.CatalogManagement;
using UnityEngine.GameFoundation.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Manage the initialization and the persistence of the Game Foundation systems.
    /// </summary>
    public static class GameFoundation
    {
        private enum InitializationStatus
        {
            NotInitialized,
            Initializing,
            Initialized,
            Failed
        }

        public static string currentVersion { get; private set; }

        private static InitializationStatus m_InitializationStatus = InitializationStatus.NotInitialized;

        /// <summary>
        /// Check if the Game Foundation is initialized.
        /// </summary>
        /// <returns>Whether the Game Foundation is initialized or not</returns>
        public static bool IsInitialized
        {
            get { return m_InitializationStatus == InitializationStatus.Initialized; }
        }

        static PromiseGenerator s_PromiseGenerator;

        static IDataAccessLayer s_DataLayer;

        /// <summary>
        /// A dummy MonoBehaviour to run updates functions and coroutines.
        /// Used since <see cref="GameFoundation"/> and its services are static.
        /// </summary>
        internal static GameFoundationUpdater updater;

        /// <summary>
        /// Initialize the GameFoundation . It need a persistence object to be passed as argument to set the default persistence layer
        /// If the initialization fails, onInitializeFailed will be called with an exception.
        /// </summary>
        /// <param name="dataLayer">The data provider for the inventory manager.</param>
        /// <param name="onInitializeCompleted">Called when the initialization process is completed with success</param>
        /// <param name="onInitializeFailed">Called when the initialization process failed</param>
        public static void Initialize(
            IDataAccessLayer dataLayer,
            Action onInitializeCompleted = null,
            Action<Exception> onInitializeFailed = null)
        {
            if (m_InitializationStatus == InitializationStatus.Initializing ||
                m_InitializationStatus == InitializationStatus.Initialized)
            {
                const string message = "GameFoundation is already initialized and cannot be initialized again.";
                Debug.LogWarning(message);
                onInitializeFailed?.Invoke(new Exception(message));

                return;
            }

            if (dataLayer == null)
            {
                const string errorMessage = "GameFoundation requires a valid data layer to be initialized.";
                onInitializeFailed?.Invoke(new NullReferenceException(errorMessage));

                return;
            }

            updater = new GameObject(nameof(GameFoundationUpdater))
                .AddComponent<GameFoundationUpdater>();

            s_PromiseGenerator = new PromiseGenerator();

            s_DataLayer = dataLayer;

            m_InitializationStatus = InitializationStatus.Initializing;

            try
            {
                EditorCatalogProvider.InitializeCatalogs();
            }
            catch (Exception e)
            {
                var customException = new Exception("GameFoundation failed to initialize runtime catalogs from editor catalogs", e);
                Debug.LogException(customException);
                m_InitializationStatus = InitializationStatus.Failed;
                onInitializeFailed?.Invoke(customException);

                return;
            }

            var routine = InitializeRoutine(onInitializeCompleted, onInitializeFailed);

#if !UNITY_EDITOR
            updater.StartCoroutine(routine);
#else

            //Schedule initialization when playing
            if (Application.isPlaying)
            {
                updater.StartCoroutine(routine);
            }

            //Force initialization in editor (can happen for editor tests)
            else
            {
                void PlayCoroutine(IEnumerator coroutine)
                {
                    bool hasNext;
                    do
                    {
                        if (coroutine.Current is IEnumerator subRoutine)
                        {
                            PlayCoroutine(subRoutine);
                        }

                        hasNext = coroutine.MoveNext();
                    } while (hasNext);
                }

                PlayCoroutine(routine);
            }
#endif
        }

        /// <summary>
        /// Initializes Game Foundation
        /// </summary>
        /// <param name="onInitializeCompleted">Called if the initialization is a success</param>
        /// <param name="onInitializeFailed">Called if the initialization is a failure</param>
        static IEnumerator InitializeRoutine(Action onInitializeCompleted, Action<Exception> onInitializeFailed)
        {
            s_PromiseGenerator.GetPromiseHandles(out var dalInitDeferred, out var dalInitCompleter);

            s_DataLayer.Initialize(dalInitCompleter);

            if (!dalInitDeferred.isDone)
            {
                yield return dalInitDeferred.Wait();
            }

            var isFulfilled = dalInitDeferred.isFulfilled;
            var error = dalInitDeferred.error;
            dalInitDeferred.Release();

            if (isFulfilled)
            {
                NotificationSystem.temporaryDisable = true;

                try
                {
                    //The order in which managers are initialized is important since they are codependent.
                    //Be cautious if you want to change it.
                    //The current order is: GameItemLookup -> StatManager -> InventoryManager;
                    GameItemLookup.Initialize(s_DataLayer);
                    StatManager.Initialize(s_DataLayer);
                    InventoryManager.Initialize(s_DataLayer);

                    AnalyticsWrapper.Initialize();
                }
                finally
                {
                    NotificationSystem.temporaryDisable = false;
                }

                m_InitializationStatus = InitializationStatus.Initialized;

                currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                Debug.Log($"Successfully initialized Game Foundation version {currentVersion}");

                onInitializeCompleted?.Invoke();
            }
            else
            {
                Uninitialize();

                Debug.LogWarning("GameFoundation can't be initialized: " + error);

                m_InitializationStatus = InitializationStatus.Failed;

                onInitializeFailed?.Invoke(error);
            }
        }

        public static void Uninitialize()
        {
            m_InitializationStatus = InitializationStatus.NotInitialized;

            // note: InventoryManager must be unitialized first since it relies on game item lookups to do its work.
            InventoryManager.Uninitialize();
            GameItemLookup.Unintialize();
            StatManager.Uninitialize();
            AnalyticsWrapper.Uninitialize();

            if (updater != null)
            {
#if !UNITY_EDITOR
                Object.Destroy(updater.gameObject);
#else
                if (Application.isPlaying)
                {
                    Object.Destroy(updater.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(updater.gameObject);
                }
#endif

                updater = null;
            }
        }

        /// <summary>
        /// Provides a couple deferred/completer with no associated result.
        /// </summary>
        /// <param name="deferred">A reference to the future value</param>
        /// <param name="completer">A target container for the future value</param>
        internal static void GetPromiseHandles(out Deferred deferred, out Completer completer)
        {
            s_PromiseGenerator.GetPromiseHandles(out deferred, out completer);
        }

        /// <summary>
        /// Provides a couple deferred/completer with an associated result.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result</typeparam>
        /// <param name="deferred">A reference to the future value</param>
        /// <param name="completer">A target container for the future value</param>
        internal static void GetPromiseHandles<TResult>(out Deferred<TResult> deferred, out Completer<TResult> completer)
        {
            s_PromiseGenerator.GetPromiseHandles(out deferred, out completer);
        }
    }
}
