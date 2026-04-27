# Segaris MCP Tool Reference

Prefer `scripts/segaris_mcp.py list-tools` for the live source of truth. The names below are known from the current MCP server.

## Identity

- `who_am_i`: verify the configured API key and return `{ authenticated, userId, userName, roles }`.

## Projects

- `segaris_list_project_programs`
- `segaris_list_project_axes`: optional `programId`.
- `segaris_list_project_statuses`
- `segaris_list_projects`
- `segaris_list_project_sub_entity_categories`
- `segaris_list_project_sub_entities`: required `projectId`.
- `segaris_list_project_risk_categories`
- `segaris_list_project_risks`: required `projectId`.
- `segaris_list_project_budgets`: required `projectId`.

## Assets

- `segaris_list_asset_categories`
- `segaris_list_asset_statuses`
- `segaris_list_assets`

## Capex

- `segaris_list_capex_categories`
- `segaris_list_capex_statuses`
- `segaris_list_capex`

## Opex

- `segaris_list_opex_categories`
- `segaris_list_opex_statuses`
- `segaris_list_opex`
- `segaris_list_opex_entries`: required `contractId`.
- `segaris_get_opex_stats`: required `contractId`.

## Travel

- `segaris_list_travel_categories`
- `segaris_list_travel_cost_centers`
- `segaris_list_travel_statuses`
- `segaris_list_travels`
- `segaris_list_travel_entry_categories`
- `segaris_list_travel_entries`: required `travelId`.

## Maintenance

- `segaris_list_maint_categories`
- `segaris_list_maint_statuses`
- `segaris_list_maint`

## Admin (Processes)

Admin is the administrative processes module. A process can have steps and aggregate status stats.

- `segaris_list_admin_categories`
- `segaris_list_admin`
- `segaris_list_admin_steps`: required `processId`.
- `segaris_get_admin_stats`: required `processId`.

## Firebird (People)

Firebird is the code name for the people management module.

- `segaris_list_firebird_categories`
- `segaris_list_firebird_statuses`
- `segaris_list_firebirds`
- `segaris_list_firebird_sub_entities`: required `firebirdId`.

## Clothes

- `segaris_list_clothes_categories`
- `segaris_list_clothes_statuses`
- `segaris_list_clothes_wash_types`
- `segaris_list_clothes_colors`
- `segaris_list_clothes_color_styles`
- `segaris_list_clothes`
- `segaris_list_clothes_color_assignments`: required `garmentId`.

## Inventory

- `segaris_list_inv_vendor_statuses`
- `segaris_list_inv_vendors`
- `segaris_get_inv_vendor_stats`: required `vendorId`.
- `segaris_list_inv_item_categories`
- `segaris_list_inv_item_statuses`
- `segaris_list_inv_items`: optional `vendorId`.
- `segaris_get_shopping_list`
- `segaris_get_inv_item_price_history`: required `itemId`.
- `segaris_list_inv_order_statuses`
- `segaris_list_inv_orders`
- `segaris_list_inv_order_lines`: required `orderId`.
- `segaris_get_inv_order_stats`: required `orderId`.
