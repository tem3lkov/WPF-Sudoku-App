# WPF Sudoku Application

A feature-rich desktop Sudoku game built with C# and Windows Presentation Foundation (WPF). This project focuses on efficient algorithmic puzzle generation, robust state management, and data adaptation for object serialization.

<img width="1000" height="537" alt="image" src="https://github.com/user-attachments/assets/d4a58b96-0d24-47ea-951c-b1364935c8b0" />

## How to Run
1. Clone the repository to your local machine.
2. Open the `.sln` (solution) file in **Visual Studio 2022** (or newer).
3. Ensure the `.NET Framework` (or .NET Core/5+, depending on your setup) is installed.
4. Build the solution (`Ctrl + Shift + B`) and run the project (`F5`).

## Core Features

- **Algorithmic Puzzle Generation & Solving:** Implemented a highly optimized Backtracking algorithm to generate and solve grids. Includes a custom `CountSolutions` mathematical validation to guarantee each generated puzzle has exactly one unique solution.
- **State Management (Undo/Redo):** Engineered a move history system using Stack (LIFO) data structures. Safely records cell state transitions without memory leaks or UI event loop bottlenecks.
- **XML Serialization Integration:** Integrated a provided XML serialization module (`XML_JSONutils`) by developing custom Data Transfer Objects (DTOs). Successfully bridged the gap between the game's 2D grid architecture and the provided serializer by flattening the arrays into 1D lists to save and load puzzle progress.
- **Optimized UI/Logic Separation:** Achieved clean separation of concerns by dynamically detaching and reattaching UI event handlers during programmatic grid updates, preventing infinite loops and maintaining $O(N)$ validation performance.

## Technologies Used
- **Language:** C#
- **Framework:** .NET / WPF (Windows Presentation Foundation)
- **Concepts:** Backtracking Algorithms, Data Structures (Stacks, 2D Arrays), Serialization (XML), DTOs, OOP Design[cite: 6]
