using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibliothek.Utility
{
    internal class FileUtil
    {
        private string filePath = "data.json";

        // این فانکشن برای ذخیره کردن متن در فایل JSON استفاده می‌شود
        public void SaveStringToJson(string data)
        {
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(filePath, json.EncryptSHA());
        }

        // این فانکشن برای خواندن متن از فایل JSON استفاده می‌شود
        public string ReadStringFromJson()
        {
            
            if (!File.Exists(filePath))
            {
                return null;
            }

            string json = File.ReadAllText(filePath);
            string data = JsonConvert.DeserializeObject<string>(json.DecryptSHA());
            return data;
        }

        public void DeleteString()
        {
            File.Delete(filePath);
        }
    }
}
