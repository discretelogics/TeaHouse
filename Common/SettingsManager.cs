using System.Windows;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace TeaTime
{
    public class SettingsManager
    {
        #region fields
        private Action<string, string, string> storeSerializedObject;
        private Func<string, string, string> readSerializedObject;
        #endregion

        #region life
        private SettingsManager()
        {
            Func<string, string> getStoreDirectory = (collectionName) => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TeaTime", collectionName);
            Func<string, string, string> getStoreFile = (collectionName, name) => Path.Combine(getStoreDirectory(collectionName), name + ".json");
            Configure(  (collectionName, name, value) => 
                            {
                                string directory = getStoreDirectory(collectionName);
                                if (!Directory.Exists(directory))
                                {
                                    Directory.CreateDirectory(directory);
                                }
                                File.WriteAllText(getStoreFile(collectionName, name), value);
                            },
                        (collectionName, name) =>
                            {
                                string file = getStoreFile(collectionName, name);
                                if (File.Exists(file))
                                {
                                    return File.ReadAllText(file);
                                }
                                return null;
                            });
        }

        public void Configure(Action<string, string, string> storeSerializedObject, Func<string, string, string> readSerializedObject)
        {
            Guard.ArgumentNotNull(storeSerializedObject, "storeSerializedObject");
            Guard.ArgumentNotNull(readSerializedObject, "readSerializedObject");

            this.storeSerializedObject = storeSerializedObject;
            this.readSerializedObject = readSerializedObject;
        }
        #endregion

        #region core

        public void Store(string collectionName, string name, object value)
        {
            Guard.ArgumentNotNullOrWhiteSpace(collectionName, "collectionName");
            Guard.ArgumentNotNullOrWhiteSpace(name, "name");

            string serialized = JsonConvert.SerializeObject(value);
            this.storeSerializedObject(collectionName, name, serialized);
        }
        public T Read<T>(string collectionName, string name, Func<T> defaultValueFactory)
        {
            Guard.ArgumentNotNullOrWhiteSpace(collectionName, "collectionName");
            Guard.ArgumentNotNullOrWhiteSpace(name, "name");

            string serialized = this.readSerializedObject(collectionName, name);
            if (string.IsNullOrEmpty(serialized))
            {
                return defaultValueFactory();
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(serialized);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Failed to load {0} from {1}.\r\n{2}", name, collectionName, ex.Message),
                                "Settings load error", MessageBoxButton.OK, MessageBoxImage.Error);

                var fallback = defaultValueFactory();
                this.Store(collectionName, name, fallback);
                return fallback;
            }
        }

        #endregion

        #region singleton
        public static SettingsManager Instance
        {
            get
            {
                return Singleton.Instance;
            }
        }

        private class Singleton
        {
            static Singleton()
            {
            }
            internal static readonly SettingsManager Instance = new SettingsManager();
        }
        #endregion
    }
}
