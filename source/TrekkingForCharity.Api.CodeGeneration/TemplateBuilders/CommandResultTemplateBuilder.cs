// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

using System.IO;
using System.Linq;
using System.Reflection;
using HandlebarsDotNet;
using TrekkingForCharity.Api.Write.CommandResult;

namespace TrekkingForCharity.Api.CodeGeneration.TemplateBuilders
{
    public class CommandResultTemplateBuilder : ITemplateBuilder
    {
        public string Template => @"namespace TrekkingForCharity.Api.Client.Executors.CommandResults
{
    public class {{Name}} : CommandResult
    {
        
{{#each Props}}
        public {{DataType}} {{Name}} { get; set; }
{{/each}}
    }
}";

        public void Build(string workingDir)
        {
            var dir = Path.Combine(workingDir, "command-results");
            Directory.CreateDirectory(dir);

            var template = Handlebars.Compile(this.Template);

            var commands = Assembly.GetAssembly(typeof(CreateTrekCommandResult)).GetTypes()
                .Where(x => x.Name.EndsWith("CommandResult"));

            foreach (var command in commands)
            {
                var data = new
                {
                    command.Name,
                    Props = command.GetProperties().Select(x => new
                    {
                        DataType = x.PropertyType,
                        x.Name
                    })
                };

                var result = template(data);
                File.WriteAllText(Path.Combine(dir, $"{command.Name}.cs"), result);
            }
        }
    }
}