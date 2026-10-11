using API.Interfaces;

namespace API.Problems.NPComplete.NPC_CSP.Solvers;

class CspSolver : ISolver<CSP> {
    public string solverName { get; } = "Core Shuffling Placeholder";
    public string solverDefinition { get; } = "Unimplemented; solving is a later assignment.";
    public string source { get; } = "No solver implemented.";
    public string sourceFile { get; } = SourceFile.Path();
    public string[] contributors { get; } = Array.Empty<string>();
    public bool timerHasExpired { get; set; }
    public SolverType solverType { get; } = SolverType.Unclassified;

    public string solve(CSP problem) =>
        throw new NotImplementedException("Core Shuffling solving is outside Assignment: Problem Implementation.");
}
