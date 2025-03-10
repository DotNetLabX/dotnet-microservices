using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Blocks.EntityFrameworkCore;
using Production.Domain.Assets;

namespace Production.Persistence.EntityConfigurations;
public class ArticleEntityConfiguration : AuditedEntityConfiguration<Article>
{
    public override void Configure(EntityTypeBuilder<Article> entity)
    {
        base.Configure(entity);

        entity.HasIndex(e => e.Title);

        //insight - using constants instead of direct numbers
        entity.Property(e => e.Title).HasMaxLength(MaxLength.C256).IsRequired();
        entity.Property(e => e.Doi).HasMaxLength(MaxLength.C64).IsRequired();
        entity.Property(e => e.VolumeId).IsRequired();
        entity.Property(e => e.Stage).HasEnumConversion().IsRequired();

        entity.Property(e => e.SubmitedOn).IsRequired();
        entity.Property(e => e.AcceptedOn).IsRequired();


				entity.HasOne<Stage>().WithMany()
					 .HasForeignKey(e => e.Stage)
					 .HasPrincipalKey(e => e.Name)
					 .IsRequired()
					 .OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(e => e.SubmitedBy).WithMany()
            .HasForeignKey(e => e.SubmitedById)
            //.HasPrincipalKey( e=> e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.PublishedBy).WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Journal).WithMany(e => e.Articles)
            .HasForeignKey(e => e.JournalId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(e => e.Assets).WithOne(e => e.Article)
            .HasForeignKey(e => e.ArticleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.StageHistories).WithOne(e => e.Article)
            .HasForeignKey(e => e.ArticleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);


        //entity.ComplexProperty(
        //   e => e.ReadOnlyData, ro =>
        //   {
        //       ro.Property(a => a.Title).HasMaxLength(500).IsRequired();
        //       ro.Property(a => a.Type).HasMaxLength(50).IsRequired();
        //       ro.Property(a => a.Doi).HasMaxLength(50).IsRequired();
        //       ro.Property(a => a.SubmissionDate).IsRequired();
        //       //ro.Property(a => a.SubmissionUser).HasMaxLength(50).IsRequired();
        //       ro.Property(a => a.AcceptedOn).IsRequired();
        //   });
    }
}
