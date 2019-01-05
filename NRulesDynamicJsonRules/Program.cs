using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NRules;
using NRules.Fluent;
using NRules.Fluent.Dsl;
using NRules.RuleModel;
using RuleModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NRulesDynamicJsonRules
{
    class Program
    {
     

        static void Main(string[] args)
        {
            RunJsonRuleClassesFromGeneratedDll();
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void RunJsonRuleClassesFromGeneratedDll()
        {
            var ruleRepo = new RuleRepository();
            var loc = Assembly.GetExecutingAssembly().Location;
            var assemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var rulesAssemblyPath = Path.Combine(assemblyFileInfo.Directory.FullName, @"Rules\JsonRulesAssembly.dll");
            var rulesAssemblyFileInfo = new FileInfo(rulesAssemblyPath);

            var rulesClassLoc = @"Rules\JsonRules.cs";

            if (!rulesAssemblyFileInfo.Exists)
            {
                var source = string.Empty;
                using (var sr = new StreamReader(rulesClassLoc))
                {
                    source = sr.ReadToEnd();
                }

                var syntaxTree = CSharpSyntaxTree.ParseText(source);
                var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
                var newtonSoft = MetadataReference.CreateFromFile(typeof(JsonWriter).Assembly.Location);
                var newtonSoftLinq = MetadataReference.CreateFromFile(typeof(JObject).Assembly.Location);
                var ruleModels = MetadataReference.CreateFromFile(typeof(RuleResult<string[]>).Assembly.Location);
                var nRule = MetadataReference.CreateFromFile(typeof(IAgenda).Assembly.Location);
                var nRuleModel = MetadataReference.CreateFromFile(typeof(IContext).Assembly.Location);
                var nRuleDsl = MetadataReference.CreateFromFile(typeof(Rule).Assembly.Location);
                var linq = MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location);

                var compilation = CSharpCompilation.Create("JsonRulesAssembly", syntaxTrees: new[] { syntaxTree }, references: new[] {
                mscorlib, newtonSoft, newtonSoftLinq, ruleModels, nRule, nRuleModel, nRuleDsl, linq},
                options: new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release));

                var emitResult = compilation.Emit(rulesAssemblyFileInfo.FullName);

                if (!emitResult.Success)
                {
                    Array.ForEach(emitResult.Diagnostics.ToArray(), d => Console.WriteLine(d));
                    return;
                }
            }

            var rulesAssembly = Assembly.LoadFile(rulesAssemblyPath);
            ruleRepo.Load(x => x.From(rulesAssembly));

            var factory = ruleRepo.Compile();
            var session = factory.CreateSession();

            var jsonFactStr = @"{questions:[{number:1, answer: true}, {number:2, answer:true}]}";
            var jsonFactObj = JObject.Parse(jsonFactStr);

            session.Insert(jsonFactObj);
            session.Fire();

            var results = new HashSet<string>();

            foreach(var rr in session.Query<RuleResult<string[]>>())
            {
                if ( rr.Result != null)
                {
                    Array.ForEach(((string[])rr.Result), x => results.Add(x));
                }
            }

            Array.ForEach(results.ToArray(), x => Console.WriteLine(x));
        }
    }
}
