namespace BenchRig.App.Components.Keyboard;

/// <summary>A single physical key in the on-screen layout.</summary>
public readonly record struct KeyCap(string Code, string Label, double Width = 1);

/// <summary>Shared ANSI keyboard layout used by the registration / ghosting visualizers.</summary>
public static class KeyboardLayout
{
    public static readonly KeyCap[][] Rows =
    [
        [
            new("Escape", "Esc"), new("F1","F1"), new("F2","F2"), new("F3","F3"), new("F4","F4"),
            new("F5","F5"), new("F6","F6"), new("F7","F7"), new("F8","F8"),
            new("F9","F9"), new("F10","F10"), new("F11","F11"), new("F12","F12"),
        ],
        [
            new("Backquote","`"), new("Digit1","1"), new("Digit2","2"), new("Digit3","3"), new("Digit4","4"),
            new("Digit5","5"), new("Digit6","6"), new("Digit7","7"), new("Digit8","8"), new("Digit9","9"),
            new("Digit0","0"), new("Minus","-"), new("Equal","="), new("Backspace","Bksp", 2),
        ],
        [
            new("Tab","Tab", 1.5), new("KeyQ","Q"), new("KeyW","W"), new("KeyE","E"), new("KeyR","R"),
            new("KeyT","T"), new("KeyY","Y"), new("KeyU","U"), new("KeyI","I"), new("KeyO","O"),
            new("KeyP","P"), new("BracketLeft","["), new("BracketRight","]"), new("Backslash","\\", 1.5),
        ],
        [
            new("CapsLock","Caps", 1.8), new("KeyA","A"), new("KeyS","S"), new("KeyD","D"), new("KeyF","F"),
            new("KeyG","G"), new("KeyH","H"), new("KeyJ","J"), new("KeyK","K"), new("KeyL","L"),
            new("Semicolon",";"), new("Quote","'"), new("Enter","Enter", 2.2),
        ],
        [
            new("ShiftLeft","Shift", 2.3), new("KeyZ","Z"), new("KeyX","X"), new("KeyC","C"), new("KeyV","V"),
            new("KeyB","B"), new("KeyN","N"), new("KeyM","M"), new("Comma",","), new("Period","."),
            new("Slash","/"), new("ShiftRight","Shift", 2.5),
        ],
        [
            new("ControlLeft","Ctrl", 1.4), new("MetaLeft","Win", 1.2), new("AltLeft","Alt", 1.2),
            new("Space","Space", 6), new("AltRight","Alt", 1.2), new("MetaRight","Win", 1.2),
            new("ContextMenu","Menu", 1.2), new("ControlRight","Ctrl", 1.4),
        ],
    ];

    /// <summary>Arrow + nav cluster shown beside the main block.</summary>
    public static readonly KeyCap[][] ArrowRows =
    [
        [ new("ArrowUp","↑") ],
        [ new("ArrowLeft","←"), new("ArrowDown","↓"), new("ArrowRight","→") ],
    ];

    public static int TotalKeys => Rows.Sum(r => r.Length) + ArrowRows.Sum(r => r.Length);
}
