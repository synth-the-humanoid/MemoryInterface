# MemoryInterface

Simple C# API to allow for easy access to Windows application-level process memory. Intended primarily for game-hacking in conjunction with software such as Cheat Engine to facilitate creation of modding APIs, data extractors, etc.

Create a new MemoryInterface using the constructor, passing the name of the process as the sole parameter. From there, you can use the Read/Write functions and use the BaseAddress property to find the Base Address. Ensure you call the Close() method at the end to free the handle.
