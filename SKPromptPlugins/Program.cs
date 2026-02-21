using Microsoft.SemanticKernel;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
var deploymentName = "gpt-4o-mini";
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;

//create a kernel builder
var builder = Kernel.CreateBuilder();

//configure it with AI service
builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

//build the kernel
var kernel = builder.Build();

// Create plugin folder and plugin prompt inside it
var funPluginDirectoryPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "..", "..", "..", "Plugins", "FunPlugin");

// Load the plugin into the kernel
var funPluginFunctions = kernel.ImportPluginFromPromptDirectory(funPluginDirectoryPath);

// Construct the arguments
var argument = new KernelArguments()
{
    ["input"] = "Bollywood celebs sarcasm"
};
var result = await kernel.InvokeAsync(funPluginFunctions["Joke"], argument);

Console.WriteLine(result);