# BackgroundChanger

A lightweight Windows system tray app that automatically changes your desktop wallpaper based on the time of day.
## Features

- Runs silently in the system tray
- Automatically switches wallpaper every 5 minutes based on time of day
- Right-click tray icon for quick actions

## Time Schedule

| Time          | Wallpaper File            |
|---------------|---------------------------|
| 00:00 - 05:59 | `Default.png`             |
| 06:00 - 17:59 | `BG.jpg`                  |
| 18:00 - 21:59 | `BG_Night.jpg`            |
| 22:00 - 23:59 | `BG_Night_LightOff.jpg`   |

## Getting Started

1. Download the latest release
2. Create a `.bg` folder next to `BackgroundChanger.exe`
3. Add your wallpaper images to the `.bg` folder using the filenames above
4. Run `BackgroundChanger.exe`

### Expected Folder Structure

```
BackgroundChanger/
├── BackgroundChanger.exe
├── icon.ico
└── .bg/
    ├── Default.png
    ├── BG.jpg
    ├── BG_Night.jpg
    └── BG_Night_LightOff.jpg
```

Replace the images with your own wallpapers. Keep the same filenames.

## Tray Menu

Right-click the tray icon to access the menu:

| Option                    | Description                                               |
|---------------------------|-----------------------------------------------------------|
| **Change Now**            | Force-apply the wallpaper for the current time bracket    |
| **Open Images Folder**    | Opens the `.bg` folder in Explorer                        |
| **Start with Windows**    | Toggle auto-launch on login                               |
| **Exit**                  | Close the app                                             |

## Requirements
- Windows 10 / 11
