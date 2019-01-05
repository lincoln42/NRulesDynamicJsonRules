using Newtonsoft.Json.Linq;
using NRules.Fluent.Dsl;
using RuleModels;
using System.Linq;

namespace NRulesDynamicJsonRules.Rules
{
    public class OptionA : Rule
    {
        public override void Define()
        {
            JObject questionSet = null;
            JObject q = null;

            When()
                .Match<JObject>(() => questionSet,
                                qs => qs != null && qs.SelectToken("questions") != null && qs["questions"] is JArray && qs["questions"].HasValues)
                .Let(() => q,
                      () => questionSet.SelectToken("questions").FirstOrDefault(x => x.SelectToken("number") != null &&
                      x["number"] is JValue && x.SelectToken("number").Value<int>() == 1 &&
                      x.SelectToken("answer") != null && x["answer"] is JValue && x.SelectToken("answer").Value<bool>() == true))
                .Having(() => q != null);
            Then()
                 .Yield(ctx => new RuleResult<string[]> { Result = new string[] { "OptionA" } });
        }
    }

    public class OptionB : Rule
    {
        public override void Define()
        {
            JObject questionSet = null;
            JObject q = null;

            When()
                .Match<JObject>(() => questionSet,
                                qs => qs != null && qs.SelectToken("questions") != null && qs["questions"] is JArray && qs["questions"].HasValues)
                .Let(() => q,
                      () => questionSet.SelectToken("questions").FirstOrDefault(x => x.SelectToken("number") != null &&
                      x["number"] is JValue && x.SelectToken("number").Value<int>() == 2 &&
                      x.SelectToken("answer") != null && x["answer"] is JValue && x.SelectToken("answer").Value<bool>() == true))
                .Having(() => q != null);
            Then()
                 .Yield(ctx => new RuleResult<string[]> { Result = new string[] { "OptionB" } });
        }
    }

}
