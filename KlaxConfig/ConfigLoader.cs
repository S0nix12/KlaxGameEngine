using System;
using KlaxIO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KlaxShared.Definitions;

namespace KlaxConfig
{
    class SConfigProperties
    {
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
    }

    struct SConfigLoader
    {
        const string USER_FOLDER_DIRECTORY   = "\\Users\\";
        const string USER_FOLDER_NAME        = "user";
        const string LOCK_FILE_NAME          = "klaxlock";
        const string SYSTEM_CONFIG_FILE_NAME = "system.cfg";


        public void Init()
        {
            ParseSystemConfiguration();
            InitUserFolder();
        }

        private void InitUserFolder()
        {
            string[] subDirectories = Directory.GetDirectories(GetUserRootFolder());
            foreach (string path in subDirectories)
            {
                string directoryName = new DirectoryInfo(path).Name;
                if (directoryName.StartsWith(USER_FOLDER_NAME) && !IsLocked(path))
                {
                    LockUserFolder(path);
                    Paths.UserDirectory = path;
                    return;
                }
            }

            //No unused user folder found. Create new.
            Paths.UserDirectory = CreateUserFolder();
        }

        private string GetUserRootFolder()
        {
            if (!string.IsNullOrEmpty(m_rootUserFolder))
            {
                return m_rootUserFolder;
            }

            string path = string.Format("{0}\\{1}", Paths.RootDirectory, USER_FOLDER_DIRECTORY);
            if (Directory.Exists(path))
            {
                return path;
            }
            else
            {
                Directory.CreateDirectory(path);
            }

            m_rootUserFolder = path;
            return path;
        }

        private bool IsLocked(string directoryPath)
        {
            return File.Exists(string.Format("{0}\\.{1}", directoryPath, LOCK_FILE_NAME));
        }

        private void LockUserFolder(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                FileStream stream = File.Create(string.Format("{0}\\.{1}", directoryPath, LOCK_FILE_NAME), 1, FileOptions.DeleteOnClose);
                File.SetAttributes(stream.Name, FileAttributes.Hidden);
            }
        }

        private string CreateUserFolder()
        {
            string[] subDirectories = Directory.GetDirectories(GetUserRootFolder());
            int newNumber = 0;

            foreach (string path in subDirectories)
            {
                string directoryName = new DirectoryInfo(path).Name;
                if (directoryName.StartsWith(USER_FOLDER_NAME))
                {
                    string numberString = new string(path.Where(char.IsDigit).ToArray());
                    int number = int.Parse(numberString) + 1;

                    newNumber = number > newNumber ? number : newNumber;
                }
            }

            string result = string.Format("{0}{1}({2})", GetUserRootFolder(), USER_FOLDER_NAME, newNumber);
            Directory.CreateDirectory(result);
            LockUserFolder(result);

            return result;
        }

        public void ParseSystemConfiguration()
        {
            string path = Paths.RootDirectory + "\\" + SYSTEM_CONFIG_FILE_NAME;
            if (!File.Exists(path))
            {
                using (FileStream file = File.Create(path))
                {
                    return;
                }
            }

            SConfigProperties properties = JsonConvert.DeserializeObject<SConfigProperties>(File.ReadAllText(path));
            if (properties != null)
            {
                foreach (var entry in properties.Variables)
                {
                    CConfigManager.Instance.SetVariable(entry.Key, entry.Value);
                }
            }
        }

        private string m_rootUserFolder;
    }
}
