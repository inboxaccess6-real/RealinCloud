-- ============================================
-- Seed SuperAdmin User + Full Permissions
-- Run this AFTER applying the EF migration
-- ============================================
-- SuperAdmin Role ID: 00000000-0000-0000-0000-000000000005
-- Phone: +919701284824
-- Provider: Otp (login via OTP)

-- 1. Insert SuperAdmin user
INSERT INTO realin.users (
    id, email, phone_number, name, provider, role_id,
    is_active, is_blocked, is_blacklisted, is_deleted,
    created_at, updated_at
) VALUES (
    'a0000000-0000-0000-0000-000000000001',
    NULL,
    '+919701284824',
    'Super Admin',
    'Otp',
    '00000000-0000-0000-0000-000000000005',
    true, false, false, false,
    NOW(), NOW()
)
ON CONFLICT (phone_number) DO NOTHING;

-- 2. Insert full permissions for SuperAdmin role on ALL 12 modules
--    Modules: USER_MGMT, PROPERTY_BROWSE, PROPERTY_MGMT, BOOKMARKS,
--             ANALYTICS, SETTINGS, BLACKLIST, AUDIT_LOGS, REPORTS, APPROVALS,
--             AGENT_MGMT, ROLE_MGMT
INSERT INTO realin.role_permissions (
    id, role_id, module_id,
    can_read, can_create, can_update, can_delete, can_manage, can_export,
    created_at, updated_at
) VALUES
    -- USER_MGMT
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000001',
     true, true, true, true, true, true, NOW(), NOW()),
    -- PROPERTY_BROWSE
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000002',
     true, true, true, true, true, true, NOW(), NOW()),
    -- PROPERTY_MGMT
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000003',
     true, true, true, true, true, true, NOW(), NOW()),
    -- BOOKMARKS
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000004',
     true, true, true, true, true, true, NOW(), NOW()),
    -- ANALYTICS
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000005',
     true, true, true, true, true, true, NOW(), NOW()),
    -- SETTINGS
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000006',
     true, true, true, true, true, true, NOW(), NOW()),
    -- BLACKLIST
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000007',
     true, true, true, true, true, true, NOW(), NOW()),
    -- AUDIT_LOGS
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000008',
     true, true, true, true, true, true, NOW(), NOW()),
    -- REPORTS
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-000000000009',
     true, true, true, true, true, true, NOW(), NOW()),
    -- APPROVALS
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-00000000000a',
     true, true, true, true, true, true, NOW(), NOW()),
    -- AGENT_MGMT
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-00000000000b',
     true, true, true, true, true, true, NOW(), NOW()),
    -- ROLE_MGMT
    (gen_random_uuid(), '00000000-0000-0000-0000-000000000005', '10000000-0000-0000-0000-00000000000c',
     true, true, true, true, true, true, NOW(), NOW())
ON CONFLICT DO NOTHING;
