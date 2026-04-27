#!/usr/bin/env python3
"""Small stdlib MCP client for the local Horizon/Segaris server."""

from __future__ import annotations

import argparse
import json
import pathlib
import sys
import urllib.error
import urllib.parse
import urllib.request


SKILL_DIR = pathlib.Path(__file__).resolve().parents[1]
CONFIG_PATH = SKILL_DIR / "config.json"


def load_config() -> dict:
    if not CONFIG_PATH.exists():
        return {}
    return json.loads(CONFIG_PATH.read_text(encoding="utf-8"))


def save_config(config: dict) -> None:
    CONFIG_PATH.write_text(json.dumps(config, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def normalize_mcp_url(server_url: str) -> str:
    url = server_url.rstrip("/")
    if url.endswith("/mcp"):
        return url
    return f"{url}/mcp"


def health_url(server_url: str) -> str:
    url = server_url.rstrip("/")
    if url.endswith("/mcp"):
        url = url[:-4].rstrip("/")
    return f"{url}/health"


def parse_response(body: bytes, content_type: str) -> dict:
    text = body.decode("utf-8-sig")
    if "text/event-stream" in content_type or text.lstrip().startswith(("event:", "data:")):
        data_lines: list[str] = []
        for line in text.splitlines():
            if line.startswith("data:"):
                data_lines.append(line[5:].strip())
            elif line == "" and data_lines:
                break
        if not data_lines:
            raise RuntimeError(f"MCP response did not contain SSE data: {text[:400]}")
        text = "\n".join(data_lines)
    return json.loads(text)


def mcp_request(method: str, params: dict | None = None, *, config: dict | None = None) -> dict:
    config = config or load_config()
    server_url = config.get("server_url", "http://node1.armali.com:5180")
    api_key = config.get("api_key")
    if not api_key:
        raise SystemExit("No API key configured. Run configure --api-key <API_KEY> first.")

    payload = {
        "jsonrpc": "2.0",
        "id": 1,
        "method": method,
        "params": params or {},
    }
    request = urllib.request.Request(
        normalize_mcp_url(server_url),
        data=json.dumps(payload).encode("utf-8"),
        method="POST",
        headers={
            "Accept": "application/json, text/event-stream",
            "Content-Type": "application/json",
            "X-Horizon-Api-Key": api_key,
            "User-Agent": "segaris-mcp-skill/1.0",
        },
    )
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            result = parse_response(response.read(), response.headers.get("Content-Type", ""))
    except urllib.error.HTTPError as exc:
        detail = exc.read().decode("utf-8", errors="replace")
        raise SystemExit(f"HTTP {exc.code}: {detail}") from exc
    except urllib.error.URLError as exc:
        raise SystemExit(f"Connection failed: {exc.reason}") from exc

    if "error" in result:
        raise SystemExit(json.dumps(result["error"], indent=2, ensure_ascii=False))
    return result.get("result", result)


def print_json(value: object) -> None:
    print(json.dumps(value, indent=2, ensure_ascii=False))


def command_configure(args: argparse.Namespace) -> None:
    config = load_config()
    if args.server_url:
        config["server_url"] = args.server_url.rstrip("/")
    if args.api_key:
        config["api_key"] = args.api_key
    if "server_url" not in config:
        config["server_url"] = "http://node1.armali.com:5180"
    save_config(config)
    print("Configuration saved. API key is persisted and will not be displayed.")


def command_config(args: argparse.Namespace) -> None:
    config = load_config()
    api_key = config.get("api_key", "")
    masked = f"{api_key[:4]}...{api_key[-4:]}" if len(api_key) >= 12 else ("configured" if api_key else "missing")
    print_json(
        {
            "server_url": config.get("server_url", "http://node1.armali.com:5180"),
            "mcp_url": normalize_mcp_url(config.get("server_url", "http://node1.armali.com:5180")),
            "api_key": masked,
        }
    )


def command_health(args: argparse.Namespace) -> None:
    config = load_config()
    url = health_url(config.get("server_url", "http://node1.armali.com:5180"))
    try:
        with urllib.request.urlopen(url, timeout=10) as response:
            print(response.read().decode("utf-8"))
    except urllib.error.URLError as exc:
        raise SystemExit(f"Health check failed: {exc.reason}") from exc


def command_list_tools(args: argparse.Namespace) -> None:
    print_json(mcp_request("tools/list"))


def command_call(args: argparse.Namespace) -> None:
    try:
        arguments = json.loads(args.arguments) if args.arguments else {}
    except json.JSONDecodeError as exc:
        raise SystemExit(f"Invalid JSON for --arguments: {exc}") from exc
    print_json(mcp_request("tools/call", {"name": args.tool, "arguments": arguments}))


def command_whoami(args: argparse.Namespace) -> None:
    print_json(mcp_request("tools/call", {"name": "who_am_i", "arguments": {}}))


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Call the local Segaris MCP server.")
    sub = parser.add_subparsers(dest="command", required=True)

    configure = sub.add_parser("configure", help="Persist server URL and API key.")
    configure.add_argument("--server-url", default=None)
    configure.add_argument("--api-key", default=None)
    configure.set_defaults(func=command_configure)

    config = sub.add_parser("config", help="Show non-secret configuration.")
    config.set_defaults(func=command_config)

    health = sub.add_parser("health", help="Call /health.")
    health.set_defaults(func=command_health)

    list_tools = sub.add_parser("list-tools", help="List MCP tools.")
    list_tools.set_defaults(func=command_list_tools)

    call = sub.add_parser("call", help="Call one MCP tool.")
    call.add_argument("tool")
    call.add_argument("--arguments", default="{}", help="JSON object with tool arguments.")
    call.set_defaults(func=command_call)

    whoami = sub.add_parser("whoami", help="Verify API key against Identity.")
    whoami.set_defaults(func=command_whoami)

    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    args.func(args)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
