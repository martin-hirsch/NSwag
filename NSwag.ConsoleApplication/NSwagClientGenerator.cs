using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;

namespace NSwag.ConsoleApplication;

public class NSwagClientGenerator
{
    private readonly CSharpClientGenerator _generator;

    public NSwagClientGenerator(string @namespace, OpenApiDocument document)
    {
        var cSharpClientGeneratorSettings = new CSharpClientGeneratorSettings
        {
            CSharpGeneratorSettings =
            {
                Namespace = @namespace,
                ClassStyle = CSharpClassStyle.Record
            },
            GenerateClientInterfaces = true,
            ClassName = "NSwag"
        };

        _generator = new(document, cSharpClientGeneratorSettings);
    }

    public string GenerateFile()
    {
        return _generator.GenerateFile();
    }
}