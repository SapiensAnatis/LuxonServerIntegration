# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-resolute AS native

RUN apt-get update && apt-get install -y --no-install-recommends \
        build-essential \
        cmake \
        ninja-build \
        git \
        ca-certificates \
        libmbedtls-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

COPY ["./submodules/LuxonServer", "."]

RUN cmake -S . -B build -G Ninja \
        -DCMAKE_BUILD_TYPE=RelWithDebInfo \
        -DCMAKE_UNITY_BUILD=ON \
        -DLUXON_SERVER_BUILD_FFI=ON \
        -DLUXON_SERVER_EXPOSE_FULL_FFI=ON \
        -DSANMAKE_ASAN=ON \
    && cmake --build build --parallel

FROM mcr.microsoft.com/dotnet/sdk:10.0-resolute AS dotnet

WORKDIR /src
COPY [".", "."]

RUN dotnet publish Luxon.Server.Host/Luxon.Server.Host.csproj -c Release -o out/host

# NOTE: PluginAssemblyLoadContext hardcodes the plugin path as literally
# ".../bin/Debug/net10.0/" (see the TODO there), hence -c Debug.
RUN dotnet publish Luxon.Server.GluonPlugin/Luxon.Server.GluonPlugin.csproj -c Debug -o out/plugin

FROM mcr.microsoft.com/dotnet/runtime:10.0-resolute AS runtime

RUN apt-get update && apt-get install -y --no-install-recommends \
        libasan8 \
        libubsan1 \
        libmbedtls21 \
    && rm -rf /var/lib/apt/lists/*


WORKDIR /app

COPY --from=native /src/build/libluxon_server_ffi.so /usr/lib/x86_64-linux-gnu/libluxon_server.so
RUN ldconfig

# DIRTY HACKS
COPY --from=dotnet /src/out/host/   ./Luxon.Server.Host/bin/Release/net10.0/
COPY --from=dotnet /src/out/plugin/ ./Luxon.Server.GluonPlugin/bin/Debug/net10.0/
WORKDIR /app/Luxon.Server.Host/bin/Release/net10.0

ENV LD_PRELOAD=libasan.so.8

EXPOSE 5055/udp 5056/udp 5058/udp 27000/udp 27001/udp 27002/udp

ENTRYPOINT ["dotnet", "Luxon.Server.Host.dll"]