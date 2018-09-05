using System.IO;
using System.Linq;
using System.Reflection;
using CaseExtensions;
using HandlebarsDotNet;
using TrekkingForCharity.Api.Write.Commands;

namespace TrekkingForCharity.Api.CodeGeneration.TemplateBuilders
{
    public class CommandsTemplateBuilder : ITemplateBuilder
    {
        private const string TemplateName = "Commands.hbs";

        public void Build()
        {
            Directory.CreateDirectory("source");
            Directory.CreateDirectory("source/commands");

            var source = File.ReadAllText($"Templates/{TemplateName}");
            var template = Handlebars.Compile(source);

            var commands = Assembly.GetAssembly(typeof(CreateTrekCommand)).GetTypes()
                .Where(x => x.Name.EndsWith("Command"));

            foreach (var command in commands)
            {
                var data = new
                {
                    command.Name,
                    Props = command.GetProperties().Select(x => new
                    {
                        DataType = x.PropertyType,
                        x.Name,
                        CamelCaseName = x.Name.ToCamelCase()
                    })
                };

                var result = template(data);
                File.WriteAllText($"source/commands/{command.Name}.cs", result);
            }
        }
    }
}