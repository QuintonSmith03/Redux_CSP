using API.DummyClasses;
using API.Interfaces;
using API.Problems.NPComplete.NPC_CSP.Solvers;
using API.Problems.NPComplete.NPC_CSP.Verifiers;
using SPADE;

namespace API.Problems.NPComplete.NPC_CSP;

class CSP : IProblem<CspSolver, CspVerifier, DummyVisualization>
{
    // --- Fields ---
    public string problemName { get; } = "Core Shuffling";
    public string formalDefinition { get; } =
        "CSP = {<C0,C1,k> | there is a shuffle that turns core C0 into core C1 where the fuel crane travels a distance <= k}";
    public string problemDefinition { get; } =
        "Decide whether a fuel crane can rearrange a reactor core from one layout to another without traveling more than a given distance.";
    public string inputDescription { get; } =
        "C0, the starting core layout; C1, the final core layout; and k, the maximum distance the crane may travel";
    public string outputDescription { get; } = "True if such a shuffle exists, False otherwise";
    public string source { get; } = "Antonio Tahhan, Proof Paper #2";
    public string sourceFile { get; } = SourceFile.Path();
    public string wikiName { get; } = "";
    public string[] contributors { get; } = { "Quinton Smith" };
    public ComplexityClass complexityClass { get; } = ComplexityClass.NPComplete;

    // Closest category in Garey & Johnson
    public ProblemType problemType { get; } = ProblemType.SequencingAndScheduling;

    public const string InstanceGrammar = "{(C0,C1,K) | C0 is list, C1 is list, K is int}";
    public const string DefaultInstance = "(((A,B),(C,D)),((B,E),(C,D)),20)";
    public string defaultInstance { get; } = DefaultInstance;
    public string instanceFormat { get; } =
        $"Format: {InstanceGrammar} Each layout is a list of rows, and each row is a list of assembly names (use _ for an empty spot). Example: {DefaultInstance}";

    // Proposed format, the verifier doesn't use it yet
    public string certificateFormat { get; } =
        "Format: an ordered list of (assembly,(fromRow,fromCol),(toRow,toCol)) moves, where (0,0) is the transfer location. Example: ((A,(1,1),(0,0)),(B,(1,2),(1,1)),(E,(0,0),(1,2)))";

    public string instance { get; }
    public List<List<string>> C0 { get; }
    public List<List<string>> C1 { get; }
    public int K { get; }

    public CspSolver defaultSolver { get; } = new();
    public CspVerifier defaultVerifier { get; } = new();
    public DummyVisualization defaultVisualization { get; } = new();

    // --- Methods Including Constructors ---
    public CSP()
        : this(DefaultInstance) { }

    public CSP(string input)
    {
        instance = input;

        StringParser csp = new(InstanceGrammar);
        csp.parse(input);
        C0 = ReadLayout(csp["C0"]);
        C1 = ReadLayout(csp["C1"]);
        K = int.Parse(csp["K"].ToString());
    }

    // Turns a parsed list of rows into a grid of assembly names
    private static List<List<string>> ReadLayout(UtilCollection layout)
    {
        return layout
            .ToList()
            .Select(row => row.ToList().Select(cell => cell.ToString()).ToList())
            .ToList();
    }
}
