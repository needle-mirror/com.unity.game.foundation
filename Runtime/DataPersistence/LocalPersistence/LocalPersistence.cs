using System.IO;
using System;
using System.Text;

namespace UnityEngine.GameFoundation.DataPersistence
{
    /// <summary>
    /// This saves data locally onto the user's device.
    /// </summary>
    public class LocalPersistence : BaseDataPersistence
    {
        public const string kBackupSuffix = "_backup";

        /// <inheritdoc />
        public LocalPersistence(IDataSerializer serializer)
            : base(serializer) { }

        /// <summary>
        /// Get the file path for the given id.
        /// </summary>
        /// <param name="identifier">Identifier of the persistence entry (filename, url, ...)</param>
        public static string GetFilePath(string identifier)
        {
            return $"{Application.persistentDataPath}/{identifier}";
        }

        /// <inheritdoc />
        public override void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null)
        {
            SaveFile(identifier, content, onSaveCompleted, onSaveFailed);
        }

        //We need to extract that code from the Save() because it will be used in the child but the child need to override the Save method sometimes
        //So to not rewrite the same code I have done a function with it
        private void SaveFile(string identifier, ISerializableData content, Action onSaveFileCompleted, Action<Exception> onSaveFileFailed)
        {
            string pathMain = GetFilePath(identifier);
            string pathBackup = $"{pathMain}{kBackupSuffix}";

            try
            {
                WriteFile(pathBackup, content);
                File.Copy(pathBackup, pathMain, true);
            }
            catch (Exception e)
            {
                onSaveFileFailed?.Invoke(e);
                return;
            }

            onSaveFileCompleted?.Invoke();
        }

        /// <inheritdoc />
        public override void Load<TSerializableData>(string identifier, Action<TSerializableData> onLoadCompleted = null, Action<Exception> onLoadFailed = null)
        {
            string path;
            string pathMain = GetFilePath(identifier);
            string pathBackup = $"{pathMain}{kBackupSuffix}";

            //If the main file doesn't exist we check for backup
            if (System.IO.File.Exists(pathMain))
            {
                path = pathMain;
            }
            else if (System.IO.File.Exists(pathBackup))
            {
                path = pathBackup;
            }
            else
            {
                onLoadFailed?.Invoke(new FileNotFoundException($"There is no file at the path \"{pathMain}\"."));
                return;
            }

            var strData = "";
            try
            {
                strData = ReadFile(path);
            }
            catch (Exception e)
            {
                onLoadFailed?.Invoke(e);
                return;
            }

            var data = DeserializeString<TSerializableData>(strData);
            onLoadCompleted?.Invoke(data);
        }

        private void WriteFile(string path, ISerializableData content)
        {
            using (var sw = new StreamWriter(path, false, Encoding.Default))
            {
                var data = SerializeString(content);
                sw.Write(data);
            }
        }

        private static string ReadFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            var str = "";

            using (StreamReader sr = new StreamReader(fileInfo.OpenRead(), Encoding.Default))
            {
                str = sr.ReadToEnd();
            }

            return str;
        }

        /// <summary>
        /// Asynchronously delete data from the persistence layer.
        /// </summary>
        /// <param name="identifier">Identifier of the persistence entry (filename, url, ...)</param>
        /// <param name="onDeletionCompleted">Called when the deletion is completed with success.</param>
        /// <param name="onDeletionFailed">Called with a detailed exception when the deletion failed.</param>
        public void Delete(string identifier, Action onDeletionCompleted = null, Action<Exception> onDeletionFailed = null)
        {
            var pathMain = GetFilePath(identifier);

            try
            {
                TryDeleteFile(pathMain);
                TryDeleteFile($"{pathMain}{kBackupSuffix}");

                onDeletionCompleted?.Invoke();
            }
            catch (Exception e)
            {
                onDeletionFailed?.Invoke(e);
            }
        }

        static bool TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }

            return false;
        }

        private string SerializeString(object o)
        {
            return serializer.Serialize(o, true);
        }

        private T DeserializeString<T>(string value) where T : ISerializableData
        {
            return serializer.Deserialize<T>(value, true);
        }
    }
}
