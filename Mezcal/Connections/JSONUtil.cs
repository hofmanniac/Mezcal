﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mezcal.Connections
{
    public class JSONUtil
    {
        public static JToken ReadFile(string filename)
        {
            JToken result = null;

            if (File.Exists(filename))
            {
                using (StreamReader reader = File.OpenText(filename))
                {
                    result = JToken.ReadFrom(new JsonTextReader(reader));
                }
            }

            return result;
        }

        public static void WriteFile(JToken jToken, string filename)
        {
            if (jToken == null) { Console.WriteLine("JSONFile - Nothing to write"); return; }

            using (StreamWriter file = File.CreateText(filename))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                jToken.WriteTo(writer);
            }
        }

        public static void AppendToFile(JToken jToken, string filename)
        {
            if (jToken == null) { Console.WriteLine("JSONFile - Nothing to write"); return; }

            using (StreamWriter file = File.AppendText(filename))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                jToken.WriteTo(writer);
            }
        }

        public static string GetText(JToken jToken, string propertyName)
        {
            if (jToken[propertyName] == null) { return null; }
            else { return jToken[propertyName].ToString(); }
        }

        public static int? GetInt32(JToken jToken, string propertyName)
        {
            int? result = null;

            var s = GetText(jToken, propertyName);
            if (s != null) { result = Int32.Parse(s); }

            return result;
        }
    }
}
