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
                ClassStyle = CSharpClassStyle.Record,
                GenerateNativeRecords = true,
                TypeNameGenerator = new SuffixTypeNameGenerator("Contract"),
                DateType = "DateOnly",
            },
            GenerateClientInterfaces = true,
            UseHttpClientCreationMethod = true,
            ClassName = "NSwagClient",
            InjectHttpClient = false,
        };

        _generator = new(document, cSharpClientGeneratorSettings);
    }

    public string GenerateFile()
    {
        return _generator.GenerateFile();
    }
}