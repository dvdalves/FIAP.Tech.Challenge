using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Mappings;

[ExcludeFromCodeCoverage]
public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("Veiculos");

        builder.HasKey(v => v.Id);

        // Mapeamento do Value Object Placa via Conversor
        builder.Property(v => v.Placa)
            .HasConversion(
                placa => placa.Valor,
                valor => new Placa(valor)
            )
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(v => v.Marca)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Modelo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(v => v.Ano)
            .IsRequired();

        // Relacionamento com Cliente
        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índice único para a Placa
        builder.HasIndex(v => v.Placa)
            .IsUnique();
    }
}