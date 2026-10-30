using API.DummyClasses;
using API.Interfaces;
using API.Problems.NPComplete.NPC_CSP.Solvers;
using API.Problems.NPComplete.NPC_CSP.Verifiers;
using SPADE;

namespace API.Problems.NPComplete.NPC_CSP;

class CSP : IProblem<CspSolver, CspVerifier, DummyVisualization>
{
    public string problemName { get; } = "Core Shuffling";
    public string formalDefinition { get; } =
        "CSP = {<C0,C1,k> | a crane can transform C0 into C1 with total travel <= k}";
    public string problemDefinition { get; } =
        "Decide whether a crane can rearrange reactor cores within a distance limit.";
    public string inputDescription { get; } =
        "C0 and C1 contain (cn,x,y) tuples. C0 ends with (P,x,y,{cores}), describing the pool. K is the distance limit.";
    public string outputDescription { get; } = "True if such a shuffle exists, False otherwise";
    public string source { get; } = "Antonio Tahhan, Proof Paper #2";
    public string sourceFile { get; } = SourceFile.Path();
    public string wikiName { get; } = "";
    public string[] contributors { get; } = { "Quinton Smith" };
    public ComplexityClass complexityClass { get; } = ComplexityClass.NPComplete;
    public ProblemType problemType { get; } = ProblemType.SequencingAndScheduling;

    public const string InstanceGrammar = "{(C0,C1,K) | C0 is list, C1 is list, K is int}";

    public const string DefaultInstance =
        "(((A,1,2),(B,2,2),(C,1,1),(D,2,1),(P,0,2,{E})),((B,1,2),(E,2,2),(C,1,1),(D,2,1)),20)";

    public string defaultInstance { get; } = DefaultInstance;
    public string instanceFormat { get; } =
        $"Format: {InstanceGrammar}. Use (cn,x,y) for cores and a final (P,x,y,{{cores}}) in C0. All coordinates share the same coordinate system. Example: {DefaultInstance}";
    public string certificateFormat { get; } =
        "An ordered list of (cn,fromX,fromY,toX,toY) transfers.";

    public string instance { get; }
    public List<(string cn, int x, int y)> C0 { get; }
    public List<(string cn, int x, int y)> C1 { get; }
    public (int x, int y) poolPosition { get; }
    public HashSet<string> poolCores { get; }
    public int K { get; }

    public CspSolver defaultSolver { get; } = new();
    public CspVerifier defaultVerifier { get; } = new();
    public DummyVisualization defaultVisualization { get; } = new();

    public CSP()
        : this(DefaultInstance) { }

    public CSP(string input)
    {
        instance = input;

        StringParser parser = new(InstanceGrammar);
        parser.parse(input);

        var initial = parser["C0"].ToList();
        var pool = initial[^1].ToList();
        initial.RemoveAt(initial.Count - 1);

        C0 = ReadLayout(initial);
        C1 = ReadLayout(parser["C1"].ToList());

        poolPosition = (int.Parse(pool[1].ToString()), int.Parse(pool[2].ToString()));

        poolCores = pool[3].ToList().Select(core => core.ToString()).ToHashSet();

        K = int.Parse(parser["K"].ToString());
    }

    private static List<(string cn, int x, int y)> ReadLayout(IEnumerable<UtilCollection> entries)
    {
        return entries
            .Select(entry =>
            {
                var fields = entry.ToList();

                return (
                    fields[0].ToString(),
                    int.Parse(fields[1].ToString()),
                    int.Parse(fields[2].ToString())
                );
            })
            .ToList();
    }
}
