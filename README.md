# LuxonServerIntgeration

## Setup

Download a .nupkg from [GitLab](https://gitlab.com/niansa/LuxonServerSharp/-/artifacts) and place it in the nuget/ folder.

## Mixed mode debugging into LuxonServer

1. Build libluxon_server_ffi.so with debug symbols. Clone the `LuxonServer` repo, check out  
   the same commit hash that LuxonServerSharp was built against, and run
   ```
   cmake -S . -B build -G Ninja -DCMAKE_BUILD_TYPE=RelWithDebInfo -DCMAKE_UNITY_BUILD=ON -DLUXON_SERVER_BUILD_FFI=ON -DLUXON_SERVER_EXPOSE_FULL_FFI=ON
   cmake --build build/
   ```
2. Copy `libluxon_server_ffi.so` somewhere, name it `libluxon_server.so` and abuse `LD_PRELOAD`
   ```
   cp build/libluxon_server_ffi.so /path/to/libluxon_server.so
   LD_PRELOAD=/path/to/libluxon_server.so dotnet run
   ```
3. Use [lldb with dotnet-extensions] and attach to the process