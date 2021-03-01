#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
#define TMP_1_4_0_OR_NEWER
#endif

#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro.EditorUtilities;
using Kyub.EmojiSearch.UI;

namespace KyubEditor.EmojiSearch.UI
{
    [CustomEditor(typeof(TMP_EmojiTextUGUI), true), CanEditMultipleObjects]
#if TMP_2_1_0_PREVIEW_1_OR_NEWER
    public class TMP_EmojiTextUGUIEditor : TMP_EditorPanelUI
#else
    public class TMP_EmojiTextUGUIEditor : TMP_UiEditorPanel
#endif
    {
    }
}

#endif
