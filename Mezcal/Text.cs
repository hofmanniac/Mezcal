using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mezcal
{
    public class Text
    {
        /// <summary>
        /// Examples:
        /// 
        /// i like apples
        /// i like apples
        /// 
        /// i like apples
        /// ?x like apples
        /// 
        /// i really like apples
        /// ?x like ?y
        /// ?x = i really, ?y = apples
        /// 
        /// i like to eat apples
        /// ?x like ?y
        /// ?x = i, ?y = to eat apples
        /// 
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static JObject UnifyStrings(string m1, string m2)
        {
            if (m1 == null || m2 == null) { return null; }

            var result = new JObject();

            //Console.Write("Comparing '{0}' to '{1}'...", m1, m2);

            string[] a1 = m1.Split(' ');
            string[] a2 = m2.Split(' ');

            int i1 = 0;
            int i2 = 0;

            bool loop = true;

            string boundvalue = "";

            while (loop)
            {
                string w1 = a1[i1];
                string w2 = a2[i2];

                if (w1 == w2)
                {
                    i1++;
                    i2++;
                }
                else
                {
                    bool v1 = false;
                    bool v2 = false;
                    bool wild1 = false;
                    bool wild2 = false;

                    if (w1.StartsWith("?")) { v1 = true; }
                    if (w2.StartsWith("?")) { v2 = true; }
                    if (w1.StartsWith("*")) { wild1 = true; }
                    if (w2.StartsWith("*")) { wild2 = true; }

                    if (wild2 == true)
                    {
                        string n1 = "";
                        string n2 = "";

                        if (i1 + 1 < a1.Length) { n1 = a1[i1 + 1]; }
                        if (i2 + 1 < a2.Length) { n2 = a2[i2 + 1]; }

                        if (w1 == n2)
                        {
                            i2++;
                        }
                        else
                        {
                            i1++;
                        }
                    }
                    else if (wild1 == true)
                    {
                        string n1 = "";
                        string n2 = "";

                        if (i1 + 1 < a1.Length) { n1 = a1[i1 + 1]; }
                        if (i2 + 1 < a2.Length) { n2 = a2[i2 + 1]; }

                        if (w2 == n1)
                        {
                            i1++;
                        }
                        else
                        {
                            i2++;
                        }
                    }
                    else if (v2 == true && v1 == false)
                    {
                        boundvalue += " " + w1;

                        string n1 = "";
                        string n2 = "";

                        if (i1 + 1 < a1.Length) { n1 = a1[i1 + 1]; }
                        if (i2 + 1 < a2.Length) { n2 = a2[i2 + 1]; }

                        if (n1 == n2)
                        {
                            result.Add(w2, boundvalue.Trim());
                            boundvalue = "";
                            i2++;
                        }

                        i1++;
                    }
                    else if (v1 == true && v2 == false)
                    {

                        boundvalue += " " + w2;

                        string n1 = "";
                        string n2 = "";

                        if (i1 + 1 < a1.Length) { n1 = a1[i1 + 1]; }
                        if (i2 + 1 < a2.Length) { n2 = a2[i2 + 1]; }

                        if (n1 == n2)
                        {
                            result.Add(w1, boundvalue.Trim());
                            boundvalue = "";
                            i1++;
                        }

                        i2++;
                    }
                    else
                    {
                        loop = false;
                    }
                }

                // if at the end of m1 and we're sitting on a wildcard in m2, advance m2
                if (i1 == a1.Length)
                {
                    if (i2 < a2.Length)
                    {
                        w2 = a2[i2];
                        if (w2.StartsWith("*") == true)
                        {
                            i2++;
                        }
                    }
                }

                if (i1 == a1.Length) { loop = false; }
                if (i2 == a2.Length) { loop = false; }
            }

            if (i1 != a1.Length || i2 != a2.Length)
            {
                if ((i1 + 1 == a1.Length) && (a1[i1] == "*")) { }
                else { result = null; }
            }

            return result;
        }

        public static JToken ApplyUnification(JToken item, JObject unification)
        {
            JToken result = item.DeepClone();

            if (result is JObject joItem)
            {
                foreach (var prop in joItem.Properties())
                {
                    var newValue = ApplyUnification(prop.Value, unification);
                    if (newValue != null) { result[prop.Name] = newValue; }
                }
            }
            else if (result.Type == JTokenType.String)
            {
                result = ApplyUnification(result.ToString(), unification);
            }

            return result;

        }

        public static string ApplyUnification(string s, JObject unification)
        {
            string result = null;

            if (s == null) { return null; }
            //s = s.ToLower();

            string[] words = s.Split(' ');
            if ((words.Length == 1) && words[0].StartsWith("?"))
            {
                if (unification.ContainsKey(words[0]))
                {
                    result = unification[words[0]].ToString();
                }
                else
                {
                    result = s;
                }
            }
            else
            {
                foreach (var item in unification.Properties())
                {
                    if (item.Name.StartsWith("?") == false) { continue; }

                    if (item.Value.Type == JTokenType.String)
                    {
                        var sItem = item.Value.ToString();
                        s = s.Replace(item.Name, sItem);
                    }
                }

                // redundant with Context.ReplaceVariables - refactor?
                s = s.Replace("@time", DateTime.Now.ToShortTimeString());
                s = s.Replace("@date-long", DateTime.Today.ToLongDateString());
                s = s.Replace("@date-file", DateTime.Today.ToString("yyyy-MM-dd"));

                result = s;
            }

            return result;
        }
    }
}
