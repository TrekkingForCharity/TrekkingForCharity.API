// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.IO;
using System.Linq;
using System.Reflection;
using CaseExtensions;
using HandlebarsDotNet;
using TrekkingForCharity.Api.Write.Commands;

namespace TrekkingForCharity.Api.CodeGeneration.TemplateBuilders
{
    public class CommandTemplateBuilder : ITemplateBuilder
    {
        public string Template => @"namespace TrekkingForCharity.Api.Client.Executors.Commands
{
    public class {{Name}} : BaseCommand
    {
        public {{Name}} (
{{#each Props}}
            {{DataType}} {{CamelCaseName}}{{#if @last}} {{else}},{{/if}}
{{/each}}) : base(""{{Route}}"") {
{{#each Props}}
            this.{{Name}} = {{CamelCaseName}};
{{/each}}
        }

{{#each Props}}
        public {{DataType}} {{Name}} { get; }

{{/each}}
    }
}";

        public void Build(string workingDir)
        {
            var dir = Path.Combine(workingDir, "commands");
            Directory.CreateDirectory(dir);

            var template = Handlebars.Compile(this.Template);

            var commands = Assembly.GetAssembly(typeof(CreateTrekCommand)).GetTypes()
                .Where(x => x.Name.EndsWith("Command"));

            foreach (var command in commands)
            {
                var data = new
                {
                    command.Name,
                    Route = $"api/commands/{command.Name.Replace("Command", "").ToKebabCase()}",
                    Props = command.GetProperties().Select(x => new
                    {
                        DataType = x.PropertyType,
                        x.Name,
                        CamelCaseName = x.Name.ToCamelCase()
                    })
                };

                var result = template(data);
                File.WriteAllText(Path.Combine(dir, $"{command.Name}.cs"), result);
            }
        }
    }
}