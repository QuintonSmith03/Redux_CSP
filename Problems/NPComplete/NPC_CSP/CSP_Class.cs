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
        "CSP = {<P,R,k> | there is an order for the crane to carry out the moves in R, visiting the points in P, with total travel <= k}";
    public string problemDefinition { get; } =
        "Decide whether a fuel crane can rearrange a reactor core from one layout to another without traveling more than a given distance.";
    public string inputDescription { get; } =
        "P, a list of (row,col) locations; R, a set of required (pickup,dropoff) moves between them; and k, the maximum distance the crane may travel";
    public string outputDescription { get; } = "True if such a shuffle exists, False otherwise";
    public string source { get; } = "Antonio Tahhan, Proof Paper #2";
    public string sourceFile { get; } = SourceFile.Path();
    public string wikiName { get; } = "";
    public string[] contributors { get; } = { "Quinton Smith" };
    public ComplexityClass complexityClass { get; } = ComplexityClass.NPComplete;

    // Closest category in Garey & Johnson
    public ProblemType problemType { get; } = ProblemType.SequencingAndScheduling;

    public const string InstanceGrammar = "{(P,R,K) | P is list, R is set, K is int}";
    public const string DefaultInstance = "(((1,1),(1,2),(0,0)),{(0,2),(1,0),(2,1)},20)"; //Real BWR cores have over 600 assemblys!!

    public string defaultInstance { get; } = DefaultInstance;
    public string instanceFormat { get; } =
        $"Format: {InstanceGrammar} P is a list of (row,col) points, R is a set of (from,to) point indices, meaning the assembly at point from moves to point to, and K is the distance limit. Example: {DefaultInstance}";

    // Proposed format
    public string certificateFormat { get; } =
        "Format: an ordered list of (pickup,dropoff) point indices, the order the crane performs the moves. Example: ((0,2),(1,0),(2,1))";
    public string instance { get; }
    public List<(int x, int y)> points { get; }
    public List<(int pickup, int dropoff)> moves { get; }
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
        points = ReadPairs(csp["P"]);
        moves = ReadPairs(csp["R"]);
        K = int.Parse(csp["K"].ToString());
    }

    // Turns a parsed list of (a,b) pairs into int tuples
    private static List<(int, int)> ReadPairs(UtilCollection pairs)
    {
        return pairs
            .ToList()
            .Select(pair =>
            {
                List<UtilCollection> c = pair.ToList();
                return (int.Parse(c[0].ToString()), int.Parse(c[1].ToString()));
            })
            .ToList();
    }
}
//dotnet run
//npm run dev
