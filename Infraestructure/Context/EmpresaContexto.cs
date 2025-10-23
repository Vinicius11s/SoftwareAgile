using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infraestructure.Context
{
    public class EmpresaContexto : DbContext
    {
        public EmpresaContexto(DbContextOptions<EmpresaContexto> options) : base(options)
        {
        }

        protected EmpresaContexto()
        {
        }

        public DbSet<Oferta> Ofertas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<FundoPersonalizado> FundosPersonalizados { get; set; }
        public DbSet<CorrecaoAprendidaEntity> CorrecoesAprendidas { get; set; }
        public DbSet<HistoricoCorrecoesEntity> HistoricoCorrecoes { get; set; }
        public DbSet<ImagemProduto> ImagensProduto { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configuração via Program.cs -> AddDbContext + appsettings.json
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplica automaticamente todas as Configurations do assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
