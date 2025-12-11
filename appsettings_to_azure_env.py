#!/usr/bin/env python3
import json
import argparse
from typing import Any, Dict, List


def flatten_json(
    data: Any,
    parent_key: str = "",
    sep: str = "__",
) -> Dict[str, str]:
    """
    Recursively flattens a nested JSON object using
    ASP.NET-style keys (Section__SubSection__Key).
    Arrays are indexed: Section__0__Key.
    """
    items: Dict[str, str] = {}

    if isinstance(data, dict):
        for k, v in data.items():
            new_key = f"{parent_key}{sep}{k}" if parent_key else k
            items.update(flatten_json(v, new_key, sep=sep))
    elif isinstance(data, list):
        for idx, v in enumerate(data):
            new_key = f"{parent_key}{sep}{idx}" if parent_key else str(idx)
            items.update(flatten_json(v, new_key, sep=sep))
    else:
        # Primitive value: convert to string
        items[parent_key] = "null" if data is None else str(data)

    return items


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Convert appsettings.json to Azure-style environment variables."
    )
    parser.add_argument(
        "input",
        help="Path to appsettings.json",
    )
    parser.add_argument(
        "--sep",
        default="__",
        help="Separator for nested keys (default: '__'; use ':' if you prefer that style).",
    )
    parser.add_argument(
        "--format",
        choices=["azure-json", "env-lines"],
        default="azure-json",
        help=(
            "Output format: "
            "'azure-json' = JSON array of {name,value} (default), "
            "'env-lines' = KEY=VALUE lines"
        ),
    )

    args = parser.parse_args()

    with open(args.input, "r", encoding="utf-8") as f:
        data = json.load(f)

    flat = flatten_json(data, sep=args.sep)

    if args.format == "azure-json":
        # Azure App Service app settings style
        result: List[Dict[str, str]] = [
            {"name": k, "value": v} for k, v in sorted(flat.items())
        ]
        print(json.dumps(result, indent=2))
    else:
        # Simple KEY=VALUE lines
        for k in sorted(flat.keys()):
            print(f"{k}={flat[k]}")


if __name__ == "__main__":
    main()
