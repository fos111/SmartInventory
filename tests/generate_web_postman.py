#!/usr/bin/env python3
"""Generate Postman collection for the Web Frontend (non-mobile) API endpoints."""

import json
from datetime import datetime

COLLECTION_NAME = "SmartInventory Web API — Full"
ENV_NAME = "SmartInventory Web - Local"

BASE_URL = "http://localhost:5000"
EMAIL = "mohamed.benali@isetma.tn"
PASSWORD = "aqzsed919.."

# ── Helpers ──────────────────────────────────────────────────────────

def req(name, method, url, body=None, auth=None, test=None, desc=""):
    """Create a Postman request object."""
    r = {
        "name": name,
        "request": {
            "method": method,
            "header": [{"key": "Content-Type", "value": "application/json", "type": "text"}],
            "url": {"raw": url, "host": ["{{baseUrl}}"], "path": url.replace("{{baseUrl}}/", "").split("/")},
            "description": desc,
        }
    }
    if body:
        r["request"]["body"] = {
            "mode": "raw",
            "raw": json.dumps(body, indent=2)
        }
    if auth:
        r["request"]["auth"] = auth
    if test:
        r["event"] = [{"listen": "test", "script": {"exec": test, "type": "text/javascript"}}]
    return r


def folder(name, items):
    return {"name": name, "item": items}


def bearer_auth():
    return {"type": "bearer", "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]}


def noauth():
    return {"type": "noauth"}


# ── Requests ─────────────────────────────────────────────────────────

# 00 — Health (no auth)
health_check = req(
    "GET /Health (health check)",
    "GET",
    "{{baseUrl}}/Health",
    auth=noauth(),
    desc="Health check endpoint. No auth required."
)

# 01 — Auth (no auth for login/register)
login_req = req(
    "POST /api/auth/login",
    "POST",
    "{{baseUrl}}/api/auth/login",
    body={"email": "{{email}}", "password": "{{password}}"},
    auth=noauth(),
    test=[
        "const pmResponse = pm.response.json();",
        "if (pmResponse.token) {",
        "    pm.collectionVariables.set('token', pmResponse.token);",
        "}",
        "pm.collectionVariables.set('userId', pmResponse.userId);"
    ],
    desc="Login — sets {{token}} collection variable via test script."
)

register_req = req(
    "POST /api/auth/register",
    "POST",
    "{{baseUrl}}/api/auth/register",
    body={"username": "newuser", "email": "newuser@example.com", "password": "Password123!"},
    auth=noauth(),
    desc="Register a new account."
)

verify_email_req = req(
    "GET /api/auth/verify-email",
    "GET",
    "{{baseUrl}}/api/auth/verify-email?token=example-token",
    auth=noauth(),
    desc="Verify email with token from registration email."
)

resend_verification_req = req(
    "POST /api/auth/resend-verification",
    "POST",
    "{{baseUrl}}/api/auth/resend-verification",
    body={"email": "{{email}}"},
    auth=noauth(),
    desc="Resend verification email."
)

request_reeval_req = req(
    "POST /api/auth/request-re-evaluation",
    "POST",
    "{{baseUrl}}/api/auth/request-re-evaluation",
    body={"userId": "{{userId}}"},
    desc="Request re-evaluation of rejected account."
)

# 02 — Assets (auth required)
assets_list = req(
    "GET /api/assets (list)",
    "GET",
    "{{baseUrl}}/api/assets?page=1&pageSize=10",
    desc="List assets with pagination. Test script extracts first asset ID.",
    test=[
        "const data = pm.response.json();",
        "if (data && data.items && data.items.length > 0) {",
        "    pm.collectionVariables.set('assetId', data.items[0].id);",
        "    pm.collectionVariables.set('assetTag', data.items[0].assetTag);",
        "}"
    ]
)

assets_get = req(
    "GET /api/assets/{id}",
    "GET",
    "{{baseUrl}}/api/assets/{{assetId}}",
    desc="Get asset by ID."
)

assets_get_by_tag = req(
    "GET /api/assets/tag/{assetTag}",
    "GET",
    "{{baseUrl}}/api/assets/tag/{{assetTag}}",
    desc="Get asset by asset tag."
)

assets_create = req(
    "POST /api/assets (create)",
    "POST",
    "{{baseUrl}}/api/assets",
    body={
        "name": "New Test Asset",
        "category": "Electronics",
        "currentRoomCode": "LI1",
        "status": "Active",
        "manufacturer": "TestCorp",
        "model": "T-1000",
        "serialNumber": "SN-001"
    },
    desc="Create a new asset.",
    test=[
        "const data = pm.response.json();",
        "if (data && data.id) {",
        "    pm.collectionVariables.set('assetId', data.id);",
        "    pm.collectionVariables.set('assetTag', data.assetTag);",
        "}"
    ]
)

assets_update = req(
    "PUT /api/assets/{id}",
    "PUT",
    "{{baseUrl}}/api/assets/{{assetId}}",
    body={
        "assetTag": "{{assetTag}}",
        "name": "Updated Asset Name",
        "category": "Electronics",
        "status": "Active",
        "currentRoomCode": "LI1"
    },
    desc="Update an asset."
)

assets_move = req(
    "PUT /api/assets/{id}/move",
    "PUT",
    "{{baseUrl}}/api/assets/{{assetId}}/move",
    body={"newRoomCode": "SALLE-INFO-1"},
    desc="Move asset to a different room."
)

assets_update_status = req(
    "PUT /api/assets/{id}/status",
    "PUT",
    "{{baseUrl}}/api/assets/{{assetId}}/status",
    body={"status": "Maintenance"},
    desc="Update asset status."
)

assets_set_maintenance = req(
    "PUT /api/assets/{id}/maintenance-due",
    "PUT",
    "{{baseUrl}}/api/assets/{{assetId}}/maintenance-due",
    body={"dueDate": "2026-12-31"},
    desc="Set maintenance due date."
)

assets_update_rfid = req(
    "PUT /api/assets/{id}/rfid",
    "PUT",
    "{{baseUrl}}/api/assets/{{assetId}}/rfid",
    body={"rfidTagId": "RFID-001"},
    desc="Update RFID tag on asset."
)

assets_delete = req(
    "DELETE /api/assets/{id}",
    "DELETE",
    "{{baseUrl}}/api/assets/{{assetId}}",
    desc="Delete an asset. Returns 204 No Content."
)

assets_qr = req(
    "GET /api/assets/{id}/qr",
    "GET",
    "{{baseUrl}}/api/assets/{{assetId}}/qr",
    desc="Generate QR code for asset (returns PNG)."
)

assets_reconciliation = req(
    "GET /api/assets/reconciliation",
    "GET",
    "{{baseUrl}}/api/assets/reconciliation",
    desc="Get reconciliation data (room vs detected location discrepancies)."
)

assets_import = req(
    "POST /api/assets/import",
    "POST",
    "{{baseUrl}}/api/assets/import",
    desc="Import assets from CSV file (multipart/form-data).",
    body={
        "mode": "formdata",
        "formdata": [
            {"key": "file", "type": "file", "src": "/path/to/assets.csv"}
        ]
    }
)

# 03 — Locations (auth required)
locations_hierarchy = req(
    "GET /api/locations/hierarchy",
    "GET",
    "{{baseUrl}}/api/locations/hierarchy",
    desc="Get full location hierarchy (sites → zones → buildings → floors → rooms)."
)

locations_room_by_code = req(
    "GET /api/locations/rooms/{code}",
    "GET",
    "{{baseUrl}}/api/locations/rooms/LI1",
    desc="Get room by room code."
)

locations_create_room = req(
    "POST /api/locations/rooms",
    "POST",
    "{{baseUrl}}/api/locations/rooms",
    body={"code": "NEWRM", "name": "New Room", "floorId": "00000000-0000-0000-0000-000000000000"},
    desc="Create a new room."
)

locations_create_building = req(
    "POST /api/locations/buildings",
    "POST",
    "{{baseUrl}}/api/locations/buildings",
    body={"name": "New Building", "zoneId": "00000000-0000-0000-0000-000000000000"},
    desc="Create a new building."
)

locations_create_floor = req(
    "POST /api/locations/floors",
    "POST",
    "{{baseUrl}}/api/locations/floors",
    body={"name": "Floor 1", "number": 1, "buildingId": "00000000-0000-0000-0000-000000000000"},
    desc="Create a new floor."
)

locations_update_room_geom = req(
    "PUT /api/locations/rooms/{id}",
    "PUT",
    "{{baseUrl}}/api/locations/rooms/{{roomId}}",
    body={"x": 100, "y": 200, "width": 50, "height": 60, "color": "#4CAF50", "stroke": "#333"},
    desc="Update room geometry (position, size, colors)."
)

locations_batch_rooms = req(
    "PUT /api/locations/rooms/batch",
    "PUT",
    "{{baseUrl}}/api/locations/rooms/batch",
    body={"updates": [{"roomId": "00000000-0000-0000-0000-000000000000", "x": 10, "y": 20, "width": 50, "height": 60}]},
    desc="Batch update room geometries."
)

locations_delete_room = req(
    "DELETE /api/locations/rooms/{id}",
    "DELETE",
    "{{baseUrl}}/api/locations/rooms/{{roomId}}",
    desc="Delete a room."
)

# 04 — Categories (auth required)
categories_list = req(
    "GET /api/categories",
    "GET",
    "{{baseUrl}}/api/categories",
    desc="Get all categories."
)

categories_groups = req(
    "GET /api/categories/groups",
    "GET",
    "{{baseUrl}}/api/categories/groups",
    desc="Get all category groups."
)

# 05 — Reports (auth required)
reports_summary = req(
    "GET /api/reports/summary",
    "GET",
    "{{baseUrl}}/api/reports/summary?groupBy=category",
    desc="Get inventory summary, grouped by category/status/location."
)

reports_asset_history = req(
    "GET /api/reports/asset/{id}/history",
    "GET",
    "{{baseUrl}}/api/reports/asset/{{assetId}}/history",
    desc="Get history for a specific asset."
)

reports_activity = req(
    "GET /api/reports/activity",
    "GET",
    "{{baseUrl}}/api/reports/activity",
    desc="Get activity log with optional date range."
)

reports_export_summary = req(
    "GET /api/reports/export/summary (CSV)",
    "GET",
    "{{baseUrl}}/api/reports/export/summary?format=csv&groupBy=category",
    desc="Export summary as CSV or PDF."
)

reports_export_history = req(
    "GET /api/reports/export/history/{id} (CSV)",
    "GET",
    "{{baseUrl}}/api/reports/export/history/{{assetId}}?format=csv",
    desc="Export asset history as CSV or PDF."
)

reports_export_activity = req(
    "GET /api/reports/export/activity (CSV)",
    "GET",
    "{{baseUrl}}/api/reports/export/activity?format=csv",
    desc="Export activity log as CSV or PDF."
)

reports_maintenance_forecast = req(
    "GET /api/reports/maintenance-forecast",
    "GET",
    "{{baseUrl}}/api/reports/maintenance-forecast?days=30",
    desc="Get maintenance forecast for next N days."
)

reports_export_maintenance_forecast = req(
    "GET /api/reports/export/maintenance-forecast (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/maintenance-forecast?days=30",
    desc="Export maintenance forecast as PDF."
)

reports_overdue_maintenance = req(
    "GET /api/reports/overdue-maintenance",
    "GET",
    "{{baseUrl}}/api/reports/overdue-maintenance",
    desc="Get overdue maintenance items."
)

reports_export_overdue = req(
    "GET /api/reports/export/overdue-maintenance (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/overdue-maintenance",
    desc="Export overdue maintenance as PDF."
)

reports_critical_issues = req(
    "GET /api/reports/critical-issues",
    "GET",
    "{{baseUrl}}/api/reports/critical-issues",
    desc="Get assets with critical issues."
)

reports_export_critical = req(
    "GET /api/reports/export/critical-issues (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/critical-issues",
    desc="Export critical issues as PDF."
)

reports_status_summary = req(
    "GET /api/reports/status-summary",
    "GET",
    "{{baseUrl}}/api/reports/status-summary",
    desc="Get status summary (count by status)."
)

reports_export_status = req(
    "GET /api/reports/export/status-summary (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/status-summary",
    desc="Export status summary as PDF."
)

reports_zone_inventory = req(
    "GET /api/reports/zone-inventory",
    "GET",
    "{{baseUrl}}/api/reports/zone-inventory",
    desc="Get zone inventory report."
)

reports_export_zone = req(
    "GET /api/reports/export/zone-inventory (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/zone-inventory",
    desc="Export zone inventory as PDF."
)

reports_building_stocktake = req(
    "GET /api/reports/building-stocktake",
    "GET",
    "{{baseUrl}}/api/reports/building-stocktake",
    desc="Get building stocktake report."
)

reports_export_building = req(
    "GET /api/reports/export/building-stocktake (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/building-stocktake",
    desc="Export building stocktake as PDF."
)

reports_room_audit = req(
    "GET /api/reports/room-audit/{roomCode}",
    "GET",
    "{{baseUrl}}/api/reports/room-audit/LI1",
    desc="Get room audit data for a specific room."
)

reports_export_room_audit = req(
    "GET /api/reports/export/room-audit/{roomCode} (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/room-audit/LI1",
    desc="Export room audit as PDF."
)

reports_empty_rooms = req(
    "GET /api/reports/empty-rooms",
    "GET",
    "{{baseUrl}}/api/reports/empty-rooms?threshold=0",
    desc="Get rooms with zero assets (or below threshold)."
)

reports_export_empty = req(
    "GET /api/reports/export/empty-rooms (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/empty-rooms?threshold=0",
    desc="Export empty rooms as PDF."
)

reports_location_discrepancies = req(
    "GET /api/reports/location-discrepancies",
    "GET",
    "{{baseUrl}}/api/reports/location-discrepancies",
    desc="Get location discrepancies (expected vs detected room)."
)

reports_export_discrepancies = req(
    "GET /api/reports/export/location-discrepancies (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/location-discrepancies",
    desc="Export location discrepancies as PDF."
)

reports_category_stocktake = req(
    "GET /api/reports/category-stocktake",
    "GET",
    "{{baseUrl}}/api/reports/category-stocktake",
    desc="Get category stocktake report."
)

reports_export_category_stocktake = req(
    "GET /api/reports/export/category-stocktake (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/category-stocktake",
    desc="Export category stocktake as PDF."
)

reports_department_report = req(
    "GET /api/reports/department-report",
    "GET",
    "{{baseUrl}}/api/reports/department-report",
    desc="Get department (zone) report."
)

reports_export_department = req(
    "GET /api/reports/export/department-report (PDF)",
    "GET",
    "{{baseUrl}}/api/reports/export/department-report",
    desc="Export department report as PDF."
)

# 06 — Notifications (auth required)
notifications_list = req(
    "GET /api/notifications",
    "GET",
    "{{baseUrl}}/api/notifications",
    desc="Get notifications for current user.",
    test=[
        "const data = pm.response.json();",
        "if (data && data.length > 0) {",
        "    pm.collectionVariables.set('notificationId', data[0].id);",
        "}"
    ]
)

notifications_unread = req(
    "GET /api/notifications/unread-count",
    "GET",
    "{{baseUrl}}/api/notifications/unread-count",
    desc="Get unread notification count."
)

notifications_mark_read = req(
    "PUT /api/notifications/{id}/read",
    "PUT",
    "{{baseUrl}}/api/notifications/{{notificationId}}/read",
    desc="Mark a notification as read."
)

# 07 — User Preferences (auth required)
preferences_get = req(
    "GET /api/users/preferences",
    "GET",
    "{{baseUrl}}/api/users/preferences",
    desc="Get current user preferences."
)

preferences_update = req(
    "PUT /api/users/preferences",
    "PUT",
    "{{baseUrl}}/api/users/preferences",
    body={"theme": "dark", "notifications": "true"},
    desc="Update user preferences."
)

preferences_role_defaults = req(
    "GET /api/users/preferences/role-defaults",
    "GET",
    "{{baseUrl}}/api/users/preferences/role-defaults",
    desc="Get role default preferences."
)

# ── Folders ─────────────────────────────────────────────────────────

collection = {
    "info": {
        "name": COLLECTION_NAME,
        "description": "Complete API collection for the SmartInventory Web Frontend. Covers all non-mobile controllers: Auth, Assets, Locations, Categories, Reports, Notifications, User Preferences, and Health.",
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "item": [
        folder("00 — Health (no auth)", [health_check]),
        folder("01 — Auth", [
            login_req,
            register_req,
            verify_email_req,
            resend_verification_req,
            request_reeval_req,
        ]),
        folder("02 — Assets", [
            assets_list,
            assets_get,
            assets_get_by_tag,
            assets_create,
            assets_update,
            assets_move,
            assets_update_status,
            assets_set_maintenance,
            assets_update_rfid,
            assets_delete,
            assets_qr,
            assets_reconciliation,
            assets_import,
        ]),
        folder("03 — Locations", [
            locations_hierarchy,
            locations_room_by_code,
            locations_create_room,
            locations_create_building,
            locations_create_floor,
            locations_update_room_geom,
            locations_batch_rooms,
            locations_delete_room,
        ]),
        folder("04 — Categories", [
            categories_list,
            categories_groups,
        ]),
        folder("05 — Reports", [
            reports_summary,
            reports_asset_history,
            reports_activity,
            reports_export_summary,
            reports_export_history,
            reports_export_activity,
            reports_maintenance_forecast,
            reports_export_maintenance_forecast,
            reports_overdue_maintenance,
            reports_export_overdue,
            reports_critical_issues,
            reports_export_critical,
            reports_status_summary,
            reports_export_status,
            reports_zone_inventory,
            reports_export_zone,
            reports_building_stocktake,
            reports_export_building,
            reports_room_audit,
            reports_export_room_audit,
            reports_empty_rooms,
            reports_export_empty,
            reports_location_discrepancies,
            reports_export_discrepancies,
            reports_category_stocktake,
            reports_export_category_stocktake,
            reports_department_report,
            reports_export_department,
        ]),
        folder("06 — Notifications", [
            notifications_list,
            notifications_unread,
            notifications_mark_read,
        ]),
        folder("07 — User Preferences", [
            preferences_get,
            preferences_update,
            preferences_role_defaults,
        ]),
    ],
    "auth": {
        "type": "bearer",
        "bearer": [{"key": "token", "value": "{{token}}", "type": "string"}]
    },
    "event": [
        {
            "listen": "prerequest",
            "script": {"exec": ["// Collection-level pre-request script"], "type": "text/javascript"}
        },
        {
            "listen": "test",
            "script": {"exec": ["// Collection-level test script"], "type": "text/javascript"}
        }
    ],
    "variable": [
        {"key": "token", "value": ""},
        {"key": "userId", "value": ""},
        {"key": "assetId", "value": ""},
        {"key": "assetTag", "value": ""},
        {"key": "roomId", "value": ""},
        {"key": "notificationId", "value": ""},
    ]
}

# ── Environment ──────────────────────────────────────────────────────

environment = {
    "name": ENV_NAME,
    "values": [
        {"key": "baseUrl", "value": BASE_URL, "type": "default", "enabled": True},
        {"key": "email", "value": EMAIL, "type": "default", "enabled": True},
        {"key": "password", "value": PASSWORD, "type": "secret", "enabled": True},
    ]
}

# ── Write files ──────────────────────────────────────────────────────

with open("/home/uar/Project-PFE/dev/backend/tests/smartinventory-web.postman_collection.json", "w") as f:
    json.dump(collection, f, indent=2)

with open("/home/uar/Project-PFE/dev/backend/tests/smartinventory-web.postman_environment.json", "w") as f:
    json.dump(environment, f, indent=2)

# ── Stats ────────────────────────────────────────────────────────────

def count_items(items):
    n = 0
    for item in items:
        if 'item' in item:
            n += count_items(item['item'])
        else:
            n += 1
    return n

total = count_items(collection['item'])
print(f"✅ Collection: {total} requests across {len(collection['item'])} folders")
print(f"✅ Environment: {len(environment['values'])} variables")
print(f"\nFolders:")
for fld in collection['item']:
    n = count_items([fld])
    print(f"  {fld['name']}: {n} requests")
