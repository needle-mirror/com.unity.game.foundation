using System;
using System.Collections;
using System.Reflection;
using UnityEngine.Promise;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// Manage the initialization and the persistence of Game Foundation's systems.
    /// </summary>
    public static class GameFoundation
    {
        enum InitializationStatus
        {
            NotInitialized,
            Initializing,
            Initialized,
            Failed
        }

        /// <summary>
        ///     Event raised when GameFoundation is successfully initialized.
        /// </summary>
        public static event Action initialized;

        /// <summary>
        ///     Event raised when GameFoundation failed its initialization.
        ///     The provided exception is the reason of the failure.
        /// </summary>
        public static event Action<Exception> initializationFailed;

        /// <summary>
        ///     Event raised when GameFoundation is uninitialized.
        /// </summary>
        public static event Action uninitialized;

        public static string currentVersion { get; private set; }

        static InitializationStatus s_InitializationStatus = InitializationStatus.NotInitialized;

        /// <summary>
        /// Check if the Game Foundation is initialized.
        /// </summary>
        /// <returns>
        /// Whether the Game Foundation is initialized or not.
        /// </returns>
        public static bool IsInitialized => s_InitializationStatus == InitializationStatus.Initialized;

        public static CatalogManager catalogs { get; private set; }

        static PromiseGenerator s_PromiseGenerator;

        /// <summary>
        /// The current Data Access Layer used by GameFoundation.
        /// </summary>
        internal static IDataAccessLayer dataLayer { get; private set; }

        /// <summary>
        /// A dummy MonoBehaviour to run updates functions and coroutines.
        /// Used since <see cref="GameFoundation"/> and its services are static.
        /// </summary>
        internal static GameFoundationUpdater updater;

        /// <summary>
        /// Initialize GameFoundation systems.
        /// </summary>
        /// <param name="dataLayer">
        /// The data provider for the inventory manager.
        /// </param>
        /// <param name="onInitializeCompleted">
        /// Called when the initialization process is completed with success.
        /// </param>
        /// <param name="onInitializeFailed">
        /// Called when the initialization process failed.
        /// </param>
        public static void Initialize(
            IDataAccessLayer dataLayer,
            Action onInitializeCompleted = null,
            Action<Exception> onInitializeFailed = null)
        {
            if (s_InitializationStatus == InitializationStatus.Initializing ||
                s_InitializationStatus == InitializationStatus.Initialized)
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

            GameFoundation.dataLayer = dataLayer;

            s_InitializationStatus = InitializationStatus.Initializing;

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
        /// Routine to initialize Game Foundation asynchronously.
        /// </summary>
        /// <param name="onInitializeCompleted">
        /// Called if the initialization is a success.
        /// </param>
        /// <param name="onInitializeFailed">
        /// Called if the initialization is a failure.
        /// </param>
        static IEnumerator InitializeRoutine(Action onInitializeCompleted, Action<Exception> onInitializeFailed)
        {
            void FailInitialization(Exception reason)
            {
                Uninitialize();

                Debug.LogWarning("GameFoundation can't be initialized: " + reason);

                s_InitializationStatus = InitializationStatus.Failed;

                onInitializeFailed?.Invoke(reason);

                //Raise event.
                initializationFailed?.Invoke(reason);
            }

            s_PromiseGenerator.GetPromiseHandles(out var dalInitDeferred, out var dalInitCompleter);

            dataLayer.Initialize(dalInitCompleter);

            if (!dalInitDeferred.isDone)
                yield return dalInitDeferred.Wait();

            var isFulfilled = dalInitDeferred.isFulfilled;
            var error = dalInitDeferred.error;
            dalInitDeferred.Release();

            if (!isFulfilled)
            {
                FailInitialization(error);

                yield break;
            }

            try
            {
                var catalogBuilder = new CatalogBuilder();
                dataLayer.Configure(catalogBuilder);
                catalogs = catalogBuilder.Build();
            }
            catch (Exception e)
            {
                var customException = new Exception("GameFoundation failed to initialize runtime catalogs from editor catalogs", e);
                Debug.LogException(customException);
                s_InitializationStatus = InitializationStatus.Failed;
                onInitializeFailed?.Invoke(customException);

                yield break;
            }

            NotificationSystem.temporaryDisable = true;

            try
            {
                InventoryManager.Initialize();
                WalletManager.Initialize();
                TransactionManager.Initialize();
                AnalyticsWrapper.Initialize();
            }
            catch (Exception e)
            {
                FailInitialization(e);

                yield break;
            }
            finally
            {
                NotificationSystem.temporaryDisable = false;
            }

            s_InitializationStatus = InitializationStatus.Initialized;

            currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Debug.Log($"Successfully initialized Game Foundation version {currentVersion}");

            onInitializeCompleted?.Invoke();

            //Raise event.
            initialized?.Invoke();
        }

        public static void Uninitialize()
        {
            s_InitializationStatus = InitializationStatus.NotInitialized;

            InventoryManager.Uninitialize();
            AnalyticsWrapper.Uninitialize();
            WalletManager.Uninitialize();
            TransactionManager.Uninitialize();

            currentVersion = null;
            catalogs = null;
            s_PromiseGenerator = null;
            dataLayer = null;

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

            //Raise event.
            uninitialized?.Invoke();
        }

        /// <summary>
        /// Provides a couple deferred/completer with no associated result.
        /// </summary>
        /// <param name="deferred">
        /// A reference to the future value.
        /// </param>
        /// <param name="completer">
        /// A target container for the future value.
        /// </param>
        internal static void GetPromiseHandles(out Deferred deferred, out Completer completer)
        {
            s_PromiseGenerator.GetPromiseHandles(out deferred, out completer);
        }

        /// <summary>
        /// Provides a couple deferred/completer with an associated result.
        /// </summary>
        /// <typeparam name="TResult">
        /// The type of the expected result.
        /// </typeparam>
        /// <param name="deferred">
        /// A reference to the future value.
        /// </param>
        /// <param name="completer">
        /// A target container for the future value.
        /// </param>
        internal static void GetPromiseHandles<TResult>(
            out Deferred<TResult> deferred, out Completer<TResult> completer)
        {
            s_PromiseGenerator.GetPromiseHandles(out deferred, out completer);
        }
    }
}
