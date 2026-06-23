# Adding Agents

Implement IAgentRunner to support a new tool.
A runner receives a RunContext, prepares the workspace if needed, invokes the external agent, and returns an AgentRunResult with output, timing, tool calls, and token usage.
Register the runner in dependency injection and add an agent profile in ppsettings.json.
For example, a Cursor runner could translate the RunContext into the corresponding CLI invocation and parse structured output just like OpenCodeRunner.
