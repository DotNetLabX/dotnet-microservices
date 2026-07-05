# Changelog - domain-event-wiring

## [Unreleased]
- Sharpened the frontmatter description to unambiguously own the "write / create / add a domain-event handler" auto-invocation trigger, resolving a discovery collision with the shipped generic `nexus-dotnet:create-domain-event-handler` skill.
- Added a lessons-capture pointer (feature `lessons.md` in a pipeline run, else `docs/skill-backlog.md`) and this changelog.

## 2026-07-05 - created
- Initial skill covering reference-model rows eventpub-1, eventpub-2, eventpub-4, eventpub-5, persistence-3, and the corrected eventpub-3: publisher variant (MediatR vs FastEndpoints), dispatch trigger (EF interceptor vs manual endpoint publish), interceptor flavor (post-save vs transactional), and the {Effect}On{Event} handler recipe. Template code lifted from live source; verdict ACCEPT under the evaluate-skill rubric.
