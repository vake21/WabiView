using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WabiView.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coordinators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsOnline = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastChecked = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailureCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeRate = table.Column<decimal>(type: "TEXT", nullable: true),
                    MinInputCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoinjoinTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TxId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    BlockHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    BlockHeight = table.Column<int>(type: "INTEGER", nullable: true),
                    FirstSeen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InputCount = table.Column<int>(type: "INTEGER", nullable: false),
                    OutputCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalInputValue = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalOutputValue = table.Column<long>(type: "INTEGER", nullable: false),
                    FeePaid = table.Column<long>(type: "INTEGER", nullable: false),
                    VSize = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    CoordinatorId = table.Column<int>(type: "INTEGER", nullable: true),
                    RoundId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Confirmations = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinjoinTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinjoinTransactions_Coordinators_CoordinatorId",
                        column: x => x.CoordinatorId,
                        principalTable: "Coordinators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rounds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoundId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CoordinatorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Phase = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InputCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TxId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "INTEGER", nullable: false),
                    FailureReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rounds_Coordinators_CoordinatorId",
                        column: x => x.CoordinatorId,
                        principalTable: "Coordinators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoinjoinTransactions_BlockHeight",
                table: "CoinjoinTransactions",
                column: "BlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_CoinjoinTransactions_CoordinatorId",
                table: "CoinjoinTransactions",
                column: "CoordinatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinjoinTransactions_TxId",
                table: "CoinjoinTransactions",
                column: "TxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coordinators_Url",
                table: "Coordinators",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_CoordinatorId_RoundId",
                table: "Rounds",
                columns: new[] { "CoordinatorId", "RoundId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rounds_RoundId",
                table: "Rounds",
                column: "RoundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoinjoinTransactions");

            migrationBuilder.DropTable(
                name: "Rounds");

            migrationBuilder.DropTable(
                name: "Coordinators");
        }
    }
}
