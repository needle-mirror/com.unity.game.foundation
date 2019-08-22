using System.IO;
using System;
using System.Text;

namespace UnityEngine.GameFoundation
{
    public class LocalPersistence : BaseDataPersistence
    {
        /// <inheritdoc />
        public LocalPersistence(IDataSerializer serializer) : base(serializer)
        {
        }

        /// <inheritdoc />
        public override void Save(string identifier, ISerializableData content, Action onSaveCompleted = null, Action<Exception> onSaveFailed = null)
        {
            SaveFile(identifier, content, onSaveCompleted, onSaveFailed);
        }

        //We need to extract that code from the Save() because it will be used in the child but the child need to override the Save method sometimes
        //So to not rewrite the same code I have done a function with it
        protected void SaveFile(string identifier, ISerializableData content, Action onSaveFileCompleted, Action<Exception> onSaveFileFailed)
        {
            string pathMain = $"{Application.persistentDataPath}/{identifier}";
            string pathBackup = $"{Application.persistentDataPath}/{identifier + "_backup"}";

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
        public override void Load<T>(string identifier, Action<ISerializableData> onLoadCompleted = null, Action<Exception> onLoadFailed = null)
        {
            string path;
            string pathMain = $"{Application.persistentDataPath}/{identifier}";
            string pathBackup = $"{Application.persistentDataPath}/{identifier + "_backup"}";

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
                onLoadFailed?.Invoke(new Exception("File doesn't exist", null));
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

            var data = DeserializeString<T>(strData);
            onLoadCompleted?.Invoke(data);
        }

        protected void WriteFile(string path, ISerializableData content)
        {
            using (var sw = new StreamWriter(path, false, Encoding.Default))
            {
                var data = SerializeString(content);
                sw.Write(data);
            }
        }

        protected static string ReadFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            var str = "";
            
            using (StreamReader sr = new StreamReader(fileInfo.OpenRead(), Encoding.Default))
            {
                str = sr.ReadToEnd();
            }

            return str;
        }

        protected static bool DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException("DeleteFile: delete failed." + e);
            }

            return false;
        }

        protected string SerializeString(object o)
        {
            return m_Serializer.Serialize(o, true);
        }

        protected T DeserializeString<T>(string value) where T : ISerializableData
        {
            return m_Serializer.Deserialize<T>(value, true);
        }
    }
}