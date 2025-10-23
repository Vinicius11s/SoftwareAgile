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
    public class CorrecaoAprendidaConfiguration : IEntityTypeConfiguration<CorrecaoAprendidaEntity>
    {
        public void Configure(EntityTypeBuilder<CorrecaoAprendidaEntity> builder)
        {
            builder.ToTable("CorrecoesAprendidas");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.TextoOriginal)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.TextoCorrigido)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.TipoCorrecao)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.UsuarioId)
                .HasMaxLength(100);

            builder.Property(c => c.EmpresaId)
                .HasMaxLength(100);

            builder.Property(c => c.UsuarioAtualizacao)
                .HasMaxLength(100);

            // Índices para performance
            builder.HasIndex(c => new { c.UsuarioId, c.EmpresaId, c.TipoCorrecao });
            builder.HasIndex(c => c.TextoOriginal);
            builder.HasIndex(c => c.Ativo);
            builder.HasIndex(c => c.DataCriacao);

            // Configurações de valor padrão
            builder.Property(c => c.FrequenciaUso)
                .HasDefaultValue(1);

            builder.Property(c => c.DataCriacao)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.UltimaUtilizacao)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.DataAtualizacao)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.Ativo)
                .HasDefaultValue(true);
        }
    }
}















