using NSwag.ConsoleApplication;

var filePath = args.ElementAtOrDefault(0);
var @namespace = args.ElementAtOrDefault(1);
var generatorSelection = args.ElementAtOrDefault(2) switch
{
    "client" => GeneratorSelection.Client,
    "controller" => GeneratorSelection.Controller,
    _ => throw new ArgumentOutOfRangeException(nameof(args), args, null)
};

ArgumentNullException.ThrowIfNull(filePath, nameof(filePath));
ArgumentNullException.ThrowIfNull(@namespace, nameof(@namespace));


var nSwagTool = new NSwagTool();
await nSwagTool.RunAsync(filePath, @namespace, generatorSelection);