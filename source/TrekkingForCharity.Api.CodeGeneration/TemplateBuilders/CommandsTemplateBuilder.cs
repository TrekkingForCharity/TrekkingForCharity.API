using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CaseExtensions;
using HandlebarsDotNet;
using TrekkingForCharity.Api.Write.Commands;

namespace TrekkingForCharity.Api.CodeGeneration.TemplateBuilders
{
    public class CommandsTemplateBuilder
    {
        private const string TemplateName = "Commands.hbs";
        public void Build()
        {
            var source = File.ReadAllText($"Templates/{TemplateName}");
            var template = Handlebars.Compile(source);

            var commands = Assembly.GetAssembly(typeof(CreateTrekCommand)).GetTypes()
                .Where(x => x.Name.EndsWith("Command"));

            foreach (var command in commands)
            {
                var data = new
                {
                    Name = command.Name,
                    Props = command.GetProperties().Select(x => new
                    {
                        DataType = x.PropertyType,
                        Name = x.Name,
                        CamelCaseName = x.Name.ToCamelCase()

                    })

                };

                var result = template(data);
                File.WriteAllText($"{command.Name}.cs", result);
            }
        }
    }
}
