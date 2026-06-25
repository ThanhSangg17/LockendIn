using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LockedIn.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialAzureDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    role = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    email_verified = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83FD1C72651", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_deleted_by",
                        column: x => x.deleted_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    actor_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    action = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    entity_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    entity_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    metadata_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__audit_lo__3213E83F4EF56003", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_logs_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "customer_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    height_cm = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    fitness_goal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    health_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__customer__3213E83F7EEE07E2", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_profiles_deleted_by",
                        column: x => x.deleted_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_customer_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__notifica__3213E83FD17F67A2", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_deleted_by",
                        column: x => x.deleted_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_notifications_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pt_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    specialization = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    experience_years = table.Column<int>(type: "int", nullable: false),
                    verification_status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    average_rating = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    total_reviews = table.Column<int>(type: "int", nullable: false),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__pt_profi__3213E83F912EDD79", x => x.id);
                    table.ForeignKey(
                        name: "fk_pt_profiles_deleted_by",
                        column: x => x.deleted_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_pt_profiles_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__refresh___3213E83FB90CC2E0", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_usage_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    feature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    token_used = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ai_usage__3213E83F94E74880", x => x.id);
                    table.ForeignKey(
                        name: "fk_ai_usage_logs_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "packages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    session_count = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__packages__3213E83F330FC9D4", x => x.id);
                    table.ForeignKey(
                        name: "fk_packages_deleted_by",
                        column: x => x.deleted_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_packages_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "pt_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    document_type = table.Column<int>(type: "int", nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__pt_docum__3213E83F008BB1B4", x => x.id);
                    table.ForeignKey(
                        name: "fk_pt_documents_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    package_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    total_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    session_count = table.Column<int>(type: "int", nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    settlement_due_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__bookings__3213E83FD21291C7", x => x.id);
                    table.ForeignKey(
                        name: "fk_bookings_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_package_id",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_bookings_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    booking_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    firebase_conversation_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__conversa__3213E83F4A447B13", x => x.id);
                    table.ForeignKey(
                        name: "fk_conversations_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_conversations_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_conversations_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "disputes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    booking_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    resolution_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resolved_by_admin_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__disputes__3213E83F9BA14776", x => x.id);
                    table.ForeignKey(
                        name: "fk_disputes_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_disputes_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_disputes_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_disputes_resolved_by_admin_id",
                        column: x => x.resolved_by_admin_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    booking_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "PayOS"),
                    order_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    checkout_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    provider_transaction_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expired_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payments__3213E83FEDB87C2A", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    booking_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_hidden = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reviews__3213E83FC246B2B0", x => x.id);
                    table.ForeignKey(
                        name: "fk_reviews_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reviews_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reviews_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "settlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    booking_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    gross_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    platform_fee = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    net_amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    settled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__settleme__3213E83FC82FEC5F", x => x.id);
                    table.ForeignKey(
                        name: "fk_settlements_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_settlements_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    booking_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    customer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    pt_profile_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    course_note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__workspac__3213E83FDD6DCD60", x => x.id);
                    table.ForeignKey(
                        name: "fk_workspaces_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_workspaces_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer_profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_workspaces_pt_profile_id",
                        column: x => x.pt_profile_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "dispute_evidences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    dispute_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    file_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    file_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__dispute___3213E83F1F7FC76C", x => x.id);
                    table.ForeignKey(
                        name: "fk_dispute_evidences_dispute_id",
                        column: x => x.dispute_id,
                        principalTable: "disputes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payment_webhook_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    payment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "PayOS"),
                    event_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    event_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    raw_payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_valid_signature = table.Column<bool>(type: "bit", nullable: false),
                    processed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    received_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payment___3213E83F77B7BFB3", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_webhook_logs_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "meal_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    workspace_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_by_pt_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    content_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    source = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__meal_pla__3213E83FE8434E9A", x => x.id);
                    table.ForeignKey(
                        name: "fk_meal_plans_created_by_pt_id",
                        column: x => x.created_by_pt_id,
                        principalTable: "pt_profiles",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_meal_plans_deleted_by",
                        column: x => x.deleted_by,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_meal_plans_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_logs_created_at",
                table: "ai_usage_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_logs_pt_profile_id",
                table: "ai_usage_logs",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_actor_user_id",
                table: "audit_logs",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_created_at",
                table: "bookings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_customer_id",
                table: "bookings",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_package_id",
                table: "bookings",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_pt_profile_id",
                table: "bookings",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_status",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_customer_id",
                table: "conversations",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_pt_profile_id",
                table: "conversations",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "UQ__conversa__5DE3A5B065808E54",
                table: "conversations",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__conversa__C704B367F57EC45D",
                table: "conversations",
                column: "firebase_conversation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_conversations_booking_id",
                table: "conversations",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_conversations_firebase_id",
                table: "conversations",
                column: "firebase_conversation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_profiles_deleted_by",
                table: "customer_profiles",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "ix_customer_profiles_is_deleted",
                table: "customer_profiles",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "UQ__customer__B9BE370E5DAC7E13",
                table: "customer_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_customer_profiles_user_id",
                table: "customer_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dispute_evidences_dispute_id",
                table: "dispute_evidences",
                column: "dispute_id");

            migrationBuilder.CreateIndex(
                name: "ix_disputes_booking_id",
                table: "disputes",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_disputes_created_at",
                table: "disputes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_disputes_customer_id",
                table: "disputes",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_disputes_pt_profile_id",
                table: "disputes",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_disputes_resolved_by_admin_id",
                table: "disputes",
                column: "resolved_by_admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_disputes_status",
                table: "disputes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_disputes_one_open_per_booking",
                table: "disputes",
                column: "booking_id",
                unique: true,
                filter: "([status] IN ((1), (2), (3)))");

            migrationBuilder.CreateIndex(
                name: "ix_meal_plans_created_by_pt_id",
                table: "meal_plans",
                column: "created_by_pt_id");

            migrationBuilder.CreateIndex(
                name: "IX_meal_plans_deleted_by",
                table: "meal_plans",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "ix_meal_plans_is_active",
                table: "meal_plans",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_meal_plans_is_deleted",
                table: "meal_plans",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_meal_plans_workspace_id",
                table: "meal_plans",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_at",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_deleted_by",
                table: "notifications",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_is_deleted",
                table: "notifications",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_is_read",
                table: "notifications",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_packages_deleted_by",
                table: "packages",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "ix_packages_is_active",
                table: "packages",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_packages_is_deleted",
                table: "packages",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_packages_price",
                table: "packages",
                column: "price");

            migrationBuilder.CreateIndex(
                name: "ix_packages_pt_profile_id",
                table: "packages",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_webhook_logs_event_id",
                table: "payment_webhook_logs",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_webhook_logs_payment_id",
                table: "payment_webhook_logs",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_webhook_logs_received_at",
                table: "payment_webhook_logs",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "ix_payments_booking_id",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_status",
                table: "payments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UQ__payments__99D12D3FD8197709",
                table: "payments",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_payments_one_success_per_booking",
                table: "payments",
                column: "booking_id",
                unique: true,
                filter: "([status]=(2))");

            migrationBuilder.CreateIndex(
                name: "ux_payments_order_code",
                table: "payments",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pt_documents_document_type",
                table: "pt_documents",
                column: "document_type");

            migrationBuilder.CreateIndex(
                name: "ix_pt_documents_pt_profile_id",
                table: "pt_documents",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_pt_documents_status",
                table: "pt_documents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_pt_profiles_average_rating",
                table: "pt_profiles",
                column: "average_rating");

            migrationBuilder.CreateIndex(
                name: "IX_pt_profiles_deleted_by",
                table: "pt_profiles",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "ix_pt_profiles_is_deleted",
                table: "pt_profiles",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_pt_profiles_verification_status",
                table: "pt_profiles",
                column: "verification_status");

            migrationBuilder.CreateIndex(
                name: "UQ__pt_profi__B9BE370E276F861C",
                table: "pt_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_pt_profiles_user_id",
                table: "pt_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_customer_id",
                table: "reviews",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_is_hidden",
                table: "reviews",
                column: "is_hidden");

            migrationBuilder.CreateIndex(
                name: "ix_reviews_pt_profile_id",
                table: "reviews",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "UQ__reviews__5DE3A5B06B553922",
                table: "reviews",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_reviews_booking_id",
                table: "reviews",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_settlements_pt_profile_id",
                table: "settlements",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_settlements_status",
                table: "settlements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UQ__settleme__5DE3A5B0FE34E5F5",
                table: "settlements",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_settlements_booking_id",
                table: "settlements",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_deleted_by",
                table: "users",
                column: "deleted_by");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_users_is_deleted",
                table: "users",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "ix_users_role",
                table: "users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "ix_users_status",
                table: "users",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UQ__users__AB6E6164EEF4CB86",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_customer_id",
                table: "workspaces",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_pt_profile_id",
                table: "workspaces",
                column: "pt_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_status",
                table: "workspaces",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UQ__workspac__5DE3A5B0181B9644",
                table: "workspaces",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_workspaces_booking_id",
                table: "workspaces",
                column: "booking_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_usage_logs");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "dispute_evidences");

            migrationBuilder.DropTable(
                name: "meal_plans");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "payment_webhook_logs");

            migrationBuilder.DropTable(
                name: "pt_documents");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "settlements");

            migrationBuilder.DropTable(
                name: "disputes");

            migrationBuilder.DropTable(
                name: "workspaces");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "customer_profiles");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "pt_profiles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
