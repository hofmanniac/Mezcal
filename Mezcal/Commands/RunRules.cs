﻿using Mezcal.Commands;
using Mezcal.Connections;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezcal.Commands
{
    public class RunRules : ICommand
    {
        public void Process(JObject command, Context context)
        {
            // get default property
            var query = JSONUtil.GetToken(command, "#run-rules");
            if (query == null) { query = JSONUtil.GetToken(command, "query"); }

            var items = context.Items.Keys.ToList();
            foreach (var item in items)
            {
                var jtItem = context.Items[item];
                var results = this.FindRules(jtItem, query);
                if (results == null) { continue; }

                foreach (var result in results)
                {
                    var then = result["then"];

                    var unification = (JObject)result["#unification"];
                    unification.Add("?#", query);
                    then = Unification.ApplyUnification(then, unification);

                    if (then is JObject)
                    {
                        context.CommandEngine.RunCommand(then, context);
                    }
                    else if (then is JArray jCommands)
                    {
                        foreach (JObject joTokenCommand in jCommands)
                        {
                            context.CommandEngine.RunCommand(joTokenCommand, context);
                        }
                    }
                }
            }
        }

        internal JArray FindRules(JToken ruleSet, JToken query)
        {
            if (ruleSet.Type != JTokenType.Array) { return null; }

            //var type = JSONUtil.GetText(ruleSet, "#type");
            //if (type != "rules") { return null; }

            var results = new JArray();

            //var rules = (JArray)ruleSet["rules"];
            var rules = (JArray)ruleSet;
            if (rules == null) { return null; }

            //var testInput = query["input"].ToString();

            foreach (JObject rule in rules)
            {
                var when = rule["when"];
                if (when == null) { continue; }

                //var unification = Text.UnifyStrings(when, query);
                var unification = Unification.Unify(when, query);
                if (unification != null)
                {
                    var clonedRule = (JObject)rule.DeepClone();
                    clonedRule.Add("#unification", unification);
                    results.Add(clonedRule);
                }
            }

            return results;
        }

        public JObject Prompt(CommandEngine commandEngine)
        {
            JObject jCommand = new JObject();
            jCommand.Add("command", "run-rules");

            JObject inputPortion = new JObject();
            commandEngine.PromptForArgument(inputPortion, "input");

            jCommand.Add("query", inputPortion);
            return jCommand;
        }
    }
}
