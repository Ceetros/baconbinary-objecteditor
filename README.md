# BaconBinary's ObjectEditor

## Overview

BaconBinary's ObjectEditor is a `.dat` and `.spr` file editor for the Tibia game, developed as part of the Open Tibia ecosystem. Built with the modern .NET stack, this project aims to offer a faster and more performant alternative to the traditional Object Builder. The tool is designed to provide a robust and efficient interface for manipulating game assets, allowing for the inspection and modification of items, creatures, and other graphical elements.

## Features

`BaconBinary.ObjectEditor` implements essential and advanced features for Tibia asset editing:

-   **High Performance:** It's faster to load or compile your projects compared to legacy tools.
-   **Frame Groups Support:** Full support for handling frame groups in newer client versions.
-   **Negative Offsets Support:** Precise manipulation of sprite positioning.
-   **Transparency Support:** Correct rendering of sprites with alpha channels.

## Architecture & Performance

A core pillar of `BaconBinary.ObjectEditor` is its fully asynchronous architecture. All I/O operations, such as reading and writing `.dat` and `.spr` files, are implemented using C#'s `async/await` pattern. This ensures the user interface remains responsive and fluid, even when handling large files. Background processing prevents freezing and provides a significantly superior user experience compared to traditional synchronous approaches.

## Supported Tibia Versions

The editor supports a wide range of Tibia client versions, spanning from **7.3** to **10.98**.

## Supported Platforms

Thanks to its .NET foundation, `BaconBinary.ObjectEditor` is a cross-platform application. It is fully compatible with the following operating systems:

-   Windows
-   macOS
-   Linux

This allows developers and users to use the tool in their preferred environment without the need for virtualization or compatibility layers.

## Future Roadmap

The project continues to evolve, with plans to expand features and platform support:

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

## Apoiadores e Inspiração

Este projeto não seria possível sem o apoio e a inspiração de diversas fontes da comunidade:

-   **PokeWorldOnline:** Projeto que contribuiu para a ideia inicial da criação da ferramenta.
-   **Ninja Chronicles:** Projeto ativo que apoia e utiliza o `BaconBinary.ObjectEditor`.
-   **Object Builder:** Ferramenta que serviu como principal inspiração para a interface de usuário (UI). 
-   **Ceetros:** Criador do projeto.

*Caso queira seu nick/projeto como apoiador, entre em contato ou faça um Pull Request com alguma correção e/ou feature.*

## Nota sobre o Código-Fonte

É importante notar que, embora o núcleo do `BaconBinary.ObjectEditor` seja open-source, certas funcionalidades avançadas, como o suporte à criptografia de clientes mais recentes, serão mantidas como módulos de código fechado.

No entanto, para fomentar a colaboração e a extensibilidade, forneceremos um template open-source que servirá como base para que a comunidade possa desenvolver e integrar suas próprias implementações de criptografia como módulos externos.

## Ecossistema BaconBinary

O `BaconBinary.ObjectEditor` é a primeira de uma série de ferramentas que utilizarão um core compartilhado para a manipulação de assets do Tibia. O plano é expandir este ecossistema com os seguintes projetos:

-   **BaconBinary.MapEditor:** A complete map editor built on the same technological base.
-   **BaconBinary.ItemEditor:** A tool dedicated to editing items and their properties.
-   **BaconBinary.GClient:** A Tibia client developed in Godot, currently in the architecture evaluation phase, which will consume the core libraries to interact with game files.

## Support the Project

If you find `BaconBinary.ObjectEditor` useful and want to support its development, consider making a donation. Your support helps cover development costs and encourages the continuous improvement of the tool.

[![Donate with PayPal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=5Q8YX497C9QWU)