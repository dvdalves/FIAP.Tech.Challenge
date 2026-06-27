using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FIAP.Tech.Challenge.Domain.Aggregates.OrdemServicoAggregate;
using FIAP.Tech.Challenge.Domain.Aggregates.PecaAggregate;

namespace FIAP.Tech.Challenge.Infrastructure.Data.Mappings;

public class ItemOrdemServicoConfiguration : IEntityTypeConfiguration<ItemOrdemServico>
{
    public void Configure(EntityTypeBuilder<ItemOrdemServico> builder)
    {
        builder.ToTable("ItensOrdemServico");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Descricao)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(i => i.Quantidade)
            .IsRequired();

        builder.Property(i => i.ValorUnitario)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.ValorMaoDeObra)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // Relacionamentos opcionais
        builder.HasOne<Peca>()
            .WithMany()
            .HasForeignKey(i => i.PecaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
