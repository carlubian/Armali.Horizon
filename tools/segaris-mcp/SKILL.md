---
name: segaris-mcp
description: Connect to the local Horizon/Segaris MCP server over Streamable HTTP for personal productivity and household/business modules. Use when Codex needs to authenticate with X-Horizon-Api-Key, verify who the Segaris user is, list available MCP tools, or query Segaris data such as projects, assets, Capex, Opex, travel, maintenance, inventory, Firebird/people records, Admin/processes, and Clothes/wardrobe records exposed by the server.
---

# Segaris MCP

Use this skill to query the local Segaris MCP server at `http://node1.armali.com:5180/mcp`. Every MCP request must include the `X-Horizon-Api-Key` header. The local helper script persists the API key in this skill folder so future invocations can reuse it.

## Natural language queries

The user could ask questions that do not directly reference a tool by name.
In those cases, follow these hints to determine which tool to call.

**Note**: If the hints don't lead to clear results, try using the `list-tools` command to see the available tools and their descriptions.
If even the list of tools doesn't clarify which one to use, ask the user for clarification.

1) Queries related to "expenses", "contracts", "income", "gastos periódicos" or "gastos puntuales" are normally resolved by the Capex or Opex modules. (Capex relates to standalone expenses like furniture purchases, presents or lottery, whereas Opex relates to periodic expenses or incomes like payroll or subscriptions).
2) Queries about "stock", "items", "vendors" or "orders" are likely related to the Inventory module. This also manages the shopping list. In some cases, the user could say "item" to mean an "Asset module entity", so if you don't find a matching element in the inventory module, check assets as well.
3) Queries mentioning "travel", "trips", "holidays" or "travel expenses" are typically handled by the Travel module.
4) Queries about "assets", "furniture", "computers", "vehicles" or similar are usually managed by the Asset module. It's possible the user refers to "items" as entities in this module.
5) Queries regarding "maintenance", "repairs", "fixes", "tasks" or similar terms are often associated with the Maintenance module. Sometimes maintenance entities are linked to assets (to indicate a maintenance task for a specific asset).
6) Queries about "projects", "axis", "programs", "risks" or "budgets" are commonly related to the Projects module. Projects are organized on a hierarchy of programs -> axes -> projects. A project can have a series of risks and a budget (including a percentage of budget currently spent).
7) Queries mentioning "people", "person", "people events" or "birthdays" are handled by the Firebird module. Note that birthdays are not an event, but part of the metadata linked to a firebird entity (person entity). Events are sub-entities linked to a person, used for reminders or notes about past encounters.
8) Queries about "admin", "administrative processes", "processes", "steps", "paperwork", "procedures", "tramites", deadlines or pending/completed stages are handled by the Admin/Processes module. Use the process list first, then fetch steps or stats by `processId` when the user asks for progress, status, or detail.
9) Queries about "clothes", "garments", "wardrobe", "ropa", "prendas", laundry/wash types, colors, or clothing color combinations are handled by the Clothes module. Use the clothes list first, then fetch color assignments by `garmentId` when the user asks about a specific garment's colors.

## Quick Start

Use the bundled Python client for deterministic MCP calls:

```powershell
python C:\Users\carlu\.codex\skills\segaris-mcp\scripts\segaris_mcp.py whoami
python C:\Users\carlu\.codex\skills\segaris-mcp\scripts\segaris_mcp.py list-tools
python C:\Users\carlu\.codex\skills\segaris-mcp\scripts\segaris_mcp.py call segaris_list_projects
```

For tools that require arguments, pass JSON:

```powershell
python C:\Users\carlu\.codex\skills\segaris-mcp\scripts\segaris_mcp.py call segaris_list_project_budgets --arguments "{`"projectId`": 12}"
```

## Workflow

1. Start with `whoami` when the user asks to connect, verify credentials, or debug authentication.
2. Use `list-tools` when the user asks what Segaris can do, when a module may have changed, or before relying on a tool name from memory.
3. Use `call <tool>` for read operations. Return a concise summary in Spanish unless the user asks for raw JSON.
4. If a call returns `{ "success": false, ... }`, surface the `errorCode` and `errorMessage` directly.
5. Never print or repeat the API key. Say whether a token is configured, valid, missing, or invalid.

## Configuration

The persistent config is `config.json` in this skill folder. To change it:

```powershell
python C:\Users\carlu\.codex\skills\segaris-mcp\scripts\segaris_mcp.py configure --server-url http://node1.armali.com:5180 --api-key <API_KEY>
```

The helper accepts either the base URL (`http://node1.armali.com:5180`) or the MCP endpoint URL (`http://node1.armali.com:5180/mcp`). It always sends the token in `X-Horizon-Api-Key`.

## Tool Reference

Read `references/tools.md` for the current known tool names and argument shapes. Prefer `list-tools` over the reference when the server is running, because the MCP server discovers tools from the application assembly and can change as Segaris evolves.

## Operational Notes

- The current server is stateless Streamable HTTP; each request is independent.
- The health endpoint is `http://node1.armali.com:5180/health`.
- The server does not store credentials; the client is responsible for persisting the API key.
- Tools are currently read-only and route through Horizon Identity and Segaris over the Horizon IO bus.
- Private Segaris records are filtered by the authenticated user.
