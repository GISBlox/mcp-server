# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade src\GISBlox.MCP.Server\GISBlox.MCP.Server.csproj
4. Upgrade tests\GISBlox.MCP.Server.Tests\GISBlox.MCP.Server.Tests.csproj
5. Run unit tests to validate upgrade in the projects listed below:
   - tests\GISBlox.MCP.Server.Tests\GISBlox.MCP.Server.Tests.csproj

## Settings

This section contains settings and data used by execution steps.

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                    | Current Version          | New Version | Description                     |
|:--------------------------------|:------------------------:|:-----------:|:--------------------------------|
| Microsoft.Extensions.Hosting    | 10.0.0-rc.2.25502.107    | 10.0.0      | Recommended for .NET 10.0       |
| System.Text.Json                | 10.0.0-rc.2.25502.107    | 10.0.0      | Recommended for .NET 10.0       |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### src\GISBlox.MCP.Server\GISBlox.MCP.Server.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Hosting should be updated from `10.0.0-rc.2.25502.107` to `10.0.0` (*recommended for .NET 10.0*)
  - System.Text.Json should be updated from `10.0.0-rc.2.25502.107` to `10.0.0` (*recommended for .NET 10.0*)

#### tests\GISBlox.MCP.Server.Tests\GISBlox.MCP.Server.Tests.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`
