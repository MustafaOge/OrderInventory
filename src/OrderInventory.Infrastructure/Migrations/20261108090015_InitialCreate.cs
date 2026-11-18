using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrderInventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CREATED_AT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    STATUS = table.Column<int>(type: "integer", nullable: false),
                    TOTAL_AMOUNT = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NAME = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SKU = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PRICE = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    STOCK_QUANTITY = table.Column<int>(type: "integer", nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ORDER_ID = table.Column<int>(type: "integer", nullable: false),
                    PRODUCT_ID = table.Column<int>(type: "integer", nullable: false),
                    QUANTITY = table.Column<int>(type: "integer", nullable: false),
                    UNIT_PRICE = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CREATED_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UPDATED_DATE = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IS_DELETED = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_ORDER_ID",
                        column: x => x.ORDER_ID,
                        principalTable: "Orders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_PRODUCT_ID",
                        column: x => x.PRODUCT_ID,
                        principalTable: "Products",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ORDER_ID",
                table: "OrderItems",
                column: "ORDER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_PRODUCT_ID",
                table: "OrderItems",
                column: "PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
