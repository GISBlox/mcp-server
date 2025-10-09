// ----------------------------------------------------
// Copyright(c) Bartels Online. All rights reserved.
// ----------------------------------------------------

namespace GISBlox.MCP.Server.Models
{
    public struct StatusResponse
    {
        public string Status { get; set; }
        
        public string Timestamp { get; set; }
        
        public string Environment { get; set; }
        
        public MCPInfo MCP { get; set; }
    }
}
