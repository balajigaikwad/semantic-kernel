using Microsoft.SemanticKernel;

public sealed class PromptLoggingFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        // 1. Let the kernel finish rendering the template
        await next(context);

        // 2. Access the final string that will be sent to the LLM
        string finalPrompt = context.RenderedPrompt;

        Console.WriteLine("--- DEBUG: RENDERED PROMPT ---");
        Console.WriteLine(finalPrompt);
        Console.WriteLine("------------------------------");
    }
}