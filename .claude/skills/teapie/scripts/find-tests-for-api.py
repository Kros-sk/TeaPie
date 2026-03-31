#!/usr/bin/env python3
"""
Find TeaPie tests that cover a specific API endpoint.

Searches through test collections to find HTTP files that match a given endpoint
pattern, optionally filtering by HTTP method.

Usage:
    python find-tests-for-api.py --endpoint <path> --collection <dir> [--method <method>] [--env-file <env.json>]

Examples:
    # Find tests for endpoint
    python find-tests-for-api.py --endpoint "/cars" --collection ./Tests

    # Find tests with specific method
    python find-tests-for-api.py --method POST --endpoint "/customers" --collection ./Tests

    # With variable resolution
    python find-tests-for-api.py --endpoint "/cars" --collection ./Tests --env-file ./.teapie/env.json

    # JSON output
    python find-tests-for-api.py --endpoint "/cars" --collection ./Tests --format json
"""

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# Import parse_http_file from parse-http-file.py
# We'll duplicate the necessary functions to avoid import issues
HTTP_METHODS = ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'HEAD', 'OPTIONS', 'TRACE']


class HttpRequest:
    """Represents a parsed HTTP request from a TeaPie HTTP file."""
    
    def __init__(self):
        self.name: Optional[str] = None
        self.method: Optional[str] = None
        self.uri: Optional[str] = None
        self.resolved_uri: Optional[str] = None
        self.separator: Optional[str] = None
        self.file_path: Optional[Path] = None
    
    def to_dict(self) -> Dict:
        """Convert to dictionary for JSON output."""
        result = {
            'method': self.method,
            'uri': self.uri
        }
        if self.name:
            result['request_name'] = self.name
        if self.resolved_uri:
            result['resolved_uri'] = self.resolved_uri
        if self.file_path:
            result['file'] = str(self.file_path)
        return result


def load_env_file(env_file: Path, environment: str = '$shared') -> Dict[str, str]:
    """Load variables from env.json file."""
    if not env_file.exists():
        return {}
    
    try:
        with open(env_file, 'r', encoding='utf-8') as f:
            env_data = json.load(f)
        
        variables = {}
        if '$shared' in env_data:
            variables.update(env_data['$shared'])
        if environment in env_data and environment != '$shared':
            variables.update(env_data[environment])
        
        return variables
    except Exception as e:
        print(f"Warning: Could not load env file {env_file}: {e}", file=sys.stderr)
        return {}


def resolve_variables(uri: str, variables: Dict[str, str]) -> str:
    """Resolve variables in URI template."""
    pattern = r'\{\{([^}]+)\}\}'
    
    def replace_var(match):
        var_name = match.group(1)
        if var_name in variables:
            return variables[var_name]
        return match.group(0)
    
    return re.sub(pattern, replace_var, uri)


def parse_http_file(http_file: Path, env_variables: Optional[Dict[str, str]] = None) -> List[HttpRequest]:
    """Parse HTTP file and extract requests."""
    if not http_file.exists():
        return []
    
    requests = []
    current_request = None
    
    with open(http_file, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    for line in lines:
        stripped = line.strip()
        
        if not stripped:
            continue
        
        if stripped.startswith('###'):
            if current_request and current_request.method:
                requests.append(current_request)
            current_request = HttpRequest()
            current_request.file_path = http_file
            current_request.separator = stripped[3:].strip()
            continue
        
        if stripped.startswith('# @name'):
            if current_request is None:
                current_request = HttpRequest()
                current_request.file_path = http_file
            name_match = re.match(r'#\s*@name\s+(.+)', stripped)
            if name_match:
                current_request.name = name_match.group(1).strip()
            continue
        
        if stripped.startswith('//') or (stripped.startswith('#') and not stripped.startswith('##')):
            continue
        
        if stripped.startswith('##'):
            continue
        
        if current_request is None:
            current_request = HttpRequest()
            current_request.file_path = http_file
        
        method_pattern = r'^(' + '|'.join(HTTP_METHODS) + r')\s+(.+)$'
        match = re.match(method_pattern, stripped, re.IGNORECASE)
        
        if match:
            current_request.method = match.group(1).upper()
            current_request.uri = match.group(2).strip()
            
            if env_variables and current_request.uri:
                current_request.resolved_uri = resolve_variables(current_request.uri, env_variables)
    
    if current_request and current_request.method:
        requests.append(current_request)
    
    return requests


def normalize_endpoint(uri: str) -> str:
    """
    Normalize endpoint by replacing ID values with {id} pattern.
    
    Examples:
        "/cars/123" -> "/cars/{id}"
        "/cars/abc" -> "/cars/{id}"
        "/customers/456/details" -> "/customers/{id}/details"
    """
    # Remove base URL if present (http://, https://)
    normalized = re.sub(r'^https?://[^/]+', '', uri)
    
    # Remove query parameters
    normalized = normalized.split('?')[0]
    
    # Replace numeric or alphanumeric segments that look like IDs with {id}
    # Pattern: segments that are likely IDs (numbers, UUIDs, etc.)
    normalized = re.sub(r'/\d+', '/{id}', normalized)
    normalized = re.sub(r'/[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}', '/{id}', normalized, flags=re.IGNORECASE)
    
    return normalized


def extract_path_from_uri(uri: str) -> str:
    """
    Extract path from URI, removing base URL and variables.
    
    Examples:
        "http://api.example.com/cars" -> "/cars"
        "{{ApiBaseUrl}}{{ApiCarsSection}}" -> "/cars" (if resolved)
        "/cars/123" -> "/cars/123"
    """
    # If URI contains variables, return as-is (will be resolved later)
    if '{{' in uri:
        return uri
    
    # Remove protocol and domain
    path = re.sub(r'^https?://[^/]+', '', uri)
    
    # Remove query parameters
    path = path.split('?')[0]
    
    return path


def matches_endpoint(request: HttpRequest, target_endpoint: str, target_method: Optional[str] = None) -> bool:
    """
    Check if request matches target endpoint.
    
    Args:
        request: Parsed HTTP request
        target_endpoint: Target endpoint path (e.g., "/cars")
        target_method: Optional HTTP method filter
        
    Returns:
        True if request matches endpoint
    """
    # Check method if specified
    if target_method and request.method != target_method.upper():
        return False
    
    # Use resolved URI if available, otherwise use original
    uri_to_check = request.resolved_uri if request.resolved_uri else request.uri
    
    if not uri_to_check:
        return False
    
    # Extract path from URI
    path = extract_path_from_uri(uri_to_check)
    
    # Normalize both paths for comparison
    normalized_path = normalize_endpoint(path)
    normalized_target = normalize_endpoint(target_endpoint)
    
    # Exact match
    if normalized_path == normalized_target:
        return True
    
    # Partial match: target "/cars" matches "/cars/{id}"
    if normalized_target.endswith('/'):
        normalized_target = normalized_target[:-1]
    
    if normalized_path.startswith(normalized_target + '/'):
        return True
    
    # Also check if target is a prefix (e.g., "/cars" matches "/cars/bulk")
    if normalized_path.startswith(normalized_target):
        return True
    
    return False


def find_tests_in_collection(
    collection_dir: Path,
    target_endpoint: str,
    target_method: Optional[str] = None,
    env_variables: Optional[Dict[str, str]] = None
) -> List[HttpRequest]:
    """
    Find all tests in collection that match target endpoint.
    
    Args:
        collection_dir: Directory containing test files
        target_endpoint: Target endpoint to find (e.g., "/cars")
        target_method: Optional HTTP method filter
        env_variables: Optional variables for resolution
        
    Returns:
        List of matching HttpRequest objects
    """
    matches = []
    
    # Recursively find all .http files
    http_files = list(collection_dir.rglob('*.http'))
    
    for http_file in http_files:
        requests = parse_http_file(http_file, env_variables)
        
        for request in requests:
            if matches_endpoint(request, target_endpoint, target_method):
                matches.append(request)
    
    return matches


def print_text_output(matches: List[HttpRequest], endpoint: str, method: Optional[str] = None):
    """Print matches in human-readable text format."""
    method_str = f"{method} " if method else ""
    print(f"Found {len(matches)} test(s) for endpoint: {method_str}{endpoint}")
    print()
    
    for i, req in enumerate(matches, 1):
        print(f"{i}. {req.file_path}")
        if req.name:
            print(f"   Request: {req.name}")
        print(f"   Method: {req.method}")
        print(f"   URI: {req.uri}")
        if req.resolved_uri and req.resolved_uri != req.uri:
            print(f"   Resolved: {req.resolved_uri}")
        print()


def print_json_output(matches: List[HttpRequest], endpoint: str, method: Optional[str] = None):
    """Print matches in JSON format."""
    output = {
        'endpoint': endpoint,
        'matches': [req.to_dict() for req in matches]
    }
    if method:
        output['method'] = method
    
    print(json.dumps(output, indent=2))


def main():
    parser = argparse.ArgumentParser(
        description='Find TeaPie tests that cover a specific API endpoint',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Find tests for endpoint
  python find-tests-for-api.py --endpoint "/cars" --collection ./Tests

  # Find tests with specific method
  python find-tests-for-api.py --method POST --endpoint "/customers" --collection ./Tests

  # With variable resolution
  python find-tests-for-api.py --endpoint "/cars" --collection ./Tests --env-file ./.teapie/env.json

  # JSON output
  python find-tests-for-api.py --endpoint "/cars" --collection ./Tests --format json
        """
    )
    
    parser.add_argument(
        '--endpoint',
        type=str,
        required=True,
        help='API endpoint to find (e.g., "/cars")'
    )
    
    parser.add_argument(
        '--collection',
        type=str,
        required=True,
        help='Directory containing test files'
    )
    
    parser.add_argument(
        '--method',
        type=str,
        choices=HTTP_METHODS,
        help='Filter by HTTP method'
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
    
    collection_dir = Path(args.collection).resolve()
    
    if not collection_dir.exists():
        print(f"Error: Collection directory does not exist: {collection_dir}", file=sys.stderr)
        sys.exit(1)
    
    if not collection_dir.is_dir():
        print(f"Error: Path is not a directory: {collection_dir}", file=sys.stderr)
        sys.exit(1)
    
    env_variables = None
    if args.env_file:
        env_file = Path(args.env_file).resolve()
        env_variables = load_env_file(env_file, args.env)
    
    matches = find_tests_in_collection(
        collection_dir,
        args.endpoint,
        args.method,
        env_variables
    )
    
    if args.format == 'json':
        print_json_output(matches, args.endpoint, args.method)
    else:
        print_text_output(matches, args.endpoint, args.method)
    
    # Exit with non-zero if no matches found
    if not matches:
        sys.exit(1)


if __name__ == '__main__':
    main()
