using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace zFrame.EditorCoroutines
{
    public class EditorCoroutine
    {
        public ICoroutineYield currentYield;
        public IEnumerator routine;
        public ScriptableObject owner;//如果使用 .net 基类 object，则无法正常的与Unity对象判等，所以由object改为基类 ScriptableObject
        public string MethodName = "";
        public bool finished = false;

        public EditorCoroutine(IEnumerator routine, ScriptableObject owner)
        {
            this.routine = routine;
            this.owner = owner;

            if (routine != null)
            {
                string[] split = routine.ToString().Split('<', '>');
                if (split.Length == 3)
                {
                    this.MethodName = split[1];
                }
            }
        }
        public EditorCoroutine(string methodName, ScriptableObject owner)
        {
            MethodName = methodName;
            this.owner = owner;
        }

        /// <summary>
        /// 断引用，便于GC
        /// </summary>
        public void Clear()
        {
            currentYield = null;
            routine = null;
            owner = null;
            finished = true;
        }
    }
}