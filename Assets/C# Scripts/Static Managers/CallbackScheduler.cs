using System;
using UnityEngine;


namespace Fire_Pixel.Utility
{
#pragma warning disable UDR0002
#pragma warning disable UDR0004
    /// <summary>
    /// Utility class to have an optimized easy access to varying callbacks by using an Action based callback system
    /// Handles callbacks and batches them for every script by an event based register system
    /// </summary>
    public static class CallbackScheduler
    {
#pragma warning disable IDE1006
        private static event Action Update;
        private static event Action LateUpdate;
#pragma warning restore IDE1006


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            Reset();

            CallbackRunnerInstance gameManager = new GameObject(">>UpdateScheduler<<").AddComponent<CallbackRunnerInstance>();
            gameManager.Init();

            GameObject.DontDestroyOnLoad(gameManager.gameObject);
        }
        private static void Reset()
        {
            Update = null;
            LateUpdate = null;
        }


        #region void Update

        /// <summary>
        /// Register a method to call every frame like Update()
        /// </summary>
        public static void RegisterUpdate(Action action)
        {
            Update += action;
        }
        /// <summary>
        /// Unregister a registerd method for Update()
        /// </summary>
        public static void UnRegisterUpdate(Action action)
        {
            Update -= action;
        }

        #endregion


        #region void LateUpdate

        /// <summary>
        /// Register a method to call every frame like LateUpdate()
        /// </summary>
        public static void RegisterLateUpdate(Action action)
        {
            Update += action;
        }
        /// <summary>
        /// Unregister a registerd method for LateUpdate()
        /// </summary>
        public static void UnRegisterLateUpdate(Action action)
        {
            Update -= action;
        }

        #endregion


        /// <summary>
        /// Callback runner instance to invoke the registered callbacks.
        /// </summary>
        private class CallbackRunnerInstance : MonoBehaviour
        {
            public static CallbackRunnerInstance Instance { get; set; }


            public void Init()
            {
                Instance = this;
            }

            private void Update()
            {
                CallbackScheduler.Update?.Invoke();
            }
            private void LateUpdate()
            {
                CallbackScheduler.LateUpdate?.Invoke();
            }
            private void OnDestroy()
            {
                CallbackScheduler.Update = null;
                CallbackScheduler.LateUpdate = null;
            }
        }
    }
#pragma warning restore UDR0002
#pragma warning restore UDR0004
}