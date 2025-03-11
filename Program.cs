using MCPSharp;
using MCPSharp.Model;
using MCPSharp.Model.Schemas;

using Microsoft.Extensions.AI;
using Pieces.Extensions.AI;
using Pieces.OS.Client;

// Define a handler for the tool.
// This takes a question as a string, and sends it to Pieces
// This uses the LTM with a 7 day memory
Func<object, Task<string>> func = static async question =>
{
    // Create a connection to PiecesOS
    PiecesClient client = new();

    // Create a chat client using GPT-4o Mini
    var model = await client.GetModelByNameAsync("GPT-4o Mini Chat Model").ConfigureAwait(false);

    // Define the chat client, and the options to enable long term memory for 7 days
    IChatClient chatClient = new PiecesChatClient(client, "Pieces MCP Server", model: model);
    var chatOptions = new ChatOptions
    {
        AdditionalProperties = new AdditionalPropertiesDictionary
        {
            { PiecesChatClient.LongTermMemoryPropertyName, true},
            { PiecesChatClient.LongTermMemoryTimeSpanPropertyName, TimeSpan.FromDays(7)}
        }
    };

    // Build a list of chat messages. Every conversation is new, so there is no chat history in the context
    List<ChatMessage> chatMessages = [
        new(ChatRole.User, question?.ToString())
    ];

    // Send the chat messages to Pieces, and get the response
    var response = await chatClient.GetResponseAsync(chatMessages, options: chatOptions).ConfigureAwait(false);

    // Return the response
    return response.Choices[0].Text!;
};

// Register the tool with the server
MCPServer.AddToolHandler(new Tool()
{
    Name = "PiecesLTM",
    Description = "Ask Pieces LTM a question",
    InputSchema = new InputSchema
    {
        Type = "object",
        Required = ["question"],
        Properties = new Dictionary<string, ParameterSchema>{
            {"question", new ParameterSchema{Type="string", Description="question to ask Pieces LTM"}}
        }
    }
}, func);

// Start the server
await MCPServer.StartAsync("PiecesLTM", "1.0.0").ConfigureAwait(false);
