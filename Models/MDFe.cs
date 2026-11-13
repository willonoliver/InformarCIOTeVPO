namespace InformarCIOTeVPO.Models;

/// <summary>
/// Modelo de dados para MDFe (Manifesto Eletrônico de Documentos Fiscais)
/// 
/// Representa a estrutura básica de um Manifesto Eletrônico, contendo a chave de acesso
/// e as informações de ANTT (Agência Nacional de Transportes Terrestres).
/// </summary>
public class MDFe
{
	/// <summary>
	/// Chave de acesso do MDFe
	/// 
	/// Identificador único do documento fiscal eletrônico gerado pela Secretaria da Fazenda.
	/// Formato: 44 dígitos (Estado + DATA/MÊS + CNPJ + SÉRIE + NÚMERO + DÍGITO VERIFICADOR)
	/// </summary>
	public string ChaveAcesso { get; set; } = string.Empty;

	/// <summary>
	/// Informações de ANTT (Agência Nacional de Transportes Terrestres)
	/// 
	/// Contém os dados de CIOT e Vale Pedágio associados a este MDFe
	/// </summary>
	public InfANTT? InfANTT { get; set; }
}
