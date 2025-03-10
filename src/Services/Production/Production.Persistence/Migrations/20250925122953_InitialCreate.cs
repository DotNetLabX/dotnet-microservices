using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Production.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleStageTransition",
                columns: table => new
                {
                    CurrentStage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DestinationStage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleStageTransition", x => new { x.CurrentStage, x.ActionType, x.DestinationStage });
                });

            migrationBuilder.CreateTable(
                name: "AssetStateTransition",
                columns: table => new
                {
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DestinationState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetStateTransition", x => new { x.CurrentState, x.ActionType, x.DestinationState });
                });

            migrationBuilder.CreateTable(
                name: "AssetStateTransitionCondition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleStage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AssetTypes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionTypes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetStateTransitionCondition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetTypeDefinition",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    DefaultCategoryId = table.Column<int>(type: "int", nullable: false),
                    DefaultFileExtension = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "pdf"),
                    MaxNumber = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    MaxFileSizeInMB = table.Column<byte>(type: "tinyint", nullable: false),
                    AllowedFileExtensions = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTypeDefinition", x => x.Id);
                    table.UniqueConstraint("AK_AssetTypeDefinition_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Person",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Affiliation = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false, comment: "Institution or organization they are associated with when they conduct their research."),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    TypeDiscriminator = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    CompanyName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Person", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stage",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stage", x => x.Id);
                    table.UniqueConstraint("AK_Stage_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Journal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    DefaultTypesetterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journal_Person_DefaultTypesetterId",
                        column: x => x.DefaultTypesetterId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Article",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Doi = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SubmitedById = table.Column<int>(type: "int", nullable: false),
                    SubmitedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false),
                    VolumeId = table.Column<int>(type: "int", nullable: false),
                    PublishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedById = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Article", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Article_Journal_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Article_Person_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Article_Person_SubmitedById",
                        column: x => x.SubmitedById,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Article_Stage_Stage",
                        column: x => x.Stage,
                        principalTable: "Stage",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArticleContributor",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "AUT")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleContributor", x => new { x.ArticleId, x.PersonId, x.Role });
                    table.ForeignKey(
                        name: "FK_ArticleContributor_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticleContributor_Person_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Person",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Number = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asset_AssetTypeDefinition_Type",
                        column: x => x.Type,
                        principalTable: "AssetTypeDefinition",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StageId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StageHistory_Article_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "Article",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StageHistory_Stage_StageId",
                        column: x => x.StageId,
                        principalTable: "Stage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetAction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetAction_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "File",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, comment: "Original full file name, with extension"),
                    FileServerId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false, comment: "Size of the file in kilobytes"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Final name of the file after renaming"),
                    Version = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedById = table.Column<int>(type: "int", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_File", x => x.Id);
                    table.ForeignKey(
                        name: "FK_File_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetCurrentFileLink",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    FileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCurrentFileLink", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_AssetCurrentFileLink_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetCurrentFileLink_File_FileId",
                        column: x => x.FileId,
                        principalTable: "File",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ArticleStageTransition",
                columns: new[] { "ActionType", "CurrentStage", "DestinationStage" },
                values: new object[,]
                {
                    { "AssignTypesetter", "Accepted", "InProduction" },
                    { "SchedulePublication", "FinalProduction", "PublicationScheduled" },
                    { "PublishArticle", "PublicationScheduled", "Published" }
                });

            migrationBuilder.InsertData(
                table: "AssetStateTransition",
                columns: new[] { "ActionType", "CurrentState", "DestinationState" },
                values: new object[,]
                {
                    { "Request", "None", "Requested" },
                    { "Upload", "None", "Uploaded" },
                    { "CancelRequest", "Requested", "Uploaded" },
                    { "Upload", "Requested", "Uploaded" },
                    { "Approve", "Uploaded", "Approved" },
                    { "Request", "Uploaded", "Requested" },
                    { "Upload", "Uploaded", "Uploaded" }
                });

            migrationBuilder.InsertData(
                table: "AssetStateTransitionCondition",
                columns: new[] { "Id", "ActionTypes", "ArticleStage", "AssetTypes" },
                values: new object[,]
                {
                    { 1, "[0,2,3]", "InProduction", "[1,11,12,10]" },
                    { 2, "[0,2,3]", "DraftProduction", "[1,11,12,10]" },
                    { 3, "[0]", "InProduction", "[3]" },
                    { 4, "[0,2,3,1]", "DraftProduction", "[3]" },
                    { 5, "[0,2,3]", "FinalProduction", "[4,5,6]" }
                });

            migrationBuilder.InsertData(
                table: "Stage",
                columns: new[] { "Id", "Description", "Info", "Name" },
                values: new object[,]
                {
                    { 101, "The Author created the Article", "The article has been created. Please upload the Manuscript and any Supplementary materials. Associate the authors with the article.", "Created" },
                    { 102, "Author uploaded the Manuscript file", "The manuscript has been uploaded. You can now submit the article for editorial checks.", "ManuscriptUploaded" },
                    { 103, "The Manuscript was submitted by the author for editorial checks", "Our editorial specialists are checking your article. We will contact you if we need additional files or information.", "Submitted" },
                    { 104, "Manuscript failed the initial editorial checks", "The manuscript does not meet the required quality standards of this journal.", "InitialRejected" },
                    { 105, "Manuscript passed the initial editorial checks", "Your manuscript has passed the initial checks. It will now move forward for review.", "InitialApproved" },
                    { 201, "Article is under peer review", "Your article is currently being reviewed by experts in the field.", "UnderReview" },
                    { 202, "All reviewer feedback received, pending editor's decision", "Reviewer feedback has been received. The editor will now make a decision on your article.", "ReadyForDecision" },
                    { 203, "Editor requested a revised manuscript from the author", "The editor has requested revisions. Please upload your revised manuscript to continue the review process.", "AwaitingRevision" },
                    { 204, "Article rejected after review", "Your article was rejected following review. Please read the feedback carefully if you plan to resubmit.", "Rejected" },
                    { 205, "Article accepted after review", "Your article has been accepted for publication. The production process will now begin.", "Accepted" },
                    { 300, "Typesetter assigned to the article", "A typesetter has been assigned and is preparing your Author’s Proof.", "InProduction" },
                    { 301, "Typesetter uploaded the draft PDF for author approval", "The Author’s Proof (draft PDF) is available for you to check and provide corrections.", "DraftProduction" },
                    { 302, "Author approved the draft PDF, finalization in progress", "The typesetter is preparing the final version of your article for publication.", "FinalProduction" },
                    { 304, "Article scheduled for online publication", "Quality checks are complete. Your article is scheduled for publication and will appear online within the next few working days.", "PublicationScheduled" },
                    { 305, "Article published", "Your article has been published and sent to repositories. Availability in repositories may vary depending on their processing times.", "Published" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Article_JournalId",
                table: "Article",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_Article_PublishedById",
                table: "Article",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "IX_Article_Stage",
                table: "Article",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_Article_SubmitedById",
                table: "Article",
                column: "SubmitedById");

            migrationBuilder.CreateIndex(
                name: "IX_Article_Title",
                table: "Article",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleContributor_PersonId",
                table: "ArticleContributor",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_ArticleId",
                table: "Asset",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_Type",
                table: "Asset",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAction_AssetId",
                table: "AssetAction",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCurrentFileLink_FileId",
                table: "AssetCurrentFileLink",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetTypeDefinition_Name",
                table: "AssetTypeDefinition",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_File_AssetId",
                table: "File",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Journal_DefaultTypesetterId",
                table: "Journal",
                column: "DefaultTypesetterId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_UserId",
                table: "Person",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Stage_Name",
                table: "Stage",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageHistory_ArticleId",
                table: "StageHistory",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_StageHistory_StageId",
                table: "StageHistory",
                column: "StageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleContributor");

            migrationBuilder.DropTable(
                name: "ArticleStageTransition");

            migrationBuilder.DropTable(
                name: "AssetAction");

            migrationBuilder.DropTable(
                name: "AssetCurrentFileLink");

            migrationBuilder.DropTable(
                name: "AssetStateTransition");

            migrationBuilder.DropTable(
                name: "AssetStateTransitionCondition");

            migrationBuilder.DropTable(
                name: "StageHistory");

            migrationBuilder.DropTable(
                name: "File");

            migrationBuilder.DropTable(
                name: "Asset");

            migrationBuilder.DropTable(
                name: "Article");

            migrationBuilder.DropTable(
                name: "AssetTypeDefinition");

            migrationBuilder.DropTable(
                name: "Journal");

            migrationBuilder.DropTable(
                name: "Stage");

            migrationBuilder.DropTable(
                name: "Person");
        }
    }
}
