# Task Authoring Guide

Tasks are JSON documents stored under 	asks/ and validated by enchmark validate-tasks.
Each task should define a fixture, prompt, expected behavior, and one or more success criteria.
Use 	est criteria for objective checks and llmJudge criteria when rubric-based grading is appropriate.
Keep prompts surgical and map them to realistic coding tasks.
