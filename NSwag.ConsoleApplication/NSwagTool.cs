using YamlDotNet.Serialization;

namespace NSwag.ConsoleApplication;

public class NSwagTool
{
    public async Task RunAsync(string filePath, string @namespace, GeneratorSelection generatorSelection)
    {
        var ymlFile = await File.ReadAllTextAsync(filePath);

        var deserializer = new Deserializer();
        var json = new Serializer(SerializationOptions.JsonCompatible)
            .Serialize(deserializer.Deserialize(new StringReader(ymlFile)));

        var document = await OpenApiDocument.FromJsonAsync(json);

        string code;
        switch (generatorSelection)
        {
            case GeneratorSelection.Client:
                var clientGenerator = new NSwagClientGenerator(@namespace, document);
                code = clientGenerator.GenerateFile();
                break;
            case GeneratorSelection.Controller:
                var controllerGenerator = new NSwagControllerGenerator(@namespace, document);
                code = controllerGenerator.GenerateFile();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(generatorSelection), generatorSelection, null);
        }

        await File.WriteAllTextAsync(filePath + ".cs", code);
    }
}