namespace InformarCIOTeVPO.Models;

/// <summary>
/// Modelo de dados para InfANTT (Informações de ANTT - Agência Nacional de Transportes Terrestres)
/// 
/// Contém os dados de CIOT e Vale Pedágio que são exigidos pela ANTT para
/// documentos de transporte rodoviário de cargas.
/// </summary>
public class InfANTT
{
	/// <summary>
	/// Informações de CIOT (Conhecimento de Transporte Interestadual Oneroso)
	/// 
	/// Dados sobre o conhecimento de transporte, incluindo identificação do CIOT,
	/// CNPJ e CPF envolvidos na operação de transporte.
	/// </summary>
	public InfCIOT? InfCIOT { get; set; }

	/// <summary>
	/// Informações de Vale Pedágio
	/// 
	/// Dados sobre os valores de pedágio e os fornecedores envolvidos na operação.
	/// Contém a lista de dispositivos (fornecedores) de vale pedágio.
	/// </summary>
	public ValePed? ValePedagio { get; set; }
}
