namespace InformarCIOTeVPO.Models;

/// <summary>
/// Modelo de dados para Dispositivo de Vale Pedágio
/// 
/// Representa um fornecedor/dispositivo de vale pedágio, contendo informações
/// sobre o fornecedor, o pagador, e os valores associados à operação.
/// </summary>
public class Disp
{
	/// <summary>
	/// CNPJ do Fornecedor de Vale Pedágio
	/// 
	/// Identificação do fornecedor (geralmente empresa de combustível ou serviços
	/// correlatos) que fornece o vale pedágio.
	/// Formato: 14 dígitos (com ou sem formatação)
	/// </summary>
	public string CNPJForn { get; set; } = string.Empty;

	/// <summary>
	/// CNPJ do Pagador do Vale Pedágio
	/// 
	/// Identificação da empresa responsável pelo pagamento do vale pedágio.
	/// Camp opcional, usado alternadamente com CPFPg.
	/// Formato: 14 dígitos (com ou sem formatação)
	/// </summary>
	public string? CNPJPg { get; set; }

	/// <summary>
	/// CPF do Pagador do Vale Pedágio
	/// 
	/// Identificação da pessoa física responsável pelo pagamento do vale pedágio.
	/// Campo opcional, usado alternadamente com CNPJPg.
	/// Formato: 11 dígitos (com ou sem formatação)
	/// </summary>
	public string? CPFPg { get; set; }

	/// <summary>
	/// Número de Compra/Autorização
	/// 
	/// Identificação da compra ou autorização associada ao vale pedágio.
	/// Pode ser o número de nota fiscal ou outro identificador de compra.
	/// </summary>
	public string? nCompra { get; set; }

	/// <summary>
	/// Valor do Vale Pedágio
	/// 
	/// Valor monetário (em reais) do vale pedágio conferido/utilizado nesta operação.
	/// Campo numérico com até 2 casas decimais.
	/// </summary>
	public decimal vValePed { get; set; }

	/// <summary>
	/// Tipo de Vale Pedágio
	/// 
	/// Classificação do tipo de vale pedágio conforme tabelas da ANTT.
	/// Exemplos: "01" (Vale Pedágio Emitido), "02" (Vale Pedágio Recebido), etc.
	/// </summary>
	public string? tpValePed { get; set; }
}
