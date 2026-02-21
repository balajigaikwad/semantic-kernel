using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
var deploymentName = "gpt-4o-mini";
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

var kernel = builder.Build();

//Create a prompt template with a placeholder for the user's input
var promptTemplate = "What is the capital of {{$country}}?";

// Create a semantic function using the prompt template
var getCapitalFunction = kernel.CreateFunctionFromPrompt(promptTemplate, executionSettings:
    new AzureOpenAIPromptExecutionSettings
    {
        MaxTokens = 50,
        Temperature = 0.2
    }, "GetCapitalCity");

var arguments = new KernelArguments
{
    ["country"] = "India"
};

Console.WriteLine(await kernel.InvokeAsync(getCapitalFunction, arguments));

Console.WriteLine("Successful");