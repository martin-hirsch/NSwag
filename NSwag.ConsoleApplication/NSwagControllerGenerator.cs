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
                ClassStyle = CSharpClassStyle.Record,
                GenerateNativeRecords = true,
                DateType = "DateOnly",
            },
            GenerateClientInterfaces = true,
            ControllerStyle = CSharpControllerStyle.Abstract,
            UseActionResultType = true,
            ClassName = "NSwag",
            CodeGeneratorSettings = { TypeNameGenerator = new SuffixTypeNameGenerator("Contract") }
        };

        _generator = new(document: document, settings: cSharpControllerGeneratorSettings);
    }

    public string GenerateFile()
    {
        return _generator.GenerateFile();
    }
}