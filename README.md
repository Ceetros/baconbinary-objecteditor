# BaconBinary Suite: ObjectEditor

**Version 0.0.1d**

## Overview

ObjectEditor is a `.dat` and `.spr` file editor for the Tibia game, developed as part of the Open Tibia ecosystem. Built with the modern .NET stack, this project aims to offer a faster and more performant alternative to the traditional Object Builder. The tool is designed to provide a robust and efficient interface for manipulating game assets, allowing for the inspection and modification of items, creatures, and other graphical elements.

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

-   **Sprite Editing:** Implementation of advanced tools for graphic editing directly within the application.
-   **Encryption Support:** Ability to handle encrypted asset files from different client versions.
-   **Browser (WebAssembly):** Porting the application to run directly in the browser using Blazor and WebAssembly, eliminating the need for installation.
-   **Mobile (iOS/Android):** Extending support to mobile devices via .NET MAUI, enabling asset editing on smartphones and tablets.

## Getting Started

To compile and run this project, you need the .NET SDK installed. The project uses submodules to manage external dependencies, so it is crucial to initialize them after cloning the repository.

### Cloning the Repository

To get the source code, clone the repository using the following command:

```bash
git clone --recursive https://github.com/Ceetros/baconbinary-objecteditor.git
cd baconbinary-objecteditor
```

If you have already cloned the repository without the `--recursive` flag, you can initialize the submodules with:

```bash
git submodule update --init --recursive
```

### Building the Project

You can build the project using the .NET CLI. Run the following command in the project root:

```bash
dotnet build
```

This command will restore NuGet dependencies and build the `BaconBinary.ObjectEditor.sln` solution.

### Running the Project

After a successful build, you can start the application. The main UI project is `BaconBinary.ObjectEditor.UI`. To run it, use:

```bash
dotnet run --project BaconBinary.ObjectEditor.UI
```

## Project Structure

The solution is organized into the following main projects and directories:

-   `BaconBinary.ObjectEditor.UI/`: Contains the user interface implementation.
-   `external/`: Directory housing submodules and external project dependencies.
-   `BaconBinary.ObjectEditor.sln`: The main solution file for Visual Studio.

## Contributing

Contributions to BaconBinary.ObjectEditor are welcome. To contribute, please follow these guidelines:

1.  Fork the repository.
2.  Create a new branch for your feature (`git checkout -b feature/new-feature`).
3.  Commit your changes (`git commit -am 'Add new feature'`).
4.  Push to the branch (`git push origin feature/new-feature`).
5.  Open a Pull Request.

## Supporters & Inspiration

This project would not be possible without the support and inspiration from various community sources:

-   **PokeWorldOnline:** Contributed to the initial idea of creating the tool.
-   **Ninja Chronicles:** Active project supporting and using `BaconBinary.ObjectEditor`.
-   **Object Builder:** The tool that served as the main inspiration for the User Interface (UI). 
-   **Ceetros:** Creator and backend developer.

*If you want your nickname/project listed as a supporter, please contact us or submit a Pull Request with a fix or feature.*

## Note on Source Code

It is important to note that while the core of `BaconBinary.ObjectEditor` is open-source, certain advanced features, such as encryption support for newer clients, will be kept as closed-source modules.

However, to foster collaboration and extensibility, we will provide an open-source template that will serve as a base for the community to develop and integrate their own encryption implementations as external modules.

## BaconBinary Ecosystem

`BaconBinary.ObjectEditor` is the first in a series of tools that will utilize a shared core for Tibia asset manipulation. The plan is to expand this ecosystem with the following projects:

-   **BaconBinary.MapEditor:** A complete map editor built on the same technological base.
-   **BaconBinary.ItemEditor:** A tool dedicated to editing items and their properties.
-   **BaconBinary.GClient:** A Tibia client developed in Godot, currently in the architecture evaluation phase, which will consume the core libraries to interact with game files.

## Support the Project

If you find `BaconBinary.ObjectEditor` useful and want to support its development, consider making a donation. Your support helps cover development costs and encourages the continuous improvement of the tool.

[![Donate with PayPal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=5Q8YX497C9QWU)
