using System;
using System.Collections.Generic;
using System.Text;

namespace Mezcal
{
    public class Context
    {
        private readonly Dictionary<string, object> Connections = null;
        private readonly Dictionary<string, object> Items = null;
        public object DefaultConnection { get; set; }
        public readonly Dictionary<string, string> Variables = null;
        public CommandEngine CommandEngine = null;

        public Context()
        {
            this.Items = new Dictionary<string, object>();
            this.Connections = new Dictionary<string, object>();
            this.Variables = new Dictionary<string, string>();
        }

        public string ReplaceVariables(string text)
        {
            foreach (var item in this.Variables)
            {
                text = text.Replace(item.Key, item.Value);
            }

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

        public void Store(string key, object value)
        {
            if (this.Items.ContainsKey(key)) { this.Items[key] = value; }
            else { this.Items.Add(key, value); }
        }

        public object Fetch(string key)
        {
            if (this.Items.ContainsKey(key)) { return this.Items[key]; }
            else { return null; }
        }
    }
}
