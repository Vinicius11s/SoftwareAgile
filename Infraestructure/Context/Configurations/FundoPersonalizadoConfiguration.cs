using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Context.Configurations
{
    public class FundoPersonalizadoConfiguration : IEntityTypeConfiguration<FundoPersonalizado>
    {
        public void Configure(EntityTypeBuilder<FundoPersonalizado> builder)
        {
            builder.ToTable("FundosPersonalizados");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.CaminhoImagem)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.NomeArquivo)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(f => f.DataUpload)
                .IsRequired();

            builder.Property(f => f.UsuarioId)
                .IsRequired();

            builder.Property(f => f.TipoImpressao)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(f => f.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Relacionamento com Usuario
            builder.HasOne(f => f.Usuario)
                .WithMany()
                .HasForeignKey(f => f.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            builder.HasIndex(f => f.UsuarioId);
            builder.HasIndex(f => new { f.UsuarioId, f.Ativo });
            builder.HasIndex(f => new { f.UsuarioId, f.TipoImpressao, f.Ativo });
        }
    }
}
