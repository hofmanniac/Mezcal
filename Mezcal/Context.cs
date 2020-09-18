using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public JToken Resolve(JToken jToken)
        {
            //var jtItem = Unification.ApplyUnification(jToken, unification);
            var jtItem = this.ReplaceVariables(jToken);

            if (jtItem.Type == JTokenType.Object)
            {
                var joItem = (JObject)jtItem;
                foreach (var prop in joItem.Properties())
                {
                    prop.Value = this.Query(prop.Value);
                }
                jtItem = joItem;
            }

            return jtItem;
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
                    if (prop.Name == "#store") { result.Add(prop.Name, prop.Value); }
                    else { result.Add(prop.Name, this.ReplaceVariables(prop.Value)); }
                }
                return result;
            }
            else if (jtItem.Type == JTokenType.String)
            {
                var sItem = (String)jtItem;
                if (sItem.StartsWith("?"))
                {
                    var storedItem = (JToken)this.Fetch(sItem);
                    if (storedItem != null) { return this.ReplaceVariables(storedItem); }
                    else { return sItem; }
                }
                else
                {
                    var s = this.ReplaceVariables(sItem);
                    s = this.ResolveItemPaths(s);
                    return s;
                }
            }

            return null;
        }

        private string ResolveItemPaths(string text)
        {
            // refactoring needed -
            // this is a rough "first cut" at resolving item information from a path
            // this is not an implementation of JSONPath, this is sort of a simplified
            // version of JSONPath meant to specify item information easily

            string result = text;

            if (text.StartsWith("$") == false) { return result; }

            string[] parts = text.Split('.');

            JToken currentItem = null;

            foreach(string part in parts)
            {
                if (part.StartsWith("$"))
                {
                    var token = part.Substring(1);
                    currentItem = this.FindItemsByName(token);
                }
                else
                {
                    if (currentItem.Type == JTokenType.String)
                    {
                        currentItem = this.FindItemsByName(currentItem.ToString());
                    }

                    if (currentItem.Type == JTokenType.Object)
                    {
                        var joCurrentItem = (JObject)currentItem;
                        if (joCurrentItem[part] != null)
                        {
                            currentItem = joCurrentItem[part];
                        }
                    }
                    else if (currentItem.Type == JTokenType.Array)
                    {
                        foreach(var jtItem in currentItem)
                        {
                            if (jtItem.Type == JTokenType.Object)
                            {
                                var joItem = (JObject)jtItem;
                                if (joItem[part] != null)
                                {
                                    currentItem = joItem[part];
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (currentItem != null)
            {
                if (currentItem.Type == JTokenType.String)
                {
                    result = currentItem.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// Find items based on the item name
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Returns matching items if found. Returns empty (non-null) array if none found.</returns>
        public JToken FindItemsByName(string item)
        {
            var query = new JObject();
            query.Add("item", item);
            return this.FindItems(query, false);
        }

        /// <summary>
        /// Find item based on the item name
        /// </summary>
        /// <param name="item"></param>
        /// <returns>A single object or null if none found. Returns the first matching item if multiple found.</returns>
        public JObject FindItemByName(string item)
        {
            var query = new JObject();
            query.Add("item", item);
            var results = this.FindItems(query, false);

            if (results.Count() > 0) { return (JObject)results[0]; }
            else { return null; }
        }

        /// <summary>
        /// Find items based on a template pattern
        /// </summary>
        /// <param name="query"></param>
        /// <param name="stopAfterFirst"></param>
        /// <returns>Returns matching items if found. Returns empty (non-null) array if none found.</returns>
        public JToken FindItems(JObject query, bool stopAfterFirst = true)
        {
            var results = new JArray();

            foreach (var source in this.Items)
            {
                if (source.Value == null) { continue; }

                foreach (var candidateItem in source.Value)
                {
                    var joItem = (JObject)candidateItem.DeepClone();

                    var u = Unification.Unify(joItem, query);
                    if (u == null) { continue; }

                    joItem.Add("#unification", u);
                    results.Add(joItem);

                    if (stopAfterFirst) { return results[0]; }
                }
            }

            return results;
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
            if (query.Type == JTokenType.Object)
            {
                var joQuery = (JObject)query;
                var select = joQuery["#select"];
                if (select == null) { return query; }
                else { joQuery.Remove("#select"); }

                var result = new JArray();

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
                                var sSelect = select.ToString();

                                if (sSelect == "*") { result.Add(joItem); }
                                else
                                {
                                    var value = joItem[select.ToString()];
                                    result.Add(value);
                                }
                            }
                            
                        }
                    }
                }

                if (result.Count == 1) { return result.First; }
                else { return result; }

            }
            else if (query.Type == JTokenType.Array)
            {
                return query;
            }
            else if (query.Type == JTokenType.String) 
            {
                return query;
            }

            return null;
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

        public void Store(string key, JToken value, StoreMode storeMode = StoreMode.Replace)
        {
            if (key.StartsWith("?") || key.StartsWith("$"))
            {
                if (this.Variables.ContainsKey(key))
                {
                    if (storeMode == StoreMode.Replace)
                    {
                        this.Variables[key] = JToken.FromObject(value);
                    }
                    // if merging
                    else if (storeMode == StoreMode.Merge)
                    {
                        // get the existing
                        var existing = this.Variables[key];

                        // convert to an object
                        if (existing.Type == JTokenType.Object)
                        {
                            var joExisting = (JObject)existing;
                            joExisting.Merge(value);
                        }
                    }
                }
                else { this.Variables.Add(key, JToken.FromObject(value)); }
            }
            else
            {
                if (this.Items.ContainsKey(key))
                {
                    if (storeMode == StoreMode.Replace)
                    {
                        this.Items[key] = value;
                    }
                }
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

    public enum StoreMode
    {
        Replace, KeepExisting, Merge
    }
}
