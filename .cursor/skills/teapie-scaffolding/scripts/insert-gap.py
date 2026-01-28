#!/usr/bin/env python3
"""
Insert a gap in TeaPie test case numbering.

Renumbers existing test cases to create space for a new test case to be added later.

Usage:
    python insert-gap.py --directory <path> --after <number> [--gap-size <size>]

Example:
    # Create gap after 002 (renumbers 003-* to 004-*)
    python insert-gap.py --directory ./Tests/002-Cars --after 002 --gap-size 1
"""

import argparse
import re
import sys
from pathlib import Path
from typing import Dict, List, Tuple


# File suffixes for test case files
SUFFIXES = ['-req.http', '-init.csx', '-test.csx']


def extract_test_case_info(filename: str) -> Tuple[int, str, str] | None:
    """
    Extract prefix number, base name, and suffix from test case filename.
    
    Args:
        filename: Test case filename (e.g., "001-Add-Car-req.http")
        
    Returns:
        Tuple of (prefix_number, base_name, suffix) or None if not a test case file
    """
    # Pattern: <number>-<name>-<suffix>
    pattern = r'^(\d+)-(.+?)(-req\.http|-init\.csx|-test\.csx)$'
    match = re.match(pattern, filename)
    
    if match:
        prefix_num = int(match.group(1))
        base_name = match.group(2)
        suffix = match.group(3)
        return (prefix_num, base_name, suffix)
    return None


def find_test_cases(directory: Path) -> Dict[int, Dict[str, Path]]:
    """
    Find all test cases in directory and group by prefix number.
    
    Args:
        directory: Directory to search
        
    Returns:
        Dictionary mapping prefix number to dict of suffixes -> file paths
    """
    test_cases: Dict[int, Dict[str, Path]] = {}
    
    for file_path in directory.iterdir():
        if not file_path.is_file():
            continue
            
        info = extract_test_case_info(file_path.name)
        if info:
            prefix_num, base_name, suffix = info
            
            if prefix_num not in test_cases:
                test_cases[prefix_num] = {
                    'base_name': base_name,
                    'files': {}
                }
            
            test_cases[prefix_num]['files'][suffix] = file_path
    
    return test_cases


def format_number(num: int, padding: int = 3) -> str:
    """Format number with zero-padding."""
    return str(num).zfill(padding)


def insert_gap(directory: Path, after: int, gap_size: int = 1) -> None:
    """
    Insert a gap in test case numbering by renumbering files after specified number.
    
    Args:
        directory: Directory containing test cases
        after: Insert gap after this number
        gap_size: Size of gap to create (default: 1)
    """
    if not directory.exists():
        print(f"❌ Error: Directory does not exist: {directory}")
        sys.exit(1)
    
    if not directory.is_dir():
        print(f"❌ Error: Path is not a directory: {directory}")
        sys.exit(1)
    
    if gap_size < 1:
        print(f"❌ Error: Gap size must be at least 1")
        sys.exit(1)
    
    test_cases = find_test_cases(directory)
    
    if not test_cases:
        print(f"ℹ️  No test cases found in {directory}")
        return
    
    # Find files that need to be renumbered (prefix > after)
    files_to_rename: List[Tuple[Path, Path]] = []
    
    for prefix_num in sorted(test_cases.keys()):
        if prefix_num > after:
            test_case = test_cases[prefix_num]
            base_name = test_case['base_name']
            files = test_case['files']
            
            new_prefix = format_number(prefix_num + gap_size)
            
            # Plan renames for all files in this test case
            for suffix, file_path in files.items():
                new_name = f"{new_prefix}-{base_name}{suffix}"
                new_path = directory / new_name
                
                if file_path != new_path:
                    files_to_rename.append((file_path, new_path))
    
    if not files_to_rename:
        print(f"ℹ️  No files need renumbering (no test cases after {format_number(after)})")
        return
    
    # Execute renames (in reverse order to avoid conflicts)
    print(f"📝 Renumbering {len(files_to_rename)} file(s) to create gap after {format_number(after)}...")
    
    # Sort by old path in reverse to rename higher numbers first
    files_to_rename.sort(key=lambda x: x[0].name, reverse=True)
    
    for old_path, new_path in files_to_rename:
        if new_path.exists():
            print(f"⚠️  Warning: Target file already exists: {new_path.name}")
            continue
        
        try:
            old_path.rename(new_path)
            print(f"  ✓ {old_path.name} → {new_path.name}")
        except Exception as e:
            print(f"❌ Error renaming {old_path.name}: {e}")
            sys.exit(1)
    
    print(f"✅ Successfully created gap of {gap_size} after {format_number(after)}")
    print(f"\n💡 You can now create test case(s) with prefix {format_number(after + 1)}")


def main():
    parser = argparse.ArgumentParser(
        description='Insert a gap in TeaPie test case numbering',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Example:
  # Create gap after 002 (renumbers 003-* to 004-*)
  python insert-gap.py --directory ./Tests/002-Cars --after 002 --gap-size 1
        """
    )
    
    parser.add_argument(
        '--directory',
        type=str,
        required=True,
        help='Directory containing test cases'
    )
    
    parser.add_argument(
        '--after',
        type=int,
        required=True,
        help='Insert gap after this number'
    )
    
    parser.add_argument(
        '--gap-size',
        type=int,
        default=1,
        help='Size of gap to create (default: 1)'
    )
    
    args = parser.parse_args()
    
    directory = Path(args.directory).resolve()
    
    insert_gap(
        directory=directory,
        after=args.after,
        gap_size=args.gap_size
    )


if __name__ == '__main__':
    main()
