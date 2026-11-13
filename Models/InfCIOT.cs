namespace InformarCIOTeVPO.Models;

/// <summary>
/// Modelo de dados para CIOT (Conhecimento de Transporte Interestadual Oneroso)
/// 
/// Representa as informações necessárias para o CIOT, que é um conhecimento de
/// transporte para operações interestaduais de carga de terceiros com cobrança.
/// </summary>
public class InfCIOT
{
	/// <summary>
	/// Número de CIOT (Código de Identificação de Operação de Transporte)
	/// 
	/// Identificação única da operação de transporte registrada na ANTT.
	/// Formato: 20 dígitos (segundo as regras da ANTT)
	/// </summary>
	public string CIOT { get; set; } = string.Empty;

	/// <summary>
	/// CNPJ da empresa responsável pela operação de transporte
	/// 
	/// Pode ser o CNPJ do transportador ou da empresa contratante, conforme
	/// configuração da operação de transporte.
	/// Formato: 14 dígitos (com ou sem formatação)
	/// </summary>
	public string? CNPJ { get; set; }

	/// <summary>
	/// CPF do responsável pela operação de transporte
	/// 
	/// Utilizado quando a operação é realizada por pessoa física.
	/// Campo opcional, usado alternadamente com CNPJ.
	/// Formato: 11 dígitos (com ou sem formatação)
	/// </summary>
	public string? CPF { get; set; }
}
