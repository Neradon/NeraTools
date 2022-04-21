using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace NeraTools.PhysicBonesOverview
{
    public class PhysicBonesOverview : EditorWindow
    {
        private GameObject avatar;

        private Vector2 scrollPos;

        private VRCPhysBone selectedBone;

        private VRCPhysBone copyFrom;

        [MenuItem("NeraTools/PhysicBonesOverview")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(PhysicBonesOverview));
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Physic Bones Overview",
                new GUIStyle()
                    {alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.BoldAndItalic, fontSize = 20});
            EditorGUILayout.LabelField("by Nera#0809", new GUIStyle() {alignment = TextAnchor.MiddleCenter});
            EditorGUILayout.Separator();
            avatar = (GameObject) EditorGUILayout.ObjectField("Avatar", avatar, typeof(GameObject),
                true);

            if (avatar == null) return;

            if (avatar.gameObject == null) return;

            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, new GUILayoutOption[] {GUILayout.Width(200)});

            
            // int i = 0;
            foreach (VRCPhysBone vBone in avatar.GetComponentsInChildren<VRCPhysBone>())
            {
                Color oldBackgroundColor = GUI.backgroundColor;
                if (vBone.rootTransform != null)
                {
                    if (vBone == selectedBone)
                    {
                        GUI.backgroundColor = new Color(0f, 1f, 0.78f);
                    }
                    else
                    {
                        GUI.backgroundColor = vBone.enabled ? Color.green : new Color(0.7f, 0.7f, 0.7f);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.890f, 0.290f, 0.290f);
                }

                if (GUILayout.Button(vBone.rootTransform != null ? vBone.rootTransform.name : "No bone"))
                {
                    selectedBone = selectedBone != vBone ? vBone : null;
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
                    var root = selectedBone.rootTransform;
                    EditorUtility.CopySerialized(copyFrom, selectedBone);
                    selectedBone.rootTransform = root;
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
                EditorGUILayout.LabelField("No physicbone selected");
            }


            EditorGUILayout.EndHorizontal();
        }
    }
}