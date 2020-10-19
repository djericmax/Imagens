using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Imagens
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CarregarGrid();
        }

        /// <summary>
        /// método para carregar as informações do banco de dados no grid.
        /// </summary>
        private void CarregarGrid()
        {
            try
            {
                using (var conn = AbrirConexao())
                {
                    conn.Open();
                    using (var comm = conn.CreateCommand())
                    {
                        comm.CommandText = "SELECT * FROM imagens";
                        var reader = comm.ExecuteReader();
                        var table = new DataTable();
                        table.Load(reader);
                        dgvMostra.DataSource = table;
                        ConfigView();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        /// <summary>
        /// método para escolher um arquivo para ser salvo
        /// </summary>
        /// <returns>Nome do arquivo a ser salvo.</returns>
        private string EscolherArquivo()
        {
            var retorno = string.Empty;
            using (var dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    retorno = dialog.FileName;
                }
            }
            return retorno;
        }

        /// <summary>
        /// método para salvar o arquivo novo
        /// </summary>
        private void Salvar()
        {
            try
            {
                var arquivo = EscolherArquivo();
                if (!string.IsNullOrWhiteSpace(arquivo))
                {
                    SalvarArquivo(arquivo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// método para salvar o arquivo, este executa o processo completo.
        /// </summary>
        /// <param name="arquivo"></param>
        private void SalvarArquivo(string arquivo)
        {
            using (var conn = AbrirConexao())
            {
                conn.Open();
                using (var comm = conn.CreateCommand())
                {
                    comm.CommandText = "INSERT INTO imagens(nomeArquivo, arquivo)values(@nomeArquivo, @arquivo)";
                    ConfigurarParametrosSalvar(comm, arquivo);
                    comm.ExecuteNonQuery();
                    CarregarGrid();
                }
            }
        }

        /// <summary>
        /// configura os parâmetros a serem salvos, não afetando quando mudar o banco de dados.
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="arquivo"></param>
        private void ConfigurarParametrosSalvar(IDbCommand comm, string arquivo)
        {
            comm.Parameters.Add(new SqlParameter("nomeArquivo", Path.GetFileName(arquivo)));
            comm.Parameters.Add(new SqlParameter("arquivo", File.ReadAllBytes(arquivo)));
        }

        /// <summary>
        /// método para abrir o arquivo, este executa o processo completo.
        /// </summary>
        private void Abrir()
        {
            try
            {
                using (var conn = AbrirConexao())
                {
                    conn.Open();
                    using (var comm = conn.CreateCommand())
                    {
                        comm.CommandText = "select arquivo from imagens where (id = @id)";
                        ConfigurarParametrosAbrir(comm);
                        var bytes = comm.ExecuteScalar() as byte[];
                        if (bytes != null)
                        {
                            var nomeArquivo = dgvMostra.CurrentRow.Cells["nomeArquivo"].Value.ToString();
                            var arquivoTemp = Path.GetTempFileName();
                            arquivoTemp = Path.ChangeExtension(arquivoTemp, Path.GetExtension(nomeArquivo));
                            File.WriteAllBytes(arquivoTemp, bytes);
                            Process.Start(arquivoTemp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        /// <summary>
        /// configura os parâmetros para abrir um arquivo existente e salvo no banco de dados, não afeta a alteração de banco.
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="arquivo"></param>
        private void ConfigurarParametrosAbrir(IDbCommand comm)
        {
            comm.Parameters.Add(new SqlParameter("id", dgvMostra.CurrentRow.Cells["id"].Value)); 
        }

        /// <summary>
        /// método para iniciar e abrir a conexão. é o endereço do banco de dados.
        /// </summary>
        /// <returns></returns>
        private IDbConnection AbrirConexao()
        {
            return new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Eric\Desktop\Imagens\BcoImagens.mdf;Integrated Security=True");
        }

        private void Excluir()
        {
            if (MessageBox.Show("Deseja excluir este registro?","Salvando imagens",MessageBoxButtons.YesNo)==DialogResult.Yes)
            {
                ExcluirArquivo();
            }
        }

        private void ExcluirArquivo()
        {
            try
            {
                using (var conn = AbrirConexao())
                {
                    conn.Open();
                    using (var comm = conn.CreateCommand())
                    {
                        comm.CommandText = "delete from imagens where (id = @id)";
                        ConfigurarParametrosExcluir(comm);
                        comm.ExecuteNonQuery();
                        CarregarGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ConfigurarParametrosExcluir(IDbCommand comm)
        {
            comm.Parameters.Add(new SqlParameter("id", dgvMostra.CurrentRow.Cells["id"].Value));
        }

        /// <summary>
        /// botão SALVAR
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            Salvar();
        }

        /// <summary>
        /// Botão Abrir
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAbrir_Click(object sender, EventArgs e)
        {
            Abrir();
        }

        /// <summary>
        /// Botão Sair
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSair_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja sair da aplicação","Salvando imagens",MessageBoxButtons.YesNo)==DialogResult.Yes)
            {
                Application.Exit();
            }            
        }

        /// <summary>
        /// Botão excluir.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExcluir_Click(object sender, EventArgs e)
        {
            Excluir();
        }

        /// <summary>
        /// Configurar a largura e o cabeçalho das colunas.
        /// </summary>
        public void ConfigView()
        {
            dgvMostra.Columns[0].HeaderText = "ID";
            dgvMostra.Columns[1].HeaderText = "Nome da Imagem";
            dgvMostra.Columns[0].Width = 35;
            dgvMostra.Columns[1].Width = 230;
            dgvMostra.Columns[2].Visible= false;
        }

        public void MudarImgGrd()
        {
            try
            {
                MemoryStream image = new MemoryStream((byte[])dgvMostra.CurrentRow.Cells[2].Value‌​);
                ptbImagem.BackgroundImage = System.Drawing.Image.FromStream(image);
            }
            catch (Exception)
            {

            }
        }

        private void dgvMostra_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            MudarImgGrd();
        }
    }
}
