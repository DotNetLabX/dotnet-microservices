using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Production.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class X_DONT_DELETE_SeedMasterData : Migration
    {
        /// <inheritdoc />
				protected override void Up(MigrationBuilder migrationBuilder)
				{
						migrationBuilder.Sql(@"
            INSERT INTO [AssetTypeDefinition] 
            (id, name, description, maxAssetCount, allowedFileExtensions, defaultFileExtension, MaxFileSizeInMB)
            VALUES
            (1, 'Manuscript', 'Author Manuscript ', 1, '[""pdf""]', 'pdf', 50),
            (2, 'ReviewReport', 'Reviewer Report', 3, '[""pdf""]', 'pdf', 50),
            (3, 'TypesetDraft', 'Draft PDF (Typesetter)', 0, '[""pdf""]', 'pdf', 50),
            (4, 'FinalPdf', 'Final PDF', 0, '[""pdf""]', 'pdf', 50),
            (5, 'FinalHtml', 'Final HTML Zip', 0, '[""zip""]', 'zip', 100),
            (6, 'FinalEpub', 'Final Epub', 0, '[""epub""]', 'epub', 5),
            (10, 'SupplementaryFile', 'Supplementary File', 10, '[]', 'pdf', 10),
            (11, 'Figure', 'HTML Figure', 10, '[""jpg"",""png"",""tif"",""tiff"",""eps""]', 'tif', 10),
            (12, 'DataSheet', 'Data Sheet', 10, '[""csv"",""xls""]', 'csv', 1);
        ");
				}

				/// <inheritdoc />
				protected override void Down(MigrationBuilder migrationBuilder)
        {
						migrationBuilder.Sql("DELETE FROM [AssetTypeDefinition]");
				}
    }
}
