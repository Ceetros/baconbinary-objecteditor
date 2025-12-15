# BaconBinary.ObjectEditor

## Visão Geral

O BaconBinary.ObjectEditor é um editor de arquivos `.dat` e `.spr` para o jogo Tibia, desenvolvido como parte do ecossistema Open Tibia. Construído com a moderna stack .NET, este projeto visa oferecer uma alternativa mais rápida e performática ao tradicional Object Builder. A ferramenta foi projetada para fornecer uma interface robusta e eficiente para a manipulação de assets do jogo, permitindo a inspeção e modificação de itens, criaturas e outros elementos gráficos.

## Features

O `BaconBinary.ObjectEditor` já implementa funcionalidades essenciais para a edição de assets do Tibia:

-   **Suporte a Offsets Negativos:** Manipulação precisa do posicionamento dos sprites.
-   **Suporte a Transparência:** Renderização correta de sprites com canais alfa.

## Arquitetura e Performance

Um dos pilares do `BaconBinary.ObjectEditor` é sua arquitetura totalmente assíncrona. Todas as operações de I/O, como leitura e escrita dos arquivos `.dat` e `.spr`, são implementadas utilizando o padrão `async/await` do C#. Isso garante que a interface do usuário permaneça responsiva e fluida, mesmo ao manipular arquivos de grande volume. O processamento em background evita travamentos e proporciona uma experiência de usuário significativamente superior em comparação com abordagens síncronas tradicionais.

## Versões do Tibia Suportadas

O editor oferece suporte a uma ampla gama de versões do cliente Tibia, abrangendo desde a versão **7.3** até a **10.98**.

## Plataformas Suportadas

Graças à sua construção sobre o .NET, o `BaconBinary.ObjectEditor` é uma aplicação multiplataforma. Ele é totalmente compatível com os seguintes sistemas operacionais:

-   Windows
-   macOS
-   Linux

Isso permite que os desenvolvedores e usuários utilizem a ferramenta em seu ambiente de preferência sem a necessidade de virtualização ou camadas de compatibilidade.

## Roadmap Futuro

O projeto continua em evolução, com planos de expandir funcionalidades e o suporte para novas plataformas:

-   **Edição de Sprites:** Implementação de ferramentas avançadas para edição gráfica diretamente na aplicação.
-   **Suporte a Criptografia:** Capacidade de lidar com arquivos de assets criptografados de diferentes versões do cliente.
-   **Browser (WebAssembly):** Porte da aplicação para rodar diretamente no navegador utilizando Blazor e WebAssembly, eliminando a necessidade de instalação.
-   **Mobile (iOS/Android):** Extensão do suporte para dispositivos móveis através do .NET MAUI, possibilitando a edição de assets em smartphones e tablets.

## Começando

Para compilar e executar este projeto, é necessário ter o SDK do .NET instalado. O projeto utiliza submódulos para gerenciar dependências externas, portanto, é crucial inicializá-los após a clonagem do repositório.

### Clonando o Repositório

Para obter o código-fonte, clone o repositório usando o seguinte comando:

```bash
git clone --recursive https://github.com/Ceetros/baconbinary-objecteditor.git
cd baconbinary-objecteditor
```

Se você já clonou o repositório sem a flag `--recursive`, pode inicializar os submódulos com o seguinte comando:

```bash
git submodule update --init --recursive
```

### Compilando o Projeto

A compilação do projeto pode ser realizada através da interface de linha de comando do .NET. Execute o seguinte comando na raiz do projeto:

```bash
dotnet build
```

Este comando irá restaurar as dependências do NuGet e compilar a solução `BaconBinary.ObjectEditor.sln`.

## Executando o Projeto

Após a compilação bem-sucedida, a aplicação pode ser iniciada. O projeto principal da interface do usuário é o `BaconBinary.ObjectEditor.UI`. Para executá-lo, utilize o seguinte comando:

```bash
dotnet run --project BaconBinary.ObjectEditor.UI
```

## Estrutura do Projeto

A solução está organizada nos seguintes projetos e diretórios principais:

-   `BaconBinary.ObjectEditor.UI/`: Contém a implementação da interface do usuário da aplicação.
-   `external/`: Diretório que abriga os submódulos e dependências externas do projeto.
-   `BaconBinary.ObjectEditor.sln`: O arquivo de solução principal para o Visual Studio.

## Contribuindo

Contribuições para o desenvolvimento do BaconBinary.ObjectEditor são bem-vindas. Para contribuir, por favor, siga estas diretrizes:

1.  Faça um fork do repositório.
2.  Crie uma nova branch para a sua feature (`git checkout -b feature/nova-feature`).
3.  Faça o commit de suas alterações (`git commit -am 'Adiciona nova feature'`).
4.  Faça o push para a branch (`git push origin feature/nova-feature`).
5.  Abra um Pull Request.

## Nota sobre o Código-Fonte

É importante notar que, embora o núcleo do `BaconBinary.ObjectEditor` seja open-source, certas funcionalidades avançadas, como o suporte à criptografia de clientes mais recentes, serão mantidas como módulos de código fechado.

No entanto, para fomentar a colaboração e a extensibilidade, forneceremos um template open-source que servirá como base para que a comunidade possa desenvolver e integrar suas próprias implementações de criptografia como módulos externos.

## Ecossistema BaconBinary

O `BaconBinary.ObjectEditor` é a primeira de uma série de ferramentas que utilizarão um core compartilhado para a manipulação de assets do Tibia. O plano é expandir este ecossistema com os seguintes projetos:

-   **BaconBinary.MapEditor:** Um editor de mapas completo, construído sobre a mesma base tecnológica.
-   **BaconBinary.ItemEditor:** Uma ferramenta dedicada à edição de itens e suas propriedades.
-   **BaconBinary.GClient:** Um cliente de Tibia desenvolvido em Godot, atualmente em fase de avaliação de arquitetura, que consumirá as bibliotecas do core para interagir com os arquivos do jogo.
