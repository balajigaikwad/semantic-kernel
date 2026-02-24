using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
var deploymentName = "gpt-4o-mini";
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;

var builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(
    deploymentName: deploymentName,
    endpoint: endpoint,
    apiKey: apiKey
);

builder.Services.AddSingleton<IPromptRenderFilter, PromptLoggingFilter>();

var kernel = builder.Build();

Console.WriteLine("Smart Shambhu is ready");

// Lets create a prompt first
var prompt = @"
Chatbot can have a conversation with you about any topic.
It can give explicit information or say I dont know if it doesn't have any answers.

Information about me, from previous conversations:
{{$fact}} {{recall $fact}}

chat: 
Bot: {{$history}}
User Input: {{$userInput}}
ChatBot:
";

// create memory
#pragma warning disable
var embeddingService = new AzureOpenAITextEmbeddingGenerationService(
    deploymentName: "text-embedding-3-small",
    endpoint: endpoint,
    apiKey: apiKey
);

var memory = new MemoryBuilder()
    .WithTextEmbeddingGeneration(embeddingService)
    .WithMemoryStore(new VolatileMemoryStore())
    .Build();


//create a semantic function from the prompt
var chatFunction = kernel.CreateFunctionFromPrompt(prompt,
    executionSettings: new AzureOpenAIPromptExecutionSettings()
    {
        MaxTokens = 100,
        Temperature = 0.7,
        TopP = 0.5
    });

// Create kernel arguments
var userInput = "";
var history = "";
var fact = "I am Shambhu.";

const string memoryCollectionName = "chatHistoryCollection";

var arguments = new KernelArguments();
arguments["fact"] = "My name is Shambhu. I am intelligent assistant. I love to learn and have fun.";
arguments["history"] = "";

arguments[TextMemoryPlugin.CollectionParam] = memoryCollectionName;
arguments[TextMemoryPlugin.LimitParam] = 2; // limit the retrieved memories to 2
arguments[TextMemoryPlugin.RelevanceParam] = 0.8; // set relevance threshold to 0.8

// Import the memory to kernel
kernel.ImportPluginFromObject(new TextMemoryPlugin(memory));

//write the chat function
Console.WriteLine("Hi, I am here, what can I help you with");

while (true)
{
    //start a basic chat loop
    var readUserInput = Console.ReadLine();

    Func<string, Task> Chat = async (string input) =>
    {
        // Save the user input to memory
        arguments["userInput"] = input;

        //var renderedPrompt = await chatFunction.RenderAsync(kernel, arguments);

        //Console.WriteLine("\n----- FINAL PROMPT SENT TO AZURE -----");
        //Console.WriteLine(renderedPrompt);
        //Console.WriteLine("----- END PROMPT -----\n");

        // Process the user message abnd get the bot response
        var response = await chatFunction.InvokeAsync(kernel, arguments);

        // Append the new instructions to the chat history
        var result = $"User: {input}\nBot: {response}\n";
        history += result;
        arguments["history"] = history;

        Console.WriteLine(response);
    };

    await Chat(readUserInput ?? string.Empty);
}