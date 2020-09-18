using Mezcal.Commands;
using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezcal.Commands
{
    public class Assert: ICommand
    {
        public void Process(JObject command, Context context)
        {
            // get the query object
            // this will be used to match against the rules
            var query = JSONUtil.GetToken(command, "#assert");
            if (query == null) { query = JSONUtil.GetToken(command, "query"); }

            // search through each Item in the Context
            var items = context.Items.Keys.ToList();
            foreach (var item in items)
            {
                // search the Item for matching rules
                // unifying any variables provided by the query
                var jtItem = context.Items[item];
                var results = this.FindRules(jtItem, query, context);
                if (results == null) { continue; }
                
                // if there were any results (rules that matched)
                foreach (var result in results)
                {
                    var jaResults = (JArray)result;

                    foreach (var jaResult in jaResults)
                    {
                        // grab the then (consequent) portion into an array of commands
                        var then = jaResult["then"];

                        // apply the unification that was determined when the rule was matched
                        var unification = (JObject)jaResult["#unification"];
                        unification.Add("?#", query);
                        then = Unification.ApplyUnification(then, unification);

                        // grab the array of commands (convert single to array)
                        var jCommands = new JArray();
                        if (then is JObject) { jCommands.Add(then); }
                        else if (then is JArray) { jCommands = (JArray)then; }

                        // run each command
                        foreach (JObject joTokenCommand in jCommands)
                        {
                            context.CommandEngine.RunCommand(joTokenCommand, context);
                        }
                    }
                }
            }
        }

        internal JArray FindRules(JToken ruleSet, JToken query, Context context)
        {
            if (ruleSet == null) { return null; }
            if (ruleSet.Type != JTokenType.Array) { return null; }

            //if (ruleSet.Type != JTokenType.Object) { return null; }
            //if (ruleSet["rules"] == null) { return null; }

            var results = new JArray();

            // grab the rules - return if none
            //var rules = (JArray)ruleSet["rules"];
            var rules = (JArray)ruleSet;
            if (rules == null) { return null; }

            // for each rule in the set
            foreach (JObject rule in rules)
            {
                var clonedRule = (JObject)rule.DeepClone();

                // detect if regular "when" rule conditions are met
                var subResults = this.RunWhenRule(query, clonedRule);
                if (subResults != null) { results.Add(subResults); }

                // detect if sequential "when-sequence" rule conditions are met
                subResults = this.RunSequenceRule(query, clonedRule, context);
                if (subResults != null) { results.Add(subResults); }
            }

            // return all rules that matched
            if (results.Count == 0) { return null; }
            return results;
        }

        private JArray RunWhenRule(JToken query, JObject rule)
        {
            var results = new JArray();

            // grab the when portion
            var when = rule["when"];
            if (when != null)
            {
                // try the unification - will return null if not able to unify
                var unification = this.Unify(query, when);
                //if (unification == null) { unification = this.Unify(when, query); }

                if (unification != null) // was able to unify
                {
                    // clone the rule and add it to the result
                    // (clone, else the rule may be modified and will fail on future use)
                    rule.Add("#unification", unification); // include the unification information
                    results.Add(rule);
                }
            }

            if (results.Count == 0) { return null; }
            return results;
        }

        private JArray RunSequenceRule(JToken query, JObject rule, Context context)
        {
            var results = new JArray();

            var whenSequence = rule["when-sequence"];
            if (whenSequence != null)
            {
                // get the object at the current sequence position
                int? pos = JSONUtil.GetInt32(rule, "#seq-pos");
                if (pos == null) { pos = 0; rule.Add("#seq-pos", 0); }
                var jaWhenSequence = (JArray)whenSequence;
                var currentItem = jaWhenSequence[pos];

                context.Trace($"Examining {JSONUtil.SingleLine(query)} against {JSONUtil.SingleLine(jaWhenSequence)} at pos = {pos} ({JSONUtil.SingleLine(currentItem)})");

                // try the unification at the current position - will return null if not able to unify
                var unification = Unification.Unify(currentItem, query);

                // if able to unify, instantiate constituent at that position
                if (unification != null)
                {
                    context.Trace($"Unified {JSONUtil.SingleLine(currentItem)} with {JSONUtil.SingleLine(query)}");

                    currentItem = Unification.ApplyUnification(currentItem, unification);
                    jaWhenSequence[pos] = currentItem;
                    pos++;
                    rule["#seq-pos"] = pos;

                    // if sequence is complete, then return unification for the "then" portion to fire
                    if (pos == jaWhenSequence.Count)
                    {
                        context.Trace($"Detected completed sequence {JSONUtil.SingleLine(jaWhenSequence)}");

                        //var clonedRule = (JObject)rule.DeepClone();
                        rule.Add("#unification", unification); // include the unification information
                        results.Add(rule);
                    }
                    // if sequence is not complete, then throw a new arc on the open arcs for next round
                    else
                    {
                        context.Trace($"Adding extended sequence {JSONUtil.SingleLine(jaWhenSequence)} [{pos}] to open arcs");

                        var arcSet = context.Fetch("#seq-arcs");
                        JObject joArcSet = null;
                        JArray arcs = null;
                        if (arcSet == null)
                        {
                            joArcSet = new JObject();
                            arcs = new JArray();
                            joArcSet.Add("rules", arcs);
                            context.Store("#seq-arcs", joArcSet);
                        }
                        else
                        {
                            joArcSet = (JObject)arcSet;
                            arcs = (JArray)joArcSet["rules"];
                        }

                        arcs.Add(rule);
                        //context.Store("#seq-arcs", joArcSet);
                    }
                }
            }

            if (results.Count == 0) { return null; }
            return results;
        }

        internal JObject Unify(JToken when, JToken query)
        {
            JObject result = null;

            if (when.Type == JTokenType.Array) // assume OR (try to prove any)
            {
                var jaWhens = (JArray)when;
                foreach (var jaWhen in jaWhens)
                {
                    var sub = this.Unify(jaWhen, query);
                    if (sub != null) { result = sub; break; }
                }
            }
            else if (when.Type == JTokenType.Object)
            {
                result = Unification.Unify(when, query);
            }

            return result;
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            JObject jCommand = new JObject();
            jCommand.Add("command", "assert");

            JObject inputPortion = new JObject();
            commandEngine.PromptForArgument(inputPortion, "input");

            jCommand.Add("query", inputPortion);
            return jCommand;
        }
    }
}
