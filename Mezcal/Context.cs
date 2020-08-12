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
        public readonly Dictionary<string, JToken> Items = null;
        public object DefaultConnection { get; set; }
        public readonly Dictionary<string, JToken> Variables = null;
        public CommandEngine CommandEngine = null;
        public int TraceLevel {get; set;}

        public Context()
        {
            this.Items = new Dictionary<string, JToken>();
            this.Connections = new Dictionary<string, object>();
            this.Variables = new Dictionary<string, JToken>();
            this.TraceLevel = 0;
        }

        public JToken ReplaceVariables(JToken jtItem)
        {
            if (jtItem == null) { return null; }

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

        public JToken Query(JToken query)
        {
            var result = new JArray();

            if (query.Type == JTokenType.Object)
            {
                var joQuery = (JObject)query;
                var select = joQuery["#select"];
                if (select != null) { joQuery.Remove("#select"); }

                // {"#item": "?x", "select": "username"}
                foreach (var querySource in this.Items)
                {
                    if (querySource.Value.Type == JTokenType.Object)
                    {
                        var joQuerySource = (JObject)querySource.Value;
                        var items = joQuerySource["items"];
                        if (items == null) { continue; }
                        var jaItems = (JArray)items;

                        foreach (var item in jaItems)
                        {
                            var joItem = (JObject)item.DeepClone();

                            var unification = Unification.Unify(joItem, joQuery);
                            if (unification == null) { continue; }

                            if (select == null)
                            {
                                joItem.Add("#unification", unification);
                                result.Add(joItem);
                            }
                            else
                            {
                                var value = joItem[select.ToString()];
                                result.Add(value);
                            }
                            
                        }
                    }
                }
            }
            else if (query.Type == JTokenType.String) 
            {
                result.Add(query.ToString());
            }

            return result;
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

        public void Store(string key, JToken value, bool overwrite = true)
        {
            if (key.StartsWith("?") || key.StartsWith("$"))
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
