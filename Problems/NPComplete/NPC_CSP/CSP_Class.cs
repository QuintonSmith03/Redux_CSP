using System.Globalization;
using System.Numerics;
using System.Text;
using API.DummyClasses;
using API.Interfaces;
using API.Problems.NPComplete.NPC_CSP.Solvers;
using API.Problems.NPComplete.NPC_CSP.Verifiers;
using SPADE;

namespace API.Problems.NPComplete.NPC_CSP;

class CSP : IProblem<CspSolver, CspVerifier, DummyVisualization>
{
    public const string InstanceGrammar = "{(C0,C1,K) | C0 is list, C1 is list, K is int}";

    public const string DefaultInstance = "(((A,B),(C,D)),((B,E),(C,D)),20)";

    public string problemName { get; } = "Core Shuffling";

    public string formalDefinition { get; } =
        "CSP = {<C0,C1,k> | there exists a legal shuffle "
        + "transforming C0 into C1 with crane travel distance <= k}";

    public string problemDefinition { get; } =
        "Determine whether a crane can transform an initial "
        + "fuel assembly layout into a specified final layout "
        + "without exceeding a travel distance limit.";

    public string inputDescription { get; } =
        "C0, the initial layout; C1, the final layout; and K, a nonnegative integer distance limit.";

    public string outputDescription { get; } =
        "True or False, whether a legal shuffle exists within the distance limit.";

    public string source { get; } = "Antonio Tahhan, Proof Paper #2, supplied course draft.";

    public string sourceFile { get; } = SourceFile.Path();

    public string wikiName { get; } = "";

    public string[] contributors { get; } = { "Quinton Smith", "Paul Carpinter" };

    // The supplied CSP-to-TSP reduction does not establish NP-hardness.

    public ComplexityClass complexityClass { get; } = ComplexityClass.Unclassified; //TODO decide if NPComplete or NPhard
    public ProblemType problemType { get; } = ProblemType.SequencingAndScheduling;
    public string defaultInstance { get; } = DefaultInstance;
    public string instanceFormat { get; } =
        "Format: (C0,C1,K), exactly three fields. Each layout is a nonempty parenthesized "
        + "list of nonempty parenthesized rows, with equal row lengths and matching layout dimensions. "
        + "Assembly IDs match [A-Za-z][A-Za-z0-9_]*, are case-sensitive, and are unique within each layout. "
        + "Use _ for outside-core positions, which must match between layouts; assembly sets may differ. "
        + "K is a nonnegative decimal integer (digits only). Whitespace around delimiters is allowed. "
        + $"Example: {DefaultInstance}";

    // Proposed format for the later verifier assignment;
    public string certificateFormat { get; } =
        "Proposed, unimplemented format: an ordered list of "
        + "(assembly,(sourceRow,sourceColumn),(destinationRow,destinationColumn)) moves. "
        + "Coordinates are one-based; (0,0) denotes a transfer location whose geometry is still to be specified. "
        + "Example: ((A,(1,1),(0,0)),(B,(1,2),(1,1)),(E,(0,0),(1,2))). "
        + "Use () for an empty move list.";

    public string instance { get; }
    public IReadOnlyList<IReadOnlyList<string>> C0 { get; }
    public IReadOnlyList<IReadOnlyList<string>> C1 { get; }
    public BigInteger K { get; }
    public CspSolver defaultSolver { get; } = new();
    public CspVerifier defaultVerifier { get; } = new();
    public DummyVisualization defaultVisualization { get; } = new();

    public CSP()
        : this(DefaultInstance) { }

    public CSP(string instance)
    {
        if (string.IsNullOrWhiteSpace(instance))
            throw new ProblemParseException(
                problemName,
                instance,
                "An instance must not be empty."
            );

        try
        {
            string normalized = NormalizeWhitespace(instance);
            UtilCollection tuple = new(normalized);
            tuple.assertOrdered();
            tuple.assertCount(3);
            // SPADE drops empty entries such as (A,,B). Require a lossless parse
            // so missing cells and trailing commas cannot silently change a layout.
            if (tuple.ToString() != normalized)
                throw new FormatException("Missing list elements or invalid delimiters.");
            if (!tuple[2].IsValue() || !IsDecimalDigits(tuple[2].ToString()))
                throw new FormatException("K must be a nonnegative decimal integer.");

            StringParser parser = new(InstanceGrammar);
            parser.parse(normalized);
            C0 = ReadLayout(parser["C0"]);
            C1 = ReadLayout(parser["C1"]);
            K = BigInteger.Parse(
                parser["K"].ToString(),
                NumberStyles.None,
                CultureInfo.InvariantCulture
            );
            if (C0.Count != C1.Count || C0[0].Count != C1[0].Count)
                throw new FormatException("C0 and C1 must have matching dimensions.");
            for (int row = 0; row < C0.Count; row++)
            {
                for (int column = 0; column < C0[row].Count; column++)
                {
                    if ((C0[row][column] == "_") != (C1[row][column] == "_"))
                        throw new FormatException(
                            "Outside-core positions must match between layouts."
                        );
                }
            }
        }
        catch (Exception ex) when (ex is not ProblemParseException)
        {
            throw new ProblemParseException(problemName, instance, ex.Message, ex);
        }
        this.instance = instance;
    }

    private static IReadOnlyList<IReadOnlyList<string>> ReadLayout(UtilCollection layout)
    {
        layout.assertOrdered();
        List<IReadOnlyList<string>> rows = new();
        HashSet<string> ids = new(StringComparer.Ordinal);
        foreach (UtilCollection row in layout.ToList())
        {
            row.assertOrdered();
            List<string> cells = new();
            foreach (UtilCollection cell in row.ToList())
            {
                string id = cell.ToString();
                if (!cell.IsValue() || !IsAssemblyId(id))
                    throw new FormatException("Each cell must be an assembly ID or _.");
                if (id != "_" && !ids.Add(id))
                    throw new FormatException($"Duplicate assembly ID '{id}' within a layout.");
                cells.Add(id);
            }
            if (cells.Count == 0 || (rows.Count > 0 && cells.Count != rows[0].Count))
                throw new FormatException("Layout rows must be nonempty and rectangular.");
            rows.Add(cells.AsReadOnly());
        }
        if (rows.Count == 0)
            throw new FormatException("A layout must contain at least one row.");
        return rows.AsReadOnly();
    }

    // SPADE's scalar types do not express the assembly ID alphabet or a
    // digits-only, arbitrarily large distance, so validate those domain rules here.
    private static bool IsAssemblyId(string id) =>
        id == "_"
        || (
            id.Length > 0
            && IsAsciiLetter(id[0])
            && id.All(c => IsAsciiLetter(c) || c is >= '0' and <= '9' || c == '_')
        );

    private static bool IsAsciiLetter(char c) => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    private static bool IsDecimalDigits(string text) =>
        text.Length > 0 && text.All(c => c is >= '0' and <= '9');

    // Trim only around delimiters: whitespace inside an ID or number must
    // remain visible to validation. SPADE 0.1.2 needs compact delimiters.
    private static string NormalizeWhitespace(string text)
    {
        StringBuilder normalized = new();
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] is '(' or ')' or '{' or '}' or ',')
            {
                normalized.Append(text[start..i].Trim());
                normalized.Append(text[i]);
                start = i + 1;
            }
        }
        normalized.Append(text[start..].Trim());
        return normalized.ToString();
    }
}
