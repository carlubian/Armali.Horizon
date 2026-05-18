using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Armali.Horizon.Althes.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SystemPrompt = table.Column<string>(type: "TEXT", nullable: false),
                    Model = table.Column<string>(type: "TEXT", nullable: true),
                    CarryOverSummary = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedSkillsCsv = table.Column<string>(type: "TEXT", nullable: false),
                    AllowedRecipientsCsv = table.Column<string>(type: "TEXT", nullable: false),
                    MaxActionsPerHour = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    RunId = table.Column<string>(type: "TEXT", nullable: false),
                    AgentName = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Skill = table.Column<string>(type: "TEXT", nullable: true),
                    Sender = table.Column<string>(type: "TEXT", nullable: true),
                    Recipient = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Visibility = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", nullable: true),
                    UserQueryId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<string>(type: "TEXT", nullable: false),
                    AgentName = table.Column<string>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Trigger = table.Column<int>(type: "INTEGER", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: true),
                    TokenCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserQueries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ConversationId = table.Column<string>(type: "TEXT", nullable: false),
                    AgentName = table.Column<string>(type: "TEXT", nullable: false),
                    RunId = table.Column<string>(type: "TEXT", nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", nullable: false),
                    Question = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: true),
                    AnsweredAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AnsweredBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQueries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_StartedAt",
                table: "Conversations",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Status",
                table: "Conversations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_CorrelationId",
                table: "Messages",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RunId",
                table: "Messages",
                column: "RunId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Visibility",
                table: "Messages",
                column: "Visibility");

            migrationBuilder.CreateIndex(
                name: "IX_Runs_AgentName_Status",
                table: "Runs",
                columns: new[] { "AgentName", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Runs_ConversationId",
                table: "Runs",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Runs_StartedAt",
                table: "Runs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserQueries_ConversationId",
                table: "UserQueries",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserQueries_CorrelationId",
                table: "UserQueries",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserQueries_Status",
                table: "UserQueries",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Runs");

            migrationBuilder.DropTable(
                name: "UserQueries");
        }
    }
}
