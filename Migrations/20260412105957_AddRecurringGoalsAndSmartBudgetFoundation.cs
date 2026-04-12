using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTracker.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringGoalsAndSmartBudgetFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RolloverFromBudgetId",
                table: "Budgets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                table: "Budgets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "BudgetRolloverRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FromBudgetId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ToBudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FromYear = table.Column<int>(type: "int", nullable: false),
                    FromMonth = table.Column<int>(type: "int", nullable: false),
                    ToYear = table.Column<int>(type: "int", nullable: false),
                    ToMonth = table.Column<int>(type: "int", nullable: false),
                    PreviousMonthlyLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PreviousSpent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RolledOverAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NewMonthlyLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AppliedStrategy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetRolloverRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetRolloverRecords_Budgets_FromBudgetId",
                        column: x => x.FromBudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BudgetRolloverRecords_Budgets_ToBudgetId",
                        column: x => x.ToBudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetRolloverRecords_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetRolloverRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BudgetTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CategoryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MonthlyLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RolloverStrategy = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetTemplates_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BudgetTemplates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecurringTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DestinationAccountId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CategoryId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MaxOccurrences = table.Column<int>(type: "int", nullable: true),
                    NextExecutionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExecutedCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Accounts_DestinationAccountId",
                        column: x => x.DestinationAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SavingsGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsGoals_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavingsGoals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecurringTransactionExecutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RecurringTransactionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ScheduledForDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GeneratedTransactionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExecutedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringTransactionExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringTransactionExecutions_RecurringTransactions_Recurri~",
                        column: x => x.RecurringTransactionId,
                        principalTable: "RecurringTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecurringTransactionExecutions_Transactions_GeneratedTransac~",
                        column: x => x.GeneratedTransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SavingsGoalContributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SavingsGoalId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TransactionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ContributionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingsGoalContributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingsGoalContributions_SavingsGoals_SavingsGoalId",
                        column: x => x.SavingsGoalId,
                        principalTable: "SavingsGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingsGoalContributions_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_RolloverFromBudgetId",
                table: "Budgets",
                column: "RolloverFromBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TemplateId",
                table: "Budgets",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRolloverRecords_CategoryId",
                table: "BudgetRolloverRecords",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRolloverRecords_FromBudgetId",
                table: "BudgetRolloverRecords",
                column: "FromBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRolloverRecords_ToBudgetId",
                table: "BudgetRolloverRecords",
                column: "ToBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRolloverRecords_UserId_CategoryId_ToYear_ToMonth",
                table: "BudgetRolloverRecords",
                columns: new[] { "UserId", "CategoryId", "ToYear", "ToMonth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetTemplates_CategoryId",
                table: "BudgetTemplates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetTemplates_UserId_CategoryId_IsActive",
                table: "BudgetTemplates",
                columns: new[] { "UserId", "CategoryId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetTemplates_UserId_Name",
                table: "BudgetTemplates",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionExecutions_GeneratedTransactionId",
                table: "RecurringTransactionExecutions",
                column: "GeneratedTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactionExecutions_RecurringTransactionId_Schedu~",
                table: "RecurringTransactionExecutions",
                columns: new[] { "RecurringTransactionId", "ScheduledForDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_AccountId",
                table: "RecurringTransactions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_CategoryId",
                table: "RecurringTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_DestinationAccountId",
                table: "RecurringTransactions",
                column: "DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_UserId_IsActive_NextExecutionDate",
                table: "RecurringTransactions",
                columns: new[] { "UserId", "IsActive", "NextExecutionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoalContributions_SavingsGoalId_ContributionDate",
                table: "SavingsGoalContributions",
                columns: new[] { "SavingsGoalId", "ContributionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoalContributions_TransactionId",
                table: "SavingsGoalContributions",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoals_AccountId",
                table: "SavingsGoals",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoals_UserId_Status",
                table: "SavingsGoals",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_BudgetTemplates_TemplateId",
                table: "Budgets",
                column: "TemplateId",
                principalTable: "BudgetTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Budgets_RolloverFromBudgetId",
                table: "Budgets",
                column: "RolloverFromBudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_BudgetTemplates_TemplateId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Budgets_RolloverFromBudgetId",
                table: "Budgets");

            migrationBuilder.DropTable(
                name: "BudgetRolloverRecords");

            migrationBuilder.DropTable(
                name: "BudgetTemplates");

            migrationBuilder.DropTable(
                name: "RecurringTransactionExecutions");

            migrationBuilder.DropTable(
                name: "SavingsGoalContributions");

            migrationBuilder.DropTable(
                name: "RecurringTransactions");

            migrationBuilder.DropTable(
                name: "SavingsGoals");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_RolloverFromBudgetId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_TemplateId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "RolloverFromBudgetId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Budgets");
        }
    }
}
