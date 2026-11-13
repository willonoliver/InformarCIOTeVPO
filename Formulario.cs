using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using InformarCIOTeVPO.Models;

namespace InformarCIOTeVPO;

/// <summary>
/// Formulario principal da aplicação - Editor de CIOT e Vale Pedágio MDFe
/// 
/// Esta classe gerencia a interface gráfica para:
/// - Carregar e editar arquivos XML de Manifesto Eletrônico de Documentos Fiscais (MDFe)
/// - Gerenciar informações de CIOT (Conhecimento de Transporte Interestadual Oneroso)
/// - Editar dados de Vale Pedágio
/// - Navegar entre múltiplos dispositivos (fornecedores)
/// - Validar e salvar alterações em formato XML
/// 
/// Funcionalidades principais:
/// - Carregamento de arquivos XML via diálogo ou drag-and-drop
/// - Visualização de informações do MDFe (número, data, percurso, emitente)
/// - Edição e validação de dados CIOT
/// - Gerenciamento de dispositivos com botões de navegação
/// - Atalhos de teclado para operações comuns
/// - Detecção automática de alterações não salvas
/// </summary>
public class Formulario : Form
{
    /// <summary>
    /// Campos e variáveis de estado da aplicação
    /// </summary>

    /// <summary>Caminho do arquivo XML carregado</summary>
    private string? _xmlFilePath;

    /// <summary>Documento XML carregado em memória</summary>
    private XDocument? _xmlDocument;

    /// <summary>Namespace XML do documento MDFe</summary>
    private XNamespace? _ns;

    /// <summary>Lista de dispositivos (fornecedores de vale pedágio) gerenciados</summary>
    private List<Disp> _dispositivos = new List<Disp>();

    /// <summary>Índice do dispositivo atualmente exibido</summary>
    private int _dispositivoAtualIndex;

    /// <summary>Flag indicando se há alterações não salvas no documento</summary>
    private bool _alteracoesNaoSalvas;

    /// <summary>Flag para suprimir detecção de alterações durante preenchimento programático</summary>
    private bool _carregandoDados;

    /// <summary>
    /// CONTROLES - Seção de Arquivo
    /// Interface para carregar e selecionar arquivos XML
    /// </summary>
    private GroupBox groupBoxArquivo = null!;
    private Button btnCarregar = null!;
    private TextBox txtArquivo = null!;
    private Button btnProcurar = null!;

    /// <summary>
    /// CONTROLES - Informações do MDFe
    /// Exibição de informações sobre o Manifesto Eletrônico de Documentos Fiscais
    /// </summary>
    private GroupBox groupBoxInfoMDFe = null!;
    private Label lblInfoMDFeVazio = null!;
    private Label lblNumero = null!;
    private Label lblNumeroValor = null!;
    private Label lblDataEmissao = null!;
    private Label lblDataEmissaoValor = null!;
    private Label lblPercurso = null!;
    private Label lblPercursoValor = null!;
    private Label lblEmitente = null!;
    private Label lblEmitenteValor = null!;
    private Label lblChave = null!;
    private Label lblChaveValor = null!;

    // CIOT
    private GroupBox groupBoxCIOT = null!;
    private Label lblCIOT = null!;
    private MaskedTextBox txtCIOT = null!;
    private Label lblCNPJCIOT = null!;
    private MaskedTextBox txtCNPJCIOT = null!;
    private Label lblCPFCIOT = null!;
    private MaskedTextBox txtCPFCIOT = null!;

    // Vale Pedágio
    private GroupBox groupBoxValePed = null!;
    private ComboBox cmbCategCombVeic = null!;
    private Label label1 = null!;
    private GroupBox groupBoxDispositivo = null!;
    private MaskedTextBox txtCNPJForn = null!;
    private Label label2 = null!;
    private MaskedTextBox txtCNPJPg = null!;
    private Label label3 = null!;
    private MaskedTextBox txtCPFPg = null!;
    private Label label4 = null!;
    private TextBox txtNCompra = null!;
    private Label label5 = null!;
    private TextBox txtVValePed = null!;
    private Label label6 = null!;
    private ComboBox cmbTpValePed = null!;
    private Label label7 = null!;
    private Panel panelNavegacao = null!;
    private Label lblDispAtual = null!;
    private Button btnAnterior = null!;
    private Button btnProximo = null!;
    private Button btnAdicionar = null!;
    private Button btnRemover = null!;

    private Button btnSalvar = null!;
    private StatusStrip statusStrip = null!;
    private Panel pnlValePed;
    private ToolStripStatusLabel toolStripStatusLabel = null!;

    /// <summary>
    /// Construtor do formulário. Inicializa todos os componentes e configura os comportamentos
    /// </summary>
    public Formulario()
    {
        InitializeComponent();
        InitializeComboBoxes();
        ConfigurarEventosPagador();
        ConfigurarEventosCIOT();
        ConfigurarDragDrop();
        ConfigurarAtalhosTeclado();
        ConfigurarTooltips();
        ConfigurarEventosAlteracao();
        FormClosing += Formulario_FormClosing;
    }

    private void ConfigurarEventosAlteracao()
    {
        EventHandler handler = (_, _) =>
        {
            if (_carregandoDados) return;

            if (!_alteracoesNaoSalvas)
            {
                _alteracoesNaoSalvas = true;
                AtualizarTitulo();
            }
        };

        txtCIOT.TextChanged += handler;
        txtCNPJCIOT.TextChanged += handler;
        txtCPFCIOT.TextChanged += handler;
        txtCNPJForn.TextChanged += handler;
        txtCNPJPg.TextChanged += handler;
        txtCPFPg.TextChanged += handler;
        txtNCompra.TextChanged += handler;
        txtVValePed.TextChanged += handler;
        cmbTpValePed.SelectedIndexChanged += handler;
        cmbCategCombVeic.SelectedIndexChanged += handler;
    }

    private void AtualizarTitulo()
    {
        string text = "Editor de CIOT e Vale Pedágio MDFe";
        if (!string.IsNullOrEmpty(_xmlFilePath))
            text = Path.GetFileName(_xmlFilePath) + " - " + text;
        if (_alteracoesNaoSalvas)
            text += " *";
        Text = text;
    }

    private void CarregarInformacoesMDFe()
    {
        // Limpa os dados anteriores
        lblNumeroValor.Text = "-";
        lblDataEmissaoValor.Text = "-";
        lblPercursoValor.Text = "-";
        lblEmitenteValor.Text = "-";
        lblChaveValor.Text = "-";

        if (_xmlDocument == null)
        {
            ExibirEstadoInfoMDFe(
                false,
                "Informe um XML acima para começarmos.");

            return;
        }

        try
        {
            var infMDFe = GetDescendant("infMDFe");

            if (infMDFe == null)
            {
                ExibirEstadoInfoMDFe(
                    false,
                    "Não foi possível localizar as informações do MDF-e no XML.");

                return;
            }

            // Chave de acesso
            var idAttr = infMDFe.Attribute("Id");

            if (idAttr != null && !string.IsNullOrEmpty(idAttr.Value))
            {
                string chave = idAttr.Value.Replace("MDFe", "");
                lblChaveValor.Text = FormatarChaveAcesso(chave);
            }

            // Informações de identificação
            var ide = GetElement(infMDFe, "ide");

            if (ide != null)
            {
                string nMDF = GetElementValue(ide, "nMDF");
                string serie = GetElementValue(ide, "serie");

                lblNumeroValor.Text = $"{nMDF} / Série {serie}";

                string dhEmi = GetElementValue(ide, "dhEmi");

                if (!string.IsNullOrWhiteSpace(dhEmi) &&
                    DateTimeOffset.TryParse(
                        dhEmi,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dt))
                {
                    lblDataEmissaoValor.Text =
                        dt.ToString("dd/MM/yyyy HH:mm");
                }

                string ufIni = GetElementValue(ide, "UFIni");
                string ufFim = GetElementValue(ide, "UFFim");

                if (!string.IsNullOrWhiteSpace(ufIni) &&
                    !string.IsNullOrWhiteSpace(ufFim))
                {
                    lblPercursoValor.Text = $"{ufIni} → {ufFim}";
                }
            }

            // Informações do emitente
            var emit = GetElement(infMDFe, "emit");

            if (emit != null)
            {
                string cnpj = GetElementValue(emit, "CNPJ");
                string xNome = GetElementValue(emit, "xNome");

                if (!string.IsNullOrEmpty(cnpj))
                    cnpj = FormatarCNPJ(cnpj);

                lblEmitenteValor.Text = !string.IsNullOrEmpty(xNome)
                    ? $"{xNome} - CNPJ: {cnpj}"
                    : $"CNPJ: {cnpj}";
            }

            ExibirEstadoInfoMDFe(true);
        }
        catch (Exception ex)
        {
            ExibirEstadoInfoMDFe(
                false,
                "Erro ao carregar informações do MDF-e: " + ex.Message);

            toolStripStatusLabel.Text =
                "Erro ao carregar informações do MDFe: " + ex.Message;
        }
    }

    private void ExibirEstadoInfoMDFe(bool exibirDados, string? mensagem = null)
    {
        groupBoxInfoMDFe.Visible = true;

        lblNumero.Visible = exibirDados;
        lblNumeroValor.Visible = exibirDados;

        lblDataEmissao.Visible = exibirDados;
        lblDataEmissaoValor.Visible = exibirDados;

        lblPercurso.Visible = exibirDados;
        lblPercursoValor.Visible = exibirDados;

        lblEmitente.Visible = exibirDados;
        lblEmitenteValor.Visible = exibirDados;

        lblChave.Visible = exibirDados;
        lblChaveValor.Visible = exibirDados;

        lblInfoMDFeVazio.Text = mensagem ?? string.Empty;
        lblInfoMDFeVazio.Visible = !exibirDados;

        if (!exibirDados)
            lblInfoMDFeVazio.BringToFront();
    }

    private string FormatarChaveAcesso(string chave)
    {
        if (string.IsNullOrEmpty(chave) || chave.Length != 44)
            return chave;
        return string.Join(" ", Enumerable.Range(0, 11).Select(i => chave.Substring(i * 4, 4)));
    }

    private void Formulario_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_alteracoesNaoSalvas && _xmlDocument != null)
        {
            var result = MessageBox.Show(
                "Existem alterações não salvas. Deseja sair mesmo assim?",
                "Alterações Não Salvas",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (result == DialogResult.No)
                e.Cancel = true;
        }
    }

    /// <summary>
    /// Configura suporte a Drag-and-drop de arquivos XML
    /// 
    /// Permite que o usuário arraste arquivos XML do Windows Explorer para a
    /// caixa de texto de arquivo, facilitando o carregamento de documentos.
    /// </summary>
    private void ConfigurarDragDrop()
    {
        AllowDrop = true;
        txtArquivo.AllowDrop = true;

        // Evento disparado quando algo é arrastado sobre o formulário
        DragEnter += (_, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        };

        // Evento disparado quando algo é solto no formulário
        DragDrop += (_, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                // Valida se é um arquivo XML
                if (files[0].EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    txtArquivo.Text = files[0];
                    btnCarregar.PerformClick();
                }
                else
                {
                    MessageBox.Show("Por favor, arraste apenas arquivos XML.", "Aviso",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        };
    }

    private void ConfigurarAtalhosTeclado()
    {
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F5 && !string.IsNullOrEmpty(txtArquivo.Text))
            {
                btnCarregar.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.S && btnSalvar.Enabled)
            {
                btnSalvar.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.N && groupBoxValePed.Enabled)
            {
                btnAdicionar.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Left && btnAnterior.Enabled)
            {
                btnAnterior.PerformClick();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Right && btnProximo.Enabled)
            {
                btnProximo.PerformClick();
                e.Handled = true;
            }
        };
    }

    private void ConfigurarTooltips()
    {
        var tip = new ToolTip
        {
            AutoPopDelay = 5000,
            InitialDelay = 500,
            ReshowDelay = 500,
            ShowAlways = true
        };

        tip.SetToolTip(txtCIOT, "Código Identificador da Operação de Transporte (CIOT)\n12 dígitos numéricos");
        tip.SetToolTip(txtCNPJCIOT, "CNPJ do responsável pelo CIOT\nPreencher apenas se for pessoa jurídica");
        tip.SetToolTip(txtCPFCIOT, "CPF do responsável pelo CIOT\nPreencher apenas se for pessoa física");
        tip.SetToolTip(txtCNPJForn, "CNPJ da empresa fornecedora do Vale-Pedágio (14 dígitos)\nDeve estar cadastrado na base da ANTT");
        tip.SetToolTip(txtCNPJPg, "CNPJ do responsável pelo pagamento\nPreencher apenas se diferente do emitente do MDFe");
        tip.SetToolTip(txtCPFPg, "CPF do responsável pelo pagamento\nPreencher apenas se diferente do emitente do MDFe");
        tip.SetToolTip(txtNCompra, "Identificador do Vale Pedagio Obrigatório (IDVPO)\nDe 5 a 20 caracteres alfanuméricos");
        tip.SetToolTip(txtVValePed, "Valor necessário para livre circulação\nFormato: 1.234,56 (exibição) → XML: 1234.56");
        tip.SetToolTip(cmbTpValePed, "01 = TAG\n04 = Leitura de placa");
        tip.SetToolTip(cmbCategCombVeic, "Categoria de combinação veicular obrigatória\nDefine o tipo de veículo comercial");
        tip.SetToolTip(btnCarregar, "Carregar arquivo XML (F5)");
        tip.SetToolTip(btnSalvar, "Salvar alterações (Ctrl+S)");
        tip.SetToolTip(btnAdicionar, "Adicionar novo dispositivo (Ctrl+N)");
        tip.SetToolTip(btnAnterior, "Dispositivo anterior (Ctrl+Seta Esquerda)");
        tip.SetToolTip(btnProximo, "Próximo dispositivo (Ctrl+Seta Direita)");
    }

    private void ConfigurarEventosCIOT()
    {
        txtCNPJCIOT.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(txtCNPJCIOT.Text.Replace(".", "").Replace("/", "").Replace("-", "")))
            {
                txtCPFCIOT.Enabled = false;
                txtCPFCIOT.BackColor = SystemColors.Control;
            }
            else
            {
                txtCPFCIOT.Enabled = true;
                txtCPFCIOT.BackColor = SystemColors.Window;
            }
        };

        txtCPFCIOT.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(txtCPFCIOT.Text.Replace(".", "").Replace("-", "")))
            {
                txtCNPJCIOT.Enabled = false;
                txtCNPJCIOT.BackColor = SystemColors.Control;
            }
            else
            {
                txtCNPJCIOT.Enabled = true;
                txtCNPJCIOT.BackColor = SystemColors.Window;
            }
        };

        txtCNPJCIOT.Leave += (_, _) => ValidarCNPJEmTempoReal(txtCNPJCIOT, "CIOT");
        txtCPFCIOT.Leave += (_, _) => ValidarCPFEmTempoReal(txtCPFCIOT);
    }

    private void ConfigurarEventosPagador()
    {
        txtCNPJPg.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(txtCNPJPg.Text.Replace(".", "").Replace("/", "").Replace("-", "")))
            {
                txtCPFPg.Enabled = false;
                txtCPFPg.BackColor = SystemColors.Control;
            }
            else
            {
                txtCPFPg.Enabled = true;
                txtCPFPg.BackColor = SystemColors.Window;
            }
        };

        txtCPFPg.TextChanged += (_, _) =>
        {
            if (!string.IsNullOrWhiteSpace(txtCPFPg.Text.Replace(".", "").Replace("-", "")))
            {
                txtCNPJPg.Enabled = false;
                txtCNPJPg.BackColor = SystemColors.Control;
            }
            else
            {
                txtCNPJPg.Enabled = true;
                txtCNPJPg.BackColor = SystemColors.Window;
            }
        };

        txtCNPJForn.Leave += (_, _) => ValidarCNPJEmTempoReal(txtCNPJForn, "Fornecedor");
        txtCNPJPg.Leave += (_, _) => ValidarCNPJEmTempoReal(txtCNPJPg, "Pagador");
        txtCPFPg.Leave += (_, _) => ValidarCPFEmTempoReal(txtCPFPg);
    }

    private void ValidarCNPJEmTempoReal(MaskedTextBox textBox, string tipo)
    {
        string text = textBox.Text.Replace(".", "").Replace("/", "").Replace("-", "");
        if (string.IsNullOrWhiteSpace(text))
        {
            textBox.BackColor = SystemColors.Window;
        }
        else if (text.Length != 14)
        {
            textBox.BackColor = Color.LightPink;
            toolStripStatusLabel.Text = "CNPJ " + tipo + " deve ter 14 dígitos";
        }
        else if (!ValidarCNPJ(text))
        {
            textBox.BackColor = Color.LightYellow;
            toolStripStatusLabel.Text = "CNPJ " + tipo + " com dígito verificador inválido";
        }
        else
        {
            textBox.BackColor = Color.LightGreen;
            toolStripStatusLabel.Text = "CNPJ " + tipo + " válido";
        }
    }

    private void ValidarCPFEmTempoReal(MaskedTextBox textBox)
    {
        string text = textBox.Text.Replace(".", "").Replace("-", "");
        if (string.IsNullOrWhiteSpace(text))
        {
            textBox.BackColor = SystemColors.Window;
        }
        else if (text.Length != 11)
        {
            textBox.BackColor = Color.LightPink;
            toolStripStatusLabel.Text = "CPF deve ter 11 dígitos";
        }
        else if (!ValidarCPF(text))
        {
            textBox.BackColor = Color.LightYellow;
            toolStripStatusLabel.Text = "CPF com dígito verificador inválido";
        }
        else
        {
            textBox.BackColor = Color.LightGreen;
            toolStripStatusLabel.Text = "CPF válido";
        }
    }

    private bool ValidarCNPJ(string cnpj)
    {
        if (cnpj.Length != 14) return false;
        if (new string(cnpj[0], 14) == cnpj) return false;

        try
        {
            int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(cnpj[i].ToString()) * mult1[i];
            int resto = soma % 11;
            int dig1 = resto < 2 ? 0 : 11 - resto;

            int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(cnpj[i].ToString()) * mult2[i];
            resto = soma % 11;
            int dig2 = resto < 2 ? 0 : 11 - resto;

            return cnpj[12].ToString() == dig1.ToString() && cnpj[13].ToString() == dig2.ToString();
        }
        catch { return false; }
    }

    private bool ValidarCPF(string cpf)
    {
        if (cpf.Length != 11) return false;
        if (new string(cpf[0], 11) == cpf) return false;

        try
        {
            int soma = 0;
            for (int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            int resto = soma % 11;
            int dig1 = resto < 2 ? 0 : 11 - resto;

            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            resto = soma % 11;
            int dig2 = resto < 2 ? 0 : 11 - resto;

            return cpf[9].ToString() == dig1.ToString() && cpf[10].ToString() == dig2.ToString();
        }
        catch { return false; }
    }

    private void InitializeComboBoxes()
    {
        cmbCategCombVeic.Items.AddRange(new object[]
        {
            "02 - Veículo Comercial 2 eixos",
            "04 - Veículo Comercial 3 eixos",
            "06 - Veículo Comercial 4 eixos",
            "07 - Veículo Comercial 5 eixos",
            "08 - Veículo Comercial 6 eixos",
            "10 - Veículo Comercial 7 eixos",
            "11 - Veículo Comercial 8 eixos",
            "12 - Veículo Comercial 9 eixos",
            "13 - Veículo Comercial 10 eixos",
            "14 - Veículo Comercial Acima de 10 eixos"
        });

        cmbTpValePed.Items.AddRange(new object[]
        {
            "",
            "01 - TAG",
            "04 - Leitura de placa"
        });
    }

    private void btnProcurar_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "Arquivos XML (*.xml)|*.xml|Todos os arquivos (*.*)|*.*",
            Title = "Selecione o arquivo MDFe XML"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
            txtArquivo.Text = ofd.FileName;
    }

    private void btnCarregar_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(txtArquivo.Text))
        {
            MessageBox.Show("Por favor, selecione um arquivo XML.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            CarregarXML(txtArquivo.Text);
            toolStripStatusLabel.Text = "Arquivo carregado com sucesso";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao carregar o arquivo:\n" + ex.Message, "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            toolStripStatusLabel.Text = "Erro ao carregar arquivo";
        }
    }

    private void CarregarXML(string filePath)
    {
        _carregandoDados = true;
        try
        {
            _xmlFilePath = filePath;
            _xmlDocument = XDocument.Load(filePath);
            _ns = _xmlDocument.Root?.GetDefaultNamespace();
            _dispositivos.Clear();
            CarregarInformacoesMDFe();

            // Carregar CIOT
            var infANTT = GetDescendant("infANTT");
            if (infANTT != null)
            {
                var infCIOT = GetElement(infANTT, "infCIOT");
                if (infCIOT != null)
                {
                    txtCIOT.Text = GetElementValue(infCIOT, "CIOT");
                    txtCNPJCIOT.Text = GetElementValue(infCIOT, "CNPJ");
                    txtCPFCIOT.Text = GetElementValue(infCIOT, "CPF");
                }
                else
                {
                    txtCIOT.Text = "";
                    txtCNPJCIOT.Text = "";
                    txtCPFCIOT.Text = "";
                }
            }
            else
            {
                txtCIOT.Text = "";
                txtCNPJCIOT.Text = "";
                txtCPFCIOT.Text = "";
            }

            // Carregar Vale Pedágio
            cmbCategCombVeic.SelectedIndex = -1;

            if (infANTT == null)
            {
                MessageBox.Show("Elemento 'infANTT' não encontrado no XML.\nSerá criado ao salvar.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                InicializarNovoDispositivo();
                groupBoxCIOT.Enabled = true;
                groupBoxValePed.Enabled = true;
                btnSalvar.Enabled = true;
                return;
            }

            var valePed = GetElement(infANTT, "valePed");
            if (valePed != null)
            {
                var categ = GetElement(valePed, "categCombVeic");
                if (categ != null)
                {
                    string value = categ.Value;
                    for (int i = 0; i < cmbCategCombVeic.Items.Count; i++)
                    {
                        if ((cmbCategCombVeic.Items[i]?.ToString() ?? "").StartsWith(value + " "))
                        {
                            cmbCategCombVeic.SelectedIndex = i;
                            break;
                        }
                    }
                }

                foreach (var dispEl in GetElements(valePed, "disp"))
                {
                    decimal valor = 0m;
                    decimal.TryParse(GetElementValue(dispEl, "vValePed"), NumberStyles.Any, CultureInfo.InvariantCulture, out valor);

                    _dispositivos.Add(new Disp
                    {
                        CNPJForn = GetElementValue(dispEl, "CNPJForn"),
                        CNPJPg = NullIfEmpty(GetElementValue(dispEl, "CNPJPg")),
                        CPFPg = NullIfEmpty(GetElementValue(dispEl, "CPFPg")),
                        nCompra = NullIfEmpty(GetElementValue(dispEl, "nCompra")),
                        vValePed = valor,
                        tpValePed = NullIfEmpty(GetElementValue(dispEl, "tpValePed"))
                    });
                }
            }

            if (_dispositivos.Count == 0)
                InicializarNovoDispositivo();
            else
            {
                _dispositivoAtualIndex = 0;
                ExibirDispositivoAtual();
            }

            groupBoxCIOT.Enabled = true;
            groupBoxValePed.Enabled = true;
            btnSalvar.Enabled = true;
        }
        finally
        {
            _carregandoDados = false;
            _alteracoesNaoSalvas = false;
            AtualizarTitulo();
        }
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private void InicializarNovoDispositivo()
    {
        _dispositivos.Add(new Disp());
        _dispositivoAtualIndex = 0;
        ExibirDispositivoAtual();
    }

    private void ExibirDispositivoAtual()
    {
        if (_dispositivos.Count == 0 || _dispositivoAtualIndex < 0 || _dispositivoAtualIndex >= _dispositivos.Count)
            return;

        bool estadoAnterior = _carregandoDados;
        _carregandoDados = true;
        try
        {
            var disp = _dispositivos[_dispositivoAtualIndex];
            txtCNPJForn.Text = disp.CNPJForn;
            txtCNPJPg.Text = disp.CNPJPg ?? "";
            txtCPFPg.Text = disp.CPFPg ?? "";
            txtNCompra.Text = disp.nCompra ?? "";
            txtVValePed.Text = disp.vValePed > 0m ? disp.vValePed.ToString("N2", new CultureInfo("pt-BR")) : "";

            cmbTpValePed.SelectedIndex = 0;
            if (disp.tpValePed == "01") cmbTpValePed.SelectedIndex = 1;
            else if (disp.tpValePed == "04") cmbTpValePed.SelectedIndex = 2;

            AtualizarNavegacao();
        }
        finally
        {
            _carregandoDados = estadoAnterior;
        }
    }

    private void SalvarDispositivoAtual()
    {
        if (_dispositivos.Count == 0 || _dispositivoAtualIndex < 0 || _dispositivoAtualIndex >= _dispositivos.Count)
            return;

        var disp = _dispositivos[_dispositivoAtualIndex];
        disp.CNPJForn = txtCNPJForn.Text.Replace(".", "").Replace("/", "").Replace("-", "");
        disp.CNPJPg = string.IsNullOrWhiteSpace(txtCNPJPg.Text) ? null : txtCNPJPg.Text.Replace(".", "").Replace("/", "").Replace("-", "");
        disp.CPFPg = string.IsNullOrWhiteSpace(txtCPFPg.Text) ? null : txtCPFPg.Text.Replace(".", "").Replace("-", "");
        disp.nCompra = string.IsNullOrWhiteSpace(txtNCompra.Text) ? null : txtNCompra.Text;

        if (decimal.TryParse(txtVValePed.Text, NumberStyles.Any, new CultureInfo("pt-BR"), out var valor))
            disp.vValePed = valor;
        else
            disp.vValePed = 0m;

        string tp = cmbTpValePed.SelectedItem?.ToString() ?? "";
        if (string.IsNullOrEmpty(tp))
            disp.tpValePed = null;
        else if (tp.StartsWith("01"))
            disp.tpValePed = "01";
        else if (tp.StartsWith("04"))
            disp.tpValePed = "04";
    }

    private void AtualizarNavegacao()
    {
        lblDispAtual.Text = $"Dispositivo {_dispositivoAtualIndex + 1} de {_dispositivos.Count}";
        btnAnterior.Enabled = _dispositivoAtualIndex > 0;
        btnProximo.Enabled = _dispositivoAtualIndex < _dispositivos.Count - 1;
        btnRemover.Enabled = _dispositivos.Count > 1;
    }

    // Helpers de XML
    private XElement? GetDescendant(string localName)
    {
        if (_xmlDocument == null) return null;
        return _ns != null
            ? _xmlDocument.Descendants(_ns + localName).FirstOrDefault()
            : _xmlDocument.Descendants(localName).FirstOrDefault();
    }

    private XElement? GetElement(XElement parent, string localName)
    {
        return _ns != null
            ? parent.Element(_ns + localName)
            : parent.Element(localName);
    }

    private IEnumerable<XElement> GetElements(XElement parent, string localName)
    {
        return _ns != null
            ? parent.Elements(_ns + localName)
            : parent.Elements(localName);
    }

    private string GetElementValue(XElement parent, string elementName)
    {
        var el = GetElement(parent, elementName);
        return el?.Value ?? string.Empty;
    }

    private void btnAnterior_Click(object? sender, EventArgs e)
    {
        if (_dispositivoAtualIndex > 0)
        {
            SalvarDispositivoAtual();
            _dispositivoAtualIndex--;
            ExibirDispositivoAtual();
        }
    }

    private void btnProximo_Click(object? sender, EventArgs e)
    {
        if (_dispositivoAtualIndex < _dispositivos.Count - 1)
        {
            SalvarDispositivoAtual();
            _dispositivoAtualIndex++;
            ExibirDispositivoAtual();
        }
    }

    private void btnAdicionar_Click(object? sender, EventArgs e)
    {
        SalvarDispositivoAtual();
        _dispositivos.Add(new Disp());
        _dispositivoAtualIndex = _dispositivos.Count - 1;
        ExibirDispositivoAtual();
        _alteracoesNaoSalvas = true;
        AtualizarTitulo();
    }

    private void btnRemover_Click(object? sender, EventArgs e)
    {
        if (_dispositivos.Count <= 1)
        {
            MessageBox.Show("É necessário ter pelo menos um dispositivo.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"Deseja realmente remover o Dispositivo {_dispositivoAtualIndex + 1}?",
            "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _dispositivos.RemoveAt(_dispositivoAtualIndex);
            if (_dispositivoAtualIndex >= _dispositivos.Count)
                _dispositivoAtualIndex = _dispositivos.Count - 1;
            ExibirDispositivoAtual();
            _alteracoesNaoSalvas = true;
            AtualizarTitulo();
        }
    }

    private void btnSalvar_Click(object? sender, EventArgs e)
    {
        try
        {
            SalvarDispositivoAtual();
            if (ValidarDados() && ExibirConfirmacao())
            {
                SalvarXML();
                MessageBox.Show("Alterações salvas com sucesso!", "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                toolStripStatusLabel.Text = "Alterações salvas com sucesso";
                _alteracoesNaoSalvas = false;
                AtualizarTitulo();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao salvar alterações:\n" + ex.Message, "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            toolStripStatusLabel.Text = "Erro ao salvar alterações";
        }
    }

    private bool ExibirConfirmacao()
    {
        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════");
        sb.AppendLine("      CONFIRMAÇÃO DE DADOS");
        sb.AppendLine("═══════════════════════════════════════════");
        sb.AppendLine();

        // CIOT
        string ciot = txtCIOT.Text.Replace(".", "").Replace("-", "").Trim();
        if (!string.IsNullOrEmpty(ciot))
        {
            sb.AppendLine("CIOT: " + ciot);
            string cnpjCiot = txtCNPJCIOT.Text.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
            string cpfCiot = txtCPFCIOT.Text.Replace(".", "").Replace("-", "").Trim();
            if (!string.IsNullOrEmpty(cnpjCiot))
                sb.AppendLine("  CNPJ: " + FormatarCNPJ(cnpjCiot));
            if (!string.IsNullOrEmpty(cpfCiot))
                sb.AppendLine("  CPF: " + FormatarCPF(cpfCiot));
            sb.AppendLine();
        }

        // Vale Pedágio
        string categ = cmbCategCombVeic.SelectedItem?.ToString() ?? "";
        sb.AppendLine("Categoria Veicular: " + categ);
        sb.AppendLine();
        sb.AppendLine("Total de Dispositivos: " + _dispositivos.Count);
        sb.AppendLine("─────────────────────────────────────────");

        for (int i = 0; i < _dispositivos.Count; i++)
        {
            var d = _dispositivos[i];
            sb.AppendLine();
            sb.AppendLine($"DISPOSITIVO {i + 1}:");
            sb.AppendLine("  CNPJ Fornecedor: " + FormatarCNPJ(d.CNPJForn));
            if (!string.IsNullOrEmpty(d.CNPJPg))
                sb.AppendLine("  CNPJ Pagador: " + FormatarCNPJ(d.CNPJPg));
            if (!string.IsNullOrEmpty(d.CPFPg))
                sb.AppendLine("  CPF Pagador: " + FormatarCPF(d.CPFPg));
            if (!string.IsNullOrEmpty(d.nCompra))
                sb.AppendLine("  IDVPO: " + d.nCompra);
            sb.AppendLine($"  Valor: R$ {d.vValePed:N2}");
            if (!string.IsNullOrEmpty(d.tpValePed))
                sb.AppendLine("  Tipo: " + (d.tpValePed == "01" ? "TAG" : "Leitura de placa"));
        }

        sb.AppendLine();
        sb.AppendLine("═══════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("Confirma as informações acima?");

        return MessageBox.Show(sb.ToString(), "Confirmar Dados",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }

    private string FormatarCNPJ(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14) return cnpj;
        return $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";
    }

    private string FormatarCPF(string cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length != 11) return cpf;
        return $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";
    }

    private bool ValidarDados()
    {
        // Validação CIOT (opcional, mas se informado deve estar completo)
        string ciot = txtCIOT.Text.Replace(".", "").Replace("-", "").Trim();
        string cnpjCiot = txtCNPJCIOT.Text.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        string cpfCiot = txtCPFCIOT.Text.Replace(".", "").Replace("-", "").Trim();

        if (!string.IsNullOrEmpty(ciot) || !string.IsNullOrEmpty(cnpjCiot) || !string.IsNullOrEmpty(cpfCiot))
        {
            if (string.IsNullOrEmpty(ciot) || ciot.Length != 12)
            {
                MessageBox.Show("CIOT deve ter 12 dígitos quando informado.", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCIOT.Focus();
                return false;
            }

            if (!string.IsNullOrEmpty(cnpjCiot) && !string.IsNullOrEmpty(cpfCiot))
            {
                MessageBox.Show("Preencha apenas CNPJ OU CPF do responsável pelo CIOT, não ambos.", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!string.IsNullOrEmpty(cnpjCiot) && cnpjCiot.Length != 14)
            {
                MessageBox.Show("CNPJ do CIOT deve ter 14 dígitos.", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCNPJCIOT.Focus();
                return false;
            }

            if (!string.IsNullOrEmpty(cpfCiot) && cpfCiot.Length != 11)
            {
                MessageBox.Show("CPF do CIOT deve ter 11 dígitos.", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCPFCIOT.Focus();
                return false;
            }
        }

        // Validação Vale Pedágio
        if (cmbCategCombVeic.SelectedIndex == -1)
        {
            MessageBox.Show("A Categoria de Combinação Veicular é obrigatória quando informar vale pedágio.", "Validação",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbCategCombVeic.Focus();
            return false;
        }

        if (_dispositivos.Count == 0)
        {
            MessageBox.Show("É necessário informar pelo menos um dispositivo de vale pedágio.", "Validação",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        for (int i = 0; i < _dispositivos.Count; i++)
        {
            var disp = _dispositivos[i];
            if (string.IsNullOrWhiteSpace(disp.CNPJForn))
            {
                MessageBox.Show($"CNPJ do Fornecedor é obrigatório (Dispositivo {i + 1}).", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dispositivoAtualIndex = i;
                ExibirDispositivoAtual();
                txtCNPJForn.Focus();
                return false;
            }
            if (disp.CNPJForn.Length != 14)
            {
                MessageBox.Show($"CNPJ do Fornecedor deve ter 14 dígitos (Dispositivo {i + 1}).", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dispositivoAtualIndex = i;
                ExibirDispositivoAtual();
                txtCNPJForn.Focus();
                return false;
            }

            bool temCnpj = !string.IsNullOrWhiteSpace(disp.CNPJPg);
            bool temCpf = !string.IsNullOrWhiteSpace(disp.CPFPg);
            if (temCnpj && temCpf)
            {
                MessageBox.Show($"Preencha apenas CNPJ OU CPF do pagador, não ambos (Dispositivo {i + 1}).", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dispositivoAtualIndex = i;
                ExibirDispositivoAtual();
                return false;
            }
            if (temCnpj && disp.CNPJPg!.Length != 14)
            {
                MessageBox.Show($"CNPJ do Pagador deve ter 14 dígitos (Dispositivo {i + 1}).", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dispositivoAtualIndex = i;
                ExibirDispositivoAtual();
                txtCNPJPg.Focus();
                return false;
            }
            if (temCpf && disp.CPFPg!.Length != 11)
            {
                MessageBox.Show($"CPF do Pagador deve ter 11 dígitos (Dispositivo {i + 1}).", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dispositivoAtualIndex = i;
                ExibirDispositivoAtual();
                txtCPFPg.Focus();
                return false;
            }
            if (disp.vValePed <= 0m)
            {
                MessageBox.Show($"Valor do Vale Pedágio é obrigatório e deve ser maior que zero (Dispositivo {i + 1}).", "Validação",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _dispositivoAtualIndex = i;
                ExibirDispositivoAtual();
                txtVValePed.Focus();
                return false;
            }
        }

        return true;
    }

    private void SalvarXML()
    {
        if (_xmlDocument == null || _xmlFilePath == null) return;

        // Backup
        try
        {
            string bak = _xmlFilePath + ".bak";
            if (File.Exists(_xmlFilePath))
            {
                File.Copy(_xmlFilePath, bak, overwrite: true);
                toolStripStatusLabel.Text = "Backup criado...";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Aviso: Não foi possível criar backup: " + ex.Message, "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        var infANTT = GetDescendant("infANTT");
        if (infANTT == null)
            throw new Exception("Elemento 'infANTT' não encontrado no XML.");

        // === CIOT ===
        var existingCIOT = GetElement(infANTT, "infCIOT");
        existingCIOT?.Remove();

        string ciotVal = txtCIOT.Text.Replace(".", "").Replace("-", "").Trim();
        string cnpjCiot = txtCNPJCIOT.Text.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        string cpfCiot = txtCPFCIOT.Text.Replace(".", "").Replace("-", "").Trim();

        if (!string.IsNullOrEmpty(ciotVal))
        {
            var infCIOT = CreateElement("infCIOT");
            AddElementIfNotEmpty(infCIOT, "CIOT", ciotVal);
            AddElementIfNotEmpty(infCIOT, "CNPJ", cnpjCiot);
            AddElementIfNotEmpty(infCIOT, "CPF", cpfCiot);
            infANTT.Add(infCIOT);
        }

        // === Vale Pedágio ===
        var existingVale = GetElement(infANTT, "valePed");
        existingVale?.Remove();

        var valePed = CreateElement("valePed");

        foreach (var dispositivo in _dispositivos)
        {
            var dispEl = CreateElement("disp");
            AddElementIfNotEmpty(dispEl, "CNPJForn", dispositivo.CNPJForn);
            AddElementIfNotEmpty(dispEl, "CNPJPg", dispositivo.CNPJPg);
            AddElementIfNotEmpty(dispEl, "CPFPg", dispositivo.CPFPg);
            AddElementIfNotEmpty(dispEl, "nCompra", dispositivo.nCompra);
            if (dispositivo.vValePed > 0m)
                AddElementIfNotEmpty(dispEl, "vValePed", dispositivo.vValePed.ToString("0.00", CultureInfo.InvariantCulture));
            AddElementIfNotEmpty(dispEl, "tpValePed", dispositivo.tpValePed);
            valePed.Add(dispEl);
        }

        string categText = cmbCategCombVeic.SelectedItem?.ToString() ?? "";
        if (!string.IsNullOrEmpty(categText))
        {
            string cod = categText[..2];
            valePed.Add(CreateElement("categCombVeic", cod));
        }

        infANTT.Add(valePed);

        // Salvar com formatação
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace,
            Encoding = new UTF8Encoding(false)
        };

        using (var writer = XmlWriter.Create(_xmlFilePath, settings))
        {
            _xmlDocument.Save(writer);
        }

        _alteracoesNaoSalvas = false;
    }

    private XElement CreateElement(string localName, object? value = null)
    {
        return _ns != null
            ? (value != null ? new XElement(_ns + localName, value) : new XElement(_ns + localName))
            : (value != null ? new XElement(localName, value) : new XElement(localName));
    }

    private void AddElementIfNotEmpty(XElement parent, string elementName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            parent.Add(CreateElement(elementName, value));
    }

    private void InitializeComponent()
    {
        groupBoxArquivo = new GroupBox();
        btnCarregar = new Button();
        txtArquivo = new TextBox();
        btnProcurar = new Button();
        groupBoxInfoMDFe = new GroupBox();
        lblChaveValor = new Label();
        lblChave = new Label();
        lblEmitenteValor = new Label();
        lblEmitente = new Label();
        lblPercursoValor = new Label();
        lblPercurso = new Label();
        lblDataEmissaoValor = new Label();
        lblDataEmissao = new Label();
        lblNumeroValor = new Label();
        lblNumero = new Label();
        lblInfoMDFeVazio = new Label();
        groupBoxCIOT = new GroupBox();
        lblCIOT = new Label();
        txtCIOT = new MaskedTextBox();
        lblCNPJCIOT = new Label();
        txtCNPJCIOT = new MaskedTextBox();
        lblCPFCIOT = new Label();
        txtCPFCIOT = new MaskedTextBox();
        groupBoxValePed = new GroupBox();
        groupBoxDispositivo = new GroupBox();
        cmbTpValePed = new ComboBox();
        label7 = new Label();
        txtVValePed = new TextBox();
        label6 = new Label();
        txtNCompra = new TextBox();
        label5 = new Label();
        txtCPFPg = new MaskedTextBox();
        label4 = new Label();
        txtCNPJPg = new MaskedTextBox();
        label3 = new Label();
        txtCNPJForn = new MaskedTextBox();
        label2 = new Label();
        panelNavegacao = new Panel();
        btnRemover = new Button();
        btnAdicionar = new Button();
        btnProximo = new Button();
        btnAnterior = new Button();
        lblDispAtual = new Label();
        cmbCategCombVeic = new ComboBox();
        label1 = new Label();
        btnSalvar = new Button();
        statusStrip = new StatusStrip();
        toolStripStatusLabel = new ToolStripStatusLabel();
        groupBoxArquivo.SuspendLayout();
        groupBoxInfoMDFe.SuspendLayout();
        groupBoxCIOT.SuspendLayout();
        groupBoxValePed.SuspendLayout();
        groupBoxDispositivo.SuspendLayout();
        panelNavegacao.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // groupBoxArquivo
        // 
        groupBoxArquivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxArquivo.BackColor = Color.White;
        groupBoxArquivo.Controls.Add(btnCarregar);
        groupBoxArquivo.Controls.Add(txtArquivo);
        groupBoxArquivo.Controls.Add(btnProcurar);
        groupBoxArquivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        groupBoxArquivo.ForeColor = Color.FromArgb(25, 55, 95);
        groupBoxArquivo.Location = new Point(14, 12);
        groupBoxArquivo.Name = "groupBoxArquivo";
        groupBoxArquivo.Padding = new Padding(10, 8, 10, 8);
        groupBoxArquivo.Size = new Size(806, 78);
        groupBoxArquivo.TabIndex = 0;
        groupBoxArquivo.TabStop = false;
        groupBoxArquivo.Text = "  Arquivo MDFe XML";
        // 
        // btnCarregar
        // 
        btnCarregar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnCarregar.BackColor = Color.FromArgb(30, 130, 80);
        btnCarregar.Cursor = Cursors.Hand;
        btnCarregar.FlatAppearance.BorderSize = 0;
        btnCarregar.FlatStyle = FlatStyle.Flat;
        btnCarregar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnCarregar.ForeColor = Color.White;
        btnCarregar.Location = new Point(695, 32);
        btnCarregar.Name = "btnCarregar";
        btnCarregar.Size = new Size(95, 32);
        btnCarregar.TabIndex = 2;
        btnCarregar.Text = "Carregar";
        btnCarregar.UseVisualStyleBackColor = false;
        btnCarregar.Click += btnCarregar_Click;
        // 
        // txtArquivo
        // 
        txtArquivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtArquivo.BackColor = Color.White;
        txtArquivo.BorderStyle = BorderStyle.FixedSingle;
        txtArquivo.Font = new Font("Segoe UI", 10F);
        txtArquivo.Location = new Point(18, 34);
        txtArquivo.Name = "txtArquivo";
        txtArquivo.PlaceholderText = "  Selecione ou arraste o arquivo XML do MDFe...";
        txtArquivo.Size = new Size(550, 30);
        txtArquivo.TabIndex = 0;
        // 
        // btnProcurar
        // 
        btnProcurar.BackColor = Color.FromArgb(0, 105, 145);
        btnProcurar.Cursor = Cursors.Hand;
        btnProcurar.FlatAppearance.BorderSize = 0;
        btnProcurar.FlatStyle = FlatStyle.Flat;
        btnProcurar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnProcurar.ForeColor = Color.White;
        btnProcurar.Location = new Point(584, 31);
        btnProcurar.Name = "btnProcurar";
        btnProcurar.Size = new Size(105, 32);
        btnProcurar.TabIndex = 1;
        btnProcurar.Text = "Procurar";
        btnProcurar.UseVisualStyleBackColor = false;
        btnProcurar.Click += btnProcurar_Click;
        // 
        // groupBoxInfoMDFe
        // 
        groupBoxInfoMDFe.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxInfoMDFe.BackColor = Color.FromArgb(235, 245, 255);
        groupBoxInfoMDFe.Controls.Add(lblChaveValor);
        groupBoxInfoMDFe.Controls.Add(lblChave);
        groupBoxInfoMDFe.Controls.Add(lblEmitenteValor);
        groupBoxInfoMDFe.Controls.Add(lblEmitente);
        groupBoxInfoMDFe.Controls.Add(lblPercursoValor);
        groupBoxInfoMDFe.Controls.Add(lblPercurso);
        groupBoxInfoMDFe.Controls.Add(lblDataEmissaoValor);
        groupBoxInfoMDFe.Controls.Add(lblDataEmissao);
        groupBoxInfoMDFe.Controls.Add(lblNumeroValor);
        groupBoxInfoMDFe.Controls.Add(lblNumero);
        groupBoxInfoMDFe.Controls.Add(lblInfoMDFeVazio);
        groupBoxInfoMDFe.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        groupBoxInfoMDFe.ForeColor = Color.FromArgb(25, 55, 95);
        groupBoxInfoMDFe.Location = new Point(14, 98);
        groupBoxInfoMDFe.Name = "groupBoxInfoMDFe";
        groupBoxInfoMDFe.Padding = new Padding(10, 6, 10, 6);
        groupBoxInfoMDFe.Size = new Size(806, 105);
        groupBoxInfoMDFe.TabIndex = 1;
        groupBoxInfoMDFe.TabStop = false;
        groupBoxInfoMDFe.Text = "  Informações do MDFe";
        // 
        // lblChaveValor
        // 
        lblChaveValor.AutoSize = true;
        lblChaveValor.Font = new Font("Segoe UI", 9.5F);
        lblChaveValor.ForeColor = Color.FromArgb(15, 70, 110);
        lblChaveValor.Location = new Point(75, 80);
        lblChaveValor.Name = "lblChaveValor";
        lblChaveValor.Size = new Size(16, 21);
        lblChaveValor.TabIndex = 9;
        lblChaveValor.Text = "-";
        lblChaveValor.Visible = false;
        // 
        // lblChave
        // 
        lblChave.AutoSize = true;
        lblChave.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblChave.ForeColor = Color.FromArgb(40, 55, 75);
        lblChave.Location = new Point(12, 80);
        lblChave.Name = "lblChave";
        lblChave.Size = new Size(63, 20);
        lblChave.TabIndex = 8;
        lblChave.Text = "  Chave:";
        lblChave.Visible = false;
        // 
        // lblEmitenteValor
        // 
        lblEmitenteValor.AutoSize = true;
        lblEmitenteValor.Font = new Font("Segoe UI", 9.5F);
        lblEmitenteValor.ForeColor = Color.FromArgb(15, 70, 110);
        lblEmitenteValor.Location = new Point(335, 58);
        lblEmitenteValor.Name = "lblEmitenteValor";
        lblEmitenteValor.Size = new Size(16, 21);
        lblEmitenteValor.TabIndex = 7;
        lblEmitenteValor.Text = "-";
        lblEmitenteValor.Visible = false;
        // 
        // lblEmitente
        // 
        lblEmitente.AutoSize = true;
        lblEmitente.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblEmitente.ForeColor = Color.FromArgb(40, 55, 75);
        lblEmitente.Location = new Point(250, 58);
        lblEmitente.Name = "lblEmitente";
        lblEmitente.Size = new Size(84, 20);
        lblEmitente.TabIndex = 6;
        lblEmitente.Text = "  Emitente:";
        lblEmitente.Visible = false;
        // 
        // lblPercursoValor
        // 
        lblPercursoValor.AutoSize = true;
        lblPercursoValor.Font = new Font("Segoe UI", 9.5F);
        lblPercursoValor.ForeColor = Color.FromArgb(15, 70, 110);
        lblPercursoValor.Location = new Point(95, 58);
        lblPercursoValor.Name = "lblPercursoValor";
        lblPercursoValor.Size = new Size(16, 21);
        lblPercursoValor.TabIndex = 5;
        lblPercursoValor.Text = "-";
        lblPercursoValor.Visible = false;
        // 
        // lblPercurso
        // 
        lblPercurso.AutoSize = true;
        lblPercurso.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblPercurso.ForeColor = Color.FromArgb(40, 55, 75);
        lblPercurso.Location = new Point(12, 58);
        lblPercurso.Name = "lblPercurso";
        lblPercurso.Size = new Size(82, 20);
        lblPercurso.TabIndex = 4;
        lblPercurso.Text = "  Percurso:";
        lblPercurso.Visible = false;
        // 
        // lblDataEmissaoValor
        // 
        lblDataEmissaoValor.AutoSize = true;
        lblDataEmissaoValor.Font = new Font("Segoe UI", 9.5F);
        lblDataEmissaoValor.ForeColor = Color.FromArgb(15, 70, 110);
        lblDataEmissaoValor.Location = new Point(415, 32);
        lblDataEmissaoValor.Name = "lblDataEmissaoValor";
        lblDataEmissaoValor.Size = new Size(16, 21);
        lblDataEmissaoValor.TabIndex = 3;
        lblDataEmissaoValor.Text = "-";
        lblDataEmissaoValor.Visible = false;
        // 
        // lblDataEmissao
        // 
        lblDataEmissao.AutoSize = true;
        lblDataEmissao.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblDataEmissao.ForeColor = Color.FromArgb(40, 55, 75);
        lblDataEmissao.Location = new Point(300, 32);
        lblDataEmissao.Name = "lblDataEmissao";
        lblDataEmissao.Size = new Size(115, 20);
        lblDataEmissao.TabIndex = 2;
        lblDataEmissao.Text = "  Data Emissão:";
        lblDataEmissao.Visible = false;
        // 
        // lblNumeroValor
        // 
        lblNumeroValor.AutoSize = true;
        lblNumeroValor.Font = new Font("Segoe UI", 9.5F);
        lblNumeroValor.ForeColor = Color.FromArgb(15, 70, 110);
        lblNumeroValor.Location = new Point(140, 32);
        lblNumeroValor.Name = "lblNumeroValor";
        lblNumeroValor.Size = new Size(16, 21);
        lblNumeroValor.TabIndex = 1;
        lblNumeroValor.Text = "-";
        lblNumeroValor.Visible = false;
        // 
        // lblNumero
        // 
        lblNumero.AutoSize = true;
        lblNumero.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblNumero.ForeColor = Color.FromArgb(40, 55, 75);
        lblNumero.Location = new Point(12, 32);
        lblNumero.Name = "lblNumero";
        lblNumero.Size = new Size(128, 20);
        lblNumero.TabIndex = 0;
        lblNumero.Text = "  Número / Série:";
        lblNumero.Visible = false;
        // 
        // lblInfoMDFeVazio
        // 
        lblInfoMDFeVazio.Dock = DockStyle.Fill;
        lblInfoMDFeVazio.Font = new Font("Segoe UI", 11F, FontStyle.Italic);
        lblInfoMDFeVazio.ForeColor = Color.FromArgb(110, 125, 145);
        lblInfoMDFeVazio.Location = new Point(10, 28);
        lblInfoMDFeVazio.Name = "lblInfoMDFeVazio";
        lblInfoMDFeVazio.Size = new Size(786, 71);
        lblInfoMDFeVazio.TabIndex = 10;
        lblInfoMDFeVazio.Text = "Informe um XML acima para começarmos.";
        lblInfoMDFeVazio.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // groupBoxCIOT
        // 
        groupBoxCIOT.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxCIOT.BackColor = Color.FromArgb(255, 250, 240);
        groupBoxCIOT.Controls.Add(lblCIOT);
        groupBoxCIOT.Controls.Add(txtCIOT);
        groupBoxCIOT.Controls.Add(lblCNPJCIOT);
        groupBoxCIOT.Controls.Add(txtCNPJCIOT);
        groupBoxCIOT.Controls.Add(lblCPFCIOT);
        groupBoxCIOT.Controls.Add(txtCPFCIOT);
        groupBoxCIOT.Enabled = false;
        groupBoxCIOT.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        groupBoxCIOT.ForeColor = Color.FromArgb(140, 80, 20);
        groupBoxCIOT.Location = new Point(14, 212);
        groupBoxCIOT.Name = "groupBoxCIOT";
        groupBoxCIOT.Padding = new Padding(10, 8, 10, 8);
        groupBoxCIOT.Size = new Size(806, 95);
        groupBoxCIOT.TabIndex = 2;
        groupBoxCIOT.TabStop = false;
        groupBoxCIOT.Text = "  CIOT  (Código Identificador da Operação de Transporte)";
        // 
        // lblCIOT
        // 
        lblCIOT.AutoSize = true;
        lblCIOT.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblCIOT.ForeColor = Color.FromArgb(40, 55, 75);
        lblCIOT.Location = new Point(18, 40);
        lblCIOT.Name = "lblCIOT";
        lblCIOT.Size = new Size(49, 21);
        lblCIOT.TabIndex = 0;
        lblCIOT.Text = "CIOT:";
        // 
        // txtCIOT
        // 
        txtCIOT.BackColor = Color.White;
        txtCIOT.BorderStyle = BorderStyle.FixedSingle;
        txtCIOT.Font = new Font("Consolas", 11F);
        txtCIOT.Location = new Point(73, 32);
        txtCIOT.Mask = "000000000000";
        txtCIOT.Name = "txtCIOT";
        txtCIOT.PromptChar = ' ';
        txtCIOT.Size = new Size(217, 29);
        txtCIOT.TabIndex = 1;
        txtCIOT.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        // 
        // lblCNPJCIOT
        // 
        lblCNPJCIOT.AutoSize = true;
        lblCNPJCIOT.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblCNPJCIOT.ForeColor = Color.FromArgb(40, 55, 75);
        lblCNPJCIOT.Location = new Point(284, 40);
        lblCNPJCIOT.Name = "lblCNPJCIOT";
        lblCNPJCIOT.Size = new Size(61, 21);
        lblCNPJCIOT.TabIndex = 2;
        lblCNPJCIOT.Text = "  CNPJ:";
        // 
        // txtCNPJCIOT
        // 
        txtCNPJCIOT.BackColor = Color.White;
        txtCNPJCIOT.BorderStyle = BorderStyle.FixedSingle;
        txtCNPJCIOT.Font = new Font("Consolas", 10.5F);
        txtCNPJCIOT.Location = new Point(347, 33);
        txtCNPJCIOT.Mask = "00,000,000/0000-00";
        txtCNPJCIOT.Name = "txtCNPJCIOT";
        txtCNPJCIOT.PromptChar = ' ';
        txtCNPJCIOT.Size = new Size(209, 28);
        txtCNPJCIOT.TabIndex = 3;
        txtCNPJCIOT.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        // 
        // lblCPFCIOT
        // 
        lblCPFCIOT.AutoSize = true;
        lblCPFCIOT.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblCPFCIOT.ForeColor = Color.FromArgb(40, 55, 75);
        lblCPFCIOT.Location = new Point(551, 40);
        lblCPFCIOT.Name = "lblCPFCIOT";
        lblCPFCIOT.Size = new Size(50, 21);
        lblCPFCIOT.TabIndex = 4;
        lblCPFCIOT.Text = "  CPF:";
        // 
        // txtCPFCIOT
        // 
        txtCPFCIOT.BackColor = Color.White;
        txtCPFCIOT.BorderStyle = BorderStyle.FixedSingle;
        txtCPFCIOT.Font = new Font("Consolas", 10.5F);
        txtCPFCIOT.Location = new Point(607, 33);
        txtCPFCIOT.Mask = "000,000,000-00";
        txtCPFCIOT.Name = "txtCPFCIOT";
        txtCPFCIOT.PromptChar = ' ';
        txtCPFCIOT.Size = new Size(183, 28);
        txtCPFCIOT.TabIndex = 5;
        txtCPFCIOT.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        // 
        // groupBoxValePed
        // 
        groupBoxValePed.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxValePed.BackColor = Color.FromArgb(240, 250, 245);
        groupBoxValePed.Controls.Add(groupBoxDispositivo);
        groupBoxValePed.Controls.Add(panelNavegacao);
        groupBoxValePed.Controls.Add(cmbCategCombVeic);
        groupBoxValePed.Controls.Add(label1);
        groupBoxValePed.Enabled = false;
        groupBoxValePed.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        groupBoxValePed.ForeColor = Color.FromArgb(20, 100, 70);
        groupBoxValePed.Location = new Point(14, 318);
        groupBoxValePed.Name = "groupBoxValePed";
        groupBoxValePed.Padding = new Padding(10, 8, 10, 8);
        groupBoxValePed.Size = new Size(806, 393);
        groupBoxValePed.TabIndex = 3;
        groupBoxValePed.TabStop = false;
        groupBoxValePed.Text = "  Vale Pedágio  (VPO)";
        // 
        // groupBoxDispositivo
        // 
        groupBoxDispositivo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        groupBoxDispositivo.BackColor = Color.White;
        groupBoxDispositivo.Controls.Add(cmbTpValePed);
        groupBoxDispositivo.Controls.Add(label7);
        groupBoxDispositivo.Controls.Add(txtVValePed);
        groupBoxDispositivo.Controls.Add(label6);
        groupBoxDispositivo.Controls.Add(txtNCompra);
        groupBoxDispositivo.Controls.Add(label5);
        groupBoxDispositivo.Controls.Add(txtCPFPg);
        groupBoxDispositivo.Controls.Add(label4);
        groupBoxDispositivo.Controls.Add(txtCNPJPg);
        groupBoxDispositivo.Controls.Add(label3);
        groupBoxDispositivo.Controls.Add(txtCNPJForn);
        groupBoxDispositivo.Controls.Add(label2);
        groupBoxDispositivo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        groupBoxDispositivo.ForeColor = Color.FromArgb(0, 120, 140);
        groupBoxDispositivo.Location = new Point(16, 72);
        groupBoxDispositivo.Name = "groupBoxDispositivo";
        groupBoxDispositivo.Padding = new Padding(10, 6, 10, 6);
        groupBoxDispositivo.Size = new Size(772, 220);
        groupBoxDispositivo.TabIndex = 2;
        groupBoxDispositivo.TabStop = false;
        groupBoxDispositivo.Text = "  Dados do Dispositivo";
        groupBoxDispositivo.Enter += groupBoxDispositivo_Enter;
        // 
        // cmbTpValePed
        // 
        cmbTpValePed.BackColor = Color.White;
        cmbTpValePed.FlatStyle = FlatStyle.Flat;
        cmbTpValePed.Font = new Font("Segoe UI", 10F);
        cmbTpValePed.ForeColor = Color.Black;
        cmbTpValePed.Location = new Point(201, 117);
        cmbTpValePed.Name = "cmbTpValePed";
        cmbTpValePed.Size = new Size(198, 31);
        cmbTpValePed.TabIndex = 11;
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        label7.ForeColor = Color.FromArgb(40, 55, 75);
        label7.Location = new Point(113, 121);
        label7.Name = "label7";
        label7.Size = new Size(56, 21);
        label7.TabIndex = 10;
        label7.Text = "  Tipo:";
        // 
        // txtVValePed
        // 
        txtVValePed.BackColor = Color.White;
        txtVValePed.BorderStyle = BorderStyle.FixedSingle;
        txtVValePed.Font = new Font("Consolas", 11F, FontStyle.Bold);
        txtVValePed.ForeColor = Color.FromArgb(0, 100, 60);
        txtVValePed.Location = new Point(631, 73);
        txtVValePed.Name = "txtVValePed";
        txtVValePed.PlaceholderText = "  R$ 0,00";
        txtVValePed.Size = new Size(125, 29);
        txtVValePed.TabIndex = 9;
        txtVValePed.TextAlign = HorizontalAlignment.Right;
        txtVValePed.TextChanged += (s, e) => FormatarMoeda(txtVValePed);
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        label6.ForeColor = Color.FromArgb(40, 55, 75);
        label6.Location = new Point(440, 81);
        label6.Name = "label6";
        label6.Size = new Size(158, 21);
        label6.TabIndex = 8;
        label6.Text = "Valor Vale Pedágio:";
        // 
        // txtNCompra
        // 
        txtNCompra.BackColor = Color.White;
        txtNCompra.BorderStyle = BorderStyle.FixedSingle;
        txtNCompra.Font = new Font("Segoe UI", 10.5F);
        txtNCompra.Location = new Point(422, 166);
        txtNCompra.Name = "txtNCompra";
        txtNCompra.Size = new Size(334, 31);
        txtNCompra.TabIndex = 7;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        label5.ForeColor = Color.FromArgb(40, 55, 75);
        label5.Location = new Point(16, 171);
        label5.Name = "label5";
        label5.Size = new Size(327, 21);
        label5.TabIndex = 6;
        label5.Text = "Identificador do Vale Pedágio (nCompra):";
        // 
        // txtCPFPg
        // 
        txtCPFPg.BackColor = Color.White;
        txtCPFPg.BorderStyle = BorderStyle.FixedSingle;
        txtCPFPg.Font = new Font("Consolas", 10.5F);
        txtCPFPg.Location = new Point(175, 74);
        txtCPFPg.Mask = "000,000,000-00";
        txtCPFPg.Name = "txtCPFPg";
        txtCPFPg.PromptChar = ' ';
        txtCPFPg.Size = new Size(224, 28);
        txtCPFPg.TabIndex = 5;
        txtCPFPg.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        label4.ForeColor = Color.FromArgb(40, 55, 75);
        label4.Location = new Point(50, 81);
        label4.Name = "label4";
        label4.Size = new Size(119, 21);
        label4.TabIndex = 4;
        label4.Text = "  CPF Pagador:";
        // 
        // txtCNPJPg
        // 
        txtCNPJPg.BackColor = Color.White;
        txtCNPJPg.BorderStyle = BorderStyle.FixedSingle;
        txtCNPJPg.Font = new Font("Consolas", 10.5F);
        txtCNPJPg.Location = new Point(558, 34);
        txtCNPJPg.Mask = "00,000,000/0000-00";
        txtCNPJPg.Name = "txtCNPJPg";
        txtCNPJPg.PromptChar = ' ';
        txtCNPJPg.Size = new Size(198, 28);
        txtCNPJPg.TabIndex = 3;
        txtCNPJPg.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        label3.ForeColor = Color.FromArgb(40, 55, 75);
        label3.Location = new Point(422, 39);
        label3.Name = "label3";
        label3.Size = new Size(130, 21);
        label3.TabIndex = 2;
        label3.Text = "  CNPJ Pagador:";
        // 
        // txtCNPJForn
        // 
        txtCNPJForn.BackColor = Color.White;
        txtCNPJForn.BorderStyle = BorderStyle.FixedSingle;
        txtCNPJForn.Font = new Font("Consolas", 10.5F);
        txtCNPJForn.Location = new Point(175, 32);
        txtCNPJForn.Mask = "00,000,000/0000-00";
        txtCNPJForn.Name = "txtCNPJForn";
        txtCNPJForn.PromptChar = ' ';
        txtCNPJForn.Size = new Size(224, 28);
        txtCNPJForn.TabIndex = 1;
        txtCNPJForn.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        label2.ForeColor = Color.FromArgb(40, 55, 75);
        label2.Location = new Point(25, 39);
        label2.Name = "label2";
        label2.Size = new Size(144, 21);
        label2.TabIndex = 0;
        label2.Text = "CNPJ Fornecedor:";
        // 
        // panelNavegacao
        // 
        panelNavegacao.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        panelNavegacao.BackColor = Color.FromArgb(230, 245, 238);
        panelNavegacao.Controls.Add(btnRemover);
        panelNavegacao.Controls.Add(btnAdicionar);
        panelNavegacao.Controls.Add(btnProximo);
        panelNavegacao.Controls.Add(btnAnterior);
        panelNavegacao.Controls.Add(lblDispAtual);
        panelNavegacao.Location = new Point(16, 328);
        panelNavegacao.Name = "panelNavegacao";
        panelNavegacao.Size = new Size(772, 50);
        panelNavegacao.TabIndex = 3;
        // 
        // btnRemover
        // 
        btnRemover.BackColor = Color.FromArgb(170, 60, 50);
        btnRemover.Cursor = Cursors.Hand;
        btnRemover.FlatAppearance.BorderSize = 0;
        btnRemover.FlatStyle = FlatStyle.Flat;
        btnRemover.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnRemover.ForeColor = Color.White;
        btnRemover.Location = new Point(550, 9);
        btnRemover.Name = "btnRemover";
        btnRemover.Size = new Size(100, 32);
        btnRemover.TabIndex = 4;
        btnRemover.Text = "− Remover";
        btnRemover.UseVisualStyleBackColor = false;
        btnRemover.Click += btnRemover_Click;
        // 
        // btnAdicionar
        // 
        btnAdicionar.BackColor = Color.FromArgb(30, 130, 80);
        btnAdicionar.Cursor = Cursors.Hand;
        btnAdicionar.FlatAppearance.BorderSize = 0;
        btnAdicionar.FlatStyle = FlatStyle.Flat;
        btnAdicionar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnAdicionar.ForeColor = Color.White;
        btnAdicionar.Location = new Point(440, 9);
        btnAdicionar.Name = "btnAdicionar";
        btnAdicionar.Size = new Size(100, 32);
        btnAdicionar.TabIndex = 3;
        btnAdicionar.Text = "+ Adicionar";
        btnAdicionar.UseVisualStyleBackColor = false;
        btnAdicionar.Click += btnAdicionar_Click;
        // 
        // btnProximo
        // 
        btnProximo.BackColor = Color.FromArgb(70, 100, 140);
        btnProximo.Cursor = Cursors.Hand;
        btnProximo.FlatAppearance.BorderSize = 0;
        btnProximo.FlatStyle = FlatStyle.Flat;
        btnProximo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnProximo.ForeColor = Color.White;
        btnProximo.Location = new Point(320, 9);
        btnProximo.Name = "btnProximo";
        btnProximo.Size = new Size(100, 32);
        btnProximo.TabIndex = 2;
        btnProximo.Text = "Próximo ►";
        btnProximo.UseVisualStyleBackColor = false;
        btnProximo.Click += btnProximo_Click;
        // 
        // btnAnterior
        // 
        btnAnterior.BackColor = Color.FromArgb(70, 100, 140);
        btnAnterior.Cursor = Cursors.Hand;
        btnAnterior.FlatAppearance.BorderSize = 0;
        btnAnterior.FlatStyle = FlatStyle.Flat;
        btnAnterior.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnAnterior.ForeColor = Color.White;
        btnAnterior.Location = new Point(210, 9);
        btnAnterior.Name = "btnAnterior";
        btnAnterior.Size = new Size(100, 32);
        btnAnterior.TabIndex = 1;
        btnAnterior.Text = "◄ Anterior";
        btnAnterior.UseVisualStyleBackColor = false;
        btnAnterior.Click += btnAnterior_Click;
        // 
        // lblDispAtual
        // 
        lblDispAtual.AutoSize = true;
        lblDispAtual.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblDispAtual.ForeColor = Color.FromArgb(25, 55, 95);
        lblDispAtual.Location = new Point(14, 14);
        lblDispAtual.Name = "lblDispAtual";
        lblDispAtual.Size = new Size(164, 23);
        lblDispAtual.TabIndex = 0;
        lblDispAtual.Text = "  Dispositivo 1 de 1";
        // 
        // cmbCategCombVeic
        // 
        cmbCategCombVeic.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategCombVeic.FlatStyle = FlatStyle.Flat;
        cmbCategCombVeic.Font = new Font("Segoe UI", 10F);
        cmbCategCombVeic.Location = new Point(289, 30);
        cmbCategCombVeic.Name = "cmbCategCombVeic";
        cmbCategCombVeic.Size = new Size(483, 31);
        cmbCategCombVeic.TabIndex = 1;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        label1.ForeColor = Color.FromArgb(40, 55, 75);
        label1.Location = new Point(18, 34);
        label1.Name = "label1";
        label1.Size = new Size(274, 21);
        label1.TabIndex = 0;
        label1.Text = "  * Categoria Combinação Veicular:";
        // 
        // btnSalvar
        // 
        btnSalvar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnSalvar.BackColor = Color.FromArgb(20, 110, 70);
        btnSalvar.Cursor = Cursors.Hand;
        btnSalvar.Enabled = false;
        btnSalvar.FlatAppearance.BorderSize = 0;
        btnSalvar.FlatStyle = FlatStyle.Flat;
        btnSalvar.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnSalvar.ForeColor = Color.White;
        btnSalvar.Location = new Point(640, 723);
        btnSalvar.Name = "btnSalvar";
        btnSalvar.Size = new Size(170, 42);
        btnSalvar.TabIndex = 4;
        btnSalvar.Text = "  Salvar Alterações";
        btnSalvar.UseVisualStyleBackColor = false;
        btnSalvar.Click += btnSalvar_Click;
        // 
        // statusStrip
        // 
        statusStrip.BackColor = Color.FromArgb(230, 235, 245);
        statusStrip.Font = new Font("Segoe UI", 9F);
        statusStrip.ImageScalingSize = new Size(20, 20);
        statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
        statusStrip.Location = new Point(0, 777);
        statusStrip.Name = "statusStrip";
        statusStrip.Padding = new Padding(8, 0, 8, 0);
        statusStrip.Size = new Size(832, 26);
        statusStrip.TabIndex = 5;
        // 
        // toolStripStatusLabel
        // 
        toolStripStatusLabel.ForeColor = Color.FromArgb(25, 55, 95);
        toolStripStatusLabel.Name = "toolStripStatusLabel";
        toolStripStatusLabel.Size = new Size(61, 20);
        toolStripStatusLabel.Text = "  Pronto";
        // 
        // Formulario
        // 
        AutoScaleDimensions = new SizeF(9F, 21F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 248, 252);
        ClientSize = new Size(832, 803);
        Controls.Add(statusStrip);
        Controls.Add(btnSalvar);
        Controls.Add(groupBoxValePed);
        Controls.Add(groupBoxCIOT);
        Controls.Add(groupBoxInfoMDFe);
        Controls.Add(groupBoxArquivo);
        Font = new Font("Segoe UI", 9.5F);
        MinimumSize = new Size(810, 820);
        Name = "Formulario";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Informar CIOT e Vale Pedágio no Manifesto de Documentos Fiscais Eletrônico - MDF-e";
        groupBoxArquivo.ResumeLayout(false);
        groupBoxArquivo.PerformLayout();
        groupBoxInfoMDFe.ResumeLayout(false);
        groupBoxInfoMDFe.PerformLayout();
        groupBoxCIOT.ResumeLayout(false);
        groupBoxCIOT.PerformLayout();
        groupBoxValePed.ResumeLayout(false);
        groupBoxValePed.PerformLayout();
        groupBoxDispositivo.ResumeLayout(false);
        groupBoxDispositivo.PerformLayout();
        panelNavegacao.ResumeLayout(false);
        panelNavegacao.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private void groupBoxDispositivo_Enter(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// Formata um TextBox com valores monetários em tempo real
    /// Converte entrada do usuário (ex: 51699) para formato de moeda (R$ 516,99)
    /// </summary>
    private void FormatarMoeda(TextBox textBox)
    {
        try
        {
            // Remove formatação anterior (R$, espaços, etc)
            string texto = textBox.Text.Replace("R$", "").Replace(" ", "").Replace(".", "").Trim();

            // Se vazio, limpa o campo
            if (string.IsNullOrEmpty(texto))
            {
                textBox.Text = "";
                return;
            }

            // Remove caracteres não numéricos
            string apenasNumeros = "";
            foreach (char c in texto)
            {
                if (char.IsDigit(c))
                    apenasNumeros += c;
            }

            // Se não houver números, limpa
            if (string.IsNullOrEmpty(apenasNumeros))
            {
                textBox.Text = "";
                return;
            }

            // Converte para decimal (dividindo por 100 para obter centavos)
            decimal valor = decimal.Parse(apenasNumeros) / 100m;

            // Formata como moeda brasileira
            textBox.Text = valor.ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));

            // Mantém o cursor ao final do texto
            textBox.SelectionStart = textBox.Text.Length;
        }
        catch
        {
            // Ignora erros de formatação durante digitação
        }
    }
}