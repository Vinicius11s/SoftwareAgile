using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructure.Context.Configurations
{
    public class ImagemProdutoConfiguration : IEntityTypeConfiguration<ImagemProduto>
    {
        public void Configure(EntityTypeBuilder<ImagemProduto> builder)
        {
            builder.ToTable("ImagensProduto");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.CodigoBarras)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.CaminhoImagem)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.NomeArquivo)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.UrlOrigem)
                .HasMaxLength(1000);

            builder.Property(i => i.FonteImagem)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.DataBusca)
                .IsRequired();

            builder.Property(i => i.DataUpload)
                .IsRequired();

            builder.Property(i => i.UsuarioId)
                .IsRequired();

            builder.Property(i => i.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(i => i.TamanhoArquivo)
                .IsRequired();

            builder.Property(i => i.TipoArquivo)
                .IsRequired()
                .HasMaxLength(10);

            // Relacionamento com Usuario
            builder.HasOne(i => i.Usuario)
                .WithMany()
                .HasForeignKey(i => i.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para performance
            builder.HasIndex(i => i.CodigoBarras);
            builder.HasIndex(i => i.UsuarioId);
            builder.HasIndex(i => new { i.UsuarioId, i.Ativo });
            builder.HasIndex(i => new { i.CodigoBarras, i.UsuarioId, i.Ativo });
            builder.HasIndex(i => i.DataBusca);
        }
    }
}

