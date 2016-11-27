using UnityEngine;
using System.Collections;
using UnityEditor;

public class CoroutineWindowExample : EditorWindow {

    [MenuItem("Window/Coroutine Example")]
    public static void ShowWindow() {
        EditorWindow window = EditorWindow.GetWindow(typeof(CoroutineWindowExample));
    }

    void OnGUI() {
        if (GUILayout.Button("Start")) {
            this.StartCoroutine(Example());
        }
        if (GUILayout.Button("Stop")) {
            this.StopCoroutine("Example");
        }
        if (GUILayout.Button("Stop all")) {
            this.StopAllCoroutines();
        }
    }

    IEnumerator Example() {
        while (true) {
            Debug.LogError("Hello EditorCoroutine!");
            yield return new WaitForSeconds(2f);
        }
    }


    class NonEditorClass {

        public void DoSomething(bool start, bool stop, bool stopAll) {
            if (start) {
                EditorCoroutine.StartCoroutine(Example(), this);
            }
            if (stop) {
                EditorCoroutine.StopCoroutine("Example", this);
            }
            if (stopAll) {
                EditorCoroutine.StopAllCoroutines(this);
            }
        }

        IEnumerator Example() {
            while (true) {
                Debug.LogError("Hello EditorCoroutine!");
                yield return new WaitForSeconds(2f);
            }
        }
    }
}
