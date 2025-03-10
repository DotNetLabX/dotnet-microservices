using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ArticleHub.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "journal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Abbreviation = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Honorific = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "article",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Doi = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Stage = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubmittedById = table.Column<int>(type: "integer", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AcceptedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PublishedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    JournalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article", x => x.Id);
                    table.ForeignKey(
                        name: "FK_article_journal_JournalId",
                        column: x => x.JournalId,
                        principalTable: "journal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_article_person_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "article_actor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "AUT"),
                    ArticleId = table.Column<int>(type: "integer", nullable: false),
                    PersonId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_actor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_article_actor_article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_article_actor_person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_article_JournalId",
                table: "article",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_article_SubmittedById",
                table: "article",
                column: "SubmittedById");

            migrationBuilder.CreateIndex(
                name: "IX_article_Title",
                table: "article",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_article_actor_ArticleId_PersonId_Role",
                table: "article_actor",
                columns: new[] { "ArticleId", "PersonId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_article_actor_PersonId",
                table: "article_actor",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_person_UserId",
                table: "person",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_actor");

            migrationBuilder.DropTable(
                name: "article");

            migrationBuilder.DropTable(
                name: "journal");

            migrationBuilder.DropTable(
                name: "person");
        }
    }
}
