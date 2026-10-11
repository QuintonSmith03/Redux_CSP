using API.Interfaces;

namespace API.Problems.NPComplete.NPC_CSP.Verifiers;

class CspVerifier : IVerifier<CSP> {
    public string verifierName { get; } = "Default Core Shuffling Verifier";
    public string verifierDefinition { get; } = "Unimplemented; certificate verification is a later assignment.";
    public string source { get; } = "No verifier implemented.";
    public string sourceFile { get; } = SourceFile.Path();
    public string[] contributors { get; } = Array.Empty<string>();
    public string certificate { get; } = "";

    public bool verify(CSP problem, string certificate) =>
        throw new NotImplementedException("Core Shuffling verification is outside Assignment: Problem Implementation.");
}
