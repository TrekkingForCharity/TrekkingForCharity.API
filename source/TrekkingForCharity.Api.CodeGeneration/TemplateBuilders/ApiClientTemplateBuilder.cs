using System.IO;
using System.Linq;
using System.Reflection;
using CaseExtensions;
using HandlebarsDotNet;
using TrekkingForCharity.Api.Write.Commands;

namespace TrekkingForCharity.Api.CodeGeneration.TemplateBuilders
{
    public class ApiClientTemplateBuilder : ITemplateBuilder
    {
        private const string TemplateName = "ApiClient.hbs";
        private const string OutName = "source/IApiClient.cs";

        public void Build()
        {
            Directory.CreateDirectory("source");
            
            var source = File.ReadAllText($"Templates/{TemplateName}");
            var template = Handlebars.Compile(source);
            var commands = Assembly.GetAssembly(typeof(CreateTrekCommand)).GetTypes()
                .Where(x => x.Name.EndsWith("Command"));
            var data = new
            {
                Commands = commands.Select(x=> new
                {
                    Name = x.Name,
                    Route = $"api/commands/{x.Name.Replace("Command", "").ToKebabCase()}",
                    MethodName = x.Name.Replace("Command", "")


                })
                
            };

            var result = template(data);
            File.WriteAllText(OutName, result);
        }
    }
}