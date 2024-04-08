//
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
//
// Maintainer: Jens Bahr
//

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using Sparrow.Verification;
using System.IO;
using OTBT.Framework.ColorPalettes;
using System.Collections.Generic;
using static Sparrow.Verification.VerifyResult;

namespace OTBT.Framework.Utils.Editor
{
#if UNITY_EDITOR
    /// <summary>
    /// Various functionality for building our Editors.
    /// </summary>
    public static class EditorUtils
    {
        private static ColorPalette _defaultColorPalette;

        public static ColorPalette defaultColorPalette
        {
            get
            {
                if (_defaultColorPalette == null)
                {
                    //_defaultColorPalette = AssetDatabase.LoadAssetAtPath<ColorPalette>("Assets/_scripts/framework/prefabs/default_colorpalette.asset");
                }
                return _defaultColorPalette;
            }
        }

        public static Color unityEditorBackgroundColor = new Color32(56, 56, 56, 255);// EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255); // 

        public static Color ActionColor = defaultColorPalette == null ? Color.blue : defaultColorPalette.GetColor(6);
        public static Color SingletonColor = defaultColorPalette == null ? Color.green : defaultColorPalette.GetColor(4);
        public static Color DarkActionColor = Color.Lerp(ActionColor, Color.black, 0.4f);
        public static Color DarkErrorColor = defaultColorPalette == null ? Color.red : defaultColorPalette.GetColor(0);
        public static Color SecondaryActionColor = defaultColorPalette == null ? Color.yellow : defaultColorPalette.GetColor(3);
        public static Color OTBTColor => defaultColorPalette == null ? Color.cyan : defaultColorPalette.GetColor(7);
        

        private static Dictionary<int, List<VerifyResult>> cache = new Dictionary<int, List<VerifyResult>>();
        private static Dictionary<int, DateTime> lastUpdated = new Dictionary<int, DateTime>();
        private static readonly TimeSpan cacheValidityDuration = TimeSpan.FromSeconds(1f); // or any other duration you consider appropriate
        private static List<VerifyCheckBase> verifyCheckBases = new List<VerifyCheckBase>();

        public static string IconVerifyCheck => Glyphicons.VerifyCheck;


        
        public static void DrawLabelWithGlyphicon(string txt, string iconFilename, string tooltip = "")
        {
            EditorGUILayout.LabelField(LabelWithGlyphicon(txt, iconFilename, tooltip));
        }

        public static GUIContent LabelWithGlyphicon(string txt, string iconFilename, string tooltip = "", bool skipIconSize = false)
        {
            if(!skipIconSize) EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            var texture = AssetDatabase.LoadAssetAtPath<Texture>($"Assets/_scripts/framework/sprites/glyphicons/{iconFilename}");
            return new GUIContent(txt, texture, tooltip);
        }

        public static GUIContent LabelWithColoredGlyphicon(string txt, string iconFilename, string tooltip = "", bool skipIconSize = false)
        {
            if (!skipIconSize) EditorGUIUtility.SetIconSize(new Vector2(15, 15));
            var texture = AssetDatabase.LoadAssetAtPath<Texture>($"Assets/_scripts/framework/sprites/generated/{iconFilename}");
            return new GUIContent(txt, texture, tooltip);
        }

        public static void VerifyLabel(string txt)
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel);
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField(txt, textStyle);
        }

        public static void LabelWithWordWrap(string txt)
        {
            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
            textStyle.wordWrap = true;
            EditorGUILayout.LabelField(txt, textStyle);
        }

        public static string ToTitleCase(string stringToConvert)
        {
            var firstChar = stringToConvert[0].ToString();
            return (stringToConvert.Length > 0 ? firstChar.ToUpper() + stringToConvert.Substring(1) : stringToConvert);
        }

        static Texture2D s_StaticRectTexture;
        static GUIStyle s_StaticRectStyle;

        // Note that this function is only meant to be called from OnGUI() functions.
        public static void GUIDrawRect(Rect position, Color color)
        {
            if (s_StaticRectTexture == null)
                s_StaticRectTexture = new Texture2D(1, 1);

            if (s_StaticRectStyle == null)
                s_StaticRectStyle = new GUIStyle();

            s_StaticRectTexture.SetPixel(0, 0, color);
            s_StaticRectTexture.Apply();

            s_StaticRectStyle.normal.background = s_StaticRectTexture;

            GUI.Box(position, GUIContent.none, s_StaticRectStyle);
        }

        public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
        {
            if (clip == null) return;
            System.Reflection.Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
            Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
            System.Reflection.MethodInfo method = audioUtilClass.GetMethod(
                "PlayPreviewClip",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
                null,
                new System.Type[] {
                    typeof(AudioClip),
                    typeof(int),
                    typeof(bool)
                },
                null
            );
            method.Invoke(
                null,
                new object[] {
                    clip,
                    startSample,
                    loop
                }
            );
        }

        public static void GuiLine(int height = 1)
        {
            try
            {
                Rect rect = EditorGUILayout.GetControlRect(false, height);
                rect.height = height;
                EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            }
            catch (ArgumentException e)
            {
                if (height < 0) Dbg.Exception(null, e);
            }
        }

        public static void Space(int height = 10) => EditorGUILayout.Space(height);

        public static void DrawLogoHeader(bool small = false)
        {
            var logo = AssetDatabase.LoadAssetAtPath<Texture>($"Assets/_scripts/framework/sprites/" + (EditorGUIUtility.isProSkin ? "logo.png" : "logo_dark.png"));
            if (logo != null)
            {
                GUI.DrawTexture(small ? new Rect(15, 15, 35, 30) : new Rect(10, 10, 70, 60), logo);
                GUILayout.Space(small ? 30 : 80);
            } else
            {
                GUILayout.Label("Off The Beaten Track", EditorStyles.boldLabel);
            }
        }

        public static int Toolbar(int curVal, string[] options)
        {
            return GUILayout.Toolbar(curVal, options);
        }

        public static bool BoolToolbar(bool curVal, string[] options)
        {
            return GUILayout.Toolbar(curVal ? 1 : 0, options) == 1;
        }

        public static void DrawSubHeader(string title)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontSize = (int)(style.fontSize * 1.25f);
            style.fontStyle = FontStyle.Bold;
            GUILayout.Label(title, style);
        }

        public static void DrawTinyLogoHeader(string title)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUIUtility.SetIconSize(new Vector2(20, 20));
            var texture = AssetDatabase.LoadAssetAtPath<Texture>($"Assets/_scripts/framework/sprites/" + (EditorGUIUtility.isProSkin ? "logo.png" : "logo_dark.png"));
            GUILayout.Label(new GUIContent(" " + title, texture), EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
        }

        public static void DrawLogoHeader(string title, bool small = false)
        {
            DrawLogoHeader(small);
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.fontSize = (int)(style.fontSize * (small ? 1.5f : 2f));
            style.fontStyle = FontStyle.Bold;
            GUI.Label(small
                ? new Rect(65, 15, EditorGUILayout.GetControlRect().width - 70, 25)
                : new Rect(100, 40, EditorGUILayout.GetControlRect().width - 70, 25), title, style);
        }

        public static void DrawLogoHeader(string title, string wikiURL, bool small = false)
        {
            DrawLogoHeader(title, small);
            DrawWikiLinkButton(wikiURL);
        }

        public static void DrawWikiLinkButton(string url)
        {
            if (GUI.Button(new Rect(100, 10, 30, 25), "?"))
            {
                Application.OpenURL(url);
            }
        }

        public static void RenameScriptableObject(ScriptableObject targetObject, string targetName)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var baseName = targetName;

            foreach (var character in invalidChars)
            {
                baseName = baseName.Replace(character.ToString(), string.Empty);
            }
            baseName = baseName.Replace(" ", "-");
            baseName = baseName.ToLowerInvariant();

            targetObject.name = baseName;
            var path = AssetDatabase.GetAssetPath(targetObject);
            AssetDatabase.RenameAsset(path, baseName);

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Generates a one pixel texture for use in Editor item backgrounds.
        /// </summary>
        /// <param name="color">Color for the texture.</param>
        public static Texture2D TextureFromColor(Color color)
        {
            var colors = new Color[1];
            colors[0] = color;

            var tex = new Texture2D(1, 1);
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        static Texture2D TextureFromColor(Texture2D texture, Color color)
        {
            var colors = texture.GetPixels();

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = colors[i].a > 0 ? color : colors[i];
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }

        public static void CheckIfDirectoryExists(string basepath, string directoryname)
        {
            string directory = Path.GetDirectoryName(basepath) + Path.DirectorySeparatorChar + directoryname;
            if (!Directory.Exists(directory))
                AssetDatabase.CreateFolder(Path.GetDirectoryName(basepath), directoryname);
        }


        static Texture2D TextureFromColorMultiply(Texture2D texture, Color color)
        {
            var colors = texture.GetPixels();

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = colors[i].a > 0 ? colors[i] *color : colors[i];
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }

        public static GameObject FindInChildren(Transform parent, string name, string parentName, bool strict = false, string notContain = "")
        {
            foreach (Transform child in parent)
            {
                if (strict ? (child.name == (name) && parent.name == (parentName))
                    : (child.name.ToLower().Contains(name.ToLower()) && parent.name.ToLower().Contains(parentName.ToLower())))
                {
                    if (string.IsNullOrEmpty(notContain) || (!child.name.ToLower().Contains(notContain.ToLower())))
                        return child.gameObject;
                }

                GameObject foundChild = FindInChildren(child, name, parentName, strict, notContain);
                if (foundChild != null)
                    return foundChild;
            }
            return null;
        }

        public static void DrawToggleButton(bool on, string caption, out bool targetbool, bool inverter = false)
        {
            GUIStyle pushButton = new GUIStyle(EditorStyles.miniButtonRight);
            Color originalColor = GUI.backgroundColor;
            bool internalOn = inverter ? !on : on;
            if (internalOn)
            {
                pushButton.fontStyle = FontStyle.Bold;
                GUI.backgroundColor = defaultColorPalette.GetColor(5);
            } else
            {
                GUI.backgroundColor = defaultColorPalette.GetColor(1);
            }

            bool internalTarget = (GUILayout.Toggle(internalOn, LabelWithGlyphicon(caption, on ? Glyphicons.SwitchOn : Glyphicons.SwitchOff), "Button", GUILayout.MinWidth(100)));
            targetbool = inverter ? !internalTarget : internalTarget;
            GUI.backgroundColor = originalColor;
        }

        public static void Separator(int height = 5)
        {
            Space();
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            Space();
        }

        public static bool ToggleFoldout(this SerializedObject serializedObject, string propertyName, string caption)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property.propertyType != SerializedPropertyType.Boolean)
                throw new ArgumentException("Serialized Property of Type Boolean expected.");

            GUIStyle pushButton = new GUIStyle(EditorStyles.miniButtonRight);
            if (property.boolValue)
            {
                pushButton.fontStyle = FontStyle.Bold;
            }

            DrawPropertyField(serializedObject, propertyName, caption);

            return property.boolValue;
        }

        public static void DrawPropertyField(this SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property);
        }

        public static void DrawPropertyField(this SerializedObject serializedObject, string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        public static void Header(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

       
        public static void DrawVerify(IVerify verify)
        {
            int hashCode = verify.GetHashCode();
            DateTime currentTime = DateTime.UtcNow;

            if (cache.ContainsKey(hashCode) && lastUpdated.ContainsKey(hashCode) && currentTime - lastUpdated[hashCode] <= cacheValidityDuration)
            {
                // Use cached results
                RenderResults(cache[hashCode]);
                return;
            }

            List<VerifyResult> results = new List<VerifyResult>();
            List<UnityEngine.Object> checkObjects = new List<UnityEngine.Object>();
             if (verify is MonoBehaviour)
             {
                 checkObjects.Add((verify as MonoBehaviour));
             }
             else if (verify is ScriptableObject)
             {
                 checkObjects.Add((verify as ScriptableObject));
             }

            if(verifyCheckBases.Count == 0)
                verifyCheckBases = VerifyWindow.InstantiateChecklist();
            foreach (VerifyCheckBase check in verifyCheckBases)
            {
                results.AddRange(check.PerformCheckWrapper(checkObjects, CheckType.Selection, "Selection", thisObjectOnly: true));
            }
            results.Sort((a, b) => -a.category.CompareTo(b.category));

            // Update the cache
            cache[hashCode] = results;
            lastUpdated[hashCode] = currentTime;

            // Render results
            RenderResults(results);
        }

        private static void RenderResults(List<VerifyResult> results)
        {
            if (results.Count > 0)
            {
                Space();
                GUILayout.Label(LabelWithGlyphicon(" Verify found " + results.Count + " problems:", "times.png"));
            }
            string lastCategory = "";
            for (int i = results.Count - 1; i >= 0; i--)
            {
                if (!results[i].category.Equals(lastCategory))
                {
                    lastCategory = results[i].category;
                    GUILayout.Label(LabelWithGlyphicon(lastCategory, Glyphicons.VerifyCheck));
                }
                results[i].Print();
            }
        }


        public static void BeginColoredEditor(bool singleton = false)
        {
            var screenRect = GUILayoutUtility.GetRect(1, 1);
            var vertRect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(new Rect(screenRect.x - 13, screenRect.y - 1, screenRect.width + 17, vertRect.height + 9), 
                !singleton ? Color.Lerp(SecondaryActionColor, unityEditorBackgroundColor, 0.8f) : Color.Lerp(SingletonColor, unityEditorBackgroundColor, 0.8f));
        }

        public static void EndColoredEditor()
        {
            EditorGUILayout.EndVertical();
        }

        public static void GenerateTintedGlyphicon(Texture2D source, Color tint, string filenameShort)
        {
            //var source = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/_scripts/framework/sprites/glyphicons/{glyphiconName}.png");

            //var texture = AssetDatabase.LoadAssetAtPath<Texture>($"Assets/_scripts/framework/sprites/glyphicons/{iconFilename}");

            //first Make sure you're using RGB24 as your texture format
            Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

            var colors = source.GetPixels();

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = colors[i].a > 0 ? colors[i] * tint : colors[i];
            }

            texture.SetPixels(colors);
            texture.Apply();

            string dirPath = "Assets/_scripts/framework/sprites/generated/";
            string filename = filenameShort + ".png";

            //then Save To Disk as PNG
            byte[] bytes = texture.EncodeToPNG();
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllBytes(dirPath + filename, bytes);

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(dirPath + filename);
        }

        public static void Save(UnityEngine.Object document)
        {
            EditorUtility.SetDirty(document);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

#endif
}
