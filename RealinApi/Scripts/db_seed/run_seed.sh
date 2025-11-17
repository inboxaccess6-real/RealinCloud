#!/usr/bin/env bash
set -euo pipefail

# Usage: ./run_seed.sh /path/to/Reality_mock\ data_With_Builders_Promoters.xlsx
workbook="$1"

env_var_db_url=${DATABASE_URL:-}
if [[ -z "$env_var_db_url" ]]; then
  echo "DATABASE_URL not set. Set it in your env or run 'export DATABASE_URL=postgres://user:pass@host:5432/db'"
fi

python3 seed_from_excel.py "$workbook" --mapping mapping_realin_example.yml --db-url "$DATABASE_URL" --upsert
