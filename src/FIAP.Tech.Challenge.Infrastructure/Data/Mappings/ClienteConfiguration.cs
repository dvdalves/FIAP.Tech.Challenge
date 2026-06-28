using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.ValueObjects;

using System.Diagnostics.CodeAnalysis;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Mappings;

[ExcludeFromCodeCoverage]
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        // Mapeamento do Value Object Cpf via Conversor
        builder.Property(c => c.Cpf)
            .HasConversion(
                cpf => cpf.Valor,
                valor => new Cpf(valor)
            )
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Telefone)
            .IsRequired()
            .HasMaxLength(20);

        // Índice único para o CPF
        builder.HasIndex(c => c.Cpf)
            .IsUnique();
    }
}
