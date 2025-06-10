using System;
using System.IO;
using System.Reflection;
using System.Text.Json; // Make sure this is present
using System.Text.Json.Serialization; // For JsonSerializerOptions if needed
using System.Collections.Generic;
using System.Windows;
using System.Diagnostics; // For Debug.WriteLine

namespace StandardLicensingGenerator
{
    public class UserSettingsManager
    {
        private readonly string _settingsFilePath;
        private Dictionary<string, object?> _settings = new Dictionary<string, object?>();

        private const string UserSettingsFolderName = "UserSettings";
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            //Converters = { new ObjectToInferredTypesConverter() } // We might need this later for deserializing object types
        };


        public UserSettingsManager(Window window, string? customFileName = null)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            string? applicationName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "UnknownApp";
            // string userName = Environment.UserName; // Removed as ApplicationData is user-specific
            string windowName = window.GetType().Name;
            string fileName = string.IsNullOrEmpty(customFileName) ? $"{windowName}.json" : $"{customFileName}.json";

            string appDataBaseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Adjusted path for better organization: AppData/AppName/UserSettings/WindowName.json
            // Removed username from the path as ApplicationData is already user-specific.
            string userApplicationSettingsPath = Path.Combine(appDataBaseDir, applicationName, UserSettingsFolderName);

            if (!Directory.Exists(userApplicationSettingsPath))
            {
                Directory.CreateDirectory(userApplicationSettingsPath);
            }

            _settingsFilePath = Path.Combine(userApplicationSettingsPath, fileName);
            Debug.WriteLine($"UserSettingsManager: Settings file path is '{_settingsFilePath}'");


            LoadSettings(); // Call LoadSettings upon initialization
        }

        public void SetSetting<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            _settings[key] = value;
        }

        public T GetSetting<T>(string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_settings.TryGetValue(key, out object? storedValue))
            {
                // When deserializing from JSON, numbers might become JsonElement
                if (storedValue is JsonElement jsonElement)
                {
                    try
                    {
                        // Attempt to deserialize the JsonElement to the target type T
                        T? result = JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), _jsonOptions);
                        if (result != null) return result;
                    }
                    catch (JsonException ex)
                    {
                        Debug.WriteLine($"Error deserializing JsonElement for key '{key}' to type {typeof(T)}. Exception: {ex.Message}");
                        return defaultValue;
                    }
                }
                else if (storedValue is T typedValue)
                {
                    return typedValue;
                }
                else if (storedValue != null) // Type mismatch, but not JsonElement
                {
                     // Attempt a direct cast or conversion if possible, or log and return default
                    try
                    {
                        return (T)Convert.ChangeType(storedValue, typeof(T));
                    }
                    catch (InvalidCastException ex)
                    {
                         Debug.WriteLine($"Type mismatch for key '{key}'. Expected {typeof(T)}, got {storedValue.GetType()}. Attempted conversion failed. Exception: {ex.Message}");
                    }
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Saves the current settings to the JSON file.
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(_settings, _jsonOptions);
                File.WriteAllText(_settingsFilePath, jsonString);
                Debug.WriteLine($"Settings saved to '{_settingsFilePath}'");
            }
            catch (IOException ex)
            {
                // Handle file I/O errors (e.g., disk full, permissions)
                Debug.WriteLine($"Error saving settings to '{_settingsFilePath}': {ex.Message}");
                // Optionally, re-throw or handle more gracefully depending on application requirements
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization errors
                Debug.WriteLine($"Error serializing settings: {ex.Message}");
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                Debug.WriteLine($"An unexpected error occurred while saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads settings from the JSON file.
        /// </summary>
        private void LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                Debug.WriteLine($"Settings file '{_settingsFilePath}' not found. Initializing with empty settings.");
                _settings = new Dictionary<string, object?>(); // Ensure settings are empty
                return;
            }

            try
            {
                string jsonString = File.ReadAllText(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonString, _jsonOptions);
                _settings = loadedSettings ?? new Dictionary<string, object?>();
                Debug.WriteLine($"Settings loaded from '{_settingsFilePath}'");
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"Error loading settings from '{_settingsFilePath}': {ex.Message}");
                _settings = new Dictionary<string, object?>(); // Initialize to empty on error
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error deserializing settings from '{_settingsFilePath}': {ex.Message}. File might be corrupt or in an unexpected format.");
                _settings = new Dictionary<string, object?>(); // Initialize to empty on error
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                Debug.WriteLine($"An unexpected error occurred while loading settings: {ex.Message}");
                _settings = new Dictionary<string, object?>(); // Initialize to empty on error
            }
        }
    }
}
