﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

        public static JToken GetToken(JToken jToken, string propertyName)
        {
            if (jToken[propertyName] == null) { return null; }
            else { return jToken[propertyName]; }
        }

        public static string GetText(JToken jToken, string propertyName)
        {
            if (jToken.Type == JTokenType.Array) { return null; }
            if (jToken.Type == JTokenType.Property)
            {
                var jpToken = (JProperty)jToken;
                if (jpToken.Name == propertyName)
                {
                    return jpToken.Value.ToString();
                }
            }
            else
            {
                if (jToken[propertyName] == null) { return null; }
                else { return jToken[propertyName].ToString(); }
            }

            return null;
        }

        public static string GetCommandName(JToken jToken)
        {
            string result = JSONUtil.GetText(jToken, "command");

            if (result == null)
            {
                var jtFirst = jToken.First;
                if (jtFirst != null)
                {
                    var jpFirst = (JProperty)jtFirst;
                    var sFirst = jpFirst.Name.ToString();
                    if (sFirst.StartsWith("#")) { result = sFirst.Substring(1); }
                }
            }

            return result;
        }

        public static int? GetInt32(JToken jToken, string propertyName)
        {
            int? result = null;

            var s = GetText(jToken, propertyName);
            if (s != null) { result = Int32.Parse(s); }

            return result;
        }

        public static string SingleLine(JToken jToken)
        {
            var result = new StringBuilder();

            bool inquotes = false;

            foreach(var c in jToken.ToString())
            {
                if (c == '\r' || c == '\n' || c == '\t') { continue; }
                if (c == '\"') { inquotes = !inquotes; }
                if (!inquotes && c == ' ') { continue; }

                result.Append(c);
            }

            //return jToken.ToString().Replace("\r\n", "").Replace("\t", "");
            return result.ToString();
        }
    }
}
