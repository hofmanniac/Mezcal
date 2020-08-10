using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Mezcal
{
    public class Context
    {
        private readonly Dictionary<string, object> Connections = null;
        public readonly Dictionary<string, object> Items = null;
        public object DefaultConnection { get; set; }
        public readonly Dictionary<string, JToken> Variables = null;
        public CommandEngine CommandEngine = null;
        public int TraceLevel {get; set;}

        public Context()
        {
            this.Items = new Dictionary<string, object>();
            this.Connections = new Dictionary<string, object>();
            this.Variables = new Dictionary<string, JToken>();
            this.TraceLevel = 0;
        }

        public JToken ReplaceVariables(JToken jtItem)
        {
            if (jtItem.Type == JTokenType.Array)
            {
                var jaItem = (JArray)jtItem;
                JArray result = new JArray();
                foreach(var item in jaItem)
                {
                    result.Add(this.ReplaceVariables(item));
                }
                return result;
            }
            else if (jtItem.Type == JTokenType.Object)
            {
                var joItem = (JObject)jtItem;
                JObject result = new JObject();
                foreach(var prop in joItem.Properties())
                {
                    result.Add(prop.Name, this.ReplaceVariables(prop.Value));
                }
                return result;
            }
            else if (jtItem.Type == JTokenType.String)
            {
                var sItem = (String)jtItem;
                if (sItem.StartsWith("?"))
                {
                    var storedItem = (JToken)this.Fetch(sItem);
                    return this.ReplaceVariables(storedItem);
                }
                else
                {
                    return this.ReplaceVariables(sItem);
                }
            }

            return null;
        }

        public void Trace(string text, int level = 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public string ReplaceVariables(string text)
        {
            foreach (var item in this.Variables)
            {
                text = text.Replace(item.Key, item.Value.ToString());
            }

            text = text.Replace("@time", DateTime.Now.ToShortTimeString());
            text = text.Replace("@date-long", DateTime.Today.ToLongDateString());
            text = text.Replace("@date-file", DateTime.Today.ToString("yyyy-MM-dd"));

            return text;
        }

        public object GetConnection(string key = null)
        {
            if (key == null || key == "") { return this.DefaultConnection; }
            else { return this.Connections[key]; }
        }

        public void AddConnection(string key, object value)
        {
            if (this.Connections.ContainsKey(key)) { this.Connections[key] = value; }
            else { this.Connections.Add(key, value); }
        }

        public void Store(string key, object value, bool overwrite = true)
        {
            if (key.StartsWith("?"))
            {
                if (this.Variables.ContainsKey(key)) { if (overwrite == true) { this.Variables[key] = JToken.FromObject(value); } }
                else { this.Variables.Add(key, JToken.FromObject(value)); }
            }
            else
            {
                if (this.Items.ContainsKey(key)) { if (overwrite == true) { this.Items[key] = value; } }
                else { this.Items.Add(key, value); }
            }
        }

        public object Fetch(string key)
        {
            if (key.StartsWith("?"))
            {
                if (this.Variables.ContainsKey(key)) { return this.Variables[key]; }
                else { return null; }
            }
            else
            {
                if (this.Items.ContainsKey(key)) { return this.Items[key]; }
                else { return null; }
            }
        }
    }
}
