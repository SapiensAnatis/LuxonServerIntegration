# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-resolute AS native

WORKDIR /src

RUN apt-get update && apt-get install -y --no-install-recommends \
        build-essential \
        cmake \
        ninja-build \
        git \
        ca-certificates \
        libmbedtls-dev \
    && rm -rf /var/lib/apt/lists/* 

ARG LUXON_SERVER_COMMIT_HASH="efc288fe6ff21764ab1bd934347f596bab955b02"

RUN git init -q && \
    git remote add origin https://gitlab.com/luxon_project/LuxonServer.git && \
    git fetch --depth 1 origin "$LUXON_SERVER_COMMIT_HASH" && \
    git checkout FETCH_HEAD && \
    git submodule update --init --recursive --depth 1

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

# Publish the plugin straight into the stable layout the host loads from:
#   <base>/plugins/<Name>/<Version>/<AssemblyName>
# (see PluginAssemblyLoadContext). Version must match plugin.yml (v1).
RUN dotnet publish Luxon.Server.GluonPlugin/Luxon.Server.GluonPlugin.csproj -c Release -o out/host/plugins/GluonPlugin/v1

FROM mcr.microsoft.com/dotnet/runtime:10.0-resolute AS runtime

RUN apt-get update && apt-get install -y --no-install-recommends \
        libasan8 \
        libubsan1 \
        libmbedtls21 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=native /src/build/libluxon_server_ffi.so /usr/lib/x86_64-linux-gnu/libluxon_server.so
RUN ldconfig

# The published host carries its plugins under ./plugins/<Name>/<Version>/.
COPY --from=dotnet /src/out/host/ ./

ENV LD_PRELOAD="libasan.so.8"
# libcoreclr trips "alloc-dealloc-mismatch (operator new [] vs operator delete)" sometimes - likely false positive
ENV ASAN_OPTIONS="alloc_dealloc_mismatch=0"

EXPOSE 5055/udp 5056/udp 5058/udp 27000/udp 27001/udp 27002/udp

ENTRYPOINT ["dotnet", "Luxon.Server.Host.dll"]