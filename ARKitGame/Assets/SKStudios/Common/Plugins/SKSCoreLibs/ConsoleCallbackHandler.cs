using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR

#endif

namespace SKStudios.Common.Utils {
    public class ConsoleCallbackHandler {
        private static ConsoleCallbackHandler _consoleCallbackHandler;

        /// <summary>
        ///     The listener for the events must be an instance
        ///     to be reliably called in play mode
        /// </summary>
        private static ConsoleCallbackHandler Instance {
            get {
                if (_consoleCallbackHandler == null) {
                    _consoleCallbackHandler = new ConsoleCallbackHandler();
                    Application.logMessageReceived += _consoleCallbackHandler.HandleLog;
                }
                return _consoleCallbackHandler;
            }
        }

        /// <summary>
        ///     Dictionary containing all active callbacks. TODO: Make serializable 
        /// </summary>
        private readonly Dictionary<ConsoleCallback, ConsoleCallback> _callbacks;


        private ConsoleCallbackHandler() {
            _callbacks = new Dictionary<ConsoleCallback, ConsoleCallback>();
        }

        /// <summary>
        ///     Add a callback to a specified type of Unity Console log.
        /// </summary>
        /// <param name="action">Action to execute on message written</param>
        /// <param name="type">Type of callback</param>
        /// <param name="message">Message of callback</param>
        public static void AddCallback(Action action, LogType type, string message) {
            var handler = Instance;

            var c = new ConsoleCallback(type, message);
            //If a callback with these params already exist, retrieve it
            if (handler._callbacks.ContainsKey(c))
                c = handler._callbacks[c];
            //Otherwise, continue using the newly-created version and add it
            else
                handler._callbacks.Add(c, c);

            c.AddAction(action);
        }

        /// <summary>
        ///     Remove a specified callback
        /// </summary>
        /// <param name="type">Type of callback</param>
        /// <param name="message">Message substring to search for</param>
        public static void RemoveCallback(LogType type, string message) {
            var handler = Instance;
            var c = new ConsoleCallback(type, message);
            if (handler._callbacks.TryGetValue(c, out c)) return;
            handler._callbacks.Remove(c);
        }

        /// <summary>
        ///     Clear all callbacks on Unity Console logs
        /// </summary>
        public static void ClearCallbacks() {
            var handler = Instance;
            handler._callbacks.Clear();
        }

        /// <summary>
        ///     Handles logs sent out by Unity's console
        /// </summary>
        /// <param name="logString">String logged</param>
        /// <param name="stackTrace">Stacktrace associated with message</param>
        /// <param name="type">Type of message</param>
        private void HandleLog(string logString, string stackTrace, LogType type) {
            ConsoleCallback callback = null;
            foreach (var c in _callbacks.Keys) {
                if (c.Type != type) continue;
                if (logString.Contains(c.Message)) {
                    callback = c;
                    break;
                }
            }
            if (callback == null) return;

            callback.ExecuteActions();
        }

        /// <summary>
        ///     Handles a callback for a given LogType + Message. All actions for that given LogType
        ///     and message combination are stored in a single ConsoleCallback instance to reduce
        ///     editor speed reduction should a large amount of ConsoleCallbacks be used.
        /// </summary>
        private class ConsoleCallback {
            public readonly Dictionary<MethodInfo, Action> Actions;
            public readonly string Message;
            public readonly LogType Type;

            public ConsoleCallback(LogType type, string message) {
                Type = type;
                Message = message;
                Actions = new Dictionary<MethodInfo, Action>();
            }

            /// <summary>
            ///     Add an action to this Callback
            /// </summary>
            /// <param name="a">the action to add</param>
            public void AddAction(Action a) {
                var method = a.Method;
                if (!Actions.Keys.Contains(method))
                    Actions.Add(method, a);
            }

            /// <summary>
            ///     Execute all actions on this callback
            /// </summary>
            public void ExecuteActions() {
                foreach (var a in Actions.Values) a.Invoke();
            }

            /// <summary>
            ///     HashCode implemented in a way to return equality if the target to check is the same
            /// </summary>
            /// <returns>HashCode of the target</returns>
            public override int GetHashCode() {
                var result = 37;

                result *= 397;
                result += Type.GetHashCode();

                result *= 397;
                result += Message.GetHashCode();

                return result;
            }

            /// <summary>
            ///     Equality checks only for HashCode, assuming that the other obj is of the right type and exists
            /// </summary>
            /// <param name="obj">Other Object</param>
            /// <returns></returns>
            public override bool Equals(object obj) {
                if (obj == null) return false;
                if (!(obj is ConsoleCallback)) return false;
                var otherCallback = (ConsoleCallback) obj;
                return GetHashCode() == otherCallback.GetHashCode();
            }
        }

#if UNITY_EDITOR && SKS_DEV //[MenuItem("Tools/SKStudios/PortalKitPro/Unit test ConsoleCallback")]
        public static void UnitTest() {
            ClearCallbacks();
            StringBuilder reportBuilder = new StringBuilder();
            //Check action concatenation
            ConsoleCallbackHandler.AddCallback(() => {
                reportBuilder.Append("Check 1 passed!\n");
            }, LogType.Log, "Test 1");
            ConsoleCallbackHandler.AddCallback(() => {
                reportBuilder.Append("Check 2 passed!\n");
            }, LogType.Log, "Test 1");
            ConsoleCallbackHandler.AddCallback(() => {
                reportBuilder.Append("Action Concatenation functions properly. (Test 1 complete)\n");
            }, LogType.Log, "Test 1");

            ConsoleCallbackHandler.AddCallback(() => {
                reportBuilder.Append("Check 3 passed!\n");
            }, LogType.Assert, "Test 1");
            ConsoleCallbackHandler.AddCallback(() => {
                reportBuilder.Append("Check 4 passed!\n");
            }, LogType.Warning, "Test 1");
            ConsoleCallbackHandler.AddCallback(() => {
                reportBuilder.Append("Action non-concatenation functions properly. (Test 2 complete)\n");
            }, LogType.Error, "Test 1");

            //Execute first test
            reportBuilder.Append("Throwing Log...\n");
            Debug.Log("Test 1");


            //Execute second test
            reportBuilder.Append("Throwing Assert...\n");
            Debug.LogAssertion("Test 1");
            reportBuilder.Append("Throwing Warning...\n");
            Debug.LogWarning("Test 1");
            reportBuilder.Append("Throwing Error...\n");
            Debug.LogError("Test 1");

            Stopwatch s = new Stopwatch();
            ClearCallbacks();

            int testNumber = 1000;
            int callbackNumber = 100;
            reportBuilder.Append(String.Format("Executing timing test on {0} messages with {1} callbacks...\n",
                testNumber, callbackNumber));
            double test1time = 0;
            double test2time = 0;
            Action timeTest = () => {
                for (int i = 0; i < testNumber; i++) {
                    Debug.Log("Test 2");
                }
            };

            s.Start();
            timeTest.Invoke();
            s.Stop();
            test1time = s.ElapsedMilliseconds;

            ClearCallbacks();
            for (int i = 0; i < callbackNumber; i++) {
                AddCallback(() => {
                    int n = 0;
                }, LogType.Log, "Test 2");
            }

            s.Reset();
            s.Start();
            timeTest.Invoke();
            s.Stop();
            test2time = s.ElapsedMilliseconds;

            reportBuilder.Append(String.Format("Initial time: {0}ms With callbacks: {1}ms Diff: {2}\n", test1time,
                test2time,
                test2time - test1time));

            EditorUtility.DisplayDialog("Unit test of Callback", reportBuilder.ToString(), "that's fly");
        }
#endif
    }
}