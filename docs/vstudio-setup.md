# Debugging in Visual Studio

This guide explains how to configure Visual Studio for debugging the GISBlox MCP Server.

Refer to the [Hosted MCP Server setup guide](hosted-mcp-guide.md) if you do not want to debug locally.

## Requirements

- Visual Studio 2022 (17.14.9 or later) installed and configured with Copilot
- GISBlox MCP Server built locally

## Debug Instructions

1. Go to the `.mcp.json` file in the `Solution Items` folder.

2. Double-check the file looks similar to the one below:

   ```json
    {
      "$schema": "https://modelcontextprotocol.io/schemas/draft/2025-07-09/server.json",
      "servers": {
        "gisblox-mcp": {
          "type": "stdio",
          "command": "dotnet",
          "args": [
            "run",
            "--project",
            "src/GISBlox.MCP.Server/GISBlox.MCP.Server.csproj"
          ]
        }
      }
    }
    ```

   **Restart Visual Studio** if you had to make changes to the `.mcp.json` file. 

3. Build the Solution. Make sure there are no build errors.
4. Set a breakpoint in the code where you want to start debugging.
5. Start the debugger with the `GISBlox.MCP.Server` debug configuration selected.
6. Create a tool request from your preferred HTTP client (`curl`, `Node.js fetch`, `Python requests`, `Bruno`, `Postman`, etc.) to trigger the breakpoint:

   ```bash
   curl --request POST \
     --url https://localhost:5001/mcp \
     --header 'authorization: Bearer <YOUR_KEY>' \
     --header 'content-type: application/json' \
     --data '{
       "method": "tool/invoke",
       "params": {
           "name": "WijkenByGemeenteNameList",
           "arguments": {
           "gemeente": "Groningen"
           }
       },
       "jsonrpc": "2.0",
       "id": 1
    }'
   ```
7. Replace `<YOUR_KEY>` with your actual API key. Refer to the [README](../README.md#%EF%B8%8F-usage) for more information on obtaining a service key.
8. Run the request. Visual Studio should hit the breakpoint, allowing you to inspect variables, step through code, and analyze the execution flow.
