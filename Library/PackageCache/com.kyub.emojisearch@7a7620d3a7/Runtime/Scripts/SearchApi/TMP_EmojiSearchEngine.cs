#if UNITY_2018_3_OR_NEWER
#define TMP_1_4_0_OR_NEWER
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kyub.EmojiSearch.Utilities
{
    public static class TMP_EmojiSearchEngine
    {
        #region Static Fields (Lookup Tables per SpriteAsset)

        //Dictionary<string, string> is TableSquence to Sprite Name
        static Dictionary<TMP_SpriteAsset, Dictionary<string, string>> s_lookupTableSequences = new Dictionary<TMP_SpriteAsset, Dictionary<string, string>>();
        static Dictionary<TMP_SpriteAsset, HashSet<string>> s_fastLookupPath = new Dictionary<TMP_SpriteAsset, HashSet<string>>(); //this will cache will save the path of each character in sequence, so for every iteration we can check if we need to continue

        #endregion

        #region Emoji Search Engine Functions

        /// <summary>
        /// Try parse text converting to supported EmojiSequence format (all char sequences will be replaced to <sprite=index>)
        /// </summary>
        /// <param name="p_spriteAsset"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool ParseEmojiCharSequence(TMP_SpriteAsset p_spriteAsset, ref string p_text)
        {
            bool v_changed = false;
            TryUpdateSequenceLookupTable(p_spriteAsset);
            if (!string.IsNullOrEmpty(p_text))
            {
                var v_mainSpriteAsset = p_spriteAsset == null ? TMPro.TMP_Settings.defaultSpriteAsset : p_spriteAsset;
                var v_fastLookupPath = v_mainSpriteAsset != null && s_fastLookupPath.ContainsKey(v_mainSpriteAsset) ? s_fastLookupPath[v_mainSpriteAsset] : new HashSet<string>();
                var v_lookupTableSequences = v_mainSpriteAsset != null && s_lookupTableSequences.ContainsKey(v_mainSpriteAsset) ? s_lookupTableSequences[v_mainSpriteAsset] : new Dictionary<string, string>();

                if (v_lookupTableSequences == null || v_lookupTableSequences.Count == 0)
                    return false;

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                //Eficient way to check characters
                for (int i = 0; i < p_text.Length; i++)
                {
                    int v_endCounter = i;
                    System.Text.StringBuilder v_auxSequence = new System.Text.StringBuilder();

                    //Look for sequences in fastLookupPath
                    while (p_text.Length > v_endCounter &&
                           (v_endCounter == i ||
                           v_fastLookupPath.Contains(v_auxSequence.ToString()))
                          )
                    {
                        //We must skip variant selectors (if found it)
                        v_auxSequence.Append(p_text[v_endCounter]);
                        v_endCounter++;
                    }

                    //Remove last added guy (the previous one is the correct)
                    if (v_auxSequence.Length > 0 && !v_fastLookupPath.Contains(v_auxSequence.ToString()))
                        v_auxSequence.Remove(v_auxSequence.Length - 1, 1);

                    var v_sequence = v_auxSequence.Length > 0 ? v_auxSequence.ToString() : "";
                    //Found a sequence, add it instead add the character
                    if (v_sequence.Length > 0 && v_lookupTableSequences.ContainsKey(v_sequence))
                    {
                        v_changed = true;
                        //Changed Index to Sprite Name to prevent erros when looking at fallbacks
                        sb.Append(string.Format("<sprite name=\"{0}\">", v_lookupTableSequences[v_sequence]));

                        i += (v_sequence.Length - 1); //jump checked characters
                    }
                    //add the char (normal character)
                    else
                    {
                        sb.Append(p_text[i]);
                    }
                }

                if (v_changed)
                    p_text = sb.ToString();
            }

            return v_changed;
        }

        /// <summary>
        /// Cache all sequences in a lookuptable (and in a fastpath) found in SpriteAsset
        /// This Lookuptable will return the Sprite Index of the Emoji in SpriteAsset (the key will be the char sequence that will be used as replacement of old unicode format)
        /// 
        /// The sequence will be the name of the TMP_Sprite in UTF32 or UTF16 HEX format separeted by '-' for each character (see below the example)
        /// Ex: 0023-fe0f-20e3.png
        /// </summary>
        /// <param name="p_spriteAsset"> The sprite asset used to cache the sequences</param>
        /// <param name="p_forceUpdate"> force update the lookup table of this SpriteAsset</param>
        /// <returns>true if lookup table changed</returns>
        public static bool TryUpdateSequenceLookupTable(TMP_SpriteAsset p_spriteAsset, bool p_forceUpdate = false)
        {
            var v_mainSpriteAsset = p_spriteAsset == null ? TMPro.TMP_Settings.defaultSpriteAsset : p_spriteAsset;

            if (v_mainSpriteAsset != null && (!s_lookupTableSequences.ContainsKey(v_mainSpriteAsset) || s_lookupTableSequences[v_mainSpriteAsset] == null || p_forceUpdate))
            {
                //Init FastlookupPath
                if (v_mainSpriteAsset != null && (!s_fastLookupPath.ContainsKey(v_mainSpriteAsset) || s_fastLookupPath[v_mainSpriteAsset] == null))
                    s_fastLookupPath[v_mainSpriteAsset] = new HashSet<string>();
                var v_fastLookupPath = v_mainSpriteAsset != null && s_fastLookupPath.ContainsKey(v_mainSpriteAsset) ? s_fastLookupPath[v_mainSpriteAsset] : new HashSet<string>();
                v_fastLookupPath.Clear();

                //Init Lookup Table
                if (v_mainSpriteAsset != null && (!s_lookupTableSequences.ContainsKey(v_mainSpriteAsset) || s_lookupTableSequences[v_mainSpriteAsset] == null))
                    s_lookupTableSequences[v_mainSpriteAsset] = new Dictionary<string, string>();
                var v_lookupTableSequences = v_mainSpriteAsset != null && s_lookupTableSequences.ContainsKey(v_mainSpriteAsset) ? s_lookupTableSequences[v_mainSpriteAsset] : new Dictionary<string, string>();
                v_lookupTableSequences.Clear();

                List<TMPro.TMP_SpriteAsset> v_spriteAssetsChecked = new List<TMPro.TMP_SpriteAsset>();
                v_spriteAssetsChecked.Add(v_mainSpriteAsset);
                //Add the main sprite asset
                if (TMPro.TMP_Settings.defaultSpriteAsset != null && !v_spriteAssetsChecked.Contains(TMPro.TMP_Settings.defaultSpriteAsset))
                    v_spriteAssetsChecked.Add(TMPro.TMP_Settings.defaultSpriteAsset);

                //Check in all spriteassets (and fallbacks)
                for (int i = 0; i < v_spriteAssetsChecked.Count; i++)
                {
                    var v_spriteAsset = v_spriteAssetsChecked[i];
                    if (v_spriteAsset != null)
                    {
                        //Check all sprites in this sprite asset
                        for (int j = 0; j < v_spriteAsset.spriteInfoList.Count; j++)
                        {
                            var v_element = v_spriteAsset.spriteInfoList[j];

                            if (v_element == null || string.IsNullOrEmpty(v_element.name) || !v_element.name.Contains("-"))
                                continue;

                            var v_elementName = BuildNameInEmojiSurrogateFormat(v_element.name);
                            var v_unicodeX8 = v_element.unicode.ToString("X8");

                            //Check for elements that Unicode is different from Name
                            if (!string.IsNullOrEmpty(v_elementName) &&
                                !string.Equals(v_elementName, v_unicodeX8, System.StringComparison.InvariantCultureIgnoreCase))
                            {
                                var v_tableStringBuilder = new System.Text.StringBuilder();
                                for (int k = 0; k < v_elementName.Length; k += 8)
                                {
                                    var v_hexUTF32 = v_elementName.Substring(k, Mathf.Min(v_elementName.Length - k, 8));
#if UNITY_2018_3_OR_NEWER
                                    var v_intValue = TMPro.TMP_TextUtilities.StringHexToInt(v_hexUTF32);
#else
                                    var v_intValue = TMPro.TMP_TextUtilities.StringToInt(v_hexUTF32);
#endif

                                    //Not a surrogate and is valid UTF32 (conditions to use char.ConvertFromUtf32 function)
                                    if (v_intValue > 0x000000 && v_intValue < 0x10ffff &&
                                        (v_intValue < 0x00d800 || v_intValue > 0x00dfff))
                                    {
                                        var v_UTF16Surrogate = char.ConvertFromUtf32(v_intValue);
                                        if (!string.IsNullOrEmpty(v_UTF16Surrogate))
                                        {
                                            //Add chars into cache (we must include the both char paths in fastLookupPath)
                                            foreach (var v_surrogateChar in v_UTF16Surrogate)
                                            {
                                                v_tableStringBuilder.Append(v_surrogateChar);
                                                //Add current path to lookup fast path
                                                v_fastLookupPath.Add(v_tableStringBuilder.ToString());
                                            }
                                        }
                                    }
                                    //Split into two chars (we failed to match conditions of char.ConvertFromUtf32 so we must split into two UTF16 chars)
                                    else
                                    {
                                        for (int l = 0; l < v_hexUTF32.Length; l += 4)
                                        {
                                            var v_hexUTF16 = v_hexUTF32.Substring(l, Mathf.Min(v_hexUTF32.Length - l, 4));
#if UNITY_2018_3_OR_NEWER
                                            var v_charValue = (char)TMPro.TMP_TextUtilities.StringHexToInt(v_hexUTF16);
#else
                                            var v_charValue = (char)TMPro.TMP_TextUtilities.StringToInt(v_hexUTF16);
#endif
                                            v_tableStringBuilder.Append(v_charValue);

                                            //Add current path to lookup fast path
                                            v_fastLookupPath.Add(v_tableStringBuilder.ToString());
                                        }
                                    }

                                }
                                var v_tableKey = v_tableStringBuilder.ToString();
                                //Add key as sequence in lookupTable
                                if (!string.IsNullOrEmpty(v_tableKey) && !v_lookupTableSequences.ContainsKey(v_tableKey))
                                {
                                    v_lookupTableSequences[v_tableKey] = v_element.name; //j;
                                }
                            }
                        }

                        //Add Fallbacks (before the next sprite asset and after this sprite asset)
                        for (int k = v_spriteAsset.fallbackSpriteAssets.Count - 1; k >= 0; k--)
                        {
                            var v_fallback = v_spriteAsset.fallbackSpriteAssets[k];
                            if (v_fallback != null && !v_spriteAssetsChecked.Contains(v_fallback))
                                v_spriteAssetsChecked.Insert(i + 1, v_fallback);
                        }
                    }
                }
                return true;
            }

            return false;
        }

        #endregion

        #region Other Helper Functions

        /*public static int GetSpriteIndexFromCharSequence(TMP_SpriteAsset p_spriteAsset, string p_charSequence)
        {
            var v_mainSpriteAsset = p_spriteAsset == null ? TMPro.TMP_Settings.defaultSpriteAsset : p_spriteAsset;
            TryUpdateSequenceLookupTable(v_mainSpriteAsset);

            Dictionary<string, int> v_lookupTable = null;
            s_lookupTableSequences.TryGetValue(v_mainSpriteAsset, out v_lookupTable);

            int v_index;
            if (v_lookupTable == null || !v_lookupTable.TryGetValue(p_charSequence, out v_index))
                v_index = -1;

            return v_index;
        }*/

        public static string GetSpriteNameFromCharSequence(TMP_SpriteAsset p_spriteAsset, string p_charSequence)
        {
            var v_mainSpriteAsset = p_spriteAsset == null ? TMPro.TMP_Settings.defaultSpriteAsset : p_spriteAsset;
            TryUpdateSequenceLookupTable(v_mainSpriteAsset);

            Dictionary<string, string> v_lookupTable = null;
            s_lookupTableSequences.TryGetValue(v_mainSpriteAsset, out v_lookupTable);

            string v_name;
            if (v_lookupTable == null || !v_lookupTable.TryGetValue(p_charSequence, out v_name))
                v_name = null;

            return v_name;
        }

        public static Dictionary<string, string> GetAllCharSequences(TMP_SpriteAsset p_spriteAsset)
        {
            var v_mainSpriteAsset = p_spriteAsset == null ? TMPro.TMP_Settings.defaultSpriteAsset : p_spriteAsset;
            TryUpdateSequenceLookupTable(v_mainSpriteAsset);

            Dictionary<string, string> v_lookupTable = null;
            if (!s_lookupTableSequences.TryGetValue(v_mainSpriteAsset, out v_lookupTable) && v_lookupTable == null)
                v_lookupTable = new Dictionary<string, string>();

            return v_lookupTable;
        }

        public static void ClearCache()
        {
            s_lookupTableSequences.Clear();
            s_fastLookupPath.Clear();
        }

        public static void ClearCache(TMP_SpriteAsset p_spriteAsset)
        {
            var v_mainSpriteAsset = p_spriteAsset == null ? TMPro.TMP_Settings.defaultSpriteAsset : p_spriteAsset;

            s_lookupTableSequences.Remove(v_mainSpriteAsset);
            s_lookupTableSequences.Remove(v_mainSpriteAsset);
        }

        #endregion

        #region Name Pattern Functions

        public static string BuildNameInEmojiSurrogateFormat(string p_name)
        {
            if (p_name == null)
                p_name = "";

            //Remove variant selectors (FE0F and FE0E)
            //ex: 2665-1F0FF5-FE0F.png will be converted to 2665-1f0f5 (remove variant selectors, cast to lower and remove file extension)
            var v_fileName = System.IO.Path.GetFileNameWithoutExtension(p_name).ToLower();

            //Split Surrogates and change to UTF16 or UTF32 (based in length of each string splitted)
            //ex: 2665-1f0f5 will be converted to [2665, 0001f0f5] and after that converted to 26650001f0f5
            if (v_fileName.Contains("-"))
            {
                var v_splitArray = v_fileName.Split(new char[] { '-' }, System.StringSplitOptions.RemoveEmptyEntries);
                v_fileName = "";
                for (int i = 0; i < v_splitArray.Length; i++)
                {
                    var v_split = v_splitArray[i];
                    while (/*v_split.Length > 4 && */v_split.Length < 8)
                    {
                        v_split = "0" + v_split;
                    }
                    v_fileName += v_split;
                }
            }
            return v_fileName;
        }

        #endregion
    }
}
