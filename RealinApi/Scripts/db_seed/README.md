# DB Seed from Excel

This script imports seed data from an Excel (`.xlsx`) workbook into a PostgreSQL database.

Requirements
- Python 3.9+ recommended
- A PostgreSQL database
- A `.env` file containing `DATABASE_URL` or pass the database URL with `--db-url`.

Installation

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
```

Usage

```bash
python seed_from_excel.py path/to/Reality_mock\ data_With_Builders_Promoters.xlsx \
    --mapping mapping.yml \
    --db-url "$DATABASE_URL" \
    --dry-run
```

Command line arguments
- `excel_path` (positional): Path to the Excel workbook
- `--mapping` (optional): YAML mapping file to map sheets to tables and columns
- `--sheet` (optional): Process only the named sheet(s), can be repeated
- `--db-url` (optional): Database URL (overrides `DATABASE_URL` env var)
- `--dry-run`: Do not insert any rows, just print actions
- `--upsert`: Use ON CONFLICT upsert where upsert keys are provided in mapping file

Mapping file
- Map sheet names to table names and column mappings
- If no mapping is provided the script will attempt to match column names to DB columns

Example mapping file included: `mapping_example.yml`.

Notes
- The script uses SQLAlchemy reflection to read target tables and columns
- Upsert uses PostgreSQL `ON CONFLICT` if `upsert_on` is specified per table in mapping file
- The script requires that tables exist (run Fluent migrations or set `AUTO_MIGRATE=true` when starting the app) before seeding
- If you prefer a Swift-based seeder that uses Fluent models, we can add a Swift command that reads a CSV/Excel and uses models to save rows; Python is easier for Excel import.

