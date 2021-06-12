using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DartCore.Localization.Backend
{
    [CustomPropertyDrawer(typeof(LocalizedKeyAttribute))]
    public class LocalizedKeyDrawer : PropertyDrawer
    {
        private static bool displayKeySuggestions = true;
        
        private const int MAX_SUGGESTIONS_TO_DISPLAY = 8;
        private int suggestionCount;
        private string key = "";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            key = Localizator.ConvertRawToKey(property.stringValue);
            var keyExists = DoesKeyExists(key);

            // Rects.
            var keyRect = new Rect(position.x, position.y, position.width, 30f);
            var helpBoxRect = new Rect(keyRect.x, keyRect.y + (keyExists ? 0f : 40f), position.width, 35f);
            var editKeyRect = new Rect(keyRect.x, helpBoxRect.y + 40f, position.width, 35f);
            var matchesRect = new Rect(keyRect.x, editKeyRect.y + 40f, position.width, 40f);
            
            // Drawing Property.
            EditorGUI.PropertyField(keyRect, property, label);
            property.stringValue = Localizator.ConvertRawToKey(property.stringValue);
            property.serializedObject.ApplyModifiedProperties();

            // Key does not exist warning.
            if (!keyExists)
                EditorGUI.HelpBox(helpBoxRect, "The key does not exist in the current context", MessageType.Warning);
            
            // Edit Button.
            if (GUI.Button(editKeyRect, Localizator.DoesContainKey(key) ? $"Edit the '{key}' key" : $"Create a key named '{key}'"))
            {
                OpenKeyOnEditor(key.Trim());
            }

            // Displaying Key Suggestions.
            if (string.IsNullOrWhiteSpace(key))
            {
                suggestionCount = 0;
                return;
            }
            
            var keys = Localizator.GetKeysArray();
            var matchingKeys = new List<string>();
            
            // Filter the keys.
            foreach (var currentKey in keys)
            {
                if (!currentKey.Contains(key)) continue;
                matchingKeys.Add(currentKey);
            }
            matchingKeys.Sort();

            if (matchingKeys.Count <= 0)
            {
                EditorGUI.LabelField(matchesRect, "No Suggestions Available");
                suggestionCount = 0;
                return;
            }

            displayKeySuggestions = EditorGUI.Foldout(matchesRect, displayKeySuggestions, "Key Suggestions");
            suggestionCount = Mathf.Min(matchingKeys.Count, MAX_SUGGESTIONS_TO_DISPLAY);
            if (!displayKeySuggestions) return;
            
            // Display the first {MAX_SUGGESTIONS_TO_DISPLAY} matching keys.
            for (var i = 0; i < suggestionCount; i++)
            {
                var rect = new Rect(matchesRect.x, matchesRect.y + 35f + 25f * i, position.width, 25f);
                if (GUI.Button(rect, matchingKeys[i], new GUIStyle(EditorStyles.miniButton)))
                {
                    property.stringValue = Localizator.ConvertRawToKey(matchingKeys[i].Trim());
                    property.serializedObject.ApplyModifiedProperties();
                    
                    GUI.FocusControl(null);
                }
            }
        }

        private static bool DoesKeyExists(string key) => Localizator.DoesContainKey(key);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) 
                   + 60f // Button and the margin
                   + (DoesKeyExists(Localizator.ConvertRawToKey(property.stringValue)) ? 0 : 40f)  // Help Box
                   + (key.Length > 0 ? 40f : 0f) + (displayKeySuggestions && suggestionCount > 0 ? suggestionCount * 25f : 0f); // Matching Keys
        }
        
        private static void OpenKeyOnEditor(string key)
        {
            if (!DoesKeyExists(key)) Localizator.AddKey(key);
            
            var window = (KeyEditor) EditorWindow.GetWindow( typeof(KeyEditor), false,"Key Editor",true);
            window.Show();
            EditorWindow.FocusWindowIfItsOpen(typeof(KeyEditor));
            
            KeyEditor.key = key;
        }
    }
}