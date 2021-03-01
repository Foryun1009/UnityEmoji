#if UNITY_2018_3_OR_NEWER && !UNITY_2019_1_OR_NEWER
#define TMP_1_4_0_OR_NEWER
#endif

#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro.EditorUtilities;
using TMPro.SpriteAssetUtilities;

namespace KyubEditor.EmojiSearch
{
    public static class EmojiDataConversorUtility
    {
        #region Helper Functions

        public static string ConvertToTexturePackerFormat(string json, Vector2Int gridSize, Vector2Int padding, Vector2Int spacing)
        {
            try
            {
                //Unity cannot deserialize Dictionary, so we converted the dictionary to List using MiniJson
                json = ConvertToUnityJsonFormat(json);
                PreConvertedSpritesheetData v_preData = JsonUtility.FromJson<PreConvertedSpritesheetData>(json);
                TexturePackerData.SpriteDataObject v_postData = v_preData.ToTexturePacketDataObject(gridSize, padding, spacing);

                return JsonUtility.ToJson(v_postData);
            }
            catch (System.Exception p_exception)
            {
                Debug.Log("Failed to convert to EmojiOne\n: " + p_exception);
            }

            return "";
        }

        static string ConvertToUnityJsonFormat(string json)
        {
            json = "{\"frames\":" + json + "}";

            var v_changed = false;
            var v_jObject = MiniJsonEditor.Deserialize(json) as Dictionary<string, object>;
            if (v_jObject != null)
            {
                var v_array = v_jObject.ContainsKey("frames") ? v_jObject["frames"] as IList : null;
                if (v_array != null)
                {
                    foreach (var v_jPreDataNonCasted in v_array)
                    {
                        var v_jPredataObject = v_jPreDataNonCasted as Dictionary<string, object>;
                        if (v_jPredataObject != null)
                        {
                            var v_skin_variation_dict = v_jPredataObject.ContainsKey("skin_variations") ? v_jPredataObject["skin_variations"] as Dictionary<string, object> : null;

                            if (v_skin_variation_dict != null)
                            {
                                v_changed = true;
                                List<object> v_skin_variation_array = new List<object>();

                                foreach (var v_skinVariationObject in v_skin_variation_dict.Values)
                                {
                                    
                                    v_skin_variation_array.Add(v_skinVariationObject);
                                }
                                v_jPredataObject["skin_variations"] = v_skin_variation_array;
                            }
                        }
                    }
                }
            }
            return v_jObject != null && v_changed ? MiniJsonEditor.Serialize(v_jObject) : json;
        }

        #endregion

        #region Helper Classes

        public class TexturePackerData
        {
            [System.Serializable]
            public struct SpriteFrame
            {
                public float x;
                public float y;
                public float w;
                public float h;

                public override string ToString()
                {
                    string s = "x: " + x.ToString("f2") + " y: " + y.ToString("f2") + " h: " + h.ToString("f2") + " w: " + w.ToString("f2");
                    return s;
                }
            }

            [System.Serializable]
            public struct SpriteSize
            {
                public float w;
                public float h;

                public override string ToString()
                {
                    string s = "w: " + w.ToString("f2") + " h: " + h.ToString("f2");
                    return s;
                }
            }

            [System.Serializable]
            public struct Frame
            {
                public string filename;
                public SpriteFrame frame;
                public bool rotated;
                public bool trimmed;
                public SpriteFrame spriteSourceSize;
                public SpriteSize sourceSize;
                public Vector2 pivot;
            }

            [System.Serializable]
            public class SpriteDataObject
            {
                public List<Frame> frames;
            }
        }

        [System.Serializable]
        public class PreConvertedSpritesheetData
        {
            public List<PreConvertedImgDataWithVariants> frames = new List<PreConvertedImgDataWithVariants>();

            public virtual TexturePackerData.SpriteDataObject ToTexturePacketDataObject(Vector2Int p_gridSize, Vector2 p_padding, Vector2 p_spacing)
            {
                TexturePackerData.SpriteDataObject v_postData = new TexturePackerData.SpriteDataObject();
                v_postData.frames = new List<TexturePackerData.Frame>();

                if (frames != null)
                {
                    var v_framesToCheck = new List<PreConvertedImgData>();
                    if (frames != null)
                    {
                        foreach (var v_frameToCheck in frames)
                        {
                            v_framesToCheck.Add(v_frameToCheck);
                        }
                    }

                    for(int i=0; i< v_framesToCheck.Count; i++)
                    {
                        var v_preFrame = v_framesToCheck[i];

                        //Add all variations in list to check (after the current PreFrame)
                        var v_preFrameWithVariants = v_framesToCheck[i] as PreConvertedImgDataWithVariants;
                        if (v_preFrameWithVariants != null && v_preFrameWithVariants.skin_variations != null && v_preFrameWithVariants.skin_variations.Count > 0)
                        {
                            for (int j = v_preFrameWithVariants.skin_variations.Count-1; j >=0; j--)
                            {
                                var v_skinVariantFrame = v_preFrameWithVariants.skin_variations[j];
                                if (v_skinVariantFrame != null)
                                    v_framesToCheck.Insert(i+1, v_skinVariantFrame);
                            }
                        }

                        //Create TexturePacker SpriteData
                        var v_postFrame = new TexturePackerData.Frame();

                        v_postFrame.filename = v_preFrame.image;
                        v_postFrame.rotated = false;
                        v_postFrame.trimmed = false;
                        v_postFrame.sourceSize = new TexturePackerData.SpriteSize() { w = p_gridSize.x, h = p_gridSize.y };
                        v_postFrame.spriteSourceSize = new TexturePackerData.SpriteFrame() { x = 0, y = 0, w = p_gridSize.x, h = p_gridSize.y };
                        v_postFrame.frame = new TexturePackerData.SpriteFrame()
                        {
                            x = (v_preFrame.sheet_x * (p_gridSize.x + p_spacing.x)) + p_padding.x,
                            y = (v_preFrame.sheet_y * (p_gridSize.y + p_spacing.y)) + p_padding.y,
                            w = p_gridSize.x,
                            h = p_gridSize.y
                        };
                        v_postFrame.pivot = new Vector2(0f, 0f);

                        v_postData.frames.Add(v_postFrame);
                    }
                }

                return v_postData;
            }
        }

        [System.Serializable]
        public class PreConvertedImgData
        {
            public string name;
            public string unified;
            public string non_qualified;
            public string docomo;
            public string au;
            public string softbank;
            public string google;
            public string image;
            public int sheet_x;
            public int sheet_y;
            public string short_name;
            public string[] short_names;
            public object text;
            public object texts;
            public string category;
            public int sort_order;
            public string added_in;
            public bool has_img_apple;
            public bool has_img_google;
            public bool has_img_twitter;
            public bool has_img_facebook;
            public bool has_img_messenger;
        }

        [System.Serializable]
        public class PreConvertedImgDataWithVariants : PreConvertedImgData
        {
            public List<PreConvertedImgData> skin_variations = new List<PreConvertedImgData>();
        }

        #endregion
    }
}

#endif
