using System.Diagnostics.CodeAnalysis;
using FIAP.Tech.Challenge.Domain.Aggregates.ClienteAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.VeiculoAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Mappings;

[ExcludeFromCodeCoverage]
public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("OrdensServico");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.DescricaoProblema)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.ValorTotal)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Salvar Enum como string no banco para facilitar leitura
        builder.Property(o => o.Status)
            .HasConversion(
                status => status.ToString(),
                valor => (StatusOrdemServico)Enum.Parse(typeof(StatusOrdemServico), valor)
            )
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(o => o.DataCriacao)
            .IsRequired();

        builder.Property(o => o.DataInicioExecucao)
            .IsRequired(false);

        builder.Property(o => o.DataFinalizacao)
            .IsRequired(false);

        // Relacionamentos
        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(o => o.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Veiculo>()
            .WithMany()
            .HasForeignKey(o => o.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento um para muitos com os itens do orçamento
        builder.HasMany(o => o.Itens)
            .WithOne()
            .HasForeignKey(i => i.OrdemServicoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Habilitar acesso ao backing field _itens
        builder.Metadata.FindNavigation(nameof(OrdemServico.Itens))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}