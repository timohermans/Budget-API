using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budget.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterTransactionJobAddFileContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionsFileJobs_StoredFilePath",
                table: "TransactionsFileJobs");

            migrationBuilder.DropColumn(
                name: "StoredFilePath",
                table: "TransactionsFileJobs");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "TransactionsFileJobs",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "TransactionsFileJobs");

            migrationBuilder.AddColumn<string>(
                name: "StoredFilePath",
                table: "TransactionsFileJobs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionsFileJobs_StoredFilePath",
                table: "TransactionsFileJobs",
                column: "StoredFilePath",
                unique: true);
        }
    }
}
