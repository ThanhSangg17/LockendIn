using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddAddonPaymentFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "addon_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_addon_orders_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addon_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    product_type = table.Column<int>(type: "int", nullable: false),
                    grant_quantity = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "meal_plan_quota_counters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    quota_date = table.Column<DateOnly>(type: "date", nullable: false),
                    consumed_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    reserved_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    daily_limit = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meal_plan_quota_counters", x => x.id);
                    table.CheckConstraint("ck_meal_plan_quota_counters_counts", "consumed_count >= 0 AND reserved_count >= 0 AND daily_limit > 0 AND consumed_count + reserved_count <= daily_limit");
                    table.ForeignKey(
                        name: "fk_meal_plan_quota_counters_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addon_payment_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    order_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    checkout_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    provider_transaction_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_payment_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_addon_payment_attempts_order_id",
                        column: x => x.order_id,
                        principalTable: "addon_orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addon_product_prices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    product_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    unit_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_by_admin_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    deactivated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_product_prices", x => x.id);
                    table.ForeignKey(
                        name: "fk_addon_product_prices_created_by_admin_id",
                        column: x => x.created_by_admin_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_addon_product_prices_product_id",
                        column: x => x.product_id,
                        principalTable: "addon_products",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addon_webhook_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    attempt_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    event_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    raw_payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_valid_signature = table.Column<bool>(type: "bit", nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    received_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_webhook_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_addon_webhook_logs_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "addon_payment_attempts",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addon_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    product_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    price_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    product_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    unit_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    fulfillment_type_snapshot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    grant_quantity_snapshot = table.Column<int>(type: "int", nullable: true),
                    duration_days_snapshot = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_addon_order_items_order_id",
                        column: x => x.order_id,
                        principalTable: "addon_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_addon_order_items_price_id",
                        column: x => x.price_id,
                        principalTable: "addon_product_prices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_addon_order_items_product_id",
                        column: x => x.product_id,
                        principalTable: "addon_products",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addon_entitlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    order_item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    product_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    fulfillment_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    quantity_granted = table.Column<int>(type: "int", nullable: false),
                    quantity_remaining = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    activated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_entitlements", x => x.id);
                    table.CheckConstraint("ck_addon_entitlements_quantity", "quantity_remaining >= 0");
                    table.ForeignKey(
                        name: "fk_addon_entitlements_order_id",
                        column: x => x.order_id,
                        principalTable: "addon_orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_addon_entitlements_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "addon_order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_addon_entitlements_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "addon_quota_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reservation_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    entitlement_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    quota_date = table.Column<DateOnly>(type: "date", nullable: true),
                    generation_request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    reserved_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    finalized_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    released_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_addon_quota_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_addon_quota_reservations_entitlement_id",
                        column: x => x.entitlement_id,
                        principalTable: "addon_entitlements",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_addon_quota_reservations_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_addon_entitlements_consumption",
                table: "addon_entitlements",
                columns: new[] { "pt_profile_id", "product_code", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_addon_entitlements_order_id",
                table: "addon_entitlements",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_addon_entitlements_order_item_id",
                table: "addon_entitlements",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_order_items_order_id",
                table: "addon_order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_addon_order_items_price_id",
                table: "addon_order_items",
                column: "price_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_order_items_product_id",
                table: "addon_order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_orders_pt_profile_id",
                table: "addon_orders",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_orders_status",
                table: "addon_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_addon_payment_attempts_order_id",
                table: "addon_payment_attempts",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_payment_attempts_status",
                table: "addon_payment_attempts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_addon_payment_attempts_one_success_per_order",
                table: "addon_payment_attempts",
                column: "order_id",
                unique: true,
                filter: "([status]=(2))");

            migrationBuilder.CreateIndex(
                name: "ux_addon_payment_attempts_order_code",
                table: "addon_payment_attempts",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_addon_product_prices_created_by_admin_id",
                table: "addon_product_prices",
                column: "created_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_product_prices_product_id",
                table: "addon_product_prices",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ux_addon_product_prices_one_active_per_product",
                table: "addon_product_prices",
                column: "product_id",
                unique: true,
                filter: "([is_active]=(1))");

            migrationBuilder.CreateIndex(
                name: "ix_addon_products_is_active",
                table: "addon_products",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ux_addon_products_code",
                table: "addon_products",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_addon_quota_reservations_cleanup",
                table: "addon_quota_reservations",
                columns: new[] { "pt_profile_id", "status", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "IX_addon_quota_reservations_entitlement_id",
                table: "addon_quota_reservations",
                column: "entitlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_quota_reservations_generation_request_id",
                table: "addon_quota_reservations",
                column: "generation_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_webhook_logs_attempt_id",
                table: "addon_webhook_logs",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_webhook_logs_event_id",
                table: "addon_webhook_logs",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_addon_webhook_logs_received_at",
                table: "addon_webhook_logs",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "ux_meal_plan_quota_counters_pt_date",
                table: "meal_plan_quota_counters",
                columns: new[] { "pt_profile_id", "quota_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "addon_quota_reservations");

            migrationBuilder.DropTable(
                name: "addon_webhook_logs");

            migrationBuilder.DropTable(
                name: "meal_plan_quota_counters");

            migrationBuilder.DropTable(
                name: "addon_entitlements");

            migrationBuilder.DropTable(
                name: "addon_payment_attempts");

            migrationBuilder.DropTable(
                name: "addon_order_items");

            migrationBuilder.DropTable(
                name: "addon_orders");

            migrationBuilder.DropTable(
                name: "addon_product_prices");

            migrationBuilder.DropTable(
                name: "addon_products");
        }
    }
}
