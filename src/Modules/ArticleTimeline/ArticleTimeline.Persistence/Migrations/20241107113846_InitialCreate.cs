using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ArticleTimeline.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ArticleTimeline");

            migrationBuilder.CreateTable(
                name: "TimelineTemplate",
                schema: "ArticleTimeline",
                columns: table => new
                {
                    SourceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TitleTemplate = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DescriptionTemplate = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineTemplate", x => new { x.SourceType, x.SourceId });
                });

            migrationBuilder.CreateTable(
                name: "TimelineVisibility",
                schema: "ArticleTimeline",
                columns: table => new
                {
                    SourceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    RoleType = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineVisibility", x => new { x.SourceType, x.SourceId, x.RoleType });
                });

            migrationBuilder.CreateTable(
                name: "Timelines",
                schema: "ArticleTimeline",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    NewStage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LasModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Timelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Timelines_TimelineTemplate_SourceType_SourceId",
                        columns: x => new { x.SourceType, x.SourceId },
                        principalSchema: "ArticleTimeline",
                        principalTable: "TimelineTemplate",
                        principalColumns: new[] { "SourceType", "SourceId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "ArticleTimeline",
                table: "TimelineTemplate",
                columns: new[] { "SourceId", "SourceType", "DescriptionTemplate", "TitleTemplate" },
                values: new object[,]
                {
                    { "Approve", "ActionExecuted", "<<RoleUser>> has approved the <<UploadedFile>>.", "<<UserName>> approved <<UploadedFile>>" },
                    { "CancelRequest", "ActionExecuted", "<<RoleUser>> has cancelled the request for a new version of the <<UploadedFile>>.", "<<UserName>> unrequested the new <<UploadedFile>>" },
                    { "Request", "ActionExecuted", "<<RoleUser>> has requested for <<UploadedFile>>.", "<<UserName>> requested a new <<UploadedFile>>" },
                    { "Upload", "ActionExecuted", "<<RoleUser>> has uploaded the <<UploadedFile>>", "<<UserName>> uploaded the <<UploadedFile>>" },
                    { "Accepted->InProduction", "StageTransition", "<<RoleUser>> has started production.", "<<CurrentStage>> stage completed" },
                    { "DraftProduction->FinalProduction", "StageTransition", "<<RoleUser>> has aproved production.", "<<CurrentStage>> stage completed" },
                    { "FinalProduction->Published", "StageTransition", "<<RoleUser>> has published the article.", "<<CurrentStage>> stage completed" },
                    { "InProduction->DraftProduction", "StageTransition", "<<RoleUser>> has uploaded the Draft Pdf.", "<<CurrentStage>> stage completed" }
                });

            migrationBuilder.InsertData(
                schema: "ArticleTimeline",
                table: "TimelineVisibility",
                columns: new[] { "RoleType", "SourceId", "SourceType" },
                values: new object[,]
                {
                    { "CORAUT", "Approve", "ActionExecuted" },
                    { "POF", "Approve", "ActionExecuted" },
                    { "TSOF", "Approve", "ActionExecuted" },
                    { "CORAUT", "CancelRequest", "ActionExecuted" },
                    { "POF", "CancelRequest", "ActionExecuted" },
                    { "TSOF", "CancelRequest", "ActionExecuted" },
                    { "CORAUT", "Request", "ActionExecuted" },
                    { "POF", "Request", "ActionExecuted" },
                    { "TSOF", "Request", "ActionExecuted" },
                    { "CORAUT", "Upload", "ActionExecuted" },
                    { "POF", "Upload", "ActionExecuted" },
                    { "TSOF", "Upload", "ActionExecuted" },
                    { "POF", "Accepted->InProduction", "StageTransition" },
                    { "TSOF", "Accepted->InProduction", "StageTransition" },
                    { "POF", "DraftProduction->FinalProduction", "StageTransition" },
                    { "TSOF", "DraftProduction->FinalProduction", "StageTransition" },
                    { "CORAUT", "FinalProduction->Published", "StageTransition" },
                    { "POF", "FinalProduction->Published", "StageTransition" },
                    { "TSOF", "FinalProduction->Published", "StageTransition" },
                    { "CORAUT", "InProduction->DraftProduction", "StageTransition" },
                    { "POF", "InProduction->DraftProduction", "StageTransition" },
                    { "TSOF", "InProduction->DraftProduction", "StageTransition" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Timelines_SourceType_SourceId",
                schema: "ArticleTimeline",
                table: "Timelines",
                columns: new[] { "SourceType", "SourceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Timelines",
                schema: "ArticleTimeline");

            migrationBuilder.DropTable(
                name: "TimelineVisibility",
                schema: "ArticleTimeline");

            migrationBuilder.DropTable(
                name: "TimelineTemplate",
                schema: "ArticleTimeline");
        }
    }
}
