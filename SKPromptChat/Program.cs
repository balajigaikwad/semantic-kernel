using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

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

// Create a prompt for chatbot
var prompt = @"
You are a helpful assistant for answering questions about the world. 
You will be given a question and you should answer it to the best of your ability. 
If you don't know the answer, say you don't know.
Always try to be helpful and informative.
.
.
.
{{$history}}
User: {{$userInput}}
ChatBot:
";

// Create a symantic function from the prompt
var chatFunction = kernel.CreateFunctionFromPrompt(prompt, 
    executionSettings: new AzureOpenAIPromptExecutionSettings()
    {
        MaxTokens = 1000,
        Temperature = 0.7,
        TopP = 0.5
    });

// Build the arguments for the function
var arguments = new KernelArguments();

// Get User input
//Console.WriteLine("Hi, I am initlligent Shambhu, ask me anything. Shambhu will answer");
//var readUserInput = Console.ReadLine() ?? string.Empty;
////var userInput = readUserInput.Trim();
//arguments["userInput"] = readUserInput;

//// Start with a basic chat
//var botAnswer = await chatFunction.InvokeAsync(kernel, arguments);
//Console.WriteLine(botAnswer);

// Create reusable chat loop
//while (true)
//{
//    Console.WriteLine("Hi, I am initlligent Shambhu, ask me anything. Shambhu will answer");
//    var readUserInput = Console.ReadLine() ?? string.Empty;
//    if (readUserInput.Trim().ToLower() == "exit")
//    {
//        Console.WriteLine("Goodbye!");
//        break;
//    }
//    arguments["userInput"] = readUserInput;
//    var botAnswer = await chatFunction.InvokeAsync(kernel, arguments);
//    Console.WriteLine(botAnswer);
//}

// Lets create above using Func<string, Task> delegate and store the chat history in the context of the kernel
var history = string.Empty;

Func<string, Task> chat = async (input) =>
{
    arguments["history"] = history;
    arguments["userInput"] = input;

    var botAnswer = await chatFunction.InvokeAsync(kernel, arguments);

    history += $"\nUser: {input}\nChatBot: {botAnswer}\n";

    Console.WriteLine(botAnswer);
};

while (true)
{
    Console.WriteLine("Hi, I am initlligent Shambhu, ask me anything. Shambhu will answer");
    var readUserInput = Console.ReadLine() ?? string.Empty;
    await chat(readUserInput);
}