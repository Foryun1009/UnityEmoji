#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
#define TMP_1_4_0_OR_NEWER
#endif

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro.EditorUtilities;
using Kyub.EmojiSearch;

namespace KyubEditor.EmojiSearch
{
    [CustomEditor(typeof(TMP_EmojiSpriteAsset), true), CanEditMultipleObjects]
    public class TMP_EmojiSpriteAssetEditor : TMP_SpriteAssetEditor
    {
        public SerializedProperty overrideIOSDefinition;
        public SerializedProperty iOSSpriteSheet;

        public SerializedProperty overrideAndroidDefinition;
        public SerializedProperty androidSpriteSheet;

        public new void OnEnable()
        {
            base.OnEnable();

            overrideIOSDefinition = serializedObject.FindProperty("overrideIOSDefinition");
            iOSSpriteSheet = serializedObject.FindProperty("iOSSpriteSheet");
            overrideAndroidDefinition = serializedObject.FindProperty("overrideAndroidDefinition");
            androidSpriteSheet = serializedObject.FindProperty("androidSpriteSheet");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.PropertyField(overrideAndroidDefinition);
            if (overrideAndroidDefinition.boolValue)
                EditorGUILayout.PropertyField(androidSpriteSheet);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(overrideIOSDefinition);
            if (overrideIOSDefinition.boolValue)
                EditorGUILayout.PropertyField(iOSSpriteSheet);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif
