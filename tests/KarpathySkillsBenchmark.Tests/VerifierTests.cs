using System.Net;
using System.Net.Http;
using System.Text;
using KarpathySkillsBenchmark.Configuration;
using KarpathySkillsBenchmark.Runners;
using KarpathySkillsBenchmark.Tasks;
using KarpathySkillsBenchmark.Verification;

namespace KarpathySkillsBenchmark.Tests;

public sealed class VerifierTests
{
    [Fact]
    public async Task TestRunnerVerifier_ExecutesCommand()
    {
        var workspace = TestWorkspace.Create(nameof(TestRunnerVerifier_ExecutesCommand));
        var context = new RunContext
        {
            WorkspacePath = workspace,
            TaskDefinition = new TaskDefinition
            {
                Id = "task",
                Title = "task",
                Difficulty = "easy",
                Fixture = "fixture",
                Prompt = "prompt",
                SuccessCriteria = [new SuccessCriterion { Type = "test", Command = "dotnet --version", ExpectPass = true }]
            },
            AgentProfile = new AgentProfile { Provider = "venice" }
        };

        var verification = await new TestRunnerVerifier().VerifyAsync(context, new AgentRunResult(), CancellationToken.None);
        Assert.True(verification.Passed);
    }

    [Fact]
    public async Task LlmJudgeVerifier_ParsesResponse()
    {
        var root = TestWorkspace.Create(nameof(LlmJudgeVerifier_ParsesResponse));
        var rubricPath = Path.Combine(root, "rubric.md");
        File.WriteAllText(rubricPath, "Judge it");
        Environment.SetEnvironmentVariable("VENICE_API_KEY", "test-key");

        var handler = new FakeHttpMessageHandler("{\"choices\":[{\"message\":{\"content\":\"{\\\"score\\\":5,\\\"summary\\\":\\\"Great\\\"}\"}}]}");
        var verifier = new LlmJudgeVerifier(new HttpClient(handler), "qwen/qwq-32b", "VENICE_API_KEY", "https://api.venice.ai/api/v1", 1);
        var context = new RunContext
        {
            RepoRoot = root,
            WorkspacePath = root,
            TaskDefinition = new TaskDefinition
            {
                Id = "task",
                Title = "task",
                Difficulty = "easy",
                Fixture = "fixture",
                Prompt = "prompt",
                ExpectedBehavior = ["works"],
                SuccessCriteria = [new SuccessCriterion { Type = "llmJudge", Rubric = "rubric.md", MinScore = 4 }]
            },
            AgentProfile = new AgentProfile { Provider = "venice" }
        };

        var result = await verifier.VerifyAsync(context, new AgentRunResult { RawOutput = "done" }, CancellationToken.None);
        Assert.True(result.Passed);
        Assert.Equal(5, result.Score);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;

        public FakeHttpMessageHandler(string response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_response, Encoding.UTF8, "application/json")
            });
    }
}
