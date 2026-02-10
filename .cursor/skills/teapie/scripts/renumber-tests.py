#!/usr/bin/env python3
"""
Renumber TeaPie test cases in a directory.

Maintains proper zero-padding and renames all related files (-req.http, -init.csx, -test.csx)
and numbered directories as a group to preserve test case integrity.

Usage:
    python renumber-tests.py --directory <path> [--start <number>] [--insert <number> <name>]

Examples:
    # Renumber all test cases in directory starting from 001
    python renumber-tests.py --directory ./Tests/002-Cars

    # Renumber starting from specific number
    python renumber-tests.py --directory ./Tests/002-Cars --start 001

    # Insert test case at specific position (renumbers existing files)
    python renumber-tests.py --directory ./Tests/002-Cars --insert 003 MyNewTest
"""

import argparse
import re
import sys
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# File suffixes for test case files
SUFFIXES = ['-req.http', '-init.csx', '-test.csx']


def extract_test_case_info(filename: str) -> Optional[Tuple[int, str, str]]:
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


def extract_directory_info(dirname: str) -> Optional[Tuple[int, str]]:
    """
    Extract prefix number and base name from numbered directory name.
    
    Args:
        dirname: Directory name (e.g., "001-Seed")
        
    Returns:
        Tuple of (prefix_number, base_name) or None if not a numbered directory
    """
    pattern = r'^(\d+)-(.+)$'
    match = re.match(pattern, dirname)
    
    if match:
        prefix_num = int(match.group(1))
        base_name = match.group(2)
        return (prefix_num, base_name)
    return None


def find_numbered_directories(directory: Path) -> Dict[int, Dict[str, Path]]:
    """
    Find all numbered directories in the given directory.
    
    Args:
        directory: Directory to search
        
    Returns:
        Dictionary mapping prefix number to dict with base_name and path
    """
    numbered_dirs: Dict[int, Dict[str, Path]] = {}
    
    for item_path in directory.iterdir():
        if not item_path.is_dir():
            continue
            
        info = extract_directory_info(item_path.name)
        if info:
            prefix_num, base_name = info
            numbered_dirs[prefix_num] = {
                'base_name': base_name,
                'path': item_path
            }
    
    return numbered_dirs


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


def renumber_test_cases(
    directory: Path,
    start_number: int = 1,
    insert_at: Optional[int] = None,
    insert_name: Optional[str] = None
) -> None:
    """
    Renumber test cases and directories in directory.
    
    Args:
        directory: Directory containing test cases and/or numbered directories
        start_number: Starting number for renumbering (default: 1)
        insert_at: If specified, insert a gap at this position
        insert_name: Name for inserted test case (required if insert_at is set)
    """
    if not directory.exists():
        print(f"❌ Error: Directory does not exist: {directory}")
        sys.exit(1)
    
    if not directory.is_dir():
        print(f"❌ Error: Path is not a directory: {directory}")
        sys.exit(1)
    
    test_cases = find_test_cases(directory)
    numbered_dirs = find_numbered_directories(directory)
    
    if not test_cases and not numbered_dirs:
        print(f"ℹ️  No test cases or numbered directories found in {directory}")
        return
    
    # Sort by prefix number
    sorted_file_numbers = sorted(test_cases.keys())
    sorted_dir_numbers = sorted(numbered_dirs.keys())
    
    # If inserting, validate
    if insert_at is not None:
        if insert_name is None:
            print("❌ Error: --insert requires a test name")
            sys.exit(1)
        
        if insert_at in test_cases or insert_at in numbered_dirs:
            print(f"⚠️  Warning: Item {format_number(insert_at)} already exists. "
                  f"Existing items will be renumbered.")
    
    # Build rename plans for files
    file_rename_plan: List[Tuple[Path, Path]] = []
    current_number = start_number
    
    for old_prefix in sorted_file_numbers:
        test_case = test_cases[old_prefix]
        base_name = test_case['base_name']
        files = test_case['files']
        
        # Skip if this is the insertion point
        if insert_at is not None and current_number == insert_at:
            print(f"ℹ️  Gap created at {format_number(current_number)} for '{insert_name}'")
            current_number += 1
        
        # Determine new prefix
        new_prefix = format_number(current_number)
        
        # Plan renames for all files in this test case
        for suffix, file_path in files.items():
            new_name = f"{new_prefix}-{base_name}{suffix}"
            new_path = directory / new_name
            
            if file_path != new_path:
                file_rename_plan.append((file_path, new_path))
        
        current_number += 1
    
    # Build rename plans for directories
    dir_rename_plan: List[Tuple[Path, Path]] = []
    current_number = start_number
    
    for old_prefix in sorted_dir_numbers:
        dir_info = numbered_dirs[old_prefix]
        base_name = dir_info['base_name']
        dir_path = dir_info['path']
        
        # Skip if this is the insertion point
        if insert_at is not None and current_number == insert_at:
            current_number += 1
        
        # Determine new prefix
        new_prefix = format_number(current_number)
        new_name = f"{new_prefix}-{base_name}"
        new_path = directory / new_name
        
        if dir_path != new_path:
            dir_rename_plan.append((dir_path, new_path))
        
        current_number += 1
    
    # Execute directory renames first (in reverse order to avoid conflicts)
    if dir_rename_plan:
        print(f"📁 Renumbering {len(dir_rename_plan)} director(y/ies)...")
        
        # Sort by old path in reverse to rename higher numbers first
        dir_rename_plan.sort(key=lambda x: x[0].name, reverse=True)
        
        for old_path, new_path in dir_rename_plan:
            if new_path.exists():
                print(f"⚠️  Warning: Target directory already exists: {new_path.name}")
                continue
            
            try:
                old_path.rename(new_path)
                print(f"  ✓ {old_path.name}/ → {new_path.name}/")
            except Exception as e:
                print(f"❌ Error renaming {old_path.name}: {e}")
                sys.exit(1)
    
    # Execute file renames (in reverse order to avoid conflicts)
    if file_rename_plan:
        print(f"📝 Renumbering {len(file_rename_plan)} file(s)...")
        
        # Sort by old path in reverse to rename higher numbers first
        file_rename_plan.sort(key=lambda x: x[0].name, reverse=True)
        
        for old_path, new_path in file_rename_plan:
            if new_path.exists():
                print(f"⚠️  Warning: Target file already exists: {new_path.name}")
                continue
            
            try:
                old_path.rename(new_path)
                print(f"  ✓ {old_path.name} → {new_path.name}")
            except Exception as e:
                print(f"❌ Error renaming {old_path.name}: {e}")
                sys.exit(1)
    
    if file_rename_plan or dir_rename_plan:
        print(f"✅ Successfully renumbered items")
        
        if insert_at is not None:
            print(f"\n💡 Next step: Create item '{insert_name}' with prefix {format_number(insert_at)}")
    else:
        print("ℹ️  No items need renumbering")


def main():
    parser = argparse.ArgumentParser(
        description='Renumber TeaPie test cases and directories',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Renumber all test cases starting from 001
  python renumber-tests.py --directory ./Tests/002-Cars

  # Renumber starting from specific number
  python renumber-tests.py --directory ./Tests/002-Cars --start 001

  # Insert test case at position 003 (renumbers existing files)
  python renumber-tests.py --directory ./Tests/002-Cars --insert 003 MyNewTest
        """
    )
    
    parser.add_argument(
        '--directory',
        type=str,
        required=True,
        help='Directory containing test cases and/or numbered directories to renumber'
    )
    
    parser.add_argument(
        '--start',
        type=int,
        default=1,
        help='Starting number for renumbering (default: 1)'
    )
    
    parser.add_argument(
        '--insert',
        type=int,
        metavar='NUMBER',
        help='Insert test case at this position (requires --insert-name)'
    )
    
    parser.add_argument(
        '--insert-name',
        type=str,
        dest='insert_name',
        help='Name for inserted test case (required with --insert)'
    )
    
    args = parser.parse_args()
    
    directory = Path(args.directory).resolve()
    
    if args.insert is not None and args.insert_name is None:
        parser.error("--insert requires --insert-name")
    
    renumber_test_cases(
        directory=directory,
        start_number=args.start,
        insert_at=args.insert,
        insert_name=args.insert_name
    )


if __name__ == '__main__':
    main()
