-- 1. Builders table
CREATE TABLE builders (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL,
  email text,
  phone text,
  established_year int,
  registration_number text,
  headquarters_address text,
  website text,
  active boolean DEFAULT true,
  created_at timestamptz DEFAULT now()
);

-- 2. Projects (optional; for listings that belong to a project)
CREATE TABLE projects (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  name text NOT NULL,
  rera_id text,
  builder_id uuid NOT NULL REFERENCES builders(id) ON DELETE RESTRICT,
  
  -- Embedded address fields
  address text,
  locality text,
  city text,
  pin_code text,
  landmark text,
  latitude double precision,   -- latitude
  longitude double precision,   -- longitude

  construction_status text,  -- ready / under_construction / new_launch
  launch_date date,
  possession_date date,
  status text,            -- active / completed / on_hold
  total_towers int,
  total_units int,
  created_at timestamptz DEFAULT now()
);

-- 3. Core properties table
CREATE TABLE properties (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  title text NOT NULL,
  listing_type text NOT NULL,          -- owner / builder / agent / bank
  listing_category text NOT NULL,      -- sale / rent / pg / flatmates
  property_type text NOT NULL,         -- apartment / villa / plot / etc
  construction_status text NOT NULL,   -- ready / under_construction / new_launch
  rera_id text,
  available_from date,
  status text DEFAULT 'active',        -- active / sold / rented / off_market

  -- linking
  project_id uuid REFERENCES projects(id) ON DELETE SET NULL,
  agent_id uuid REFERENCES agents(id) ON DELETE SET NULL,

  CHECK (
        (project_id IS NOT NULL AND agent_id IS NULL) OR
        (project_id IS NULL AND agent_id IS NOT NULL)
  ),

  -- Embedded address fallback (if not using locations table)
  address text,
  locality text,
  city text,
  pin_code text,
  landmark text,
  latitude double precision,   -- latitude
  longitude double precision,   -- longitude

  -- floor info
  floor_number int,
  total_floors int, 
  facing text NOT NULL,            -- north / south / east / west / northeast / northwest / southeast / southwest

  -- pricing
  price numeric(18,2) NOT NULL,          -- for sale
  currency text DEFAULT 'INR' NOT NULL,
  monthly_rent numeric(18,2),   -- for rent
  security_deposit numeric(18,2),
  maintenance_charges numeric(18,2),
  price_negotiable boolean,

  -- area & layout
  carpet_area numeric(10,2),
  builtup_area numeric(10,2),
  super_builtup_area numeric(10,2),
  bedrooms int,                -- store as '2BHK' or normalized int/enum if you prefer
  bathrooms int,
  balconies int,
  furnishing_status text,       -- unfurnished / semi / fully
  property_age int,

  -- legal and seller info
  ownership_type text,          -- freehold / leasehold
  loan_available boolean,

  -- media counts
  video_url text,
  image_url text,

  -- JSONB columns for flexible features
  amenities jsonb DEFAULT '{}'::jsonb, -- Lift / Car Parking / Power Backup / Security/CCTV / Swimming Pool / Gym / Visitor Parking / Children Play Area
  interior_features jsonb DEFAULT '{}'::jsonb, -- Modular Kitchen / Wardrobes / AC / Geysers
  utilities jsonb DEFAULT '{}'::jsonb, -- Flooring Type / Water Supply / 24x7 Water / 24x7 Electricity / Gas Pipeline / Pet Friendly

  -- flags and status
  is_published boolean DEFAULT false,
  is_featured boolean DEFAULT false,
  created_at timestamptz DEFAULT now(),
  updated_at timestamptz DEFAULT now()
);

-- 4. Photos / media (store keys/urls; don't store binary in DB)
CREATE TABLE media (
  id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
  property_id uuid REFERENCES properties(id) ON DELETE CASCADE,
  storage_bucket text NOT NULL,
  storage_key text NOT NULL,
  url text,   -- pre-signed or CDN URL
  uploaded_at timestamptz DEFAULT now()
);

CREATE TABLE agents (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid UNIQUE NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    license_number text UNIQUE,
    agency_name text,
    experience_years int,
    rating numeric(2,1),
    created_at timestamptz DEFAULT now(),
    updated_at timestamptz DEFAULT now()
);

-- 5. Index queue (for async indexing to OpenSearch)
-- CREATE TABLE property_index_queue (
--   id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
--   property_id uuid NOT NULL,
--   operation text NOT NULL, -- create/update/delete
--   payload jsonb,
--   enqueued_at timestamptz DEFAULT now(),
--   processed boolean DEFAULT false
-- );