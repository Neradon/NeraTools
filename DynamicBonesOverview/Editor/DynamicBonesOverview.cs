using UnityEditor;
using UnityEngine;

namespace NeraTools.DynamicBonesOverview
{
    public class DynamicBonesOverview : EditorWindow
    {
        private GameObject avatar;

        private Vector2 scrollPos;

        private DynamicBone selectedBone;

        private DynamicBone copyFrom;

        [MenuItem("NeraTools/DynamicBonesOverview")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DynamicBonesOverview));
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dynamic Bones Overview",
                new GUIStyle()
                    {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.BoldAndItalic, fontSize = 20});
            EditorGUILayout.LabelField("by Nera#2898", new GUIStyle() {alignment = TextAnchor.MiddleCenter});
            EditorGUILayout.Separator();
            avatar = (GameObject) EditorGUILayout.ObjectField("Avatar", avatar, typeof(GameObject),
                true);

            if (avatar == null) return;

            if (avatar.gameObject == null) return;

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, new GUILayoutOption[] {GUILayout.Width(200)});

            
            // int i = 0;
            foreach (DynamicBone dbone in avatar.GetComponentsInChildren<DynamicBone>())
            {
                Color oldBackgroundColor = GUI.backgroundColor;
                if (dbone.m_Root != null)
                {
                    if (dbone == selectedBone)
                    {
                        GUI.backgroundColor = new Color(0f, 1f, 0.78f);
                    }
                    else
                    {
                        GUI.backgroundColor = dbone.enabled ? Color.green : new Color(0.7f, 0.7f, 0.7f);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.890f, 0.290f, 0.290f);
                }

                if (GUILayout.Button(dbone.m_Root != null ? dbone.m_Root.name : "No bone"))
                {
                    selectedBone = selectedBone != dbone ? dbone : null;
                }

                GUI.backgroundColor = oldBackgroundColor;
            }

            EditorGUILayout.EndScrollView();


            if (selectedBone != null)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Find"))
                {
                    Selection.activeObject = selectedBone.gameObject;
                }

                if (GUILayout.Button("Copy"))
                {
                    copyFrom = selectedBone;
                }

                if (copyFrom != null && GUILayout.Button("Paste"))
                {
                    var root = selectedBone.m_Root;
                    EditorUtility.CopySerialized(copyFrom, selectedBone);
                    selectedBone.m_Root = root;
                }

                EditorGUILayout.EndHorizontal();


                selectedBone.enabled = EditorGUILayout.Toggle("Enabled", selectedBone.enabled);
                var serializedBone = new SerializedObject(selectedBone);
                SerializedProperty iterator = serializedBone.GetIterator();
                for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
                {
                    using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                    {
                        if (iterator.propertyPath == "m_Script") continue;
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }

                serializedBone.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("No dynamicbone selected");
            }


            EditorGUILayout.EndHorizontal();
        }
    }
}