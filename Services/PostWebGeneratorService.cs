using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Domain.DTOs;

namespace Services
{
    public class PostWebGeneratorService
    {
        public byte[] GerarPostWeb(PostWebDTO post, string fundoSelecionado, string webRootPath)
        {
            // Carregar template de fundo
            var templatePath = Path.Combine(webRootPath, "fundos", "padraoPostWEB.png");
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Template de fundo não encontrado");

            using var template = new Bitmap(templatePath);
            using var graphics = Graphics.FromImage(template);
            
            // Configurar qualidade de renderização
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            // Definir posições dos quadros pretos no template
            var quadros = ObterPosicoesQuadros(template.Width, template.Height);
            
            // Aplicar imagem do produto no primeiro quadro disponível
            if (post.TemImagem && post.ImagemBytes != null)
            {
                var quadro = quadros.FirstOrDefault();
                if (quadro != null)
                {
                    AplicarImagemNoQuadro(graphics, post.ImagemBytes, quadro, post.Preco);
                }
            }

            // Converter para byte array
            using var memoryStream = new MemoryStream();
            template.Save(memoryStream, ImageFormat.Png);
            return memoryStream.ToArray();
        }

        private List<QuadroTemplate> ObterPosicoesQuadros(int templateWidth, int templateHeight)
        {
            // Posições dos quadros pretos baseadas no template
            // Ajustar essas coordenadas conforme o template real
            return new List<QuadroTemplate>
            {
                new QuadroTemplate
                {
                    X = 50,  // Posição X do primeiro quadro
                    Y = 200, // Posição Y do primeiro quadro
                    Width = 200,  // Largura do quadro
                    Height = 200, // Altura do quadro
                    LabelX = 200, // Posição X do label de preço
                    LabelY = 350  // Posição Y do label de preço
                },
                new QuadroTemplate
                {
                    X = 300,
                    Y = 200,
                    Width = 200,
                    Height = 200,
                    LabelX = 450,
                    LabelY = 350
                },
                new QuadroTemplate
                {
                    X = 50,
                    Y = 450,
                    Width = 200,
                    Height = 200,
                    LabelX = 200,
                    LabelY = 600
                },
                new QuadroTemplate
                {
                    X = 300,
                    Y = 450,
                    Width = 200,
                    Height = 200,
                    LabelX = 450,
                    LabelY = 600
                }
            };
        }

        private void AplicarImagemNoQuadro(Graphics graphics, byte[] imagemBytes, QuadroTemplate quadro, decimal preco)
        {
            using var imagemStream = new MemoryStream(imagemBytes);
            using var imagemProduto = Image.FromStream(imagemStream);
            
            // Redimensionar imagem para caber no quadro
            var imagemRedimensionada = RedimensionarImagem(imagemProduto, quadro.Width, quadro.Height);
            
            // Aplicar imagem no quadro
            graphics.DrawImage(imagemRedimensionada, quadro.X, quadro.Y, quadro.Width, quadro.Height);
            
            // Adicionar label de preço
            AdicionarLabelPreco(graphics, preco, quadro.LabelX, quadro.LabelY);
        }

        private Image RedimensionarImagem(Image imagemOriginal, int largura, int altura)
        {
            var ratioX = (double)largura / imagemOriginal.Width;
            var ratioY = (double)altura / imagemOriginal.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var novaLargura = (int)(imagemOriginal.Width * ratio);
            var novaAltura = (int)(imagemOriginal.Height * ratio);

            var imagemRedimensionada = new Bitmap(novaLargura, novaAltura);
            using var graphics = Graphics.FromImage(imagemRedimensionada);
            
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            
            graphics.DrawImage(imagemOriginal, 0, 0, novaLargura, novaAltura);
            
            return imagemRedimensionada;
        }

        private void AdicionarLabelPreco(Graphics graphics, decimal preco, int x, int y)
        {
            // Configurar fonte para o preço
            using var fonte = new Font("Arial", 14, FontStyle.Bold);
            using var brush = new SolidBrush(Color.White);
            
            // Formatar preço
            var precoFormatado = preco.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
            
            // Desenhar fundo do label (retângulo escuro)
            using var brushFundo = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            var rectFundo = new Rectangle(x - 5, y - 5, 100, 30);
            graphics.FillRectangle(brushFundo, rectFundo);
            
            // Desenhar texto do preço
            graphics.DrawString(precoFormatado, fonte, brush, x, y);
        }
    }

    public class QuadroTemplate
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int LabelX { get; set; }
        public int LabelY { get; set; }
    }
}
