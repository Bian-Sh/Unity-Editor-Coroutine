using System.Collections;
using UnityEditor;
using UnityEngine;

namespace zFrame.EditorCoroutines
{
    public static class EditorCoroutineExtensions
    {
        public static EditorCoroutine StartCoroutine(this ScriptableObject thisRef, IEnumerator coroutine)
        {
            return CoroutineManager.Instance.StartCoroutine(thisRef, coroutine);
        }

        public static EditorCoroutine StartCoroutine(this ScriptableObject thisRef, string methodName)
        {
            return CoroutineManager.Instance.StartCoroutine(thisRef, methodName);
        }

        public static EditorCoroutine StartCoroutine(this ScriptableObject thisRef, string methodName, object value)
        {
            return CoroutineManager.Instance.StartCoroutine(thisRef, methodName, value);
        }

        public static void StopCoroutine(this ScriptableObject thisRef, EditorCoroutine coroutine)
        {
            CoroutineManager.Instance.StopCoroutine(thisRef, coroutine);
        }

        public static void StopCoroutine(this ScriptableObject thisRef, IEnumerator coroutine)
        {
            CoroutineManager.Instance.StopCoroutine(thisRef, coroutine);
        }

        public static void StopCoroutine(this ScriptableObject thisRef, string methodName)
        {
            CoroutineManager.Instance.StopCoroutine(thisRef, methodName);
        }

        public static void StopAllCoroutines(this ScriptableObject thisRef)
        {
            CoroutineManager.Instance.StopAllCoroutines(thisRef);
        }
    }
}