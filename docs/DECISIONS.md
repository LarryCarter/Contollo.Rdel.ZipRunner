# RDEL Architecture Decisions

## ADR-016 — Continue Session captures Output Window panes

Decision:

The Continue Session should capture Visual Studio Output Window panes instead of requiring manual paste.

Reason:

Manual copying of build, test, and Contollo RDEL output is one of the user's main pain points. The session context feature should reduce that friction.

Consequence:

Output capture is now part of the context provider layer. Future versions can add pane filtering, selected panes, token estimates, and preview/edit before copy.
