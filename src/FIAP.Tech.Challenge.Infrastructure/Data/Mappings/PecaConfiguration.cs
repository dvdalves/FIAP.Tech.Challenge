using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Mappings;

public class PecaConfiguration : IEntityTypeConfiguration<Peca>
{
    public void Configure(EntityTypeBuilder<Peca> builder)
    {
        builder.ToTable("Pecas");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Preco)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.QuantidadeEstoque)
            .IsRequired();

        // Seed inicial do catálogo de peças em estoque
        builder.HasData(
            new Peca(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Filtro de Óleo", 45.90m, 15),
            new Peca(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Pastilha de Freio", 180.00m, 8),
            new Peca(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Vela de Ignição", 25.50m, 40)
        );
    }
}
