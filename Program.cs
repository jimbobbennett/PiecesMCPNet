using MCPSharp;
using MCPSharp.Model;
using MCPSharp.Model.Schemas;

using Microsoft.Extensions.AI;
using Pieces.Extensions.AI;
using Pieces.OS.Client;

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
}, async (object question) =>
{
    // Create a connection to PiecesOS
    PiecesClient client = new();

    // Create a chat client using GPT-4o Mini
    var model = await client.GetModelByNameAsync("GPT-4o Mini Chat Model");

    IChatClient chatClient = new PiecesChatClient(client, "Pieces MCP Server", model: model);
    var chatOptions = new ChatOptions
    {
        AdditionalProperties = new AdditionalPropertiesDictionary
    {
        { PiecesChatClient.LongTermMemoryPropertyName, true},
        { PiecesChatClient.LongTermMemoryTimeSpanPropertyName, TimeSpan.FromDays(7)}
    }
    };

    List<ChatMessage> chatMessages = [
        new(ChatRole.User, question?.ToString())
    ];
    var response = await chatClient.GetResponseAsync(chatMessages, options: chatOptions);
    return response.Choices[0].Text;
});

await MCPServer.StartAsync("PiecesLTM", "1.0.0");
