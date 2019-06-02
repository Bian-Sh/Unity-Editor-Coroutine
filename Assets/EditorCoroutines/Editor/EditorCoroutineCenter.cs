using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace zFrame.EditorCoroutines
{
    public class CoroutineManager
    {
        #region Feild
        List<EditorCoroutine> coroutines = new List<EditorCoroutine>();
        List<EditorCoroutine> tempCoroutineList = new List<EditorCoroutine>();
        DateTime previousTimeSinceStartup;
        #endregion

        #region 单例
        static CoroutineManager instance = null;
        static readonly object lockSyncObj = new object();
        public static CoroutineManager Instance
        {
            get
            {
                if (null == instance)
                {
                    lock (lockSyncObj)
                    {
                        instance = new CoroutineManager();
                    }
                }
                return instance;
            }
        }
        #endregion
        #region Yield Wrap
        struct YieldDefault : ICoroutineYield
        {
            public bool IsDone(float deltaTime)
            {
                return true;
            }
        }

        struct YieldWaitForSeconds : ICoroutineYield
        {
            public float timeLeft;

            public bool IsDone(float deltaTime)
            {
                timeLeft -= deltaTime;
                return timeLeft < 0;
            }
        }

        struct YieldCustomYieldInstruction : ICoroutineYield
        {
            public CustomYieldInstruction customYield;

            public bool IsDone(float deltaTime)
            {
                return !customYield.keepWaiting;
            }
        }

        struct YieldWWW : ICoroutineYield
        {
            public WWW Www;

            public bool IsDone(float deltaTime)
            {
                return Www.isDone;
            }
        }

        struct YieldAsync : ICoroutineYield
        {
            public AsyncOperation asyncOperation;

            public bool IsDone(float deltaTime)
            {
                return asyncOperation.isDone;
            }
        }

        struct YieldNestedCoroutine : ICoroutineYield
        {
            public EditorCoroutine coroutine;

            public bool IsDone(float deltaTime)
            {
                return coroutine.finished;
            }
        }
        #endregion

        internal CoroutineManager()
        {
            previousTimeSinceStartup = DateTime.Now;
            EditorApplication.update += OnUpdate;
        }

        #region Coroutine Manager Behaviours
        /// <summary>
        /// 开启一个协程
        /// </summary>
        /// <param name="target">协程所在的对象</param>
        /// <param name="routine">指定的协程</param>
        /// <returns></returns>
        public EditorCoroutine StartCoroutine(ScriptableObject target, IEnumerator routine)
        {
            if (routine == null)
            {
                Debug.LogException(new Exception("IEnumerator is null!"), null);
            }
            EditorCoroutine coroutine = new EditorCoroutine(routine, target);
            coroutines.Add(coroutine);
            MoveNext(coroutine);
            return coroutine;
        }
        /// <summary>Starts a coroutine.</summary>
        /// <param name="target">Reference to the instance of the class containing the method.</param>
        /// <param name="methodName">The name of the coroutine method to start.</param>
        public EditorCoroutine StartCoroutine(ScriptableObject target, string methodName)
        {
            return StartCoroutine(target, methodName, null);
        }

        /// <summary>Starts a coroutine.</summary>
        /// <param name="target">Reference to the instance of the class containing the method.</param>
        /// <param name="methodName">The name of the coroutine method to start.</param>
        /// <param name="value">The parameter to pass to the coroutine.</param>
        public EditorCoroutine StartCoroutine(ScriptableObject target, string methodName, object value)
        {
            MethodInfo methodInfo = target.GetType()
                .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                Debug.LogError("Coroutine '" + methodName + "' couldn't be started, the method doesn't exist!");
            }
            object returnValue;

            if (value == null)
            {
                returnValue = methodInfo.Invoke(target, null);
            }
            else
            {
                returnValue = methodInfo.Invoke(target, new object[] { value });
            }

            if (returnValue is IEnumerator)
            {
                return StartCoroutine(target, (IEnumerator)returnValue);
            }
            else
            {
                Debug.LogError("Coroutine '" + methodName + "' couldn't be started, the method doesn't return an IEnumerator!");
            }

            return null;
        }

        /// <summary>
        /// 移除指定的协程
        /// </summary>
        /// <param name="target">迭代器所在的对象</param>
        /// <param name="routine">运行中的协程</param>
        public void StopCoroutine(ScriptableObject target, EditorCoroutine routine)
        {
            coroutines.Remove(routine);
        }
        public void StopCoroutine(ScriptableObject target, IEnumerator routine)
        {
            Predicate<EditorCoroutine> predicate = (v) => //断言，顺便自动完成协程，便于临时list自动脱落该条数据。
            {
                return v.owner == target && v.routine.GetType().Name == routine.GetType().Name;
            };
            coroutines.RemoveAll(predicate); // todo :  移除所有？？还是移除首个？ 
        }


        public void StopCoroutine(ScriptableObject target, string methodName)
        {
            Predicate<EditorCoroutine> predicate = (v) => //断言，顺便自动完成协程，便于临时list自动脱落该条数据。
            {
                return v.owner == target && v.MethodName == methodName;
            };
            coroutines.RemoveAll(predicate);
        }

        public void StopAllCoroutines(ScriptableObject target)
        {
            Predicate<EditorCoroutine> predicate = (v) => //断言，顺便自动完成断言成功的对象，便于临时list的该条数据自动脱落。
            {
                return v.owner == target;
            };
            coroutines.RemoveAll(predicate);
        }
        #endregion

        #region Main Drive Logic
        void OnUpdate()
        {
            float deltaTime = (float)(DateTime.Now.Subtract(previousTimeSinceStartup).TotalMilliseconds / 1000.0f);

            previousTimeSinceStartup = DateTime.Now;
            if (coroutines.Count == 0)
            {
                return;
            }

            tempCoroutineList.Clear();
            tempCoroutineList.AddRange(coroutines);

            for (var i = tempCoroutineList.Count - 1; i >= 0; i--)
            {
                EditorCoroutine coroutine = tempCoroutineList[i];

                if (!coroutine.currentYield.IsDone(deltaTime))
                {
                    continue;
                }

                if (!MoveNext(coroutine))
                {
                    coroutine.Clear();
                    coroutines.Remove(coroutine);
                }
            }
        }

        static bool MoveNext(EditorCoroutine coroutine)
        {
            //如果使用 .net 基类 object，则无法正常的与Unity对象判等
            if (coroutine.routine.MoveNext() && null != coroutine.owner) //协成会随着 目标 的消亡而完成,在Coroutine被移除时，也就没有下一步了
            {
                return Process(coroutine);
            }
            return false;
        }

        // returns false if no next, returns true if OK
        static bool Process(EditorCoroutine coroutine)
        {
            object current = coroutine.routine.Current;
            if (current == null)
            {
                coroutine.currentYield = new YieldDefault();
            }
            else if (current is WaitForSeconds)
            {
                float seconds = float.Parse(GetInstanceField(typeof(WaitForSeconds), current, "m_Seconds").ToString());
                coroutine.currentYield = new YieldWaitForSeconds() { timeLeft = seconds };
            }
            else if (current is CustomYieldInstruction)
            {
                coroutine.currentYield = new YieldCustomYieldInstruction()
                {
                    customYield = current as CustomYieldInstruction
                };
            }
            else if (current is WWW)
            {
                coroutine.currentYield = new YieldWWW { Www = (WWW)current };
            }
            else if (current is WaitForFixedUpdate || current is WaitForEndOfFrame)
            {
                coroutine.currentYield = new YieldDefault();
            }
            else if (current is AsyncOperation)
            {
                coroutine.currentYield = new YieldAsync { asyncOperation = (AsyncOperation)current };
            }
            else if (current is EditorCoroutine)
            {
                coroutine.currentYield = new YieldNestedCoroutine { coroutine = (EditorCoroutine)current };
            }
            else
            {
                Debug.LogException(
                    new Exception("<" + coroutine.MethodName + "> yielded an unknown or unsupported type! (" + current.GetType() + ")"),
                    null);
                coroutine.currentYield = new YieldDefault();
            }
            return true;
        }
        #endregion

        #region private helper function
        static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
        #endregion
    }
}
