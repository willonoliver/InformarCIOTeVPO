# InformarCIOTeVPO

**Ferramenta auxiliar para inclusão e edição de informações de CIOT e Vale-Pedágio Obrigatório em arquivos XML de MDF-e.**

## Sobre o projeto

O **InformarCIOTeVPO** é uma aplicação desktop desenvolvida em C# e Windows Forms, destinada à inclusão, edição e complementação das informações de CIOT (Código Identificador da Operação de Transporte) e Vale-Pedágio Obrigatório (VPO) em arquivos XML do MDF-e (Manifesto Eletrônico de Documentos Fiscais).

O projeto atende principalmente empresas de transporte rodoviário de cargas que utilizam sistemas TMS que ainda não disponibilizam suporte adequado ao preenchimento dessas informações no XML do MDF-e.

A ferramenta oferece uma alternativa operacional independente, permitindo que o usuário complemente documentos fiscais previamente gerados pelo sistema de origem, sem necessidade de integração direta com o TMS ou com os fornecedores de CIOT e Vale-Pedágio.

O objetivo é facilitar a adequação dos arquivos XML às exigências de preenchimento do MDF-e, preservando as demais informações do documento original.

## Demonstração da aplicação

Abaixo, uma captura de tela da aplicação em funcionamento, demonstrando a interface de edição do MDF-e, com identificação do documento e preenchimento das informações de CIOT e Vale-Pedágio Obrigatório.

![InformarCIOTeVPO - Interface principal](Docs/Imagens/InformaCIOTeVPO.png)

*Interface principal do InformarCIOTeVPO com um MDF-e carregado e os dados de CIOT e VPO preenchidos.*

## Escopo e responsabilidades

O InformarCIOTeVPO é exclusivamente uma ferramenta de edição e complementação de arquivos XML.

**Não são responsabilidades da aplicação:**

- Gerar ou emitir CIOT.
- Cadastrar operações de transporte junto à ANTT.
- Contratar operações de transporte.
- Comprar ou emitir Vale-Pedágio.
- Gerenciar créditos ou pagamentos de pedágio.
- Realizar comunicação com fornecedores de CIOT ou VPO.
- Assinar digitalmente documentos fiscais.
- Transmitir ou autorizar MDF-e junto à SEFAZ.
- Substituir as funcionalidades do sistema TMS de origem.

Os números, códigos e demais informações utilizados devem ter sido previamente obtidos junto aos sistemas, instituições e fornecedores responsáveis pelas respectivas operações.

## Funcionalidades

### MDF-e

- Abertura de arquivos XML por diálogo de seleção.
- Suporte a Drag and Drop para carregamento de arquivos.
- Leitura e interpretação da estrutura XML.
- Identificação das informações existentes no documento.
- Inclusão e atualização dos grupos de CIOT e Vale-Pedágio.
- Preservação das demais informações do manifesto.
- Salvamento do XML atualizado.
- Tratamento de arquivos com estruturas incompatíveis ou informações incompletas.

### CIOT

- Inclusão e edição do número do CIOT.
- Informação do CNPJ correspondente.
- Leitura dos dados existentes no documento.
- Identificação de informações previamente preenchidas.
- Validação dos campos obrigatórios conforme as regras implementadas.
- Atualização do grupo correspondente no XML do MDF-e.

### Vale-Pedágio Obrigatório (VPO)

- Inclusão e edição das informações do Vale-Pedágio.
- Informação do CNPJ do fornecedor.
- Informação do CNPJ do pagador.
- Informação do número da compra.
- Gerenciamento de dispositivos e fornecedores.
- Navegação entre múltiplos dispositivos vinculados ao documento.
- Inclusão, edição e atualização das informações de dispositivos.
- Leitura dos dados de Vale-Pedágio existentes no XML.

### Interface e usabilidade

- Interface gráfica desenvolvida em Windows Forms.
- Navegação entre dispositivos de Vale-Pedágio.
- Detecção de alterações não salvas.
- Validação dos campos antes da gravação.
- Feedback visual das operações realizadas.
- Suporte a monitores de alta resolução (High DPI).
- Interface independente do sistema TMS de origem.

## Fluxo operacional

O funcionamento da aplicação segue o seguinte fluxo:

1. O sistema TMS gera o XML original do MDF-e.
2. O CIOT é obtido previamente junto ao sistema ou instituição responsável.
3. O Vale-Pedágio é adquirido junto a um fornecedor habilitado.
4. O usuário abre o XML no InformarCIOTeVPO.
5. A aplicação interpreta a estrutura do documento.
6. O usuário informa ou atualiza os dados de CIOT e Vale-Pedágio.
7. A aplicação valida os campos e atualiza os grupos correspondentes no XML.
8. O usuário salva o documento complementado.
9. O XML atualizado retorna ao sistema TMS.
10. O sistema emissor realiza a assinatura digital e a transmissão do MDF-e à SEFAZ.

## Notas Técnicas do MDF-e

O desenvolvimento do InformarCIOTeVPO considera as especificações técnicas do Manifesto Eletrônico de Documentos Fiscais (MDF-e), incluindo suas Notas Técnicas, atualizações de leiaute, schemas XML e regras de validação.

As Notas Técnicas publicadas pelo ENCAT e disponibilizadas no Portal Nacional do MDF-e estabelecem alterações que podem impactar a estrutura dos arquivos XML, o preenchimento dos campos e as regras de validação dos documentos.

### Atualizações técnicas de referência

| Documento | Publicação | Descrição |
|---|---|---|
| NT 2026.001 | 29/05/2026 | Alterações relacionadas à informação do CIOT e suas regras de validação no MDF-e. |
| NT 2025.001 v1.04 | 25/04/2026 | Atualização do pacote de schemas XML do MDF-e. |
| Ajuste SINIEF 03/2026 | 29/05/2026 | Disposições relacionadas à informação do CIOT no MDF-e. |

**Observação:** as datas e versões acima identificam documentos técnicos de referência. A ferramenta não implementa automaticamente todas as alterações publicadas. Cada atualização deve ser analisada para verificar seu impacto nas funcionalidades do projeto.

### Destaque: Nota Técnica 2026.001 — CIOT

A NT 2026.001 introduz alterações relacionadas à informação do CIOT no MDF-e, incluindo regras de validação associadas ao preenchimento dessa informação.

Essa atualização é especialmente relevante para o InformarCIOTeVPO, pois reforça a necessidade de permitir a inclusão e a manutenção do CIOT no grupo correspondente do XML do manifesto.

A aplicação atua exclusivamente na complementação do documento, não realizando a geração do CIOT nem a comunicação com os sistemas responsáveis pelo seu registro.

### Grupos XML relacionados ao projeto

A aplicação trabalha especialmente com os seguintes grupos e elementos da estrutura XML do MDF-e:

| Grupo ou elemento | Finalidade |
|---|---|
| `infANTT` | Informações relacionadas ao transporte rodoviário. |
| `infCIOT` | Identificação e informação do CIOT. |
| `valePed` | Informações do Vale-Pedágio Obrigatório. |
| `disp` | Identificação dos dispositivos de Vale-Pedágio. |
| `CNPJForn` | CNPJ do fornecedor do Vale-Pedágio. |
| `CNPJPg` | CNPJ do responsável pelo pagamento. |
| `nCompra` | Identificação da compra do Vale-Pedágio. |

A compatibilidade dos elementos deve respeitar a versão do leiaute e os schemas XML aplicáveis ao documento.

### Acompanhamento de atualizações

Para manter a compatibilidade do projeto, recomenda-se acompanhar periodicamente:

- Publicação de novas Notas Técnicas do MDF-e.
- Atualizações dos schemas XML.
- Alterações nas regras de validação.
- Alterações nos Ajustes SINIEF.
- Atualizações da regulamentação da ANTT relacionadas ao CIOT e ao VPO.

## Tecnologias utilizadas

| Tecnologia | Finalidade |
|---|---|
| C# 12 | Linguagem de programação. |
| .NET 8 | Plataforma de desenvolvimento. |
| Windows Forms | Interface gráfica desktop. |
| System.Xml.Linq | Manipulação de documentos XML. |
| XDocument | Leitura, interpretação e processamento do XML. |
| High DPI | Suporte a diferentes resoluções e escalas de tela. |
| LINQ | Consulta e manipulação de elementos XML. |

## Estrutura do projeto

```text
InformarCIOTeVPO/
│
├── Docs/
│   └── imagens/
│       └── InformarCIOTeVPO.png
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

## Requisitos do sistema

- Windows 10 ou superior.
- .NET 8 Desktop Runtime.
- Arquivo XML de MDF-e compatível com o leiaute implementado.
- Permissão de leitura e gravação no diretório dos arquivos XML.

## Compilação e execução

É necessário possuir o SDK do .NET 8 instalado.

**Compilar o projeto:**

```powershell
dotnet build
```

**Executar a aplicação:**

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

A publicação independente permite executar a aplicação sem exigir a instalação prévia do .NET 8 Desktop Runtime na máquina de destino.

## Integridade dos documentos e limitações

- A aplicação deve ser utilizada preferencialmente antes da assinatura digital do MDF-e.
- Qualquer modificação em um XML assinado invalida sua assinatura digital.
- Após a edição, o documento deverá ser novamente assinado pelo sistema emissor.
- A ferramenta não substitui os sistemas responsáveis pela emissão e transmissão dos documentos fiscais.
- A validação realizada pela aplicação não representa autorização ou confirmação da validade de um CIOT.
- A aplicação não confirma a efetiva aquisição ou o pagamento de um Vale-Pedágio.
- A integridade das informações originais e a compatibilidade do documento devem ser verificadas antes da transmissão.
- A compatibilidade com novas versões do leiaute depende da atualização e manutenção do projeto.

**Importante:** o usuário é responsável por verificar a correção dos dados informados e garantir que o documento esteja em conformidade com a legislação e as especificações técnicas vigentes.

## Referências normativas e técnicas

### CIOT e Pagamento Eletrônico de Frete

- Resolução ANTT nº 5.862/2019 — regulamentação do cadastramento das operações de transporte e geração do CIOT, considerando suas alterações posteriores.
- Resolução ANTT nº 6.078/2026 — alterações na regulamentação do CIOT.
- Resolução ANTT nº 6.090/2026 — regulamentação complementar relacionada ao CIOT.
- Medida Provisória nº 1.343/2026 — alterações relacionadas ao cadastramento das operações de transporte e à geração do CIOT.
- Portaria SUROC nº 6/2026 — regras operacionais e validações aplicáveis ao CIOT.

### Vale-Pedágio Obrigatório

- Lei nº 10.209/2001 — instituição do Vale-Pedágio obrigatório.
- Resolução ANTT nº 6.024/2023 — regulamentação do Vale-Pedágio obrigatório, considerando suas alterações posteriores.

### MDF-e

- Ajuste SINIEF nº 03/2026 — disposições relacionadas à informação do CIOT no MDF-e.
- Nota Técnica MDF-e 2026.001.
- Nota Técnica MDF-e 2025.001 e suas atualizações.
- Manual de Orientação do Contribuinte (MOC) do MDF-e.
- Schemas XML e regras de validação vigentes.

### Documentação oficial

- [ANTT — Legislação CIOT](https://www.gov.br/antt/pt-br/assuntos/cargas/pagamento-eletronico-de-fretes-pef-ciot/legislacao-pef-ciot)
- [ANTT — CIOT Para Todos](https://www.gov.br/antt/pt-br/assuntos/cargas/ciot-para-todos-1)
- [ANTT — Legislação Vale-Pedágio Obrigatório](https://www.gov.br/antt/pt-br/assuntos/cargas/vale-pedagio-obrigatorio/legislacao-vpo-1)
- [Portal Nacional do MDF-e — Documentação técnica](https://dfe-portal.svrs.rs.gov.br/Mdfe)
- [Portal Nacional do MDF-e — Legislação](https://dfe-portal.svrs.rs.gov.br/Mdfe/Legislacao)
- [CONFAZ — Ajustes SINIEF](https://www.confaz.fazenda.gov.br/legislacao/ajustes)

## Público-alvo

O InformarCIOTeVPO é destinado principalmente a:

- Transportadoras e empresas de transporte rodoviário de cargas.
- Empresas que utilizam sistemas TMS.
- Empresas que necessitam complementar informações de CIOT e VPO em seus MDF-e.
- Empresas cujos sistemas de gestão ainda não disponibilizam essas funcionalidades.
- Desenvolvedores e fornecedores de sistemas de transporte.

## Considerações finais

O **InformarCIOTeVPO** é uma ferramenta auxiliar independente, desenvolvida para facilitar a inclusão e a edição de informações