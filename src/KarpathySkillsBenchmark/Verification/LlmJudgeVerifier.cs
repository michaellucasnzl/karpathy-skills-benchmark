using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KarpathySkillsBenchmark.Runners;

namespace KarpathySkillsBenchmark.Verification;

public sealed class LlmJudgeVerifier : IVerifier
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly string _apiKeyEnvironmentVariable;
    private readonly string _apiBaseUrl;
    private readonly int _repeats;

    public LlmJudgeVerifier(HttpClient httpClient, string model, string apiKeyEnvironmentVariable, string apiBaseUrl, int repeats)
    {
        _httpClient = httpClient;
        _model = model;
        _apiKeyEnvironmentVariable = apiKeyEnvironmentVariable;
        _apiBaseUrl = apiBaseUrl.TrimEnd('/');
        _repeats = Math.Max(1, repeats);
    }

    public string Name => "llm-judge";

    public async Task<VerificationResult> VerifyAsync(RunContext context, AgentRunResult result, CancellationToken cancellationToken)
    {
        var criteria = context.TaskDefinition.SuccessCriteria.Where(x => string.Equals(x.Type, "llmJudge", StringComparison.OrdinalIgnoreCase)).ToList();
        if (criteria.Count == 0)
        {
            return new VerificationResult { Passed = true, Score = 0, Summary = "No llmJudge criteria defined." };
        }

        var apiKey = Environment.GetEnvironmentVariable(_apiKeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new VerificationResult { Passed = false, Score = 0, Summary = $"Missing environment variable '{_apiKeyEnvironmentVariable}'." };
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var steps = new List<VerificationStepResult>();

        foreach (var criterion in criteria)
        {
            var rubricPath = Path.Combine(context.RepoRoot, criterion.Rubric!);
            var rubric = File.Exists(rubricPath) ? await File.ReadAllTextAsync(rubricPath, cancellationToken) : criterion.Rubric!;
            var scores = new List<double>();
            var summaries = new List<string>();

            for (var iteration = 0; iteration < _repeats; iteration++)
            {
                var response = await SendJudgeRequestAsync(context, result, rubric, cancellationToken);
                scores.Add(response.Score);
                summaries.Add(response.Summary);
            }

            var averageScore = scores.Average();
            steps.Add(new VerificationStepResult
            {
                Name = criterion.Rubric!,
                Passed = averageScore >= (criterion.MinScore ?? 0),
                Score = averageScore,
                Summary = string.Join(" ", summaries)
            });
        }

        return new VerificationResult
        {
            Passed = steps.All(step => step.Passed),
            Score = steps.Average(step => step.Score ?? 0),
            Summary = $"Completed {steps.Count} LLM judge evaluation(s).",
            Steps = steps
        };
    }

    private async Task<(double Score, string Summary)> SendJudgeRequestAsync(RunContext context, AgentRunResult result, string rubric, CancellationToken cancellationToken)
    {
        var prompt = $"""
You are grading an agent benchmark result.
Return strict JSON with properties score (1-5 number) and summary (string).

Task prompt:
{context.TaskDefinition.Prompt}

Expected behavior:
{string.Join(Environment.NewLine, context.TaskDefinition.ExpectedBehavior)}

Rubric:
{rubric}

Agent output excerpt:
{TrimForPrompt(result.RawOutput)}
""";

        var request = new
        {
            model = _model,
            messages = new object[]
            {
                new { role = "system", content = "You are a strict benchmark judge." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1
        };

        using var response = await _httpClient.PostAsync(
            _apiBaseUrl + "/chat/completions",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"),
            cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(payload);
        var content = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "{}";

        try
        {
            using var contentJson = JsonDocument.Parse(content);
            var score = contentJson.RootElement.GetProperty("score").GetDouble();
            var summary = contentJson.RootElement.GetProperty("summary").GetString() ?? string.Empty;
            return (score, summary);
        }
        catch (Exception)
        {
            var firstDigit = content.FirstOrDefault(char.IsDigit);
            var score = firstDigit == default ? 0 : double.Parse(firstDigit.ToString());
            return (score, content);
        }
    }

    private static string TrimForPrompt(string input)
        => input.Length <= 4000 ? input : input[..4000];
}
