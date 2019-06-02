using UnityEngine;
using System.Collections;
using UnityEditor;


namespace zFrame.EditorCoroutines
{
    public class CoroutineWindowExample : EditorWindow
    {
        [MenuItem("Window/Coroutine Example")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CoroutineWindowExample));
        }

        private void OnEnable()
        {

        }
        EditorCoroutine coroutine;
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("StartByString"))
            {
                this.StartCoroutine("Example");
            }
            if (GUILayout.Button("StartByFunc"))
            {
                coroutine= this.StartCoroutine(Example());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("StopByString"))
            {
                this.StopCoroutine("Example");
            }
            if (GUILayout.Button("StopByFunc"))
            {
                //this.StopCoroutine(Example());
                this.StopCoroutine(coroutine);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Start WWW"))
            {
                this.StartCoroutine(ExampleWWW());
            }

            if (GUILayout.Button("Start Nested"))
            {
                this.StartCoroutine(ExampleNested());
            }

            if (GUILayout.Button("Start WaitUntil/WaitWhile"))
            {
                status = false;
                this.StartCoroutine(ExampleWaitUntilWhile());
            }
            if (GUILayout.Button("Switch For WaitUntil/WaitWhile:" + (status ? "On" : "Off")))
            {
                status = !status;
                EditorUtility.SetDirty(this);
            }
            if (GUILayout.Button("Stop all"))
            {
                this.StopAllCoroutines();
            }

        }

        private bool status;

        IEnumerator ExampleWaitUntilWhile()
        {
            yield return new WaitUntil(() => status);
            Debug.Log("Switch On");
            yield return new WaitWhile(() => status);
            Debug.Log("Switch Off");
        }

        IEnumerator Example()
        {
            while (true)
            {
                Debug.Log("Hello EditorCoroutine!");
                yield return new WaitForSeconds(2f);
            }
        }

        IEnumerator ExampleWWW()
        {
            while (true)
            {
                var www = new WWW("https://unity.com/");
                yield return www;
                Debug.Log("Hello EditorCoroutine!" + www.text);
                yield return new WaitForSeconds(2f);
            }
        }

        IEnumerator ExampleNested()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                Debug.Log("I'm not nested");
                yield return this.StartCoroutine(ExampleNestedOneLayer());
            }
        }

        IEnumerator ExampleNestedOneLayer()
        {
            yield return new WaitForSeconds(2f);
            Debug.Log("I'm one layer nested");
            yield return this.StartCoroutine(ExampleNestedTwoLayers());
        }

        IEnumerator ExampleNestedTwoLayers()
        {
            yield return new WaitForSeconds(2f);
            Debug.Log("I'm two layers nested");
        }


        //class NonEditorClass
        //{
        //    public void DoSomething(bool start, bool stop, bool stopAll)
        //    {
        //        if (start)
        //        {
        //            CoroutineManager.Instance.StartCoroutine( this,Example());
        //        }
        //        if (stop)
        //        {
        //            CoroutineManager.Instance.StopCoroutine( this,"Example");
        //        }
        //        if (stopAll)
        //        {
        //            CoroutineManager.Instance.StopAllCoroutines(this);
        //        }
        //    }

        //    IEnumerator Example()
        //    {
        //        while (true)
        //        {
        //            Debug.Log("Hello EditorCoroutine!");
        //            yield return new WaitForSeconds(2f);
        //        }
        //    }
        //}
    }
}