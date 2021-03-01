#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
#define TMP_1_4_0_OR_NEWER
#endif

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using TMPro.SpriteAssetUtilities;
using TMPro;
using Kyub.EmojiSearch;
#if TMP_1_4_0_OR_NEWER
using System.Reflection;
#endif

namespace KyubEditor.EmojiSearch
{
    public class TMP_EmojiSpriteAssetImporter : EditorWindow
    {
        public enum EmojiSpriteAssetImportFormats
        {
            None = 0,
            TexturePackerJsonArray = 1,
            EmojiDataJson = 2
        }

        // Create Sprite Asset Editor Window
        [MenuItem("Window/TextMeshPro/Sprite Emoji Importer", false)]
        public static void ShowFontAtlasCreatorWindow()
        {
            var window = GetWindow<TMP_EmojiSpriteAssetImporter>();
            window.titleContent = new GUIContent("Sprite Emoji Importer");
            window.Focus();
        }

        #region Private Variables

        //EmojiOne Conversion Fields
        [SerializeField]
        Vector2Int m_gridSize = new Vector2Int(32, 32);
        [SerializeField]
        Vector2Int m_padding = new Vector2Int(1, 1);
        [SerializeField]
        Vector2Int m_spacing = new Vector2Int(2, 2);
        [SerializeField]
        float m_globalGlyphScale = 1.28f;

        //Other Fields
        Texture2D m_SpriteAtlas;
        EmojiSpriteAssetImportFormats m_SpriteDataFormat = EmojiSpriteAssetImportFormats.EmojiDataJson;

        TextAsset m_JsonFile;

        string m_CreationFeedback;

        TMP_SpriteAsset m_SpriteAsset;
        List<TMP_Sprite> m_SpriteInfoList = new List<TMP_Sprite>();
        bool m_CreatedWithEmojiJson = false;

        #endregion

        #region Unity Functions

        void OnEnable()
        {
            // Set Editor Window Size
            SetEditorWindowSize();
        }

        public void OnGUI()
        {
            DrawEditorPanel();
        }

        #endregion

        #region Helper Functions

        void DrawEditorPanel()
        {
            // label
            GUILayout.Label("Import Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            // Sprite Texture Selection
            m_JsonFile = EditorGUILayout.ObjectField("Sprite Data Source", m_JsonFile, typeof(TextAsset), false) as TextAsset;

            m_SpriteDataFormat = (EmojiSpriteAssetImportFormats)EditorGUILayout.EnumPopup("Import Format", m_SpriteDataFormat);

            //EditorGUILayout.HelpBox("Use this parameters to convert EmojiData Format to TexturePacket Json format", MessageType.Info);
            if (m_SpriteDataFormat == EmojiSpriteAssetImportFormats.EmojiDataJson)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("EmojiOne Fields", EditorStyles.boldLabel);
                    GUILayout.Space(3);
                    m_gridSize = EditorGUILayout.Vector2IntField("Grid Size", m_gridSize);
                    m_padding = EditorGUILayout.Vector2IntField("Padding", m_padding);
                    m_spacing = EditorGUILayout.Vector2IntField("Spacing", m_spacing);
                    m_globalGlyphScale = EditorGUILayout.Slider("Glyph Scale", m_globalGlyphScale, 0.01f, 5f);
                    GUILayout.Space(3);
                }
            }

            // Sprite Texture Selection
            m_SpriteAtlas = EditorGUILayout.ObjectField("Sprite Texture Atlas", m_SpriteAtlas, typeof(Texture2D), false) as Texture2D;

            if (EditorGUI.EndChangeCheck())
            {
                m_CreationFeedback = string.Empty;
            }

            GUILayout.Space(10);
            GUI.enabled = m_JsonFile != null && m_SpriteAtlas != null && m_SpriteDataFormat != EmojiSpriteAssetImportFormats.None;

            // Create Sprite Asset
            if (GUILayout.Button("Create Sprite Asset"))
            {
                m_CreationFeedback = string.Empty;

                var jsonFileText = m_JsonFile != null ? m_JsonFile.text : string.Empty;
                if (m_SpriteDataFormat == EmojiSpriteAssetImportFormats.EmojiDataJson)
                    jsonFileText = EmojiDataConversorUtility.ConvertToTexturePackerFormat(jsonFileText, m_gridSize, m_padding, m_spacing);

                m_CreatedWithEmojiJson = m_SpriteDataFormat == EmojiSpriteAssetImportFormats.EmojiDataJson;
                // Read json data file
                if (m_JsonFile != null && m_SpriteDataFormat != EmojiSpriteAssetImportFormats.None)
                {
#if TMP_2_1_0_PREVIEW_1_OR_NEWER
                    TexturePacker_JsonArray.SpriteDataObject sprites = JsonUtility.FromJson<TexturePacker_JsonArray.SpriteDataObject>(jsonFileText);
#else
                    TexturePacker.SpriteDataObject sprites = JsonUtility.FromJson<TexturePacker.SpriteDataObject>(jsonFileText);
#endif

                    if (sprites != null && sprites.frames != null && sprites.frames.Count > 0)
                    {
                        int spriteCount = sprites.frames.Count;

                        // Update import results
                        m_CreationFeedback = "<b>Import Results</b>\n--------------------\n";
                        m_CreationFeedback += "<color=#C0ffff><b>" + spriteCount + "</b></color> Sprites were imported from file.";

                        // Create sprite info list
                        m_SpriteInfoList = CreateSpriteInfoList(sprites);
                    }
                }

            }

            GUI.enabled = true;

            // Creation Feedback
            GUILayout.Space(5);
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(60));
            {
                EditorGUILayout.LabelField(m_CreationFeedback, TMP_UIStyleManager.label);
            }
            GUILayout.EndVertical();

            GUILayout.Space(5);
            GUI.enabled = m_JsonFile != null && m_SpriteAtlas && m_SpriteInfoList != null && m_SpriteInfoList.Count > 0;    // Enable Save Button if font_Atlas is not Null.
            if (GUILayout.Button("Save Sprite Asset") && m_JsonFile != null)
            {
                string filePath = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(m_JsonFile)).DirectoryName, m_JsonFile.name, "asset");

                if (filePath.Length == 0)
                    return;

                SaveSpriteAsset(filePath);
            }
            GUI.enabled = true;
        }

#if TMP_2_1_0_PREVIEW_1_OR_NEWER
        List<TMP_Sprite> CreateSpriteInfoList(TexturePacker_JsonArray.SpriteDataObject spriteDataObject)
#else
        List<TMP_Sprite> CreateSpriteInfoList(TexturePacker.SpriteDataObject spriteDataObject)
#endif
        {
#if TMP_2_1_0_PREVIEW_1_OR_NEWER
            List<TexturePacker_JsonArray.Frame> importedSprites = spriteDataObject.frames;
#else
            List<TexturePacker.SpriteData> importedSprites = spriteDataObject.frames;
#endif

            List<TMP_Sprite> spriteInfoList = new List<TMP_Sprite>();

            for (int i = 0; i < importedSprites.Count; i++)
            {
                TMP_Sprite sprite = new TMP_Sprite();

                sprite.id = i;
                sprite.name = Path.GetFileNameWithoutExtension(importedSprites[i].filename) ?? "";
                sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(sprite.name);

                // Attempt to extract Unicode value from name
                int unicode;
                int indexOfSeparator = sprite.name.IndexOf('-');
                if (indexOfSeparator != -1)
                {
                    string substring = sprite.name.Substring(0, indexOfSeparator);
#if TMP_1_4_0_OR_NEWER
                    unicode = TMP_TextUtilities.StringHexToInt(substring);
#else
                    unicode = TMP_TextUtilities.StringToInt(substring);
#endif
                }
                else
                {
#if TMP_1_4_0_OR_NEWER
                    unicode = TMP_TextUtilities.StringHexToInt(sprite.name);
#else
                    unicode = TMP_TextUtilities.StringToInt(sprite.name);
#endif
                }

                sprite.unicode = unicode;
                
                sprite.x = importedSprites[i].frame.x;
                sprite.y = m_SpriteAtlas.height - (importedSprites[i].frame.y + importedSprites[i].frame.h);
                sprite.width = importedSprites[i].frame.w;
                sprite.height = importedSprites[i].frame.h;

                //Calculate sprite pivot position
                sprite.pivot = importedSprites[i].pivot;

                //Extra Properties
                //var scaledOffset = (sprite.height * ((m_globalGlyphScale - 1) * 0.5f)) * sprite.pivot.y;
                sprite.xAdvance = sprite.width;
#if TMP_1_4_0_OR_NEWER
                sprite.scale = 1.0f;
#else
                sprite.scale = m_globalGlyphScale;
#endif
                sprite.xOffset = 0 - (sprite.width * sprite.pivot.x);
                sprite.yOffset = sprite.height - (sprite.height * sprite.pivot.y);
                spriteInfoList.Add(sprite);
            }

            return spriteInfoList;
        }

        void SaveSpriteAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            string dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, System.StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
                return;
            }

            string relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            string dirName = Path.GetDirectoryName(relativeAssetPath);
            string fileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            string pathNoExt = dirName + "/" + fileName;


            // Create new Sprite Asset using this texture
            m_SpriteAsset = CreateInstance<TMP_EmojiSpriteAsset>();
            AssetDatabase.CreateAsset(m_SpriteAsset, pathNoExt + ".asset");

            // Compute the hash code for the sprite asset.
            m_SpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(m_SpriteAsset.name);

            // Assign new Sprite Sheet texture to the Sprite Asset.
            m_SpriteAsset.spriteSheet = m_SpriteAtlas;
            m_SpriteAsset.spriteInfoList = m_SpriteInfoList;

            // Add new default material for sprite asset.
            AddDefaultMaterial(m_SpriteAsset);

#if TMP_1_4_0_OR_NEWER
            //Upgrade Lookup tables (we must set Version to Empty before)
            FieldInfo versionField = typeof(TMP_SpriteAsset).GetField("m_Version", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (versionField != null)
                versionField.SetValue(m_SpriteAsset, string.Empty);
            m_SpriteAsset.UpdateLookupTables();
            if(m_SpriteInfoList != null)
                FixLookupTable(m_SpriteAsset, m_globalGlyphScale, m_CreatedWithEmojiJson);
#endif

        }

        void FixLookupTable(TMP_SpriteAsset spriteAtlas, float globalGlyphScale, bool fixOffset)
        {
#if TMP_1_4_0_OR_NEWER
            if (spriteAtlas == null)
                return;

            var changed = false;
            for (int i = 0; i < spriteAtlas.spriteCharacterTable.Count; i++)
            {
                var sprite = spriteAtlas.spriteCharacterTable[i];
                var requiredIndex = (uint)i;
                if (sprite != null && sprite.glyphIndex != requiredIndex)
                {
                    sprite.glyphIndex = requiredIndex;
                    changed = true;
                }
            }

            //Update GlyphTable Index (fix bug in some textmesh pro versions) and Update Global Scale
            for (int i = 0; i < spriteAtlas.spriteGlyphTable.Count; i++)
            {
                var glyph = spriteAtlas.spriteGlyphTable[i];
                //glyph.index = (uint)i;

                //Change metrics if required
                if (glyph != null && m_CreatedWithEmojiJson)
                {
                    changed = true;
                    glyph.scale = globalGlyphScale;
                    //Change metrics based in Scale
                    var newOffset = glyph.metrics.height * 0.75f;
                    var metrics = glyph.metrics;
                    //Fix offset scale (really dont know why this bug happens so we must fix this)
                    metrics.horizontalBearingY = newOffset;
                    glyph.metrics = metrics;
                }
            }
            if (changed)
                spriteAtlas.UpdateLookupTables();
            EditorUtility.SetDirty(spriteAtlas);
#endif
        }


        /// <summary>
        /// Create and add new default material to sprite asset.
        /// </summary>
        /// <param name="spriteAsset"></param>
        static void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
        {
            Shader shader = Shader.Find("TextMeshPro/Sprite");
            Material material = new Material(shader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

            spriteAsset.material = material;
            material.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(material, spriteAsset);
        }


        /// <summary>
        /// Limits the minimum size of the editor window.
        /// </summary>
        void SetEditorWindowSize()
        {
            EditorWindow editorWindow = this;

            Vector2 currentWindowSize = editorWindow.minSize;

            editorWindow.minSize = new Vector2(Mathf.Max(230, currentWindowSize.x), Mathf.Max(300, currentWindowSize.y));
        }

#endregion
    }
}