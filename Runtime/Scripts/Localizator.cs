﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using UnityEditor;

namespace DartCore.Localization
{
    public class Localizator
    {
        private static string[] keysArray;
        private static Dictionary<SystemLanguage, string[]> languageArrays;
        private static Dictionary<SystemLanguage, string> languageNames;

        private static SystemLanguage currentLanguage = SystemLanguage.English;

        private const string LNG_FILES_PATH = "Assets/Localization/Resources/";
        private const string DEFAULT_LNG_FILES_PATH = "Packages/com.dartcore.localization/Default Localization Dir/";
        private const string KEYS_FILE_NAME = "_keys";
        private const string LNG_NAMES_FILE = "_lng_names";
        private const string LINE_BREAK_TEXT = "<line_break>";

        /// <summary>
        /// returns an array which consists of all the keys.
        /// </summary>
        public static string[] GetKeysArray() => keysArray ??= ReadAllLines(KEYS_FILE_NAME);

        private static Dictionary<SystemLanguage, string> GetLanguageNames()
        {
            if (languageNames == null) LoadLanguageFile();
            return languageNames;
        }

        private static Dictionary<SystemLanguage, string[]> GetLanguageArrays()
        {
            if (languageArrays == null) LoadLanguageFile();
            return languageArrays;
        }

        private static string GetLanguageFilesPath()
        {
            if (Directory.Exists(LNG_FILES_PATH)) return LNG_FILES_PATH;
            else
            {
                var defaultDir = Directory.CreateDirectory(DEFAULT_LNG_FILES_PATH);
                if (!defaultDir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Default Localization Directory was not found.");
                }

                Directory.CreateDirectory(LNG_FILES_PATH);

                FileInfo[] files = defaultDir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(LNG_FILES_PATH, file.Name);
                    file.CopyTo(tempPath, false);
                }

                return GetLanguageFilesPath();
            }
        }

        /// <summary>
        /// Returns the localized value of the given key in the current language.
        /// </summary>
        public static string GetString(string key, bool returnErrorString = true)
        {
            return GetString(key, currentLanguage, returnErrorString: returnErrorString);
        }

        /// <summary>
        /// Returns the localized value of the given key in the specified language.
        /// </summary>
        public static string GetString(string key, SystemLanguage language, bool returnErrorString = true)
        {
            if (!GetLanguageNames().ContainsKey(language))
                return "";

            var languageArray = GetLanguageArrays()[language];
            var index = GetIndexOfKey(key);

            var doesLngFileContainsKey = languageArray.Length > index && index >= 0;

            if (!doesLngFileContainsKey || index == -1)
                return returnErrorString ? ConvertSavedStringToUsableString(languageArray[1]) : "";

            return ConvertSavedStringToUsableString(languageArray[index]);
        }

        /// <summary>
        /// Returns a list of booleans that corresponds to the localization status of the key in the available languages.
        /// </summary>
        public static bool[] GetLocalizationStatusOfKey(string key)
        {
            var availableLanguages = GetAvailableLanguages();
            var localizationStatuses = new bool[availableLanguages.Length];

            var keyIndex = GetIndexOfKey(key);
            for (var i = 0; i < availableLanguages.Length; i++)
                localizationStatuses[i] = !string.IsNullOrWhiteSpace(GetLanguageArrays()[availableLanguages[i]][keyIndex]);

            return localizationStatuses;
        }

        public static string GetStringRaw(string key, SystemLanguage language)
        {
            return ConvertUsableStringToSavedString(GetString(key, language, returnErrorString: false));
        }

        public static string ConvertRawToKey(string raw) => raw.Replace(' ', '_').ToLower();
        private static string ConvertSavedStringToUsableString(string savedString) => savedString.Trim().Replace(LINE_BREAK_TEXT, "\n");
        private static string ConvertUsableStringToSavedString(string usableString) => usableString
            .Replace(Environment.NewLine, LINE_BREAK_TEXT)
            .Replace("\n", LINE_BREAK_TEXT)
            .Trim();

        /// <summary>
        /// Works like DartCore.Localization.GetString(), if there is no localized value for the given key in the current language
        /// returns the localized value in the given fallBackLanguage, if the key is not present in the fallback language returns an
        /// error string with the current language if returnErrorString is set to True else it will just return an empty string.
        /// </summary>
        public static string GetStringWithFallBackLanguage(string key, SystemLanguage fallBackLanguage, bool returnErrorString = true)
        {
            var result = GetString(key, returnErrorString: false);
            if (!string.IsNullOrWhiteSpace(result)) return result.Trim();

            result = GetString(key, fallBackLanguage, returnErrorString: false);
            if (!string.IsNullOrWhiteSpace(result)) return result.Trim();

            return returnErrorString ? GetString("lng_error") : "";
        }

        public static void UpdateKeyFile()
        {
            keysArray = ReadAllLines(KEYS_FILE_NAME);
        }

        public static void AddKey(string key)
        {
            key = key.Replace('\n', new char());
            key = key.Replace(' ', '_');
            if (!DoesContainKey(key))
            {
                var lines = ReadAllLines(KEYS_FILE_NAME, false);
                lines[lines.Length - 1] = key;

                var linesText = "";
                foreach (var line in lines)
                    linesText += line + "\n";
                
                File.WriteAllText(GetLanguageFilesPath() + KEYS_FILE_NAME + ".txt", linesText);

                UpdateKeyFile();
                foreach (var language in GetAvailableLanguages())
                {
                    // Add new line.
                    var languageLines = ReadAllLines(GetLanguageNames()[language], false);
                    var languageText = "";
                    foreach (var line in languageLines)
                        languageText += line + "\n";
                
                    File.WriteAllText(GetLanguageFilesPath() + GetLanguageNames()[language] + ".txt", languageText);
                }
                
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
        }

        public static void RemoveKey(string key)
        {
            if (key == "lng_name" || key == "lng_error")
            {
                Debug.LogWarning($"You can not remove the \"{key}\" key");
                return;
            }

            key = ConvertRawToKey(key);
            if (DoesContainKey(key))
            {
                var index = GetIndexOfKey(key);

                // KEY REMOVAL
                var newText = "";
                var keys = GetKeysArray();
                for (var i = 0; i < keys.Length; i++)
                {
                    if (i != index)
                        newText += keys[i] + "\n";
                }
                File.WriteAllText(GetLanguageFilesPath() + KEYS_FILE_NAME + ".txt", newText);

                // VALUE REMOVAL
                foreach (var language in GetLanguageNames().Keys)
                {
                    newText = "";
                    for (var i = 0; i < keys.Length; i++)
                    {
                        if (i != index)
                            newText += GetStringRaw(keys[i], language) + "\n";
                    }
                    File.WriteAllText(GetLanguageFilesPath() + GetLanguageNames()[language] + ".txt", newText);
                }
            }
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            UpdateKeyFile();
            LoadLanguageFile();
        }

        public static void RenameKey(string oldName, string newName)
        {
            var keys = GetKeysArray();
            var newText = "";
            for (int i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                newText += (key.Trim() == oldName.Trim() ? newName.Trim() : key.Trim()) + "\n";
            }

            File.WriteAllText(GetLanguageFilesPath() + KEYS_FILE_NAME + ".txt", newText);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            UpdateKeyFile();
            LoadLanguageFile();
        }

        public static void AddLocalizedValue(string key, string localizedValue, SystemLanguage language)
        {
            if (GetString(key, language, true) != localizedValue)
            {
                localizedValue = ConvertUsableStringToSavedString(localizedValue);
                var lines = ReadAllLines(GetLanguageNames()[language]);
                var index = GetIndexOfKey(key);
                
                var endString = "";
                for (var i = 0; i < lines.Length; i++)
                    endString += (i == index ? localizedValue : lines[i]) + "\n";

                File.WriteAllText(GetLanguageFilesPath() + GetLanguageNames()[language] + ".txt", endString);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
            }
        }

        private static void LoadLanguageFile()
        {
            UpdateLanguageDictionary();
            var languages = GetAvailableLanguages();

            languageArrays = new Dictionary<SystemLanguage, string[]>();
            for (var i = 0; i < languages.Length; i++)
                languageArrays[languages[i]] = ReadAllLines(GetLanguageNames()[languages[i]]);
        }

        public static bool UpdateLanguage(SystemLanguage language)
        {
            if (!GetLanguageNames().ContainsKey(language))
                return false;

            currentLanguage = language;
            LoadLanguageFile();

            OnLanguageChange?.Invoke();

            return true;
        }

        public static void SetLanguageAccordingToSystem()
        {
            var language = Application.systemLanguage;
            if (GetLanguageNames().ContainsKey(language))
                UpdateLanguage(language);
        }

        public static int GetLanguageCount() => GetLanguageNames().Count;

        public static SystemLanguage[] GetAvailableLanguages()
        {
            var languages = new SystemLanguage[GetLanguageNames().Keys.Count];
            for (var i = 0; i < GetLanguageNames().Count; i++)
                languages[i] = GetLanguageNames().Keys.ElementAt(i);

            return languages;
        }

        public static string[] GetCurrentLanguageFiles()
        {
            var languageFiles = new string[GetLanguageNames().Values.Count];
            for (var i = 0; i < GetLanguageNames().Count; i++)
                languageFiles[i] = GetLanguageNames().Values.ElementAt(i);

            return languageFiles;
        }

        public static bool DoesContainKey(string key)
        {
            return GetIndexOfKey(key) != -1;
        }

        private static int GetIndexOfKey(string key)
        {
            for (int i = 0; i < GetKeysArray().Length; i++)
            {
                if (GetKeysArray()[i].Trim() == key.Trim())
                    return i;
            }

            return -1;
        }

        public static void CreateLanguage(SystemLanguage language, string fileName, string lngName,
            string lngErrorMessage)
        {
            fileName = fileName.Trim().Replace(' ', '_');
            if (lngName == "")
                lngName = language.ToString();
            if (lngErrorMessage == "")
                lngErrorMessage = $"Localization Error ({lngName})";

            if (!GetLanguageNames().ContainsKey(language) && !GetLanguageNames().ContainsValue(fileName))
            {
                // The new Language's File.
                var fileContent = $"{lngName.Trim()}\n{lngErrorMessage.Trim()}\n";
                UpdateKeyFile();

                // i starts from 2 as index 0 is lng_name and 1 is lng_error.
                for (var i = 2; i < keysArray.Length; i++)
                    fileContent += "\n";
                File.WriteAllText(GetLanguageFilesPath() + fileName + ".txt", fileContent);

                // Language Names File
                var lines = ReadAllLines(LNG_NAMES_FILE);
                var fileNameText = "";
                for (var i = 0; i < Enum.GetNames(typeof(SystemLanguage)).Length; i++)
                    fileNameText += $"{(i == (int)language ? fileName : i < lines.Length ? lines[i] : "")}\n";

                fileNameText = fileNameText.Remove(fileNameText.Length - 1);
                File.WriteAllText(GetLanguageFilesPath() + LNG_NAMES_FILE + ".txt", fileNameText);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
                UpdateLanguageDictionary();
            }
        }

        public static void RemoveLanguage(SystemLanguage language)
        {
            if (GetLanguageNames().Values.Count == 1)
            {
                Debug.LogError("You can not remove the only language available");
                return;
            }

            var lines = ReadAllLines(LNG_NAMES_FILE);

            lines[(int)language] = "";
            var text = "";
            foreach (var line in lines)
                text += line + "\n";

            text = text.Remove(text.Length - 1);
            File.WriteAllText(GetLanguageFilesPath() + LNG_NAMES_FILE + ".txt", text);

            File.Delete(GetLanguageFilesPath() + GetLanguageNames()[language] + ".txt");

            GetLanguageNames().Remove(language);
        }

        private static void UpdateLanguageDictionary()
        {
            languageNames = new Dictionary<SystemLanguage, string>();
            var lines = ReadAllLines(LNG_NAMES_FILE, false);
            for (var i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    languageNames.Add((SystemLanguage)i, lines[i].Trim());
            }

            if (!languageNames.ContainsKey(currentLanguage))
                currentLanguage = languageNames.Keys.ElementAt(0);
        }

        private static string[] ReadAllLines(string fileName, bool isLastLineEmpty = true)
        {
#if UNITY_EDITOR
            var lines = File.ReadAllText(GetLanguageFilesPath() + fileName + ".txt");
#else
            var lines = Resources.Load<TextAsset>(fileName).text;
#endif 
            var linesArray = lines.Split('\n');
            return isLastLineEmpty ? linesArray.Where((source, index) => index != linesArray.Length - 1).ToArray() :
                linesArray;
        }

        public static SystemLanguage GetCurrentLanguage()
        {
            return currentLanguage;
        }

        public static void RefreshAll()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            UpdateKeyFile();
            LoadLanguageFile();
        }

        public delegate void LanguageChange();

        public static event LanguageChange OnLanguageChange;

        public static Dictionary<SystemLanguage, float> GetLocalizationPercentages()
        {
            var dict = new Dictionary<SystemLanguage, float>();
            foreach (var language in GetAvailableLanguages())
            {

                var lines = ReadAllLines(languageNames[language]);
                var filledRowCount = 0;

                foreach (var line in lines)
                    if (!string.IsNullOrWhiteSpace(line))
                        filledRowCount++;

                dict.Add(language, (float)Math.Round((decimal)(100f * filledRowCount / lines.Length), 2));
            }

            return dict;
        }
    }
}