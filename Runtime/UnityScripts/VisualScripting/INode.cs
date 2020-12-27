using ScriptingLanguage.VisualScripting.CodeGeneration;
using System.Collections.Generic;

namespace ScriptingLanguage.VisualScripting
{
    public interface INode
    {
        IEnumerable<Endpoint> Endpoints { get; }
        string GenerateCode(Endpoint endpointGuid, NodesDB nodesDB, CodeGenerationContext context);
    }
}