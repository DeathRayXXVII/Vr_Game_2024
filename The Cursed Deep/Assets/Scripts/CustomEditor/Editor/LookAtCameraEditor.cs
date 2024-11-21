using UnityEditor;

namespace ZPTools
{
    [CustomEditor(typeof(LookAtCamera))]
    public class LookAtCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LookAtCamera lookAtCamera = (LookAtCamera)target;

            SerializedProperty targetObjectProp = serializedObject.FindProperty("targetObject");
            if (targetObjectProp != null)
            {
                targetObjectProp.isExpanded = false;
            }

            DrawPropertiesExcluding(serializedObject, "targetObject");

            serializedObject.ApplyModifiedProperties();
        }
    }
}