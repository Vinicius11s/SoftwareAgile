using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Context.Configurations
{
    public class HistoricoCorrecoesConfiguration : IEntityTypeConfiguration<HistoricoCorrecoesEntity>
    {
        public void Configure(EntityTypeBuilder<HistoricoCorrecoesEntity> builder)
        {
            builder.ToTable("HistoricoCorrecoes");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.TextoOriginal)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.TextoCorrigido)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.TipoCorrecao)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(h => h.UsuarioId)
                .HasMaxLength(100);

            builder.Property(h => h.EmpresaId)
                .HasMaxLength(100);

            builder.Property(h => h.SessaoId)
                .HasMaxLength(100);

            builder.Property(h => h.IpAddress)
                .HasMaxLength(100);

            builder.Property(h => h.UserAgent)
                .HasMaxLength(500);

            // Índices para performance
            builder.HasIndex(h => new { h.UsuarioId, h.EmpresaId });
            builder.HasIndex(h => h.DataCorrecao);
            builder.HasIndex(h => h.TipoCorrecao);
            builder.HasIndex(h => h.SessaoId);

            // Configurações de valor padrão
            builder.Property(h => h.DataCorrecao)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}















