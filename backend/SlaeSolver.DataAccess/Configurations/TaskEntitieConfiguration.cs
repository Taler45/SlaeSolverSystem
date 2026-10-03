using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlaeSolver.Domain.Entities;

namespace SlaeSolver.DataAccess.Configurations;

public class TaskEntitieConfiguration: IEntityTypeConfiguration<TaskEntitie>
{
    public void Configure(EntityTypeBuilder<TaskEntitie> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ParameterFilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.MatrixSize)
            .IsRequired();

        builder.ToTable("Tasks", t =>
            {
                t.HasCheckConstraint("CK_Matrix_Size", "\"MatrixSize\">0");
                t.HasCheckConstraint("CK_Progress_p", "\"Progress\">0 and \"Progress\"<=100");
            }
        );

        builder.Property(t => t.ServerId)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Progress)
            .IsRequired();
        builder.ToTable("Tasks", t =>
            t.HasCheckConstraint("CK_Progress", "\"Progress\">0 and \"Progress\"<101")
        );
        
        builder.Property(t => t.IsCanceled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.ResultFilePath)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.StartedAt)
            .IsRequired(false);

        builder.Property(t => t.CompletedAt)
            .IsRequired(false);
    }
}