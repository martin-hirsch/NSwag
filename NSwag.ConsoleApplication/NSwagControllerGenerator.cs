using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp.Models;

namespace NSwag.ConsoleApplication;

public class NSwagControllerGenerator
{
    private readonly CSharpControllerGenerator _generator;

    public NSwagControllerGenerator(string @namespace, OpenApiDocument document)
    {
        var cSharpControllerGeneratorSettings = new CSharpControllerGeneratorSettings
        {
            CSharpGeneratorSettings =
            {
                Namespace = @namespace,
                ClassStyle = CSharpClassStyle.Record
            },
            GenerateClientInterfaces = true,
            ControllerStyle = CSharpControllerStyle.Abstract,
            UseActionResultType = true,
            ClassName = "NSwag"
        };

        _generator = new(document, cSharpControllerGeneratorSettings);
    }

    public string GenerateFile()
    {
        return _generator.GenerateFile();
    }
}