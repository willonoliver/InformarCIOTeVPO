# InformarCIOTeVPO

**Ferramenta auxiliar para edição de informações de CIOT e Vale Pedágio Obrigatório em arquivos XML de MDF-e.**

## Sobre

O InformarCIOTeVPO é uma aplicação desktop desenvolvida em C# e Windows Forms para inclusão e edição dos dados de CIOT (Código Identificador da Operação de Transporte) e Vale Pedágio Obrigatório (VPO) em arquivos XML de MDF-e (Manifesto Eletrônico de Documentos Fiscais).

O projeto atende empresas que utilizam sistemas TMS que ainda não disponibilizam suporte ao preenchimento dessas informações no MDF-e, oferecendo uma alternativa operacional enquanto as implementações necessárias são desenvolvidas pelos fornecedores de software.

A aplicação trabalha sobre arquivos XML previamente gerados pelo sistema de origem, permitindo complementar as informações do documento sem a necessidade de integração direta com o TMS.

## Escopo da aplicação

O InformarCIOTeVPO é exclusivamente uma ferramenta de edição e complementação de XML.

**Não são responsabilidades da aplicação:**
- Gerar ou emitir CIOT.
- Contratar operações de transporte.
- Comprar ou emitir Vale Pedágio.
- Gerenciar créditos ou pagamentos de pedágio.
- Realizar comunicação com fornecedores de CIOT ou VPO.
- Assinar digitalmente ou transmitir MDF-e à SEFAZ.

Os números e dados utilizados devem ter sido previamente obtidos junto aos sistemas, instituições e fornecedores responsáveis pelas respectivas operações.

## Funcionalidades

### MDF-e
- Abertura de arquivos XML por diálogo ou Drag and Drop.
- Leitura e interpretação da estrutura XML.
- Identificação das informações existentes no documento.
- Inclusão e atualização das informações de CIOT e Vale Pedágio.
- Preservação dos demais dados do manifesto.
- Salvamento do XML atualizado.

### CIOT
- Inclusão e edição do número do CIOT.
- Informação do CNPJ correspondente.
- Leitura dos dados existentes no documento.
- Validação de preenchimento.

### Vale Pedágio Obrigatório
- Inclusão e edição dos dados do VPO.
- Informação do CNPJ do fornecedor.
- Informação do CNPJ do pagador.
- Informação do número da compra.
- Gerenciamento de dispositivos e fornecedores.
- Navegação entre múltiplos dispositivos vinculados ao documento.

### Interface e usabilidade
- Interface gráfica Windows Forms.
- Navegação entre dispositivos.
- Detecção de alterações não salvas.
- Validação dos campos antes da gravação.
- Suporte a monitores de alta resolução (High DPI).

## Fluxo operacional

1. O sistema TMS gera o XML original do MDF-e.
2. O CIOT é obtido previamente pelo serviço responsável.
3. O Vale Pedágio é adquirido junto ao fornecedor habilitado.
4. O usuário abre o XML no InformarCIOTeVPO.
5. Preenche os dados de CIOT e VPO.
6. A aplicação atualiza e salva o XML.
7. O arquivo atualizado retorna ao sistema TMS.
8. O TMS realiza a assinatura digital e a transmissão do MDF-e.

## Tecnologias utilizadas

| Tecnologia | Finalidade |
|---|---|
| C# 12 | Linguagem de programação |
| .NET 8 | Plataforma de desenvolvimento |
| Windows Forms | Interface gráfica |
| System.Xml.Linq | Manipulação de documentos XML |
| XDocument | Leitura e processamento do XML |
| High DPI | Suporte a diferentes resoluções de tela |

## Estrutura do projeto

```text
InformarCIOTeVPO/
│
├── Formulario.cs
├── Formulario.resx
├── Program.cs
├── ConfiguracaoAplicacao.cs
│
├── Models/
│   ├── MDFe.cs
│   ├── InfANTT.cs
│   ├── InfCIOT.cs
│   ├── ValePed.cs
│   └── Disp.cs
│
└── Properties/
    └── AssemblyInfo.cs
```

## Requisitos

- Windows 10 ou superior.
- .NET 8 Desktop Runtime.
- Arquivo XML de MDF-e compatível com o leiaute implementado.

## Compilação e execução

É necessário possuir o SDK do .NET 8 instalado.

**Compilar:**

```powershell
dotnet build
```

**Executar:**

```powershell
dotnet run
```

**Publicar em Release:**

```powershell
dotnet publish -c Release
```

**Publicar como executável independente para Windows 64 bits:**

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

## Integridade e limitações

- A aplicação deve ser utilizada preferencialmente antes da assinatura digital do MDF-e.
- Qualquer modificação em um XML assinado invalida sua assinatura digital.
- Após a edição, o documento deverá ser novamente assinado pelo sistema emissor.
- A aplicação não substitui os sistemas responsáveis pela emissão e transmissão dos documentos fiscais.
- A validação realizada pela ferramenta não representa autorização ou confirmação da validade de um CIOT ou de uma aquisição de VPO.
- A integridade das informações originais e a compatibilidade do documento devem ser verificadas antes da transmissão.

## Referências normativas

O projeto considera as disposições e especificações técnicas relacionadas ao CIOT e ao Vale Pedágio Obrigatório:

- **Resolução ANTT nº 5.862/2019:** regulamentação do pagamento eletrônico de frete e CIOT, considerando suas alterações posteriores.
- **Resolução ANTT nº 6.078/2026:** alterações na regulamentação do CIOT.
- **Ajuste SINIEF nº 03/2026:** disposições relativas à informação do CIOT no MDF-e.
- **Lei nº 10.209/2001:** instituição do Vale-Pedágio obrigatório.
- **Resolução ANTT nº 6.024/2023:** regulamentação do Vale-Pedágio obrigatório.

Documentação oficial:

- [ANTT – Legislação CIOT](https://www.gov.br/antt/pt-br/assuntos/cargas/pagamento-eletronico-de-fretes-pef-ciot/legislacao-pef-ciot)
- [ANTT – Legislação VPO](https://www.gov.br/antt/pt-br/assuntos/cargas/vale-pedagio-obrigatorio/legislacao-vpo-1)
- [CONFAZ – Ajustes SINIEF](https://www.confaz.fazenda.gov.br/legislacao/ajustes)
- [Portal MDF-e – Documentação técnica](https://dfe-portal.svrs.rs.gov.br/Mdfe/Legislacao)

## Público-alvo

- Transportadoras e empresas de transporte rodoviário de cargas.
- Empresas que utilizam sistemas TMS.
- Empresas que ainda não possuem implementação nativa de CIOT e VPO em seus sistemas.
- Desenvolvedores e fornecedores de sistemas de gestão de transporte.



O InformarCIOTeVPO é uma ferramenta auxiliar independente, destinada à edição e complementação de arquivos XML de MDF-e.