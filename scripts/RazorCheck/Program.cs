using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using System.Web.Razor;
using System.Web.Razor.Generator;
class Program
{
    static int Main(string[] args)
    {
        Directory.CreateDirectory(args[1]); int errors=0,index=0;
        foreach(var file in Directory.GetFiles(args[0],"*.cshtml",SearchOption.AllDirectories))
        {
            var source=File.ReadAllText(file); var match=Regex.Match(source,@"(?m)^@model\s+([^\r\n]+)");
            var model=match.Success?match.Groups[1].Value.Trim():"dynamic"; if(match.Success)source=source.Remove(match.Index,match.Length);
            var host=new RazorEngineHost(new CSharpRazorCodeLanguage()) { DefaultBaseClass="System.Web.Mvc.WebViewPage<"+model+">",DefaultClassName="View"+index++,DefaultNamespace="RazorValidation" };
            host.GeneratedClassContext=new GeneratedClassContext("Execute","Write","WriteLiteral","WriteTo","WriteLiteralTo","System.Web.WebPages.HelperResult","DefineSection");
            foreach(var ns in new[]{"System","System.Linq","System.Collections.Generic","System.Web","System.Web.Mvc","System.Web.Mvc.Html","System.Web.Mvc.Ajax","System.Web.Optimization","System.Web.Routing","RestaurantWebsite","RestaurantWebsite.Infrastructure"})host.NamespaceImports.Add(ns);
            var result=new RazorTemplateEngine(host).GenerateCode(new StringReader(source),null,null,file);
            foreach(var error in result.ParserErrors){Console.WriteLine(file+":"+error.Location.LineIndex+": "+error.Message);errors++;}
            using(var writer=new StreamWriter(Path.Combine(args[1],host.DefaultClassName+".cs")))new Microsoft.CSharp.CSharpCodeProvider().GenerateCodeFromCompileUnit(result.GeneratedCode,writer,new CodeGeneratorOptions());
        }
        Console.WriteLine("Parsed "+index+" Razor views; errors: "+errors);return errors==0?0:1;
    }
}
