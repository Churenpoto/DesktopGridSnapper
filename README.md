# Desktop Grid Snapper

A Windows desktop utility that creates grid overlays on your monitors and automatically snaps desktop icons to grid positions.

## Features

- **Multi-Monitor Support**: Automatically creates grid overlays for all connected monitors
- **Customizable Grid**: Configure cell width, height, and offsets for icon positioning
- **Grid Color Control**: Customize grid color and opacity
- **Keyboard Shortcuts**: Use Alt + Arrow keys to move selected icons by one grid cell
- **Auto-Snap**: Desktop icons automatically snap to grid positions when dragged
- **System Tray Integration**: Runs in the background with system tray icon
- **Settings Persistence**: Grid settings are saved and restored on application restart

## Multi-Monitor Configuration

The application automatically detects all connected monitors and creates a grid overlay for each one. Icons on any monitor will snap to the grid on that specific monitor, ensuring proper alignment across your entire desktop setup.

When monitors are added or removed, the application automatically adjusts the grid overlays accordingly.

## Usage

1. Launch the application - it will run in the system tray
2. Double-click the tray icon or right-click and select "Open Settings" to configure the grid
3. Adjust the grid parameters:
   - **Screen**: Select which screen to preview (grid applies to all screens)
   - **Cell (px)**: Set the grid cell width and height
   - **Offset (x,y)**: Fine-tune icon positioning within each cell
   - **Grid Color**: Choose the grid line color
   - **Grid Opacity**: Adjust grid transparency (0-255)
4. Click "Apply" to update the grid
5. Use Alt + Arrow keys to move selected icons by one grid cell
6. Drag icons normally - they will snap to the grid on release

## System Requirements

- Windows 10 or later
- .NET 9.0 or later
- Multi-monitor support works with any number of connected displays

## Technical Details

The application uses the following approach for multi-monitor support:
- Creates a `GridOverlay` window for each connected monitor
- Automatically detects which monitor an icon is on based on its coordinates
- Snaps each icon to the appropriate grid based on its screen location
- Handles monitor configuration changes dynamically
