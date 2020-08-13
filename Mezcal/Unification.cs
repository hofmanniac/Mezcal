using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mezcal
{
    public class Unification
    {
        public static JObject Unify(JToken term1, JToken term2)
        {
            if (term1 == null && term2 == null) { return new JObject(); }
            if (term1 == null || term2 == null) { return null; }

            // https://www.javatpoint.com/ai-unification-in-first-order-logic 

            //Step. 1: If Ψ1 or Ψ2 is a variable or constant, then:
            if (term1.Type == JTokenType.String || term2.Type == JTokenType.String)
            {
                var sTerm1 = term1.ToString();
                var sTerm2 = term2.ToString();

                //return Text.TextbookUnifyStrings(sTerm1, sTerm2);
                return Unify(sTerm1, sTerm2);
            }
            else if (term1.Type == JTokenType.Object && term2.Type == JTokenType.Object)
            {
                var joTerm1 = (JObject)term1;
                var joTerm2 = (JObject)term2;

                // Step.2: If the initial Predicate symbol in Ψ1 and Ψ2 are not same, then return FAILURE.
                // Relaxed - JSON Objects will not have a "head" predicate argument

                // Step. 3: IF Ψ1 and Ψ2 have a different number of arguments, then return FAILURE.
                // Relaxed - Will allow term1 to match term2 as long as term1 contains all arguments
                // that are in term2 (term1 may contain more arguments than term2). Arguments will be
                // matched by name

                //Step. 4: Set Substitution set(SUBST) to NIL. 
                JObject subSet = new JObject();

                //Step. 5: For i = 1 to the number of elements in Ψ1.
                foreach (var prop in joTerm2.Properties())
                {
                    //	a) Call Unify function with the ith element of Ψ1 and ith element of Ψ2, and put the result into S.
                    var jtTest1 = joTerm1[prop.Name];
                    if (jtTest1 == null) { return null; }

                    //	b) If S = failure then returns Failure
                    var subSet2 = Unify(jtTest1, prop.Value);
                    if (subSet2 == null) { return null; }
                    //	c) If S ≠ NIL then do,
                    else if (subSet2.Properties().Count() > 0) { subSet.Merge(subSet2); }
                }

                return subSet;
            }

            return null;
        }


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
        public static JObject Unify(string m1, string m2)
        {
            if (m1 == null || m2 == null) { return null; }

            var result = new JObject();

            if (m1.Equals(m2)) { return result; }

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

        public static JObject TextbookUnify(string sTerm1, string sTerm2)
        {
            //	a) If Ψ1 or Ψ2 are identical, then return NIL.
            if (sTerm1.Equals(sTerm2)) { return new JObject(); }

            //	b) Else if Ψ1 is a variable,
            else if (sTerm1.StartsWith("?") && sTerm1.Contains(" ") == false)
            {
                //		a. then if Ψ1 occurs in Ψ2, then return FAILURE
                if (sTerm2.Contains(sTerm1)) { return null; }
                //		b.Else return { (Ψ2 / Ψ1)}.
                else { return NewSubstitution(sTerm1, sTerm2); }
            }

            //	c) Else if Ψ2 is a variable, 
            else if (sTerm2.StartsWith("?") && sTerm2.Contains(" ") == false)
            {
                //		a.If Ψ2 occurs in Ψ1 then return FAILURE,
                if (sTerm1.Contains(sTerm2)) { return null; }
                //		b.Else return { (Ψ1 / Ψ2)}. 
                else { return NewSubstitution(sTerm2, sTerm1); }
            }
            //	d) Else return FAILURE.
            else { return null; }
        }

        private static JObject NewSubstitution(string name, JToken value)
        {
            var result = new JObject();
            result.Add(name, value);
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
            else if (result is JArray jaItems)
            {
                var newArray = new JArray();
                foreach(var jaItem in jaItems)
                {
                    newArray.Add(ApplyUnification(jaItem, unification));
                }
                result = newArray;
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

        public static JToken Replace(JToken item, string oldValue, string newValue)
        {
            JToken result = null;

            if (item.Type == JTokenType.Object)
            {
                var joItem = (JObject)item.DeepClone();
                foreach(var prop in joItem.Properties())
                {
                    prop.Value = Replace(prop.Value, oldValue, newValue);
                }

                return joItem;
            }
            else if (item.Type == JTokenType.Array)
            {
                var newArray = new JArray();
                var jaItems = (JArray)item;
                foreach(var jtItem in jaItems)
                {
                    var newArrayItem = Replace(jtItem, oldValue, newValue);
                    newArray.Add(newArrayItem);
                }
                result = newArray;
            }
            else if (item.Type == JTokenType.String)
            {
                var jsItem = item.ToString();
                result = jsItem.Replace(oldValue, newValue);
            }

            return result;
        }
    }
}
