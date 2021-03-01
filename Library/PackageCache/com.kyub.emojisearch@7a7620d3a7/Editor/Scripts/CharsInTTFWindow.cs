#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
#define TMP_1_4_0_OR_NEWER
#endif

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KyubEditor.EmojiSearch
{
    public class CharsInTTFWindow : EditorWindow
    {
        #region Private Variables

        [SerializeField]
        string m_charsRange = "";
        [SerializeField]
        Font m_font = null;

        Vector2 _scroll = Vector2.zero;

        #endregion

        #region Static Init

        [MenuItem("Window/TextMeshPro/Chars in Font")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            CharsInTTFWindow window = (CharsInTTFWindow)EditorWindow.GetWindow(typeof(CharsInTTFWindow));
            window.titleContent = new GUIContent("Chars in Font");
            window.ShowUtility();
        }

        #endregion

        #region Unity Functions

        protected virtual void OnGUI()
        {
            if (m_font != null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Recalculate", GUILayout.Width(150)))
                    {
                        m_charsRange = PickAllCharsRangeFromFont(m_font);
                    }
                }
            }

            EditorGUILayout.Space();
            var v_newFont = EditorGUILayout.ObjectField("Source Font File", m_font, typeof(Font), false) as Font;
            if (m_font != v_newFont)
            {
                m_font = v_newFont;
                m_charsRange = PickAllCharsRangeFromFont(m_font);
            }
            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField("Character Sequence (Decimal)", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(m_charsRange);
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Helper Static Functions

        public static string PickAllCharsRangeFromFont(Font p_font)
        {
            string v_charsRange = "";
            if (p_font != null)
            {
                TrueTypeFontImporter v_fontReimporter = null;

                //A GLITCH: Unity's Font.CharacterInfo doesn't work
                //properly on dynamic mode, we need to change it to Unicode first
                if (p_font.dynamic)
                {
                    var assetPath = AssetDatabase.GetAssetPath(p_font);
                    v_fontReimporter = (TrueTypeFontImporter)AssetImporter.GetAtPath(assetPath);

                    v_fontReimporter.fontTextureCase = FontTextureCase.Unicode;
                    v_fontReimporter.SaveAndReimport();
                }

                //Only Non-Dynamic Fonts define the characterInfo array
                Vector2Int v_minMaxRange = new Vector2Int(-1, -1);
                for (int i = 0; i < p_font.characterInfo.Length; i++)
                {
                    var v_charInfo = p_font.characterInfo[i];
                    var v_apply = true;
                    if (v_minMaxRange.x < 0 || v_minMaxRange.y < 0)
                    {
                        v_apply = false;
                        v_minMaxRange = new Vector2Int(v_charInfo.index, v_charInfo.index);
                    }
                    else if (v_charInfo.index == v_minMaxRange.y + 1)
                    {
                        v_apply = false;
                        v_minMaxRange.y = v_charInfo.index;
                    }

                    if (v_apply || i == p_font.characterInfo.Length - 1)
                    {
                        if (!string.IsNullOrEmpty(v_charsRange))
                            v_charsRange += "\n,";
                        v_charsRange += v_minMaxRange.x + "-" + v_minMaxRange.y;

                        if (i == p_font.characterInfo.Length - 1)
                        {
                            if (v_charInfo.index >= 0 && (v_charInfo.index  < v_minMaxRange.x || v_charInfo.index > v_minMaxRange.y))
                                v_charsRange += "\n," + v_charInfo.index + "-" + v_charInfo.index;
                        }
                        else
                            v_minMaxRange = new Vector2Int(v_charInfo.index, v_charInfo.index);

                    }
                }

                // Change back to dynamic font
                if (v_fontReimporter != null)
                {
                    v_fontReimporter.fontTextureCase = FontTextureCase.Dynamic;
                    v_fontReimporter.SaveAndReimport();
                }
            }
            return v_charsRange;
        }

        #endregion
    }
}

#endif
