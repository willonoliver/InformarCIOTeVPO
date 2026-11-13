using System.Collections.Generic;

namespace InformarCIOTeVPO.Models;

/// <summary>
/// Modelo de dados para Vale Pedágio (Informações de Pedágio)
/// 
/// Contém as informações sobre vale pedágio, incluindo a lista de fornecedores
/// (dispositivos) que fornecem o vale e a categoria do combustível do veículo.
/// </summary>
public class ValePed
{
	/// <summary>
	/// Lista de Dispositivos (Fornecedores de Vale Pedágio)
	/// 
	/// Cada dispositivo representa um fornecedor de vale pedágio, com informações
	/// sobre CNPJ do fornecedor, CNPJ/CPF do pagador, e valores associados.
	/// </summary>
	public List<Disp> Dispositivos { get; set; } = new List<Disp>();

	/// <summary>
	/// Categoria do combustível do veículo
	/// 
	/// Define a categoria de combustível usado pelo veículo para fins de
	/// classificação e cálculo de pedágio.
	/// Valores comuns: "01" (Gasolina), "02" (Diesel), "03" (Etanol), etc.
	/// </summary>
	public string CategCombVeic { get; set; } = string.Empty;
}
