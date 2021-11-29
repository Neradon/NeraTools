using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

public class FreezeFrameWindow : EditorWindow
{
    private VRCAvatarDescriptor _avatarDescriptor;

    private GameObject _avatarBody;

    private Transform _avatarArmature;
    
    
    [MenuItem("NeraTools/FreezeFrame")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FreezeFrameWindow));
    }

    private void OnGUI()
    {
        _avatarDescriptor = (VRCAvatarDescriptor) EditorGUILayout.ObjectField("Avatar",_avatarDescriptor,typeof(VRC_AvatarDescriptor),true);


        if (_avatarDescriptor == null) return;
        
        _avatarBody = (GameObject) EditorGUILayout.ObjectField("Avatar Body",_avatarBody,typeof(GameObject),true);

        if (_avatarBody == null) return;  
        
        _avatarArmature = (Transform) EditorGUILayout.ObjectField("Avatar Armature",_avatarArmature,typeof(Transform),true);

        if (_avatarArmature == null) return;

        if (GUILayout.Button("Create"))
        {
            CreateDuplicate(_avatarDescriptor.gameObject);
        }
    }

    private void CreateDuplicate(GameObject original)
    {
        GameObject duplicate = Instantiate(original,original.transform);
        duplicate.transform.localPosition = Vector3.zero;
        foreach (var component in duplicate.GetComponents<Component>())
        {
            if (component is Transform) continue;
            
            DestroyImmediate(component);
        }

        ConstraintSource constraintSource = new ConstraintSource();
        constraintSource.weight = -0.5f;
        constraintSource.sourceTransform = _avatarBody.transform;

        var rCon = duplicate.AddComponent<RotationConstraint>();
        rCon.constraintActive = true;
        rCon.weight = 1f;
        rCon.AddSource(constraintSource);

        var pCon = duplicate.AddComponent<PositionConstraint>();
        pCon.constraintActive = true;
        pCon.weight = 0.5f;
        constraintSource.weight = -1;
        pCon.AddSource(constraintSource);
        
        var duplicateArmature = duplicate.transform.Find(_avatarArmature.name);

        var apCon = duplicateArmature.gameObject.AddComponent<ParentConstraint>();

        apCon.constraintActive = true;
        apCon.weight = 1;
        constraintSource.weight = 1f;
        apCon.AddSource(constraintSource);


        foreach (Component component in duplicateArmature.GetComponentsInChildren<Component>())
        {
            switch (component)
            {
                case DynamicBone dBone:
                    DestroyImmediate(dBone);
                    break;
                case DynamicBoneCollider dBoneCollider:
                    DestroyImmediate(dBoneCollider);
                    break;
            }
        }


        var duplicateBody = duplicate.transform.Find(_avatarBody.name).gameObject;

        duplicateBody.SetActive(false);



        var transforms = _avatarArmature.GetComponentsInChildren<Transform>();
        var duplicateTransforms = duplicateArmature.GetComponentsInChildren<Transform>();
        
        
        string path = EditorUtility.SaveFilePanelInProject("Save animation", _avatarDescriptor.name, "anim", "UwU");

        
        AnimationClip animationClipOn = new AnimationClip();
        string uniqueOn = AssetDatabase.GenerateUniqueAssetPath(path.Insert(path.Length-5,"_on"));
                
        AssetDatabase.CreateAsset(animationClipOn,uniqueOn);
        
        AnimationClip animationClipOff = new AnimationClip();
        string uniqueOff = AssetDatabase.GenerateUniqueAssetPath(path.Insert(path.Length-5,"_off"));
                
        AssetDatabase.CreateAsset(animationClipOff,uniqueOff);
        
        
        AnimationCurve animationCurveOn = new AnimationCurve();
        animationCurveOn.AddKey(0, 1);
        animationCurveOn.AddKey(1f / 60f, 1);        
        
        AnimationCurve animationCurveOff = new AnimationCurve();
        animationCurveOff.AddKey(0, 0);
        animationCurveOff.AddKey( 1f / 60f, 0);
        
        
        animationClipOff.SetCurve(AnimationUtility.CalculateTransformPath(duplicateBody.transform,_avatarDescriptor.transform),typeof(GameObject),"m_IsActive",animationCurveOn);
        animationClipOn.SetCurve(AnimationUtility.CalculateTransformPath(duplicateBody.transform,_avatarDescriptor.transform),typeof(GameObject),"m_IsActive",animationCurveOff);

        animationClipOn.SetCurve(AnimationUtility.CalculateTransformPath(duplicateArmature,_avatarDescriptor.transform),typeof(ParentConstraint),"m_Active",animationCurveOn);
        animationClipOff.SetCurve(AnimationUtility.CalculateTransformPath(duplicateArmature,_avatarDescriptor.transform),typeof(ParentConstraint),"m_Active",animationCurveOff);
        
        ParentConstraint bpCon;
        constraintSource.weight = 1f;
        for (int i = 0; i < transforms.Length; i++)
        {
            if (duplicateTransforms[i] == duplicateArmature) continue;

            bpCon = duplicateTransforms[i].gameObject.AddComponent<ParentConstraint>();
            bpCon.constraintActive = true;
            constraintSource.sourceTransform = transforms[i];
            bpCon.AddSource(constraintSource);
            
            

            animationClipOn.SetCurve(AnimationUtility.CalculateTransformPath(duplicateTransforms[i],_avatarDescriptor.transform),typeof(ParentConstraint),"m_Active",animationCurveOn);
            animationClipOff.SetCurve(AnimationUtility.CalculateTransformPath(duplicateTransforms[i],_avatarDescriptor.transform),typeof(ParentConstraint),"m_Active",animationCurveOff);
        }
        
        AssetDatabase.SaveAssets();
    }
    
    
}
