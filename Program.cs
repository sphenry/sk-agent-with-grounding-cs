// See https://aka.ms/new-console-template for more information
using System.ComponentModel;
using System.Diagnostics.Tracing;
using Azure;
using Azure.AI.Projects;
using Azure.Core.Diagnostics;
using Azure.Identity;
using DotNetEnv;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
Env.Load();

var builder = Kernel.CreateBuilder();

// builder.AddOpenAIChatCompletion(
//                 Env.GetString("OPENAI_CHAT_MODEL_ID"),
//                 Env.GetString("OPENAI_API_KEY"));

builder.AddAzureOpenAIChatCompletion(
    deploymentName: Env.GetString("AZURE_OPENAI_DEPLOYMENT"),
    apiKey: Env.GetString("AZURE_OPENAI_API_KEY"),
    endpoint: Env.GetString("AZURE_OPENAI_ENDPOINT")
);

var kernel = builder.Build();
kernel.Plugins.Add(KernelPluginFactory.CreateFromType<GroundingWithBingPlugin>());

string Name = "AgentWithInternet";
string Instructions = "Answer questions about current events";

ChatCompletionAgent agent =
    new()
    {
        Name = Name,
        Instructions = Instructions,
        Kernel = kernel,
        Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
    };

ChatHistory chat = [];
Console.WriteLine("Agent loaded. Start chatting!");

while (true)
{
    Console.Write("User > ");
    string input = Console.ReadLine();

    if (string.IsNullOrEmpty(input))
        continue;
        
    if (input.ToLower() == "exit")
        break;

    ChatMessageContent message = new(AuthorRole.User, input);
    chat.Add(message);
        
    await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
    {
        chat.Add(response);
            Console.WriteLine($"Agent > {response}");

    }
}

sealed class GroundingWithBingPlugin
{
    [KernelFunction, Description("The search term to use for the Grounding with Bing search.")]
    public async Task<string> Search(
        [Description("The search term to use for the Grounding with Bing search.")]
        string searchQuery)
    {
        string returnString = string.Empty;
        var connString = Env.GetString("AZURE_AI_PROJECT_CONNECTION_STRING");
        var agentId = Env.GetString("AZURE_AI_AGENT_ID");
        var projectClient = new AIProjectClient(connString, new DefaultAzureCredential());
        AgentsClient agentsClient = projectClient.GetAgentsClient();
        var agents = await agentsClient.GetAgentsAsync();
        Azure.AI.Projects.Agent definition = await agentsClient.GetAgentAsync(agentId);

        Response<AgentThread> threadResponse = await agentsClient.CreateThreadAsync();
        AgentThread thread = threadResponse.Value;

        Response<ThreadMessage> messageResponse = await agentsClient.CreateMessageAsync(
            thread.Id,
            MessageRole.User,
            searchQuery);
        ThreadMessage message = messageResponse.Value;

        Response<ThreadRun> runResponse = await agentsClient.CreateRunAsync(
            thread.Id,
            agentId,
            additionalInstructions: "");
        ThreadRun run = runResponse.Value;

        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            runResponse = await agentsClient.GetRunAsync(thread.Id, runResponse.Value.Id);
        }
        while (runResponse.Value.Status == RunStatus.Queued
            || runResponse.Value.Status == RunStatus.InProgress);


        Response<PageableList<ThreadMessage>> afterRunMessagesResponse
            = await agentsClient.GetMessagesAsync(thread.Id);
        IReadOnlyList<ThreadMessage> messages = afterRunMessagesResponse.Value.Data;

        // Note: messages iterate from newest to oldest, with the messages[0] being the most recent
        foreach (MessageContent contentItem in messages[0].ContentItems)
        {
            if (contentItem is MessageTextContent textItem)
            {
                Console.WriteLine("DEBUG: response from Grounding with Bing: " + textItem.Text);
                returnString =  textItem.Text;
            }

        }

        await agentsClient.DeleteThreadAsync(thread.Id);

        return returnString;
    }
}