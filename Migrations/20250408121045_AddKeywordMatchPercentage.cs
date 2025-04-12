using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddKeywordMatchPercentage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "KeywordMatchPercentage",
                table: "TestAnswerResults",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeywordMatchPercentage",
                table: "TestAnswerResults");
        }
    }
}
