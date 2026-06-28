using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Domain.Aggregates.ServicoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Mappings;

[ExcludeFromCodeCoverage]
public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("Servicos");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.PrecoMaoDeObra)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
    }
}
