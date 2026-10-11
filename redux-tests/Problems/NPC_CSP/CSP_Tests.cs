using System.Numerics;
using API.DummyClasses;
using API.Interfaces;
using API.Problems.NPComplete.NPC_CSP;
using Xunit;

namespace redux_tests;
#pragma warning disable CS1591

public class CSP_Tests {
    [Fact]
    public void DefaultInstance_HasExpectedPositionsAndMetadata() {
        CSP problem = new();
        Assert.Equal(CSP.DefaultInstance, problem.instance);
        Assert.Equal(problem.defaultInstance, new CSP(problem.defaultInstance).instance);
        Assert.Equal(new[] { "A", "B" }, problem.C0[0]);
        Assert.Equal(new[] { "C", "D" }, problem.C0[1]);
        Assert.Equal(new[] { "B", "E" }, problem.C1[0]);
        Assert.Equal(new[] { "C", "D" }, problem.C1[1]);
        Assert.Equal(new BigInteger(20), problem.K);
        IProblem metadata = problem;
        Assert.False(metadata is IGraphProblem);
        Assert.IsType<DummyVisualization>(metadata.defaultVisualization);
        Assert.Equal(ComplexityClass.Unclassified, metadata.complexityClass);
        Assert.Equal(ProblemType.SequencingAndScheduling, metadata.problemType);
        Assert.Empty(metadata.contributors);
        foreach (string value in new[] { metadata.problemName, metadata.formalDefinition,
            metadata.problemDefinition, metadata.inputDescription, metadata.outputDescription,
            metadata.source, metadata.sourceFile, metadata.instanceFormat, metadata.certificateFormat })
            Assert.False(string.IsNullOrWhiteSpace(value));
    }

    [Fact]
    public void RectangularLayout_PreservesEveryPosition() {
        CSP problem = new("(((Z,A,B),(F,C,D)),((B,Z,E),(C,F,D)),20)");
        string[][] initial = { new[] { "Z", "A", "B" }, new[] { "F", "C", "D" } };
        string[][] final = { new[] { "B", "Z", "E" }, new[] { "C", "F", "D" } };
        for (int row = 0; row < 2; row++) {
            for (int column = 0; column < 3; column++) {
                Assert.Equal(initial[row][column], problem.C0[row][column]);
                Assert.Equal(final[row][column], problem.C1[row][column]);
            }
        }
    }

    [Fact]
    public void Whitespace_IsAcceptedAroundTokensAndPreservedInInstance() {
        const string input = " \n( ( ( A_1 , b2 ) , ( C , D ) ), (( b2 , Fresh_3 ), (C,D)), 20 )\t";
        CSP problem = new(input);
        Assert.Equal(input, problem.instance);
        Assert.Equal(new[] { "A_1", "b2" }, problem.C0[0]);
        Assert.Equal(new[] { "b2", "Fresh_3" }, problem.C1[0]);
        Assert.Equal(new BigInteger(20), problem.K);
    }

    [Theory]
    [InlineData("(((A)),((A)),0)", 1, 1, 0)] // Identical singleton.
    [InlineData("(((A,B)),((A,B)),0)", 1, 2, 0)] // Identical row.
    [InlineData("(((A),(B)),((B),(A)),0)", 2, 1, 0)] // Feasibility is not parsing.
    [InlineData("(((Spent)),((Fresh)),1)", 1, 1, 1)]
    [InlineData("(((A,B)),((C,D)),20)", 1, 2, 20)] // Entire assembly set replaced.
    [InlineData("(((A,a)),((a,A)),20)", 1, 2, 20)] // Case-sensitive IDs.
    [InlineData("(((_,A,_),(B,C,_)),((_,D,_),(C,B,_)),20)", 2, 3, 20)]
    [InlineData("(((_)),((_)),0)", 1, 1, 0)] // Nonempty matrix with no active core cells.
    [InlineData("(((A)),((A)),00020)", 1, 1, 20)]
    [InlineData("(((Z9_,a_0)),((a_0,Z9_)),20)", 1, 2, 20)]
    public void ValidLayouts_AreAccepted(string input, int rows, int columns, int bound) {
        CSP problem = new(input);
        Assert.Equal(rows, problem.C0.Count);
        Assert.Equal(rows, problem.C1.Count);
        Assert.All(problem.C0, row => Assert.Equal(columns, row.Count));
        Assert.All(problem.C1, row => Assert.Equal(columns, row.Count));
        Assert.Equal(new BigInteger(bound), problem.K);
    }

    [Fact]
    public void PaperLayout_PreservesOutsideCoreMaskAndFreshAssemblies() {
        CSP problem = new("(((_,A1,_),(B1,_,C2),(C1,_,B2),(_,A2,_)),"
            + "((_,B1,_),(C1,_,D2),(D1,_,C2),(_,B2,_)),20)");
        Assert.Equal(4, problem.C0.Count);
        Assert.Equal(3, problem.C0[0].Count);
        Assert.Equal("A1", problem.C0[0][1]);
        Assert.Equal("D2", problem.C1[1][2]);
        Assert.Equal("D1", problem.C1[2][0]);
        for (int row = 0; row < 4; row++) {
            for (int column = 0; column < 3; column++)
                Assert.Equal(problem.C0[row][column] == "_", problem.C1[row][column] == "_");
        }
    }

    [Fact]
    public void Distance_DoesNotImposeAnInt32Limit() {
        CSP problem = new("(((A)),((A)),999999999999999999999999)");
        Assert.Equal(BigInteger.Parse("999999999999999999999999"), problem.K);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\n")]
    [InlineData("A")]
    [InlineData("()")]
    [InlineData("(((A)),((A)))")]
    [InlineData("(((A)),((A)),0,99)")]
    [InlineData("(((A)),((A)),0)garbage")]
    [InlineData("(((A)),((A)),0))")]
    [InlineData("((((A)),((A)),0)")]
    [InlineData("(((A)),((A)),0")]
    [InlineData("(((A)),,0)")]
    [InlineData("(((A)),((A)),)")]
    [InlineData("((),((A)),0)")]
    [InlineData("((()),((A)),0)")]
    [InlineData("((A,B),((A,B)),0)")]
    [InlineData("(((A,B)),(A,B),0)")]
    [InlineData("((((A,B))),((A,B)),0)")]
    [InlineData("(({A,B}),((A,B)),0)")]
    [InlineData("{((A)),((A)),0}")]
    [InlineData("({(A,B)},((A,B)),0)")]
    [InlineData("(((A,B)),{(A,B)},0)")]
    [InlineData("(((A)),((A)),{0})")]
    [InlineData("(((A,,B)),((A,B)),0)")]
    [InlineData("(((A,B,)),((A,B)),0)")]
    [InlineData("(((,A,B)),((A,B)),0)")]
    [InlineData("(((A,B),),((A,B)),0)")]
    [InlineData("(((A,B),C),((A,B)),0)")]
    [InlineData("(((A,B),(C)),((A,B),(C,D)),0)")]
    [InlineData("(((A,B),(C,D)),((A),(B,C)),0)")]
    [InlineData("(((A,B)),((A),(B)),0)")]
    [InlineData("(((A)),((A,B)),0)")]
    [InlineData("(((A,A)),((A,B)),0)")]
    [InlineData("(((A),(A)),((A),(B)),0)")]
    [InlineData("(((A,B)),((B,B)),0)")]
    [InlineData("(((1A)),((A)),0)")]
    [InlineData("(((_A)),((A)),0)")]
    [InlineData("(((A-B)),((A)),0)")]
    [InlineData("(((A.B)),((A)),0)")]
    [InlineData("(((A B)),((AB)),0)")]
    [InlineData("(((A\nB)),((AB)),0)")]
    [InlineData("(((\"A\")),((A)),0)")]
    [InlineData("(((é)),((A)),0)")]
    [InlineData("(((A)),((?)),0)")]
    [InlineData("(((_,A)),((A,_)),0)")]
    [InlineData("(((A)),((_)),0)")]
    [InlineData("(((_)),((A)),0)")]
    [InlineData("(((A)),((A)),-1)")]
    [InlineData("(((A)),((A)),+1)")]
    [InlineData("(((A)),((A)),1.0)")]
    [InlineData("(((A)),((A)),1e2)")]
    [InlineData("(((A)),((A)),one)")]
    [InlineData("(((A)),((A)),(1))")]
    [InlineData("(((A)),((A)),1 0)")]
    [InlineData("(((A)),((A)),١)")]
    public void MalformedInput_ThrowsProblemParseException(string? input) {
        Assert.Throws<ProblemParseException>(() => new CSP(input!));
    }

    [Fact]
    public void LaterAssignmentHelpers_ExplicitlyThrow() {
        CSP problem = new();
        Assert.Throws<NotImplementedException>(() => problem.defaultSolver.solve(problem));
        Assert.Throws<NotImplementedException>(() => problem.defaultVerifier.verify(problem, "()"));
        Assert.Throws<NotImplementedException>(() => ((ISolver)problem.defaultSolver).solve(problem.instance));
        Assert.Throws<NotImplementedException>(() => ((IVerifier)problem.defaultVerifier).verify(problem.instance, "()"));
        Assert.Equal(SolverType.Unclassified, problem.defaultSolver.solverType);
    }
}
