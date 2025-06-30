# StarOS

StarOS is a real, low-level operating system written in C# using the Cosmos framework. It features both a command-line terminal and a graphical user interface (GUI) with a desktop environment and dock.

---

## Features

- **Command-Line Terminal**  
  - Supports common file operations: `copy`, `cut`, `echo`, `create`, `delete`, `help`, `exit`  
  - Interactive shell with command parsing and execution  

- **Graphical User Interface (GUI)**  
  - Desktop environment with a dock and icons  
  - Resizable and interactive dock icons with hover effects  
  - Terminal window integrated into the GUI  
  - Custom window rendering with rounded corners  

- **Bootloader Menu**  
  - Allows user to select between text mode and GUI mode at startup  
  - System shutdown option  

---

## Project Structure

- `Kernel.cs` – Core kernel logic managing boot menu and switching between terminal and GUI modes  
- `Terminal.cs` – Terminal implementation with command execution  
- `Gui.cs` – Graphical interface handling drawing, input, and window management  
- `Boot.cs` – Initialization and setup routines for the GUI and system  

---

## Requirements

- [Cosmos OS](https://cosmosos.github.io) — A C# open-source OS development framework  
- Visual Studio with Cosmos extension installed  
- Emulator (e.g. QEMU) or compatible x86_64 hardware for deployment and testing  

---

## Getting Started

1. Set up the Cosmos OS development environment in Visual Studio  
2. Import the StarOS source code into your Cosmos project  
3. Build the project and launch in your preferred emulator or hardware  
4. Upon boot, select the desired mode: terminal or GUI  

---

## Example Terminal Commands

```bash
copy <source> <destination>    # Copy files or directories
cut <source> <destination>     # Move files or directories
echo <text>                    # Print text to console
create <filename>              # Create an empty file
delete <filename>              # Delete a file
help                          # Show available commands
exit                          # Exit the terminal shell
