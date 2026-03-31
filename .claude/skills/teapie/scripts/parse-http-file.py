#!/usr/bin/env python3
"""
Parse TeaPie HTTP file and extract endpoint information.

Extracts HTTP methods, URIs, named requests, and optionally resolves variables
from env.json files.

Usage:
    python parse-http-file.py <http-file> [--env-file <env.json>] [--format json]

Examples:
    # Basic parsing
    python parse-http-file.py ./Tests/002-Cars/001-Add-Car-req.http

    # With variable resolution
    python parse-http-file.py ./Tests/002-Cars/001-Add-Car-req.http --env-file ./.teapie/env.json

    # JSON output
    python parse-http-file.py ./Tests/002-Cars/001-Add-Car-req.http --format json
"""

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Dict, List, Optional


# HTTP methods supported by TeaPie
HTTP_METHODS = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'HEAD', 'OPTIONS', 'TRACE']


class HttpRequest:
    """Represents a parsed HTTP request from a TeaPie HTTP file."""
    
    def __init__(self):
        self.name: Optional[str] = None
        self.method: Optional[str] = None
        self.uri: Optional[str] = None
        self.resolved_uri: Optional[str] = None
        self.separator: Optional[str] = None
    
    def to_dict(self) -> Dict:
        """Convert to dictionary for JSON output."""
        result = {
            'method': self.method,
            'uri': self.uri
        }
        if self.name:
            result['name'] = self.name
        if self.resolved_uri:
            result['resolved_uri'] = self.resolved_uri
        if self.separator:
            result['separator'] = self.separator
        return result


def load_env_file(env_file: Path, environment: str = '$shared') -> Dict[str, str]:
    """
    Load variables from env.json file.
    
    Args:
        env_file: Path to env.json file
        environment: Environment to load (default: '$shared')
        
    Returns:
        Dictionary of variable names to values
    """
    if not env_file.exists():
        return {}
    
    try:
        with open(env_file, 'r', encoding='utf-8') as f:
            env_data = json.load(f)
        
        variables = {}
        
        # Load $shared variables first
        if '$shared' in env_data:
            variables.update(env_data['$shared'])
        
        # Load environment-specific variables (override $shared)
        if environment in env_data and environment != '$shared':
            variables.update(env_data[environment])
        
        return variables
    except Exception as e:
        print(f"Warning: Could not load env file {env_file}: {e}", file=sys.stderr)
        return {}


def resolve_variables(uri: str, variables: Dict[str, str]) -> str:
    """
    Resolve variables in URI template.
    
    Args:
        uri: URI template with {{VariableName}} placeholders
        variables: Dictionary of variable names to values
        
    Returns:
        Resolved URI with variables replaced
    """
    resolved = uri
    
    # Find all variable placeholders {{VariableName}}
    pattern = r'\{\{([^}]+)\}\}'
    
    def replace_var(match):
        var_name = match.group(1)
        if var_name in variables:
            return variables[var_name]
        return match.group(0)  # Keep original if not found
    
    resolved = re.sub(pattern, replace_var, resolved)
    
    return resolved


def parse_http_file(http_file: Path, env_variables: Optional[Dict[str, str]] = None) -> List[HttpRequest]:
    """
    Parse HTTP file and extract requests.
    
    Args:
        http_file: Path to HTTP file
        env_variables: Optional dictionary of variables for resolution
        
    Returns:
        List of parsed HttpRequest objects
    """
    if not http_file.exists():
        print(f"Error: File does not exist: {http_file}", file=sys.stderr)
        sys.exit(1)
    
    requests = []
    current_request = None
    
    with open(http_file, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    for line_num, line in enumerate(lines, 1):
        stripped = line.strip()
        
        # Skip empty lines
        if not stripped:
            continue
        
        # Check for request separator (###)
        if stripped.startswith('###'):
            # Save previous request if exists
            if current_request and current_request.method:
                requests.append(current_request)
            
            # Start new request
            current_request = HttpRequest()
            current_request.separator = stripped[3:].strip()  # Remove ###
            continue
        
        # Check for named request (# @name RequestName)
        if stripped.startswith('# @name'):
            if current_request is None:
                current_request = HttpRequest()
            name_match = re.match(r'#\s*@name\s+(.+)', stripped)
            if name_match:
                current_request.name = name_match.group(1).strip()
            continue
        
        # Skip comments
        if stripped.startswith('//') or (stripped.startswith('#') and not stripped.startswith('##')):
            continue
        
        # Skip directives (## DIRECTIVE)
        if stripped.startswith('##'):
            continue
        
        # Check for HTTP method + URI line
        if current_request is None:
            current_request = HttpRequest()
        
        # Try to match HTTP method at start of line
        method_pattern = r'^(' + '|'.join(HTTP_METHODS) + r')\s+(.+)$'
        match = re.match(method_pattern, stripped, re.IGNORECASE)
        
        if match:
            current_request.method = match.group(1).upper()
            current_request.uri = match.group(2).strip()
            
            # Resolve variables if provided
            if env_variables and current_request.uri:
                current_request.resolved_uri = resolve_variables(current_request.uri, env_variables)
    
    # Add last request if exists
    if current_request and current_request.method:
        requests.append(current_request)
    
    return requests


def print_text_output(requests: List[HttpRequest], http_file: Path):
    """Print requests in human-readable text format."""
    print(f"Parsed {len(requests)} request(s) from {http_file}")
    print()
    
    for i, req in enumerate(requests, 1):
        if req.separator:
            print(f"Request {i}: {req.separator}")
        elif req.name:
            print(f"Request {i}: {req.name}")
        else:
            print(f"Request {i}:")
        
        if req.name:
            print(f"  Name: {req.name}")
        print(f"  Method: {req.method}")
        print(f"  URI: {req.uri}")
        
        if req.resolved_uri and req.resolved_uri != req.uri:
            print(f"  Resolved URI: {req.resolved_uri}")
        
        print()


def print_json_output(requests: List[HttpRequest], http_file: Path):
    """Print requests in JSON format."""
    output = {
        'file': str(http_file),
        'requests': [req.to_dict() for req in requests]
    }
    print(json.dumps(output, indent=2))


def main():
    parser = argparse.ArgumentParser(
        description='Parse TeaPie HTTP file and extract endpoint information',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Basic parsing
  python parse-http-file.py ./Tests/002-Cars/001-Add-Car-req.http

  # With variable resolution
  python parse-http-file.py ./Tests/002-Cars/001-Add-Car-req.http --env-file ./.teapie/env.json

  # JSON output
  python parse-http-file.py ./Tests/002-Cars/001-Add-Car-req.http --format json
        """
    )
    
    parser.add_argument(
        'http_file',
        type=str,
        help='Path to HTTP file to parse'
    )
    
    parser.add_argument(
        '--env-file',
        type=str,
        help='Path to env.json file for variable resolution'
    )
    
    parser.add_argument(
        '--env',
        type=str,
        default='$shared',
        help='Environment to use from env.json (default: $shared)'
    )
    
    parser.add_argument(
        '--format',
        choices=['text', 'json'],
        default='text',
        help='Output format (default: text)'
    )
    
    args = parser.parse_args()
    
    http_file = Path(args.http_file).resolve()
    env_variables = None
    
    if args.env_file:
        env_file = Path(args.env_file).resolve()
        env_variables = load_env_file(env_file, args.env)
    
    requests = parse_http_file(http_file, env_variables)
    
    if args.format == 'json':
        print_json_output(requests, http_file)
    else:
        print_text_output(requests, http_file)


if __name__ == '__main__':
    main()
