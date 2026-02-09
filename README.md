<p align="center">

# convertTool

**Windows CLI Image → ICO Converter with Shell Integration**

</p>

<p align="center">
PNG / JPG to ICO conversion tool designed for Windows power users and developers.
</p>

---

## 🚀 Overview

`convertTool` is a Windows-focused CLI utility for converting PNG and JPG images into ICO icon files.

The tool is designed for reliability, portability, and deep Windows shell integration.

---

## ✨ Core Features

- PNG / JPG → ICO conversion
- Single-size and multi-size icon generation
- Custom resolution icon creation
- Multi-output batch generation (1–10 icons per run)
- Console progress rendering
- Convert history tracking
- Windows **Open With** integration
- Embedded shell helper utility
- Internal developer debug interface

---

## 🖥 Platform Support

| OS | Status |
|---|---|
| Windows 10 | Supported |
| Windows 11 | Supported |
| Linux | Not Supported |
| macOS | Not Supported |

---

## 📦 Project Architecture
convertTool/
├ convertTool/ Main converter engine
├ convertTool_shell/ Shell helper CLI utility
└ convertTool.sln Solution file

---

## ⚙ Usage

### Launch Tool
convertTool.exe


### Launch with File
convertTool.exe image.png

### Windows Shell Integration
---

## 🧠 Convert Modes

| Mode | Description |
|---|---|
| Single Size | Generate one icon size |
| Multi Size | Generate standard icon set |
| Custom Size | User-defined icon sizes |

---
## 📁 History System

The tool stores converted source images inside:
Allows quick reuse when launching tool without selecting a file.

---

## 🪟 Windows Integration

Optional Open With registration available during first launch.

