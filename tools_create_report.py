"""Compatibility entry point for the portable report builder."""
import runpy
from pathlib import Path
runpy.run_path(str(Path(__file__).resolve().parent/'Tools/tools_create_report.py'),run_name='__main__')
