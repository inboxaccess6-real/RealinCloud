#!/usr/bin/env python3
"""
Seed PostgreSQL from Excel workbook
Reads all sheets or selected sheets and inserts into tables using SQLAlchemy
"""

import argparse
import os
import sys
import yaml
from dotenv import load_dotenv
from sqlalchemy import create_engine, MetaData, Table, insert, text
from sqlalchemy.exc import SQLAlchemyError
import pandas as pd
from sqlalchemy.dialects.postgresql import insert as pg_insert
from datetime import datetime

load_dotenv()


def parse_args():
    parser = argparse.ArgumentParser(description="Seed PostgreSQL DB from Excel workbook")
    parser.add_argument("excel_path", help="Path to the Excel workbook (.xlsx)")
    parser.add_argument("--mapping", help="YAML mapping file that maps sheet names to tables and columns")
    parser.add_argument("--sheet", action="append", help="Only import the given sheet(s). Repeat to include multiple")
    parser.add_argument("--db-url", help="Postgres DATABASE_URL - overrides env var")
    parser.add_argument("--dry-run", action="store_true", help="Print actions but do not insert")
    parser.add_argument("--upsert", action="store_true", help="Use ON CONFLICT upsert when mapping has `upsert_on`")
    return parser.parse_args()


def load_mapping(path):
    if not path:
        return {}
    with open(path, "r") as fh:
        return yaml.safe_load(fh) or {}


def get_engine(db_url):
    if not db_url:
        db_url = os.getenv("DATABASE_URL")
    if not db_url:
        raise RuntimeError("DATABASE_URL not set; pass --db-url or set env var")
    return create_engine(db_url)


def read_workbook(excel_path, sheets=None):
    # Read using pandas; sheet_name=None returns dict of DataFrames
    try:
        if sheets:
            # read only these sheets
            df = pd.read_excel(excel_path, sheet_name=sheets, engine="openpyxl")
            # If sheet list has single name, it returns single DataFrame
            if isinstance(df, pd.DataFrame):
                return {sheets[0]: df}
            return df
        else:
            return pd.read_excel(excel_path, sheet_name=None, engine="openpyxl")
    except Exception as e:
        print("Failed to read excel workbook:", e)
        sys.exit(1)


def find_table(metadata, table_name):
    try:
        return Table(table_name, metadata, autoload_with=metadata.bind)
    except Exception as e:
        return None


def insert_dataframe(conn, table, df, mapping=None, dry_run=False, upsert=False, upsert_keys=None):
    # Map columns if mapping is provided
    if mapping:
        df = df.rename(columns=mapping)

    # Keep only columns that exist on the table
    table_cols = [c.name for c in table.columns]
    df = df[[c for c in df.columns if c in table_cols]]

    # Convert NaN to None
    df = df.where(pd.notnull(df), None)

    rows = df.to_dict(orient="records")
    if not rows:
        print(f"No rows to insert for {table.name}")
        return

    if dry_run:
        print(f"DRY-RUN - would insert into {table.name}: {len(rows)} rows")
        for r in rows[:5]:
            print(r)
        return

    try:
        if upsert and upsert_keys:
            # Use Postgres ON CONFLICT
            stmt = pg_insert(table).values(rows)
            update_dict = {c.name: stmt.excluded[c.name] for c in table.columns if c.name not in upsert_keys}
            stmt = stmt.on_conflict_do_update(index_elements=upsert_keys, set_=update_dict)
            conn.execute(stmt)
        else:
            conn.execute(table.insert(), rows)
        print(f"Inserted {len(rows)} rows into {table.name}")
    except SQLAlchemyError as e:
        print("Insert failed:", e)
        raise


def main():
    args = parse_args()
    mapping = load_mapping(args.mapping) if args.mapping else {}
    engine = get_engine(args.db_url)

    metadata = MetaData(bind=engine)
    workbook = read_workbook(args.excel_path, args.sheet)

    with engine.connect() as conn:
        for sheet_name, df in workbook.items():
            if sheet_name not in df.index and df.empty:
                print(f"Sheet {sheet_name} is empty; skipping")
                continue

            map_cfg = mapping.get(sheet_name)
            if map_cfg is None and mapping and sheet_name not in mapping:
                # Not in mapping, skip if mapping file exists and doesn't reference it
                print(f"Sheet '{sheet_name}' not found in mapping and mapping file provided; skipping")
                continue

            if map_cfg is None:
                # Attempt to auto-map: use sheet name as table name
                table_name = sheet_name.strip().lower().replace(" ", "_")
                map_columns = None
                upsert_on = None
            else:
                if map_cfg is False:
                    print(f"Mapping for '{sheet_name}' set to false/null - skipping")
                    continue
                table_name = map_cfg.get("table")
                map_columns = map_cfg.get("columns")
                upsert_on = map_cfg.get("upsert_on")

            if not table_name:
                print(f"No table assigned for sheet '{sheet_name}'; skipping")
                continue

            table = find_table(metadata, table_name)
            if not table:
                print(f"Table '{table_name}' not found in DB; skipping sheet '{sheet_name}'")
                continue

            # Convert DateTime columns to proper timezone naive datetimes if needed
            # For now simply run insertion
            print(f"Loading sheet '{sheet_name}' -> table '{table_name}' ({len(df)} rows)")
            insert_dataframe(conn, table, df, mapping=map_columns, dry_run=args.dry_run, upsert=args.upsert, upsert_keys=upsert_on)

    print("Done")


if __name__ == "__main__":
    main()
